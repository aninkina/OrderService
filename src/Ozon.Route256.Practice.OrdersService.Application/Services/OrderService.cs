using GeoCoordinatePortable;
using Microsoft.Extensions.Logging;
using Ozon.Route256.Practice.OrderService.Application.Services;
using Ozon.Route256.Practice.OrdersService.Application.Repositories;
using Ozon.Route256.Practice.OrdersService.Dto.Requests;
using Ozon.Route256.Practice.OrdersService.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Exceptions;
using Ozon.Route256.Practice.OrdersService.Grpc;
using Ozon.Route256.Practice.OrdersService.Services.Interfaces;

namespace Ozon.Route256.Practice.OrdersService.Services;

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;
    private readonly IOrdersRepositoryV2 _ordersRepository;
    private readonly ILogisticsSimulator _logisticsSimulator;

    private static readonly HashSet<OrderState> ForbiddenToCancelStates = new()
    {
        OrderState.Cancelled,
        OrderState.Delivered
    };

    public OrderService(IOrdersRepositoryV2 ordersRepository,
    ILogisticsSimulator logisticsSimulator,
    ILogger<OrderService> logger
    )
    {
        _ordersRepository = ordersRepository;
        _logisticsSimulator = logisticsSimulator;
        _logger = logger;  
    }

    public async Task<CancelOrderDto> CancelOrder(long id, CancellationToken token)
    {
        //var logisticResult = await _logisticsSimulator.OrderCancelAsync(
        // request: new Protos.Order()
        // {
        //     Id = id,
        // },
        // cancellationToken: token);

        var logisticResult = await _logisticsSimulator.CancelOrder(id);

        if (logisticResult is null)
        {
            // Изменить обработку
            throw new NotFoundException($"LogisticsSimulatorService returned null");
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

    public async Task<IReadOnlyList<OrderDto>> GetCustomerOrders(GetCustomerOrdersDto parameters, CancellationToken token)
    {
        return await _ordersRepository.GetCustomerOrders(parameters, token);
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

    public async Task<IReadOnlyList<OrderDto>> GetRegionOrders(GetRegionOrdersDto parameters, CancellationToken token)
    {
        return await _ordersRepository.GetRegionOrders(parameters, token);
    }

    public async Task<IReadOnlyList<string>> GetRegions(CancellationToken token)
    {
        var res = await _ordersRepository.GetAllRegions(token);

        return res.Select(x=>x.Name).ToList();
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

    public async Task<bool> ValidateAddress(AddressDto address, CancellationToken token)
    {
        var regions = await _ordersRepository.GetAllRegions(token);

        var regionStock = regions.First(x => x.Name == address.Region);

        var stockLocation = new GeoCoordinate(regionStock.Latitude, regionStock.Longitude);
        var customerLocation = new GeoCoordinate(address.Latitude, address.Longitude);

        _logger.LogInformation($"stock = {stockLocation.Latitude}, {stockLocation.Longitude}");
        _logger.LogInformation($"customer = {customerLocation.Latitude}, {customerLocation.Longitude}");

        return stockLocation.GetDistanceTo(customerLocation) < 5000 * 1000;
    }
}
