using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;
using Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Services.Models.Requests;
using Ozon.Route256.Practice.OrdersService.Services.Models.Responses;

namespace Ozon.Route256.Practice.OrdersService.Services.Interfaces;

public interface IOrderService
{
    Task<CancelOrderDto> CancelOrder(long id, CancellationToken token);

    Task<OrderState> GetOrderState(long id, CancellationToken token);

    Task UpdateOrderState(EventOrderDto eventOrder, CancellationToken token);

    IAsyncEnumerable<OrderDto> GetRegionOrders(GetRegionOrdersDto parameters, CancellationToken token);

    IAsyncEnumerable<OrderDto> GetCustomerOrders(GetCustomerOrdersDto parameters, CancellationToken token);

    IAsyncEnumerable<string> GetRegions(CancellationToken token);

    IAsyncEnumerable<OrdersAggregationDto> GetOrdersAggregation(DateTime start, ICollection<string> regions, CancellationToken token);

    Task<bool> ValidateAddress(AddressDto address, CancellationToken token);

    Task Insert(OrderDto order, CancellationToken token);
}

