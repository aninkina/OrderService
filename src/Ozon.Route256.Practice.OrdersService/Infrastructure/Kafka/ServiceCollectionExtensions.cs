using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers.Impl;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers.NewOrders;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection collection)
    {
        collection
            .AddOptions<PreOrderConsumerConfig>()
            .Configure<IConfiguration>(
                (opt, config) =>
                    config
                        .GetSection("Kafka:Consumers:PreOrder")
                        .Bind(opt));


        collection
            .AddOptions<KafkaProducerConfig>()
            .Configure<IConfiguration>(
                (opt, config) =>
                {
                    config
                        .GetSection("Kafka:Producers:NewOrder")
                        .Bind(opt);
                });

        collection
            .AddOptions<EventOrderConsumerConfig>()
            .Configure<IConfiguration>(
                (opt, config) =>
                    config
                        .GetSection("Kafka:Consumers:EventOrder")
                        .Bind(opt));

        collection
            .AddSingleton<IConsumerProvider, ConsumerProvider>()
            .AddHostedService<PreOrderConsumer>()
            .AddHostedService<EventOrderConsumer>();

        collection
            .AddSingleton<IProducerProvider, ProducerProvider>()
            .AddScoped<NewOrderProducer>();

        return collection;
    }
}
