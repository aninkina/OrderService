using Ozon.Route256.Practice.OrdersService.Dto.Common;

namespace Ozon.Route256.Practice.OrdersService.Dto.Requests;

public record struct GetCustomerOrdersDto(
    int Id,
    PaginationDto PaginationDto,
    DateTime StartTime
);
