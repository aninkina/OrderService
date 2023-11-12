using Ozon.Route256.Practice.OrdersService.Exceptions;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Redis;
using Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;
using StackExchange.Redis;
using System.Text.Json;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Impl;

public class CustomerRedisRepository
{
    private readonly IDatabase _redisDatabase;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new();

    public CustomerRedisRepository(
        IRedisDatabaseFactory redisDatabaseFactory)
    {
        _redisDatabase = redisDatabaseFactory.GetDatabase();
    }

    private static string GetKey(long customerId) =>
        $"customer:{customerId}";

    public async Task<CustomerDto?> Find(long customerId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var value = await _redisDatabase
            .StringGetAsync(GetKey(customerId))
            .WaitAsync(token);

        return ToDomain(value);
    }

    public async Task Insert(CustomerDto customer, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var redisValue = ToRedisValue(customer);
        await _redisDatabase
            .StringSetAsync(
                GetKey(customer.Id),
                redisValue)
            .WaitAsync(token);
    }

    private RedisValue ToRedisValue(CustomerDto customer)
    {
        var redisCustomer = new RedisCustomer(
            customer.Id,
            customer.Name,
            customer.Surname,
            customer.PhoneNumber,
            new RedisAddress(
                customer.Address.Region,
                customer.Address.City,
                customer.Address.Street,
                customer.Address.Building,
                customer.Address.Apartment,
                customer.Address.Latitude,
                customer.Address.Longitude
                ));

        return JsonSerializer.Serialize(
            redisCustomer,
            _jsonSerializerOptions);
    }

    private CustomerDto? ToDomain(RedisValue redisValue)
    {
        if (string.IsNullOrWhiteSpace(redisValue))
        {
            return null;
        }

        var redisCustomer = JsonSerializer.Deserialize<RedisCustomer>(
            redisValue!,
            _jsonSerializerOptions);

        return redisCustomer?.ToDomain();
    }

    private record RedisCustomer(
          long Id,
          string Name,
          string Surname,
          string PhoneNumber,
          RedisAddress Address)
    {
        public CustomerDto ToDomain()
        {
            return new CustomerDto(
                Id,
                Name,
                Surname,
                PhoneNumber,
                new AddressDto(
                    Address.Region,
                    Address.City,
                    Address.Street,
                    Address.Building,
                    Address.Apartment,
                    Address.Latitude,
                    Address.Longitude)
                );
        }
    }

    private record RedisAddress(
           string Region,
           string City,
           string Street,
           string Building,
           string Apartment,
           double Latitude,
           double Longitude);
}
