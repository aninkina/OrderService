namespace Ozon.Route256.Practice.OrdersService.Dal.Shard;

public class DbOptions
{
    public required string ClusterName { get; set; }
    public required string DatabaseName { get; set; }
    public required string User { get; set; }
    public required string Password { get; set; }
}
