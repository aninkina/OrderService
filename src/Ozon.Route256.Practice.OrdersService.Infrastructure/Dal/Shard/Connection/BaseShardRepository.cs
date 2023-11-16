using System.Data.Common;
using Npgsql;
using Ozon.Route256.Practice.OrdersService.Dal.Shard.Rules;

namespace Ozon.Route256.Practice.OrdersService.Dal.Shard.Connection;

public class BaseShardRepository
{

    protected readonly IShardConnectionFactory _connectionFactory;
    protected readonly IShardingRule<long> _longShardingRule;
    private readonly IShardingRule<string> _stringShardingRule;

    public BaseShardRepository(
        IShardConnectionFactory connectionFactory,
        IShardingRule<long> longShardingRule,
        IShardingRule<string> stringShardingRule)
    {
        _connectionFactory = connectionFactory;
        _longShardingRule = longShardingRule;
        _stringShardingRule = stringShardingRule;
    }

    protected DbConnection GetConnectionByShardKey(
        int shardKey)
    {
        var bucketId = _longShardingRule.GetBucketId(shardKey);
        var connection = _connectionFactory.GetConnectionByBucket((int)bucketId);
        return connection;
    }

    protected DbConnection GetConnectionBySearchKey(
        string searchKey)
    {
        var bucketId = _stringShardingRule.GetBucketId(searchKey);
        return _connectionFactory.GetConnectionByBucket((int)bucketId);
    }

    protected DbConnection GetConnectionByBucket(
        int bucketId,
        CancellationToken token)
    {
        return _connectionFactory.GetConnectionByBucket(bucketId);
    }

}
