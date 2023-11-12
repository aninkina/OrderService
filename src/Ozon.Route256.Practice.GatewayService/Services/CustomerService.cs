using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ozon.Route256.Practice.GatewayService.Protos;
using Ozon.Route256.Practice.GatewayService.Services.Interfaces;

namespace Ozon.Route256.Practice.GatewayService.Services;

public class CustomerService : ICustomerService
{
    private readonly Customers.CustomersClient _client;

    public CustomerService(Customers.CustomersClient client)
    {
        _client = client;
    }

    public async Task<Customer> GetCustomer(int id, CancellationToken token)
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

        return result;
    }

    public async Task<GetCustomersResponse> GetCustomers(CancellationToken token)
    {
        var result = await _client.GetCustomersAsync(new Empty(), cancellationToken: token);

        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Customer service returns null"));
        }

        return result;
    }
}
