using Npgsql;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Entity;

namespace Ozon.Route256.Practice.OrdersService.Dal.Common;

public class PostgresMapping
{
    [Obsolete]
    public static void MapCompositeTypes()
    {
        var mapper = NpgsqlConnection.GlobalTypeMapper;
        mapper.MapComposite<OrderEntity>("order");
        mapper.MapComposite<RegionEntity>("region");
        mapper.MapComposite<AddressEntity>("address");
    }
}
