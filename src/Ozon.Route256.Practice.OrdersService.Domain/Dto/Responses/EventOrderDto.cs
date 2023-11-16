using Ozon.Route256.Practice.OrdersService.Grpc;

namespace Ozon.Route256.Practice.OrdersService.Dto.Responses;

public record struct EventOrderDto(
    long OrderId,
    OrderState OrderState,
    DateTime ChangeAt
);
