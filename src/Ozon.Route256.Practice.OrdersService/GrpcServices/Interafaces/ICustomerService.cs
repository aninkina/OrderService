
using Ozon.Route256.Practice.OrdersService.Protos;
using Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;

namespace Ozon.Route256.Practice.OrdersService.GrpcServices.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto> GetCustomer(int id, CancellationToken token);
}
