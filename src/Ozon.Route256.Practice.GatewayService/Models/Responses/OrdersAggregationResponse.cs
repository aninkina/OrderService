namespace Ozon.Route256.Practice.GatewayService.Models.Responses;

public record struct OrdersAggregationResponse(
    string Region,
    int OrderCount,
    int CustomerCount,
    decimal Price,
    uint Weight
);
