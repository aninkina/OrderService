namespace Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;

public record struct RegionStockDto(
    string Region,
    double Latitude,
    double Longitude
);
