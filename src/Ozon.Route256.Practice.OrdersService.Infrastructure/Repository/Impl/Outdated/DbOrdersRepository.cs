using Microsoft.AspNetCore.Connections;
using Npgsql;
using System.Data;
using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Entity;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Extensions;
using System.Runtime.CompilerServices;
using System.Text;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Dal.Common;
using Ozon.Route256.Practice.OrdersService.Domain.Dto.Requests;
using Ozon.Route256.Practice.OrdersService.Domain.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Application.Services;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Impl.Outdated;

public class DbOrdersRepository : IOrdersRepository
{
    private readonly IPostgresConnectionFactory _connectionFactory;
    private readonly ILogger<DbOrdersRepository> _logger;

    private const string Fields = "id, count, price, weight, source, state, start_time, region, customer_id";
    private const string FieldsForInsert = "id, count, price, weight, source, state, start_time, region, customer_id";
    private const string Table = "orders";

    public DbOrdersRepository(IPostgresConnectionFactory connectionFactory, ILogger<DbOrdersRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<OrderDto?> Find(long orderId, CancellationToken token)
    {
        await using var connection = _connectionFactory.GetConnection();
        await connection.OpenAsync(token);

        var order = await QueryOrder(orderId, connection, token).FirstOrDefaultAsync(token);

        if (order.Id == 0)
        {
            return null;
        }

        var customer = await QueryCustomer(order.CustomerId, connection, token).FirstOrDefaultAsync(token);

        var address = await QueryAddress(customer.AddressId, connection, token).FirstOrDefaultAsync(token);

        return order.ToDtoType(customer, address);
    }

    public async IAsyncEnumerable<RegionDto> GetAllRegions([EnumeratorCancellation] CancellationToken token)
    {
        const string sql = """
            select name, latitude, longitude
            from 
            regions
        """;

        await using var connection = _connectionFactory.GetConnection();
        await using var command = new NpgsqlCommand(sql, connection);

        await connection.OpenAsync(token);
        await using var reader = await command.ExecuteReaderAsync(token);

        while (await reader.ReadAsync(token))
        {
            yield return ReadRegionEntity(reader).ToDtoType();
        }
    }

    public async IAsyncEnumerable<OrderDto> GetCustomerOrders(GetCustomerOrdersDto request, [EnumeratorCancellation] CancellationToken token)
    {
        const string sql = $@"
            select  {Fields}
            from {Table}
            where customer_id = :id
            and  start_time <= :start
            limit :pageLimit offset :pageOffset
        ";

        await using var connection = _connectionFactory.GetConnection();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add("id", request.Id);
        command.Parameters.Add("start", request.StartTime);
        command.Parameters.Add("pageLimit", request.PaginationDto.PageSize);
        command.Parameters.Add("pageOffset", (request.PaginationDto.PageNumber - 1) * request.PaginationDto.PageSize);

        await connection.OpenAsync(token);
        await using var reader = await command.ExecuteReaderAsync(token);

        while (await reader.ReadAsync(token))
        {
            yield return ReadOrderEntity(reader).ToSimpleDtoType();
        }
    }

    public IAsyncEnumerable<OrdersAggregationDto> GetOrdersAggregation(DateTime start, ICollection<string> regions, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<OrderDto> GetRegionOrders(GetRegionOrdersDto request, [EnumeratorCancellation] CancellationToken token)
    {
        string sql = $@"
            select  {Fields}
            from {Table}
            where region = any(:regions::text[])
            and source = :orderSource
            limit :pageLimit offset :pageOffset
        ";
        //order by :sortObject: sortType

        await using var connection = _connectionFactory.GetConnection();
        await using var command = new NpgsqlCommand(sql, connection);

        var sortObject = request.SortObject == SortObject.None ? "id" : request.SortObject.ToString();

        var a = request.SortType.ToString();
        command.Parameters.Add("regions", request.Regions);
        command.Parameters.Add("orderSource", (int)request.OrderSource);
        command.Parameters.Add("pageLimit", request.PaginationDto.PageSize);
        command.Parameters.Add("pageOffset", (request.PaginationDto.PageSize - 1) * request.PaginationDto.PageNumber);
        command.Parameters.Add("sortObject", sortObject);
        command.Parameters.Add("sortType", request.SortType.ToString());

        var c = command.Parameters;
        await connection.OpenAsync(token);
        await using var reader = await command.ExecuteReaderAsync(token);

        while (await reader.ReadAsync(token))
        {
            var res = ReadOrderEntity(reader).ToSimpleDtoType();
            yield return res;
        }
    }

    public async Task Insert(OrderDto order, CancellationToken token)
    {
        await using var connection = _connectionFactory.GetConnection();
        await connection.OpenAsync(token);

        await InsertAddress(order.Customer.Address, connection, token);

        await InsertCustomer(order.Customer, order.Customer.Address.GetHashCode(), connection, token);

        await InsertOrder(order, connection, token);
    }

    public async Task UpdateState(OrderDto order, CancellationToken token)
    {
        const string sql = @$"
            update {Table}
            set state = :state
            where id = :id
        ";

        await using var connection = _connectionFactory.GetConnection();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("state", (int)order.State);
        command.Parameters.Add("id", order.Id);

        await connection.OpenAsync(token);
        await command.ExecuteNonQueryAsync(token);
    }

    private async Task InsertOrder(OrderDto order, NpgsqlConnection connection, CancellationToken token)
    {
        const string sql = @$"
            insert into {Table} ({FieldsForInsert})
            values (:id, :count, :price, :weight, :source, :state, :start_time, :region, :customer_id)
        ";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("id", order.Id);
        command.Parameters.Add("count", order.Count);
        command.Parameters.Add("price", order.Price);
        command.Parameters.Add("weight", (int)order.Weight);
        command.Parameters.Add("source", (int)order.Source);
        command.Parameters.Add("state", (int)order.State);
        command.Parameters.Add("start_time", order.StartTime);
        command.Parameters.Add("region", order.Region);
        command.Parameters.Add("customer_id", order.Customer.Id);

        await command.ExecuteNonQueryAsync(token);
    }

    private async Task InsertCustomer(CustomerDto customer, int addressId, NpgsqlConnection connection, CancellationToken token)
    {
        const string sql = @$"
            insert into customers (id, name, phone_number, address_id)
            values (:id, :name, :phone_number, :address_id)
            on conflict (id) do nothing
        ";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("id", customer.Id);
        command.Parameters.Add("name", customer.Name);
        command.Parameters.Add("phone_number", customer.PhoneNumber);
        command.Parameters.Add("address_id", addressId);

        await command.ExecuteNonQueryAsync(token);
    }

    private async Task InsertAddress(AddressDto address, NpgsqlConnection connection, CancellationToken token)
    {
        const string sql = @$"
            insert into addresses (id, region, city, street, building, apartment, latitude, longitude)
            values (:id, :region, :city, :street, :building, :apartment, :latitude, :longitude)
            on conflict (id) do nothing
        ";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("id", address.GetHashCode());
        command.Parameters.Add("region", address.Region);
        command.Parameters.Add("city", address.City);
        command.Parameters.Add("street", address.Street);
        command.Parameters.Add("building", address.Building);
        command.Parameters.Add("apartment", address.Apartment);
        command.Parameters.Add("latitude", address.Latitude);
        command.Parameters.Add("longitude", address.Longitude);

        await command.ExecuteNonQueryAsync(token);
    }

    private async IAsyncEnumerable<OrderEntity> QueryOrder(long id, NpgsqlConnection connection, [EnumeratorCancellation] CancellationToken token)
    {
        const string sql = $@"
            select  {Fields}
            from {Table}
            where id = :id
        ";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("id", id);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, token);

        while (await reader.ReadAsync(token))
        {
            yield return ReadOrderEntity(reader);
        }
    }

    private async IAsyncEnumerable<CustomerEntity> QueryCustomer(int id, NpgsqlConnection connection, [EnumeratorCancellation] CancellationToken token)
    {
        const string sql = $@"
            select id, name, phone_number, address_id
            from customers
            where id = :id
        ";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("id", id);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, token);

        while (await reader.ReadAsync(token))
        {
            yield return ReadCustomerEntity(reader);
        }
    }

    private async IAsyncEnumerable<AddressEntity> QueryAddress(int id, NpgsqlConnection connection, [EnumeratorCancellation] CancellationToken token)
    {
        const string sql = $@"
            select id, region, city, street, building, apartment, latitude, longitude
            from addresses
            where id = :id
        ";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("id", id);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, token);

        while (await reader.ReadAsync(token))
        {
            yield return ReadAddressEntity(reader);
        }
    }

    private static OrderEntity ReadOrderEntity(NpgsqlDataReader reader) =>
        new(
            Id: reader.GetFieldValue<long>(0),
            Count: reader.GetFieldValue<int>(1),
            Price: reader.GetFieldValue<decimal>(2),
            Weight: (uint)reader.GetFieldValue<int>(3),
            Source: (OrderSource)reader.GetFieldValue<int>(4),
            State: (OrderState)reader.GetFieldValue<int>(5),
            StartTime: reader.GetFieldValue<DateTime>(6),
            Region: reader.GetFieldValue<string>(7),
            CustomerId: reader.GetFieldValue<int>(8)
         );

    private static CustomerEntity ReadCustomerEntity(NpgsqlDataReader reader) =>
        new(
            Id: reader.GetFieldValue<int>(0),
            Name: reader.GetFieldValue<string>(1),
            PhoneNumber: reader.GetFieldValue<string>(2),
            AddressId: reader.GetFieldValue<int>(3)
         );

    private static AddressEntity ReadAddressEntity(NpgsqlDataReader reader) =>
        new(
            Id: reader.GetFieldValue<int>(0),
            Region: reader.GetFieldValue<string>(1),
            City: reader.GetFieldValue<string>(2),
            Street: reader.GetFieldValue<string>(3),
            Building: reader.GetFieldValue<string>(4),
            Apartment: reader.GetFieldValue<string>(5),
            Latitude: reader.GetFieldValue<double>(6),
            Longitude: reader.GetFieldValue<double>(7)
        );

    private static RegionEntity ReadRegionEntity(NpgsqlDataReader reader) =>
        new(
            Name: reader.GetFieldValue<string>(0),
            Latitude: reader.GetFieldValue<double>(1),
            Longitude: reader.GetFieldValue<double>(2)
        );
}
