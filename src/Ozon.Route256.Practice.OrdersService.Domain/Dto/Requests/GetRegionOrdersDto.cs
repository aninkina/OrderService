using Ozon.Route256.Practice.OrdersService.Dto.Common;
using Ozon.Route256.Practice.OrdersService.Grpc;

namespace Ozon.Route256.Practice.OrdersService.Dto.Requests;

public record struct GetRegionOrdersDto(
    ICollection<string> Regions,
    PaginationDto PaginationDto,
    OrderSource OrderSource,
    SortType SortType,
    SortObject SortObject
);
