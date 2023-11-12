namespace Ozon.Route256.Practice.GatewayService.Models.Responses;

public record struct CustomerResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string MobileNumber,
    Address DefaultAddress,
    ICollection<Address> Addresses
);
