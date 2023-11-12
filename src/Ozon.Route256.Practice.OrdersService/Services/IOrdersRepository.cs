using Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Services.Models.Requests;
using Ozon.Route256.Practice.OrdersService.Services.Models.Responses;

namespace Ozon.Route256.Practice.OrdersService.Services;

public interface IOrdersRepository
{
    Task<OrderDto?> Find(long orderId, CancellationToken token);
    Task UpdateState(OrderDto order, CancellationToken token);
    Task Insert(OrderDto order, CancellationToken token);
    IAsyncEnumerable<OrderDto> GetCustomerOrders(GetCustomerOrdersDto request,CancellationToken token);
    IAsyncEnumerable<OrderDto> GetRegionOrders(GetRegionOrdersDto request, CancellationToken token);
    IAsyncEnumerable<OrdersAggregationDto> GetOrdersAggregation(DateTime start, ICollection<string> regions, CancellationToken token);
    IAsyncEnumerable<RegionDto> GetAllRegions(CancellationToken token);
}
