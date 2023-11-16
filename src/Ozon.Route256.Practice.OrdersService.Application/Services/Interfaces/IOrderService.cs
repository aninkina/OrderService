using Ozon.Route256.Practice.OrdersService.Dto.Requests;
using Ozon.Route256.Practice.OrdersService.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Grpc;

namespace Ozon.Route256.Practice.OrdersService.Services.Interfaces;

public interface IOrderService
{
    Task<CancelOrderDto> CancelOrder(long id, CancellationToken token);

    Task<OrderState> GetOrderState(long id, CancellationToken token);

    Task UpdateOrderState(EventOrderDto eventOrder, CancellationToken token);

    Task<IReadOnlyList<OrderDto>> GetRegionOrders(GetRegionOrdersDto parameters, CancellationToken token);

    Task<IReadOnlyList<OrderDto>> GetCustomerOrders(GetCustomerOrdersDto parameters, CancellationToken token);

    Task<IReadOnlyList<string>> GetRegions(CancellationToken token);
        
//    IReadOnlyList<OrdersAggregationDto> GetOrdersAggregation(DateTime start, ICollection<string> regions, CancellationToken token);

    Task<bool> ValidateAddress(AddressDto address, CancellationToken token);

    Task Insert(OrderDto order, CancellationToken token);
}

