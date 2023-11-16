using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Entity;

public record struct OrderEntity(
    long Id,
    int Count,
    decimal Price,
    uint Weight,
    string Region,
    DateTime StartTime,
    OrderState State,
    OrderSource Source,
    int CustomerId
);
