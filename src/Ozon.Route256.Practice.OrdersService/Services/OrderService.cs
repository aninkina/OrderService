using System.Runtime.CompilerServices;
using GeoCoordinatePortable;
using Ozon.Route256.Practice.OrdersService.Exceptions;
using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;
using Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Services.Interfaces;
using Ozon.Route256.Practice.OrdersService.Services.Models.Requests;
using Ozon.Route256.Practice.OrdersService.Services.Models.Responses;

namespace Ozon.Route256.Practice.OrdersService.Services;

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;
    private readonly IOrdersRepository _ordersRepository;
    private readonly Protos.LogisticsSimulatorService.LogisticsSimulatorServiceClient _logisticsClient;

    private static readonly HashSet<OrderState> ForbiddenToCancelStates = new()
    {
        OrderState.Cancelled,
        OrderState.Delivered
    };

    public OrderService(IOrdersRepository ordersRepository,
    Protos.LogisticsSimulatorService.LogisticsSimulatorServiceClient client,
    ILogger<OrderService> logger
    )
    {
        _ordersRepository = ordersRepository;
        _logisticsClient = client;
        _logger = logger;  
    }

    public async Task<CancelOrderDto> CancelOrder(long id, CancellationToken token)
    {
        var logisticResult = await _logisticsClient.OrderCancelAsync(
         request: new Protos.Order()
         {
             Id = id,
         },
         cancellationToken: token);

        if (logisticResult is null)
        {
            // Изменить обработку
            throw new NotFoundException($"LogisticsSimulatorService returned null");
        }

        if (!logisticResult.Success)
        {
            return new CancelOrderDto() { IsSucceed = false, Error = logisticResult.Error };
        }

        var order = await _ordersRepository.Find(id, token);

        if (order is null)
        {
            return new CancelOrderDto() { IsSucceed = false, Error = $"Order with Id:{id} not found" };
        }

        if (ForbiddenToCancelStates.Contains(order.Value.State))
        {
            return new CancelOrderDto()
            {
                IsSucceed = false,
                Error = $"Cannot cancel orderModel {id} in state {order.Value.State}"
            };
        }

        var cancelledOrder = order.Value with
        {
            State = OrderState.Cancelled,
            StartTime = DateTime.UtcNow
        };

        await _ordersRepository.UpdateState(cancelledOrder, token);

        return new CancelOrderDto() { IsSucceed = true, Error = "" };
    }

    public async IAsyncEnumerable<OrderDto> GetCustomerOrders(GetCustomerOrdersDto parameters, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var region in _ordersRepository.GetCustomerOrders(parameters, token).WithCancellation(token))
        {
            if (token.IsCancellationRequested)
                yield break;

            yield return region;
        }
    }

    public async IAsyncEnumerable<OrdersAggregationDto> GetOrdersAggregation(DateTime start, ICollection<string> regions,
        [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var region in _ordersRepository.GetOrdersAggregation(start, regions, token).WithCancellation(token))
        {
            if (token.IsCancellationRequested)
                yield break;

            yield return region;
        }
    }

    public async Task<OrderState> GetOrderState(long id, CancellationToken token)
    {
        var orderModel = await _ordersRepository.Find(id, token);

        if (orderModel is null)
        {
            throw new NotFoundException($"Order with Id:{id} not found");
        }

        return orderModel.Value.State;
    }

    public async IAsyncEnumerable<OrderDto> GetRegionOrders(GetRegionOrdersDto parameters, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var region in _ordersRepository.GetRegionOrders(parameters, token).WithCancellation(token))
        {
            if (token.IsCancellationRequested)
                yield break;

            yield return region;
        }
    }

    public async IAsyncEnumerable<string> GetRegions([EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var region in _ordersRepository.GetAllRegions(token).WithCancellation(token))
        {
            if (token.IsCancellationRequested)
                yield break;

            yield return region.Name;
        }
    }

    public async Task Insert(OrderDto order, CancellationToken token)
    {
        var existOrder = await _ordersRepository.Find(order.Id, token);

        if (existOrder is not null)
        {
            throw new NotFoundException($"Order with Id:{order.Id} already exists");
        }

        await _ordersRepository.Insert(order, token);
    }

    public async Task UpdateOrderState(EventOrderDto eventOrder, CancellationToken token)
    {
        var order = await _ordersRepository.Find(eventOrder.OrderId, token);

        if (order is null)
        {
            throw new NotFoundException($"Order with Id:{eventOrder.OrderId} not found");
        }

        _logger.LogInformation($"order with id={eventOrder.OrderId} found. new state = {eventOrder.OrderState}");

        var updatedOrder = order.Value with
        {
            State = eventOrder.OrderState,
            StartTime = eventOrder.ChangeAt
        };

        await _ordersRepository.UpdateState(updatedOrder, token);
    }

    public Task<bool> ValidateAddress(AddressDto address, CancellationToken token)
    {
        var regionStock = _ordersRepository.GetAllRegions(token).ToEnumerable().First(x => x.Name == address.Region);

        var stockLocation = new GeoCoordinate(regionStock.Latitude, regionStock.Longitude);
        var customerLocation = new GeoCoordinate(address.Latitude, address.Longitude);

        _logger.LogInformation($"stock = {stockLocation.Latitude}, {stockLocation.Longitude}");
        _logger.LogInformation($"customer = {customerLocation.Latitude}, {customerLocation.Longitude}");

        return Task.FromResult(stockLocation.GetDistanceTo(customerLocation) < 5000 * 1000);
    }
}
