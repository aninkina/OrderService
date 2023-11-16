using Confluent.Kafka;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

public class EventOrderConsumerConfig
{
    public string Topic { get; init; } = null!;
    public ConsumerConfig Config { get; init; } = new();
}
