using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ozon.Route256.Practice.OrdersService.GrpcServices.Extensions;
using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;
using Ozon.Route256.Practice.OrdersService.Services.Interfaces;

namespace Ozon.Route256.Practice.OrdersService.GrpcServices;

public sealed class OrdersGrpcService : Orders.OrdersBase
{

    private readonly IOrderService _orderService;

    public OrdersGrpcService(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task<CancelOrderResponse> CancelOrder(GetOrderByIdRequest request, ServerCallContext context)
    {
        var result = await _orderService.CancelOrder(request.Id, context.CancellationToken);

        return result.ToProtoType();
    }

    public override async Task<GetOrdersResponse> GetCustomerOrders(GetCustomerOrdersRequest request, ServerCallContext context)
    {
        var items = _orderService.GetCustomerOrders(request.ToDtoType(), context.CancellationToken);

        var result = new GetOrdersResponse();

        await foreach (var item in items)
        {
            result.Orders.Add(item.ToProtoType());
        }

        return result;
    }

    public override async Task<GetOrdersResponse> GetRegionOrders(GetRegionOrdersRequest request, ServerCallContext context)
    {
        var items =  _orderService.GetRegionOrders(request.ToDtoType(), context.CancellationToken);

        var result = new GetOrdersResponse();

        await foreach (var item in items)
        {
            result.Orders.Add(item.ToProtoType());
        }

        return result;
    }

    public override async Task<GetOrderStateResponse> GetOrderState(GetOrderByIdRequest request, ServerCallContext context)
    {
        var state = await _orderService.GetOrderState(request.Id, context.CancellationToken);

        return new GetOrderStateResponse { State = state };
    }

    public  override async Task<GetRegionsResponse> GetRegions(Empty empty, ServerCallContext context)
    {
        var items = _orderService.GetRegions(context.CancellationToken);

        var result = new GetRegionsResponse();

        await foreach (var item in items)
        {
            result.Regions.Add(item);
        }

        return result;
    }

    public override async Task<GetOrdersAggregationResponse> GetOrdersAggregation(GetOrdersAggregationRequest request, ServerCallContext context)
    {
        var items = _orderService.GetOrdersAggregation(request.StartTime.ToDateTime(), request.Regions, context.CancellationToken);

        var result = new GetOrdersAggregationResponse();

        await foreach (var item in items)
        {
            result.Orders.Add(item.ToProtoType());
        }

        return result;
    }
}
