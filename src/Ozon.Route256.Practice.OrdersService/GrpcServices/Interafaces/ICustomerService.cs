
using Ozon.Route256.Practice.OrdersService.Domain.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Protos;

namespace Ozon.Route256.Practice.OrdersService.GrpcServices.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto> GetCustomer(int id, CancellationToken token);
}
