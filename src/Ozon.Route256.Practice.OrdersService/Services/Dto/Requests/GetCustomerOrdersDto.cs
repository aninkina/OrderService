using Ozon.Route256.Practice.OrdersService.Services.Models.Common;

namespace Ozon.Route256.Practice.OrdersService.Services.Models.Requests;

public record struct GetCustomerOrdersDto(
    int Id,
    PaginationDto PaginationDto,
    DateTime StartTime
);
