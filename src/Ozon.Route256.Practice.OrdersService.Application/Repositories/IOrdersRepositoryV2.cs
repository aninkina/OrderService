using Ozon.Route256.Practice.OrdersService.Dto.Requests;
using Ozon.Route256.Practice.OrdersService.Dto.Responses;

namespace Ozon.Route256.Practice.OrdersService.Application.Repositories;

public interface IOrdersRepositoryV2
{
    Task<OrderDto?> Find(long orderId, CancellationToken token);
    Task UpdateState(OrderDto order, CancellationToken token);
    Task Insert(OrderDto order, CancellationToken token);
    Task<IReadOnlyList<OrderDto>> GetCustomerOrders(GetCustomerOrdersDto request, CancellationToken token);
    Task<IReadOnlyList<OrderDto>> GetRegionOrders(GetRegionOrdersDto request, CancellationToken token);
    Task<IEnumerable<RegionDto>> GetAllRegions(CancellationToken token);
}
