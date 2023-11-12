using Ozon.Route256.Practice.GatewayService.Protos;

namespace Ozon.Route256.Practice.GatewayService.Services.Interfaces;

public interface ICustomerService
{
    Task<GetCustomersResponse> GetCustomers(CancellationToken token);

    Task<Customer> GetCustomer(int id, CancellationToken token);
}
