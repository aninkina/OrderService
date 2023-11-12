using Google.Api;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Ozon.Route256.Practice.GatewayService.Models.Responses;
using Ozon.Route256.Practice.GatewayService.Protos;
using Ozon.Route256.Practice.GatewayService.Services.Interfaces;

namespace Ozon.Route256.Practice.GatewayService.Services;

public class OrderService : IOrderService
{
    private readonly Orders.OrdersClient _client;

    private readonly ICustomerService _customerService;

    public OrderService(Orders.OrdersClient client, ICustomerService customerService)
    {
        _client = client;
        _customerService = customerService;
    }

    public async Task<CancelOrderResponse> CancelOrder(long id, CancellationToken token)
    {
        var result = await _client.CancelOrderAsync(
            new GetOrderByIdRequest
            {
                Id = id,
            },
            cancellationToken: token);

        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Order service returned null"));
        }

        return result;
    }

    public async Task<GetOrdersResponse> GetRegionOrders(GetRegionOrdersRequest request, CancellationToken token)
    {
        var result = await _client.GetRegionOrdersAsync(
            request,
            cancellationToken: token);

        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Order service returned null"));
        }

        return result;
    }

    public async Task<GetOrdersResponse> GetCustomerOrders(GetCustomerOrdersRequest request, CancellationToken token)
    {
        // проверка не дает посмотреть ручку

        //var exists = await _customerService.GetCustomer(
        //   id: request.Id,
        //   token: token);

        //if (exists == null)
        //{
        //    throw new RpcException(new Status(StatusCode.NotFound, "Custumer not found"));
        //}

        var result = await _client.GetCustomerOrdersAsync(
            request: request,
            cancellationToken: token);

        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Order service returned null"));
        }

        return result;
    }

    public async Task<OrderState> GetOrderState(long id, CancellationToken token)
    {
        var result = await _client.GetOrderStateAsync(
          new GetOrderByIdRequest
          {
              Id = id,
          },
          cancellationToken: token);

        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Order service returned null"));
        }

        return result.State;
    }

    public async Task<GetOrdersAggregationResponse> GetOrdersAggregation(GetOrdersAggregationRequest request, CancellationToken token)
    {
        var result = await _client.GetOrdersAggregationAsync(
          request: request,
          cancellationToken: token);

        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Order service returned null"));
        }

        return result;
    }

    public async Task<GetRegionsResponse> GetRegions(CancellationToken token)
    {
        var result = await _client.GetRegionsAsync(
          request: new Empty(),
          cancellationToken: token);

        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Order service returned null"));
        }

        return result;
    }
}
