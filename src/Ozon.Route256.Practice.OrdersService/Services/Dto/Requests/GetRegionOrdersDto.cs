using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;
using Ozon.Route256.Practice.OrdersService.Services.Models.Common;

namespace Ozon.Route256.Practice.OrdersService.Services.Models.Requests;

public record struct GetRegionOrdersDto(
    ICollection<string> Regions,
    PaginationDto PaginationDto,
    OrderSource OrderSource,
    SortType SortType,
    SortObject SortObject
);
