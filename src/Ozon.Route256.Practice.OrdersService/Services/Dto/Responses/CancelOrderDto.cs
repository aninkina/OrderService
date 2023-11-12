namespace Ozon.Route256.Practice.OrdersService.Services.Models.Responses;

public record struct CancelOrderDto(
    bool IsSucceed,
    string Error
);
