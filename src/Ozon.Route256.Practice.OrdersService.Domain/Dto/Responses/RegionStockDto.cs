namespace Ozon.Route256.Practice.OrdersService.Dto.Responses;

public record struct RegionStockDto(
    string Region,
    double Latitude,
    double Longitude
);
