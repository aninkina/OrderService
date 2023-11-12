namespace Ozon.Route256.Practice.GatewayService.Models.Requests;

public record struct GetCustomerOrdersRequest(
    int CustomerId,
    Pagination Pagination,
    DateTime StartTime
);
