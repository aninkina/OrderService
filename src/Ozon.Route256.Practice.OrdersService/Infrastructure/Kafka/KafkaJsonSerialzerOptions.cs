using System.Text.Json;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

public class KafkaJsonSerializerOptions
{
    public static JsonSerializerOptions Default => 
        new();
}
