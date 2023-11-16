using Ozon.Route256.Practice.OrdersService.Application.Services;
using Ozon.Route256.Practice.OrdersService.Domain.Dto.Requests;
using Ozon.Route256.Practice.OrdersService.Domain.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Exceptions;
using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Impl.Outdated;

public class OrdersRepository : IOrdersRepository
{
    private readonly InMemoryStorage _fakeStorage;
    private readonly ILogger<OrdersRepository> _logger;

    public OrdersRepository(InMemoryStorage fakeStorage, ILogger<OrdersRepository> logger)
    {
        _fakeStorage = fakeStorage;
        _logger = logger;
    }

    public Task<OrderDto?> Find(long orderId, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<OrderDto?>(token);

        return _fakeStorage.Orders.TryGetValue((int)orderId, out var order)
            ? Task.FromResult<OrderDto?>(order)
            : Task.FromResult<OrderDto?>(null);
    }

    public Task UpdateState(OrderDto order, CancellationToken token)
    {
        if (!_fakeStorage.Orders.ContainsKey((int)order.Id))
        {
            throw new RepositoryException($"Order id={order.Id} not found");
        }

        _fakeStorage.Orders[(int)order.Id] = order;

        return Task.CompletedTask.WaitAsync(token);
    }

    public IAsyncEnumerable<OrderDto> GetCustomerOrders(GetCustomerOrdersDto request, CancellationToken token)
    {
        return _fakeStorage.Orders.Values
            .Where(x => x.Customer.Id == request.Id && x.StartTime >= request.StartTime)
            .Skip((request.PaginationDto.PageNumber - 1) * request.PaginationDto.PageSize)
            .Take(request.PaginationDto.PageSize)
            .ToAsyncEnumerable();
    }

    public IAsyncEnumerable<OrderDto> GetRegionOrders(GetRegionOrdersDto request, CancellationToken token)
    {
        var orders = _fakeStorage.Orders.Values
            .Where(x => _fakeStorage.Regions.Select(x => x.Region).Contains(x.Region))
            .Where(x => x.Source == request.OrderSource)
            .Skip((request.PaginationDto.PageNumber - 1) * request.PaginationDto.PageSize)
            .OrderBy(x => x.Id)
            .Take(request.PaginationDto.PageSize);

        if (request.SortObject != SortObject.None)
        {
            var propertyInfo = typeof(OrderDto).GetProperty(request.SortObject.ToString());

            if (propertyInfo == null)
            {
                throw new RepositoryException($"SortObject = {request.SortObject} doesn't match to any fields");
            }

            var sortBySortObjectOrders = request.SortType == SortType.Asc
                ? orders.OrderBy(x => propertyInfo.GetValue(x, null))
                : orders.OrderByDescending(x => propertyInfo.GetValue(x, null));

            return sortBySortObjectOrders.ToAsyncEnumerable();

        }
        else
        {
            return orders.ToAsyncEnumerable();
        }
    }

    public IAsyncEnumerable<RegionDto> GetAllRegions(CancellationToken token)
    {
        _logger.LogInformation($"fakeStorage = {_fakeStorage.GetHashCode()}");
        return _fakeStorage.Regions.Select(x => new RegionDto(Name: x.Region, 0, 0)).ToAsyncEnumerable();
    }

    public IAsyncEnumerable<OrdersAggregationDto> GetOrdersAggregation(DateTime start, ICollection<string> regions, CancellationToken token)
    {
        return _fakeStorage.Orders.Values
               .Where(x => x.StartTime >= start)
               .Where(x => regions.Contains(x.Region))
               .GroupBy(x => x.Region)
               .Select(regionOrder => new OrdersAggregationDto()
               {
                   Region = regionOrder.Key,
                   CustomerCount = regionOrder.Select(x => x.Customer.Id).Distinct().Count(),
                   OrderCount = regionOrder.Count(),
                   Weight = (uint)regionOrder.Sum(x => x.Weight),
                   Price = regionOrder.Sum(x => x.Price),
               }).ToAsyncEnumerable();
    }

    public Task Insert(OrderDto order, CancellationToken token)
    {
        var addResult = _fakeStorage.Orders.TryAdd((int)order.Id, order);

        _logger.LogInformation($"INSERT RESULT = {addResult} ID = {(int)order.Id}  fakeStorage = {_fakeStorage.GetHashCode()}");

        return Task.CompletedTask.WaitAsync(token);
    }


}
