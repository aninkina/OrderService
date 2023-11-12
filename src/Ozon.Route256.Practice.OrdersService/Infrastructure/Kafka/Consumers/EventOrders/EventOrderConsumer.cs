using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;
using Ozon.Route256.Practice.OrdersService.Services;
using Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Services.Interfaces;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

public class EventOrderConsumer : BackgroundService
{
    private readonly ILogger<EventOrderConsumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConsumerProvider _consumerProvider;
    private readonly IOptions<EventOrderConsumerConfig> _config;

    public EventOrderConsumer(
        ILogger<EventOrderConsumer> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConsumerProvider consumerProvider,
        IOptions<EventOrderConsumerConfig> config)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _consumerProvider = consumerProvider;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested is false)
        {
            using var consumer = _consumerProvider
                .Create(_config.Value.Config);

            try
            {
                consumer.Subscribe(_config.Value.Topic);
                await Task.Run(() => ConsumeCycle(consumer, stoppingToken), stoppingToken);
                //await ConsumeCycle(
                //    consumer,
                //    stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteAsync !!! Consumer error");

                try
                {
                    consumer.Unsubscribe();
                }
                catch
                {
                    // ignored
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(1),
                    stoppingToken);
            }
        }
    }

    private async Task ConsumeCycle(
        IConsumer<string, string> consumer,
        CancellationToken ct)
    {
        while (ct.IsCancellationRequested is false)
        {
            var consumeResult = consumer.Consume(ct);
            await Handle(consumeResult, ct);
            consumer.Commit();
        }
    }

    private async Task Handle(
        ConsumeResult<string, string> consumeResult,
        CancellationToken ct)
    {
        _logger.LogInformation("Event Start desirialize ");

        var kafkaOrderEvent = JsonSerializer.Deserialize<KafkaEventOrder>(
            consumeResult.Message.Value,
            KafkaJsonSerializerOptions.Default);

        if (kafkaOrderEvent is null)
        {
            return;
        }

        _logger.LogInformation($"EVENT ORDER: {kafkaOrderEvent}");

        using var scope = _serviceScopeFactory.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
        var orderEvent = kafkaOrderEvent.ToDomain();
        await orderService
            .UpdateOrderState(
                orderEvent,
                ct);
    }

    private record KafkaEventOrder(
        long OrderId,
        int OrderState,
        DateTime ChangeAt
        )
    {
        public EventOrderDto ToDomain()
        {
            return new EventOrderDto(
              OrderId,
              (OrderState)OrderState,
              ChangeAt
                );
        }
    }

 
}
