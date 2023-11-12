using StackExchange.Redis;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Redis;

public interface IRedisDatabaseFactory
{
    IDatabase GetDatabase();
    IServer GetServer();
}
