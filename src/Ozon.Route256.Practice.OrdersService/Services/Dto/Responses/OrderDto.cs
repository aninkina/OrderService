using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;
using Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;

namespace Ozon.Route256.Practice.OrdersService.Services.Models.Responses;

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
