namespace Ozon.Route256.Practice.GatewayService.Models.Responses;

public record struct Address(
    string Region,
    string City,
    string Street,
    string Building,
    string Apartment,
    double Latitude,
    double Longitude
);
