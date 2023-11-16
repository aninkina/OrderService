using System.Data.Common;
using Dapper;
using Ozon.Route256.Practice.OrdersService.Application.Services;
using Ozon.Route256.Practice.OrdersService.Domain.Dto.Requests;
using Ozon.Route256.Practice.OrdersService.Domain.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Dal.Shard.Connection;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Dal.Shard.Migrator;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Dal.Shard.Rules;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Extensions;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Entity;

namespace Ozon.Route256.Practice.OrdersService.Repository.Impl;

public class ShardOrderRepository : BaseShardRepository, IOrdersRepositoryV2
{
    public ShardOrderRepository(
        IShardConnectionFactory connectionFactory,
        IShardingRule<long> longShardingRule,
        IShardingRule<string> stringShardingRule) : base(connectionFactory, longShardingRule, stringShardingRule)
    { }

    private const string Fields = "id, count, price, weight, source, state, start_time, region, customer_id";
    private const string FieldsForInsert = "id, count, price, weight, source, state, start_time, region, customer_id";

    private const string Table = $"{Shards.BucketPlaceholder}.orders";
    private const string CustomerIndex = $"{Shards.BucketPlaceholder}.idx_customer_id";
    private const string RegionIndex = $"{Shards.BucketPlaceholder}.idx_region_id";

    public async Task<OrderDto?> Find(long orderId, CancellationToken token)
    {
        await using var connection = GetConnectionByBucket((int)orderId, token);

        var order = await QueryOrder(orderId, connection, token);

        if (order.Id == 0)
        {
            return null;
        }

        var customer = await QueryCustomer(order.CustomerId, connection, token);

        var address = await QueryAddress(customer.AddressId, connection, token);

        return order.ToDtoType(customer, address);
    }

    public async Task<IEnumerable<RegionDto>> GetAllRegions(CancellationToken token)
    {
        //TODO: добавить миграцию регионов в константный эндпоинт
        var regionTable = $"{Shards.BucketPlaceholder}.regions";

        string sql = @$"
            SELECT name, latitude, longitude
            FROM {regionTable}
        ";

        await using var connection = GetConnectionByBucket(0, token);
        var items = await connection.QueryAsync<RegionDto>(sql);

        return items.ToArray();
    }

    public async Task<IReadOnlyList<OrderDto>> GetCustomerOrders(GetCustomerOrdersDto request, CancellationToken token)
    {
        const string indexSql = @$"
            select order_id 
            from {CustomerIndex}
            where customer_id = :customer_id
        ";

        IEnumerable<int> orderIds;
        await using (var connectionIndex = GetConnectionByShardKey(request.Id))
        {
            orderIds = await connectionIndex.QueryAsync<int>(indexSql, new { request.Id });
        }

        const string sql = $@"
            select  {Fields}
            from {Table}
            where id = any(:ids)
            and  start_time <= :start
            limit :pageLimit offset :pageOffset
        ";

        var bucketToIdsMap = orderIds
            .Select(id => (BucketId: _longShardingRule.GetBucketId(id), Id: id))
            .GroupBy(x => x.BucketId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Id).ToArray());

        var result = new List<OrderEntity>();
        foreach (var (bucketId, idsInBucket) in bucketToIdsMap)
        {
            await using var connection = GetConnectionByBucket((int)bucketId, token);
            var ordersInBucket = await connection.QueryAsync<OrderEntity>(sql,
                new
                {
                    ids = idsInBucket,
                    start = request.StartTime,
                    pageLimit = request.PaginationDto.PageSize,
                    pageOffset = (request.PaginationDto.PageNumber - 1) * request.PaginationDto.PageSize
                });
            result.AddRange(ordersInBucket);
        }

        return result.Select(x => x.ToSimpleDtoType()).ToList();
    }

    public Task<IReadOnlyList<OrderDto>> GetRegionOrders(GetRegionOrdersDto request, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task Insert(OrderDto order, CancellationToken token)
    {
        await using var connection = GetConnectionByBucket((int)order.Id, token);

        await using var transaction = connection.BeginTransaction();

        await InsertOrder(order, connection, token);

        await InsertCustomer((int)order.Id, order.Customer, connection, token);

        await InsertAddress((int)order.Id, order.Customer.Address, connection, token);

        transaction.Commit();
    }

    public async Task UpdateState(OrderDto order, CancellationToken token)
    {
        const string sql = @$"
            update {Table}
            set state = :state
            where id = :id
        ";

        await using var connection = GetConnectionByBucket((int)order.Id, token);

        await connection.QuerySingleOrDefaultAsync(
            new CommandDefinition(
                sql,
                cancellationToken: token,
                parameters: new
                {
                    state = (int)order.State,
                    order.Id,
                }));
    }

    private async Task InsertOrder(OrderDto order, DbConnection connection, CancellationToken token)
    {
        const string sql = @$"
            insert into {Table} ({FieldsForInsert})
            values (:id, :count, :price, :weight, :source, :state, :start_time, :region, :customer_id)
        ";


        await connection.QuerySingleOrDefaultAsync(
            new CommandDefinition(
                sql,
                cancellationToken: token,
                parameters:
                new
                {
                    order.Id,
                    order.Count,
                    order.Price,
                    order.Weight,
                    order.Source,
                    order.State,
                    order.StartTime,
                    order.Region,
                    customerId = order.Customer.Id,
                }));

        // -------------------------

        const string indexSql = $@"
            insert into  {CustomerIndex} (customer_id, order_id)
            VALUES (:customer_id, :order_id)
        ";

        await using var connection2 = GetConnectionByShardKey((int)order.Customer.Id);

        await connection2.ExecuteAsync(indexSql,
            new
            {
                customerId = order.Customer.Id,
                orderId = order.Id
            });
    }

    private static async Task InsertCustomer(int orderId, CustomerDto customer, DbConnection connection, CancellationToken token)
    {
        const string sql = @$"
            insert into customers (id, name, phone_number, address_id)
            values (:id, :name, :phone_number, :address_id)
            on conflict (id) do nothing
        ";

        await connection.QuerySingleOrDefaultAsync(
            new CommandDefinition(
                sql,
                cancellationToken: token,
                parameters: new
                {
                    orderId,
                    customer.Id,
                    customer.Name,
                    customer.PhoneNumber
                }));
    }

    private static async Task InsertAddress(int orderId, AddressDto address, DbConnection connection, CancellationToken token)
    {
        const string sql = @$"
            insert into addresses (id, region, city, street, building, apartment, latitude, longitude)
            values (:id, :region, :city, :street, :building, :apartment, :latitude, :longitude)
            on conflict (id) do nothing
        ";

        await connection.QuerySingleOrDefaultAsync(
            new CommandDefinition(
                sql,
                cancellationToken: token,
                parameters: new
                {
                    orderId,
                    address.Region,
                    address.City,
                    address.Street,
                    address.Building,
                    address.Apartment,
                    address.Latitude,
                    address.Longitude
                }));
    }

    private static async Task<OrderEntity> QueryOrder(long id, DbConnection connection, CancellationToken token)
    {
        const string sql = $@"
            select  {Fields}
            from {Table}
            where id = :id
        ";

        return await connection.QuerySingleOrDefaultAsync<OrderEntity>(
           new CommandDefinition(
               sql,
               cancellationToken: token,
               parameters: new { id })); ;
    }

    private static async Task<CustomerEntity> QueryCustomer(int id, DbConnection connection, CancellationToken token)
    {
        const string sql = $@"
            select id, name, phone_number, address_id
            from {Shards.BucketPlaceholder}.customers
            where id = :id
        ";


        return await connection.QuerySingleOrDefaultAsync<CustomerEntity>(
           new CommandDefinition(
               sql,
               cancellationToken: token,
               parameters: new { id })); ;
    }

    private static async Task<AddressEntity> QueryAddress(int id, DbConnection connection, CancellationToken token)
    {
        const string sql = $@"
            select id, region, city, street, building, apartment, latitude, longitude
             from {Shards.BucketPlaceholder}.addresses
            where id = :id
        ";

        return await connection.QuerySingleOrDefaultAsync<AddressEntity>(
           new CommandDefinition(
               sql,
               cancellationToken: token,
               parameters: new { id })); ;
    }
}
