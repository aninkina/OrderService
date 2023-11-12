namespace Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;

public record struct CustomerDto(
    long Id,
    string Name,
    string Surname,
    string PhoneNumber,
    AddressDto Address
);
