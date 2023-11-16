using Confluent.Kafka;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers.Impl;

public class PreOrderConsumerConfig
{
    public string Topic { get; init; } = null!;
    public ConsumerConfig Config { get; init; } = new();
}
