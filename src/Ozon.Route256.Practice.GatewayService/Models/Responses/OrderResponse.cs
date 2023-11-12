namespace Ozon.Route256.Practice.GatewayService.Models.Responses;

public record struct OrderResponse(
    long Id,
    long CustomerId,
    int Count,
    decimal Price,
    uint Weight,
    Protos.OrderSource Source,
    DateTime StartTime,
    string Region,
    Protos.OrderState State
);
