using Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Entity;
using Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Services.Models.Responses;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Extensions;

public static class MappingExtensions
{

    public static AddressDto ToDtoType(this AddressEntity entity) =>
        new()
        {
            Apartment = entity.Apartment,
            Building = entity.Building,
            City = entity.City,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Region = entity.Region,
            Street = entity.Street,
        };

    public static AddressEntity ToEntityType(this AddressDto dto) =>
    new()
    {
        Apartment = dto.Apartment,
        Building = dto.Building,
        City = dto.City,
        Latitude = dto.Latitude,
        Longitude = dto.Longitude,
        Region = dto.Region,
        Street = dto.Street,
    };

    public static OrderDto ToDtoType(this OrderEntity order, CustomerEntity customer, AddressEntity address) =>
    new()
    {
        Id = order.Id,
        Count = order.Count,
        Price = order.Price,
        Weight = order.Weight,
        Source = order.Source,
        StartTime = order.StartTime,
        Region = order.Region,
        State = order.State,
        Customer = new CustomerDto(
            Id: customer.Id,
            Name: customer.Name,
            Surname: customer.Name,
            PhoneNumber: customer.PhoneNumber,
            Address: new AddressDto(
                Region: address.Region,
                City: address.City,
                Street: address.Street,
                Building: address.Building,
                Apartment: address.Apartment,
                Latitude: address.Latitude,
                Longitude: address.Longitude
                )
            )
    };

    public static OrderDto ToSimpleDtoType(this OrderEntity order) =>
    new()
    {
        Id = order.Id,
        Count = order.Count,
        Price = order.Price,
        Weight = order.Weight,
        Source = order.Source,
        StartTime = order.StartTime,
        Region = order.Region,
        State = order.State,
    };

    public static CustomerDto ToDtoType(this CustomerEntity customer) =>
    new()
    {
        Id = customer.Id,
        Name = customer.Name,
        Surname = customer.Name,
        PhoneNumber = customer.PhoneNumber,
    };

    public static OrderEntity ToEntityType(this OrderDto dto) =>
        new()
        {
            Id = dto.Id,
            Count = dto.Count,
            Price = dto.Price,
            Weight = dto.Weight,
            Source = dto.Source,
            StartTime = dto.StartTime,
            State = dto.State,
            CustomerId = (int)dto.Customer.Id,
            Region = dto.Region
        };

    public static RegionDto ToDtoType(this RegionEntity entity) =>
    new()
    {
        Name = entity.Name,
        Latitude = entity.Latitude,
        Longitude = entity.Longitude
    };
}
