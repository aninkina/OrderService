namespace Ozon.Route256.Practice.GatewayService.Models.Requests;

public record struct GetOrdersAggregationRequest(
    string[] Regions,
    DateTime StartTime
);
