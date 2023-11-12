using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;

namespace Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;

public record struct EventOrderDto(
    long OrderId,
    OrderState OrderState,
    DateTime ChangeAt
);
