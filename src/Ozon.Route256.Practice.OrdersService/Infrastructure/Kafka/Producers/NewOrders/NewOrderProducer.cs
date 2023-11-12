using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers.NewOrders;

public class NewOrderProducer
{
    private readonly ILogger<NewOrderProducer> _logger;
    private readonly IProducerProvider _producerProvider;
    private readonly IOptions<KafkaProducerConfig> _config;

    public NewOrderProducer(
        IProducerProvider producerProvider,
        IOptions<KafkaProducerConfig> config,
        ILogger<NewOrderProducer> logger
        )
    {
        _producerProvider = producerProvider;
        _config = config;
        _logger = logger;
    }

    public async Task Produce(
        long id,
        CancellationToken ct)
    {
        _logger.LogInformation($"produce new order with id={id}  kafkaTopic = {_config.Value.Topic}");
        var producer = _producerProvider.Get();
        var kafkaMessage = ToKafka(id);
        await producer.ProduceAsync(
            _config.Value.Topic,
            kafkaMessage,
            ct);
    }

    private static Message<string, string> ToKafka(long id)
    {
        var kafkaOrder = new KafkaNewOrder(id);

        var value = JsonSerializer.Serialize(
            kafkaOrder,
            KafkaJsonSerializerOptions.Default);

        return new Message<string, string>
        {
            Key = id.ToString(),
            Value = value
        };
    }

    private record KafkaNewOrder(long OrderId);
}
