namespace Ozon.Route256.Practice.OrdersService.Dto.Responses;

public record struct CancelOrderDto(
    bool IsSucceed,
    string Error
);
