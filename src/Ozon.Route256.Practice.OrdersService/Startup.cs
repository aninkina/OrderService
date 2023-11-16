using Ozon.Route256.Practice.OrdersService.GrpcServices;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;
using Ozon.Route256.Practice.OrdersService.GrpcServices.Interfaces;
using Ozon.Route256.Practice.OrdersService.Protos;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Redis;
using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;
using Ozon.Route256.Practice.OrdersService.Services.Models.Requests;
using Ozon.Route256.Practice.OrdersService.Repository.Impl;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Impl.Outdated;
using Ozon.Route256.Practice.OrdersService.Middlewares;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Dal;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Dal.Common;
using Ozon.Route256.Practice.OrdersService.Infrastructure.ClientBalancing;
using Ozon.Route256.Practice.OrdersService.Application.Services;
using Ozon.Route256.Practice.OrdersService.Application.Services.Interfaces;

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
        services.AddScoped<IOrdersRepositoryV2, ShardOrderRepository>();
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
        services.AddHostedService<SdConsumerHostedService>();

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddGrpcSwagger();

        var connectionString = _configuration.GetConnectionString("OrderDb");

        services.AddSingleton<IPostgresConnectionFactory>(_ => new PostgresConnectionFactory(connectionString!));

        services.AddMigrations(_configuration);
    }

    public async void Configure(IApplicationBuilder app)
    {
        app.MigrateUp();

        app.UseRouting();
        app.UseEndpoints(x =>
        {
            x.MapGrpcService<OrdersGrpcService>();
            x.MapGrpcReflectionService();
        });

        using var scope = app.ApplicationServices.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IOrdersRepository>();

        var req = new GetRegionOrdersDto
        {
            SortObject = SortObject.Region,
            OrderSource = OrderSource.WebSite,
            PaginationDto = new Services.Models.Common.PaginationDto(1, 1),
            Regions = new string[] { "Moscow" },
            SortType = SortType.Asc
        };
        var res = await runner.GetRegionOrders(req, CancellationToken.None).ToArrayAsync();
        var res2 = res;
    }
}
