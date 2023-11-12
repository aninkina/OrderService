using System.Text.Json.Serialization;
using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Ozon.Route256.Practice.GatewayService.Middlewaries;
using Ozon.Route256.Practice.GatewayService.Protos;
using Ozon.Route256.Practice.GatewayService.Services;
using Ozon.Route256.Practice.GatewayService.Services.Interfaces;

namespace Ozon.Route256.Practice.GatewayService;

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
        services.AddControllers().AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });


        services.AddSwaggerGen();
        services.AddEndpointsApiExplorer();

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerService, CustomerService>();


        var factory = new StaticResolverFactory(address => new[]
                   {
                //new BalancerAddress("localhost", 6081),
                //new BalancerAddress("localhost", 6082),
                new BalancerAddress("order-service-1", 5071),
                new BalancerAddress("order-service-2", 5072)
            });

        services.AddSingleton<ResolverFactory>(factory);

        services.AddGrpcClient<Orders.OrdersClient>(options =>
            {
                var url = _configuration.GetValue<string>("ROUTE256_OD_ADDRESS");
                if (string.IsNullOrEmpty(url))
                    throw new Exception("ROUTE256_OD_ADDRESS variable is empty");

                options.Address = new Uri(url);
        }).ConfigureChannel(x =>
            {
                x.Credentials = ChannelCredentials.Insecure;
                x.ServiceConfig = new ServiceConfig()
                {
                    LoadBalancingConfigs = { new LoadBalancingConfig("round_robin") }
                };
            });

        services.AddGrpcClient<Customers.CustomersClient>(options =>
        {
            var url = _configuration.GetValue<string>("ROUTE256_CS_ADDRESS");
            if (string.IsNullOrEmpty(url))
                throw new Exception("ROUTE256_CS_ADDRESS variable is empty");

            options.Address = new Uri(url);
        });

        //services.AddHostedService<SdConsumerHostedService>();
        services.AddGrpcReflection();
        services.AddGrpcSwagger();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseMiddleware<ErrorMiddleware>();

        app.UseEndpoints(x =>
        {
            x.MapGrpcReflectionService();
            x.MapControllers();
        });
    }
}
