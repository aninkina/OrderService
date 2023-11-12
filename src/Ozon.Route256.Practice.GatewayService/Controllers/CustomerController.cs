using Microsoft.AspNetCore.Mvc;
using Ozon.Route256.Practice.GatewayService.Models.Extensions;
using Ozon.Route256.Practice.GatewayService.Models.Responses;
using Ozon.Route256.Practice.GatewayService.Services.Interfaces;

namespace Ozon.Route256.Practice.GatewayService.Controlllers;

[ApiController]
[Route("v1/api/customer-service")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService service)
    {
        _customerService = service;
    }

    [HttpGet("customers")]
    public async Task<ICollection<CustomerResponse>> GetCustomers(CancellationToken cancellationToken)
    {
        var result = await _customerService.GetCustomers(cancellationToken);

        return result.ToRestType();
    }

    [HttpGet("customer/{id:int:min(0)}")]
    public async Task<CustomerResponse> GetCustomer([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _customerService.GetCustomer(id, cancellationToken);

        return result.ToRestType();
    }
}

