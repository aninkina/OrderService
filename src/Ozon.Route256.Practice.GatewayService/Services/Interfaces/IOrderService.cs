using Ozon.Route256.Practice.GatewayService.Models.Responses;
using Ozon.Route256.Practice.GatewayService.Protos;

namespace Ozon.Route256.Practice.GatewayService.Services.Interfaces;

public interface IOrderService
{
    Task<CancelOrderResponse> CancelOrder(long id, CancellationToken token);

    Task<OrderState> GetOrderState(long id, CancellationToken token);

    Task<GetOrdersResponse> GetRegionOrders(GetRegionOrdersRequest request, CancellationToken token);

    Task<GetOrdersResponse> GetCustomerOrders(GetCustomerOrdersRequest request, CancellationToken token);

    Task<GetRegionsResponse> GetRegions(CancellationToken token);

    Task<GetOrdersAggregationResponse> GetOrdersAggregation(GetOrdersAggregationRequest request, CancellationToken token);
}

