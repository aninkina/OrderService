using Microsoft.AspNetCore.Mvc;
using Ozon.Route256.Practice.GatewayService.Models.Extensions;
using Ozon.Route256.Practice.GatewayService.Models.Requests;
using Ozon.Route256.Practice.GatewayService.Models.Responses;
using Ozon.Route256.Practice.GatewayService.Services.Interfaces;

namespace Ozon.Route256.Practice.GatewayService.Controlllers;

[ApiController]
[Route("v1/api/orders-service")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService service)
    {
        _orderService = service;
    }

    [HttpGet("cancel/{id:int:min(0)}")]
    public async Task<BaseApiResponse> CancelOrder([FromRoute] long id, CancellationToken cancellationToken)
    {
        var result = await _orderService.CancelOrder(id, cancellationToken);

        return result.ToRestType();
    }

    [HttpGet("state/{id:int:min(0)}")]
    public async Task<Protos.OrderState> GetOrderState([FromRoute] long id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetOrderState(id, cancellationToken);

        return result;
    }

    [HttpPost("customer-orders")]
    public async Task<ICollection<OrderResponse>> GetCustomerOrders([FromBody] GetCustomerOrdersRequest request, CancellationToken cancellationToken)
    {
        var grpcRequest = request.ToProtoType();

        var result = await _orderService.GetCustomerOrders(grpcRequest, cancellationToken);

        return result.ToRestType();
    }

    [HttpPost("region-orders")]
    public async Task<ICollection<OrderResponse>> GetRegionOrders([FromBody] GetRegionOrdersRequest request, CancellationToken cancellationToken)
    {
        var grpcRequest = request.ToProtoType();

        var result = await _orderService.GetRegionOrders(grpcRequest, cancellationToken);

        return result.ToRestType();
    }

    [HttpGet("regions")]
    public async Task<ICollection<string>> GetRegions(CancellationToken cancellationToken)
    {
        var result = await _orderService.GetRegions(cancellationToken);

        return result.ToRestType();
    }

    [HttpPost("region-orders-aggregation")]
    public async Task<ICollection<OrdersAggregationResponse>> GetOrdersAggregation([FromBody] GetOrdersAggregationRequest request,CancellationToken cancellationToken)
    {
        var grpcRequest = request.ToProtoType();

        var result = await _orderService.GetOrdersAggregation(grpcRequest, cancellationToken);

        return result.ToRestType();
    }
}

