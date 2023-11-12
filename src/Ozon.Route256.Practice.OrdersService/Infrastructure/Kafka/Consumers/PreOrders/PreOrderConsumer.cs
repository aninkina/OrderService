using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Ozon.Route256.Practice.OrdersService.GrpcServices.Interfaces;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers.NewOrders;
using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;
using Ozon.Route256.Practice.OrdersService.Services;
using Ozon.Route256.Practice.OrdersService.Services.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Services.Interfaces;
using Ozon.Route256.Practice.OrdersService.Services.Models.Responses;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers.Impl;

public class PreOrderConsumer : BackgroundService
{
    private readonly ILogger<PreOrderConsumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConsumerProvider _consumerProvider;
    private readonly IOptions<PreOrderConsumerConfig> _config;

    public PreOrderConsumer(
        ILogger<PreOrderConsumer> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConsumerProvider consumerProvider,
        IOptions<PreOrderConsumerConfig> config)
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
                _logger.LogError(ex, "Consumer error preorder");

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
        _logger.LogInformation("Start desirialize ");

        var kafkaOrder = JsonSerializer.Deserialize<KafkaPreOrder>(
            consumeResult.Message.Value,
            KafkaJsonSerializerOptions.Default);

        if (kafkaOrder is null)
        {
            return;
        }

        _logger.LogInformation($"PRE ORDER: {kafkaOrder}");

        using var scope = _serviceScopeFactory.CreateScope();

        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();
        var customer = await customerService.GetCustomer((int)kafkaOrder.Customer.Id, ct);

        _logger.LogInformation($"CustumerName = {customer.Name}, {customer.Surname}, {customer.PhoneNumber}");

        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
        var order = kafkaOrder.ToDomain(customer);
        await orderService
            .Insert(
                order,
                ct);

        var validationDistanceResult = await orderService.ValidateAddress(order.Customer.Address, ct);
        if (validationDistanceResult)
        {
            var newOrderProducer = scope.ServiceProvider.GetRequiredService<NewOrderProducer>();
            await newOrderProducer.Produce(order.Id, ct);
        }
        _logger.LogInformation($"valid ={validationDistanceResult},address = {order.Customer.Address}");

    }



    private record KafkaPreOrder(
        long Id,
        OrderSource Source,
        KafkaCustomer Customer,
        KafkaGood[] Goods
        )
    {
        public OrderDto ToDomain(CustomerDto customer)
        {
            return new OrderDto(
                Id: Id,
                Count: Goods.Select(x => x.Quantity).Sum(),
                Price: Goods.Select(x => x.Price).Sum(),
                Weight: 100,
                Region: customer.Address.Region,
                StartTime: DateTime.Now,
                State: OrderState.Created,
                Source: Source,
                Customer: new CustomerDto(
                    Id: Customer.Id,
                    Name: customer.Name,
                    Surname: customer.Surname,
                    PhoneNumber: customer.PhoneNumber,
                    Address: new AddressDto()
                    {
                        Region = Customer.Address.Region,
                        City = Customer.Address.City,
                        Longitude = Customer.Address.Longitude,
                        Latitude = Customer.Address.Latitude,
                        Apartment = Customer.Address.Apartment,
                        Building = Customer.Address.Building,
                        Street = Customer.Address.Street
                    }
                    )
                );
        }
    }

    private record KafkaCustomer(
        long Id,
        KafkaAddress Address
        );


    private record KafkaAddress(
        string Region,
        string City,
        string Street,
        string Building,
        string Apartment,
        double Latitude,
        double Longitude
        );


    private record KafkaGood(
        long Id,
        string Name,
        int Quantity,
        decimal Price,
        uint Weight
        );
}
