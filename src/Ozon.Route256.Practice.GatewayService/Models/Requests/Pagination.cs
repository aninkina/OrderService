namespace Ozon.Route256.Practice.GatewayService.Models.Requests;

public record struct Pagination(
    int PageNumber,
    int PageSize
);
