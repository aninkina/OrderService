namespace Ozon.Route256.Practice.OrdersService.Dal.Shard.Rules;

public interface IShardingRule<TShardKey>
{
    uint GetBucketId(
        TShardKey shardKey);
}
