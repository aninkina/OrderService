using Confluent.Kafka;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers;

public class KafkaProducerConfig
{
    public ProducerConfig Config { get; init; } = new();
    public string Topic { get; init; } = null!;
}
