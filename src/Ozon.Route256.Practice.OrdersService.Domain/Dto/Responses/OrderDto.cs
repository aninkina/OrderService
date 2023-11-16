using Ozon.Route256.Practice.OrdersService.Grpc;

namespace Ozon.Route256.Practice.OrdersService.Dto.Responses;

public record struct OrderDto(
    long Id,
    int Count,
    decimal Price,
    uint Weight,
    string Region,
    DateTime StartTime,
    OrderState State,
    OrderSource Source,
    CustomerDto Customer
);
