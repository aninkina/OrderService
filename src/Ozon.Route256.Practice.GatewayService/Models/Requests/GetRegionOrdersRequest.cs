namespace Ozon.Route256.Practice.GatewayService.Models.Requests;

public record struct GetRegionOrdersRequest(
    string[] Regions,
    Pagination Pagination,
    Protos.OrderSource OrderSource,
    Protos.SortObject SortObject,
    Protos.SortType SortType 
);
