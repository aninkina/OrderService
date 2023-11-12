namespace Ozon.Route256.Practice.OrdersService.Services.Models.Responses;

public record struct OrdersAggregationDto(
    string Region,
    int OrderCount,
    int CustomerCount,
    decimal Price,
    uint Weight
);
