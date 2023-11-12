using Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Impl;
using Ozon.Route256.Practice.OrdersService.Services;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        //  services.AddSingleton<InMemoryStorage>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();

        return services;
    }
}
