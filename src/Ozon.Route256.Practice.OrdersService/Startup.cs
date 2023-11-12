using Ozon.Route256.Practice.OrdersService.Infrastructure;
using Ozon.Route256.Practice.OrdersService.GrpcServices;
using Ozon.Route256.Practice.OrdersService.Services.Interfaces;
using Ozon.Route256.Practice.OrdersService.Services;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Impl;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;
using Ozon.Route256.Practice.OrdersService.GrpcServices.Interfaces;
using Ozon.Route256.Practice.OrdersService.Protos;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Redis;

namespace Ozon.Route256.Practice.OrdersService;

public sealed class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IHostEnvironment _hostEnvironment;

    public Startup(
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        IHostEnvironment hostEnvironment)
    {
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
        _hostEnvironment = hostEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddGrpc(x => x.Interceptors.Add<LoggerInterceptor>());

        services.AddSingleton<InMemoryStorage>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerService, CustomerService>();

        services.AddRedis(_configuration);
        services.AddKafka();

        services.AddGrpcClient<Protos.LogisticsSimulatorService.LogisticsSimulatorServiceClient>(options =>
        {
            var url = _configuration.GetValue<string>("ROUTE256_LS_ADDRESS");
            if (string.IsNullOrEmpty(url))
                throw new Exception("ROUTE256_LS_ADDRESS variable is empty");

            options.Address = new Uri(url);
        });


        services.AddGrpcClient<Customers.CustomersClient>(options =>
        {
            var url = _configuration.GetValue<string>("ROUTE256_CS_ADDRESS");
            if (string.IsNullOrEmpty(url))
                throw new Exception("ROUTE256_CS_ADDRESS variable is empty");

            options.Address = new Uri(url);
        });

        services.AddGrpcReflection();
        
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddGrpcSwagger();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(x =>
        {
            x.MapGrpcService<OrdersGrpcService>();
            x.MapGrpcReflectionService();
        });
    }
}
