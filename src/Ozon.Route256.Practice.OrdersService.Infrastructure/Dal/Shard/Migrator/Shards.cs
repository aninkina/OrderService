namespace Ozon.Route256.Practice.OrdersService.Dal.Shard.Migrator;

public class Shards
{
    public const string BucketPlaceholder = "__bucket__";

    public static string GetSchemaName(int bucketId) => $"bucket_{bucketId}";
}
