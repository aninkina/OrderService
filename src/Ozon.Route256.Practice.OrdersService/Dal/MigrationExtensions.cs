using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Ozon.Route256.Practice.OrdersService.Dal.Common;

namespace Ozon.Route256.Practice.OrdersService.Dal;

public static class MigrationExtensions
{
    public static IApplicationBuilder MigrateUp(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();

        return app;
    }

    public static IServiceCollection AddMigrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(cfg => cfg
                          .AddPostgres()
                          .ScanIn(typeof(MigrationExtensions).Assembly)
                          .For.Migrations())
                      .AddOptions<ProcessorOptions>()
                      .Configure(
            options =>
            {
                var connectionString = configuration.GetConnectionString("OrderDb");
                options.ConnectionString = connectionString;
                options.Timeout = TimeSpan.FromSeconds(30);
            });

        //PostgresMapping.MapCompositeTypes();

        return services;
    }
}
