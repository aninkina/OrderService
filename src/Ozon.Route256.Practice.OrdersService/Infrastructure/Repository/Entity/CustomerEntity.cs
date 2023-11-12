namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Entity;

public record struct CustomerEntity(
    int Id,
    string Name,
    string PhoneNumber,
    int AddressId
    );

