# OrderService project



## Technology stack

Route 256 Middle C#-developer NET 7 .0, PostgreSQL, Dapper, Kafka, Docker, FluentMigrator, Redis.

2 **completed**  microservices (**OrdersService and GatewayService**) with interaction with 4 other microservices (LogisticSimulator, OrdersGenerator, CustomerService, Service discovery) 

## Microservice architecture
Ozon store simulator with the functionality to create orders, manage them, and move the order along the logistics chain.
![Untitled](https://github.com/aninkina/OrderService/assets/43490035/a30da57b-5a24-4cd4-9f5b-09175fb17ce6)

***

## OrdersGenerator

All orders are generated by the services of the Order Generator group.

Three services are absolutely identical, except that each one sequentially ordered its own “subtype”.



## Customer service

Stores customer data.

Provides grpc API for other CustomerService clients.

Interacts with the SD service to obtain data from sharding databases.



## Service discovery


Provides API for retrieving database addresses.

Simulates periodic change of addresses by sending “new” data to subscribers.



## LogisticSimulator

Consumes **new_orders** topic to receive new orders.

Adds new orders to its storage and stores them there until they reach final status.

After entering the final status, it stores them for some time - 1-2 minutes. The time required for **OrderService** to eventually receive the final order status.

Changes the logistics status of orders that are in storage.

Sends the order change status to the **orders_events** topic

## OrdersService

- Stores detailed order entity (Npgsql (without ORM) and Sharding + Dapper repository impliminations)
- Provides GRPC Api for GatewayService
- Consumes **pre_orders** Kafka and saves orders to its storage.
- Reads additional data from CustomerService (customer conatcts)
- Validates and produces **new_orders**  for logistics.
- Consumes **orders_events**  from logistics and updates status data in the storage.

### OrdersService.Grpc

Содержит .proto файлы внешних сервисов

### OrdersService.Domain

Не зависит от других проектов.
Не имеет внешних зависимостей.
Содержит DTO, а также связанные с ними Exceptions.

### OrdersService.Application

Зависит от OrderService.Domain.
Из внешних зависимостей имеет только DependencyInjection и Logging.

Содержит:
- интерфейсы всех четырех репозиториев приложения,
- интерфейс и реализацию сервиса IOrderService, содержащего бизнес-логику приложения,
- интерфейсы вспомогательных сервисов ICustomersService и ILogisticsSimulator (независимых от реализации),
- ServiceCollectionExtension, регистрирующий IOrderService в DI-контейнере приложения.

### OrdersService.Infrastructure

Зависит от OrderService.Application.

Внешние зависимости:
- Kafka
- Fluent Migrator
- Grpc AspNetCore
- MurmurHash
- Npgsql
- Redis

Содержит:
- реализацию всех четырех репозиториев (три через Postgres, один через Redis),
- proto-файлы все трех GRPC-клиентов (CustomersService, LogisticsSimulator, ServiceDiscovery),
- интерфейс вспомогательного сервиса IServiceDiscovery,
- реализацию вспомогательных сервисов ICustomersService, ILogisticsSimulator и IServiceDiscovery,
- интерфейсы и реализацию ClientBalancing,
- интерфейсы и реализацию DAL (включая миграции и шардирование),
- консьюмеров и продюсеров Kafka-сообщений,
- ServiceCollectionExtension, регистрирующий все инфраструктурные сервисы в DI-контейнере приложения.
  
## GatewayService

Implement an HTTP API gateway that is the single entry point for all clients. (OrdersService and CustomerService)

