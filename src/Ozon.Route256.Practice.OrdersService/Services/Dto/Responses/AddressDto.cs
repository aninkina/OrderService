namespace Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;

public record struct AddressDto(
    string Region,
    string City,
    string Street,
    string Building,
    string Apartment,
    double Latitude,
    double Longitude);
