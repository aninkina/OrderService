using System;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Ozon.Route256.Practice.GatewayService.Models.Requests;
using Ozon.Route256.Practice.GatewayService.Models.Responses;

namespace Ozon.Route256.Practice.GatewayService.Models.Extensions;

public static class MappingExtensions
{

    public static OrderResponse ToRestType(this Protos.Order response) =>
        new()
        {
            Id = response.Id,
            Count = response.Count,
            Price = response.Price,
            Weight = response.Weight,
            Source = response.OrderSource,
            StartTime = response.StartTime.ToDateTime(),
            Region = response.Region,
            State = response.State,
            CustomerId = response.CustomerId
        };

    public static ICollection<OrderResponse> ToRestType(this Protos.GetOrdersResponse orders)
    {
        return new List<OrderResponse>(orders.Orders.Select(ToRestType));
    }

    public static Address ToRestType(this Protos.Address address) =>
        new()
        {
            Region = address.Region,
            City = address.City,
            Street = address.Street,
            Building = address.Building,
            Apartment = address.Apartment,
            Latitude = address.Latitude,
            Longitude = address.Longitude
        };

    public static CustomerResponse ToRestType(this Protos.Customer customer) =>
        new()
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            MobileNumber = customer.MobileNumber,
            DefaultAddress = ToRestType(customer.DefaultAddress),
            Addresses = new List<Address>(customer.Addresses.Select(ToRestType))
        };

    public static ICollection<CustomerResponse> ToRestType(this Protos.GetCustomersResponse protoCustomers)
    {
        return new List<CustomerResponse>(protoCustomers.Customers.Select(ToRestType));
    }

    public static Protos.Pagination ToProtoType(this Pagination pagination) =>
        new()
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };


    public static Protos.GetCustomerOrdersRequest ToProtoType(this GetCustomerOrdersRequest request) =>
        new()
        {
            Id = request.CustomerId,
            Pagination = request.Pagination.ToProtoType(),
            StartTime = Timestamp.FromDateTime(request.StartTime)
        };

    public static Protos.GetRegionOrdersRequest ToProtoType(this GetRegionOrdersRequest request) 
    {
        var grpcRequest = new Protos.GetRegionOrdersRequest()
        {
            Pagination = request.Pagination.ToProtoType(),
            OrderSource = request.OrderSource,
            SortType = request.SortType,
            SortObject = request.SortObject
        };

        grpcRequest.Regions.Add(request.Regions);

        return grpcRequest;
    }

    public static Protos.GetOrdersAggregationRequest ToProtoType(this GetOrdersAggregationRequest request) 
    {
        var grpcRequest = new Protos.GetOrdersAggregationRequest()
        {
            StartTime = Timestamp.FromDateTime(request.StartTime)
        };

        grpcRequest.Regions.Add(request.Regions);

        return grpcRequest;
    }

    public static OrdersAggregationResponse ToRestType(this Protos.OrdersAggregation orderAggregation) =>
        new()
        {
            Region = orderAggregation.Region,
            OrderCount = orderAggregation.OrderCount,
            Price = orderAggregation.Price,
            Weight = orderAggregation.Weight,
            CustomerCount = orderAggregation.CustomerCount
        };

    public static ICollection<OrdersAggregationResponse> ToRestType(this Protos.GetOrdersAggregationResponse response)
    {
        return new List<OrdersAggregationResponse>(response.Orders.Select(ToRestType));
    }

    public static BaseApiResponse ToRestType(this Protos.CancelOrderResponse response) =>
        new(isSucceeded: response.Success, message: response.Error)
        {};


    public static ICollection<string> ToRestType(this Protos.GetRegionsResponse response) 
    {
        return response.Regions;
    }
}
