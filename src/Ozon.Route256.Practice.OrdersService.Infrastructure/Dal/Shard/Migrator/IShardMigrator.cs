namespace Ozon.Route256.Practice.OrdersService.Dal.Shard.Migrator;

public interface IShardMigrator
{
    Task Migrate(
        CancellationToken token);
}