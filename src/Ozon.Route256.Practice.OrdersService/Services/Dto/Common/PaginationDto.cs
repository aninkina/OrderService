namespace Ozon.Route256.Practice.OrdersService.Services.Models.Common;

public record struct PaginationDto(
    int PageNumber,
    int PageSize
);
