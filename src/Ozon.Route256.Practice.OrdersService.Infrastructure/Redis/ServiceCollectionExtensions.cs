using Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Impl;
using Ozon.Route256.Practice.OrdersService.Services;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Redis;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedis(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("Redis_ConnectionString");

        collection
            .AddScoped<IRedisDatabaseFactory>(_ =>
                new RedisDatabaseFactory(connectionString!));

        collection.AddScoped<CustomerRedisRepository>();

        return collection;
    }

}
