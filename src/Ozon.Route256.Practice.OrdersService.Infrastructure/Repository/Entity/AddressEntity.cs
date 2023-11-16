namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Entity;

public record struct AddressEntity(
    int Id,
    string Region,
    string City,
    string Street,
    string Building,
    string Apartment,
    double Latitude,
    double Longitude);
