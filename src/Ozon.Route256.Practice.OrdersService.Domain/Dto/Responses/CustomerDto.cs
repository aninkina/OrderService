namespace Ozon.Route256.Practice.OrdersService.Dto.Responses;

public record struct CustomerDto(
    long Id,
    string Name,
    string Surname,
    string PhoneNumber,
    AddressDto Address
);
