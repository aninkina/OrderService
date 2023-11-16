using Google.Protobuf.WellKnownTypes;

namespace Ozon.Route256.Practice.OrdersService.GrpcServices.Extensions;

public static class MappingExtensions
{

    public static Protos.OrdersProto.Order ToProtoType(this OrderDto model) =>
        new()
        {
            Id = model.Id,
            Count = model.Count,
            Price = (long)model.Price,
            Weight = model.Weight,
            OrderSource = model.Source,
            StartTime = Timestamp.FromDateTime(DateTime.SpecifyKind(model.StartTime, DateTimeKind.Utc)),
            Region = model.Region,
            State = model.State,
            CustomerId = model.Customer.Id
        };

    public static GetOrdersResponse ToProtoType(this IEnumerable<OrderDto> orders)
    {
        var protoOrders = new GetOrdersResponse();
        protoOrders.Orders.Add(orders.Select(ToProtoType));
        return protoOrders;
    }

    public static OrdersAggregation ToProtoType(this OrdersAggregationDto model) =>
        new()
        {
            Region = model.Region,
            OrderCount = model.OrderCount,
            Price = (long)model.Price,
            Weight = model.Weight,
            CustomerCount = model.CustomerCount,
        };

    public static CancelOrderResponse ToProtoType(this CancelOrderDto model) =>
        new()
        {
            Success = model.IsSucceed,
            Error = model.Error
        };


    public static PaginationDto ToDtoType(this Pagination pagination) =>
        new()
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };

    public static GetRegionOrdersDto ToDtoType(this GetRegionOrdersRequest protoRequest) =>
        new()
        {
            Regions = new List<string>(protoRequest.Regions),
            PaginationDto = ToDtoType(protoRequest.Pagination),
            OrderSource = protoRequest.OrderSource,
            SortType = protoRequest.SortType,
            SortObject = protoRequest.SortObject
        };

    public static GetCustomerOrdersDto ToDtoType(this GetCustomerOrdersRequest protoRequest) =>
    new()
    {
        Id = protoRequest.Id,
        PaginationDto = ToDtoType(protoRequest.Pagination),
        StartTime = protoRequest.StartTime.ToDateTime(),
    };
    public static AddressDto ToDtoType(this Address customer) =>
    new()
    {
        Apartment = customer.Apartment,
        Building = customer.Building,
        City = customer.City,
        Latitude = customer.Latitude,
        Longitude = customer.Longitude,
        Region = customer.Region,
        Street = customer.Street,
    };

    public static CustomerDto ToDtoType(this Customer customer) =>
    new()
    {
        Id = customer.Id,
        Name = customer.FirstName,
        Surname = customer.LastName,
        PhoneNumber = customer.MobileNumber,
        Address = customer.DefaultAddress.ToDtoType(),
    };
}
