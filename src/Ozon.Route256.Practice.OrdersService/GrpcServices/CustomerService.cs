using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ozon.Route256.Practice.OrdersService.GrpcServices.Extensions;
using Ozon.Route256.Practice.OrdersService.GrpcServices.Interfaces;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Impl;
using Ozon.Route256.Practice.OrdersService.Protos;
using Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;

namespace Ozon.Route256.Practice.OrdersService.GrpcServices;

public class CustomerService : ICustomerService
{
    private readonly ILogger<CustomerService> _logger;
    private readonly Customers.CustomersClient _client;
    private readonly CustomerRedisRepository _customerRedisRepository;
    public CustomerService(ILogger<CustomerService> logger,
        Customers.CustomersClient client,
        CustomerRedisRepository customerRedisRepository)
    {
        _logger = logger;
        _client = client;
        _customerRedisRepository = customerRedisRepository;
    }

    public async Task<CustomerDto> GetCustomer(int id, CancellationToken token)
    {
        var cacheResult = await _customerRedisRepository.Find(id, token);

        if (cacheResult == null)
        {
            var result = await _client.GetCustomerAsync(
               new GetCustomerByIdRequest
               {
                   Id = id,
               },
               cancellationToken: token);

            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Customer service returns null"));
            }

            await _customerRedisRepository.Insert(result.ToDtoType(), token);
            _logger.LogInformation($"Set cache customer = {result.ToDtoType()}");

            return result.ToDtoType();
        }

        _logger.LogInformation($"Get cache customer = {cacheResult.Value}");

        return cacheResult.Value;
    }



}
