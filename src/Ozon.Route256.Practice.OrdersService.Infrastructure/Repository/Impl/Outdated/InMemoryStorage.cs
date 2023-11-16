using System.Collections.Concurrent;
using Ozon.Route256.Practice.OrdersService.Domain.Dto.Responses;
using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Repository.Impl.Outdated;

public class InMemoryStorage
{
    public readonly ConcurrentDictionary<int, OrderDto> Orders = new(2, 10);

    public readonly List<RegionStockDto> Regions = new()
    {
        new RegionStockDto() { Region = "Moscow", Latitude = 56.050941, Longitude = 37.362532 },
        new RegionStockDto() { Region = "StPetersburg", Latitude = 60.240494, Longitude = 30.333441 },
        new RegionStockDto() { Region = "Novosibirsk", Latitude = 55.035503,  Longitude = 83.239251 }
    };

    public InMemoryStorage()
    {
        FakeOrders();
    }

    private void FakeOrders()
    {
        var adress = new AddressDto()
        {
            Apartment = "def",
            Building = "def",
            City = "def",
            Latitude = 50,
            Longitude = 50,
            Region = "Moscow",
            Street = "def"
        };

        var customer = new CustomerDto()
        {
            Id = Faker.RandomNumber.Next(100, 400),
            Address = adress,
            Name = "defName",
            PhoneNumber = "def",
            Surname = "def",
        };

        var orders = Enumerable.Range(1, 100)
            .Select(order => new OrderDto()
            {
                Id = order,
                Customer = customer,
                Count = Faker.RandomNumber.Next(1, 4),
                Price = Faker.RandomNumber.Next(2000, 2500),
                Weight = (uint)Faker.RandomNumber.Next(100, 1000),
                Source = Faker.Enum.Random<OrderSource>(),
                StartTime = DateTime.UtcNow.AddDays(-Faker.RandomNumber.Next(0, 10)),
                Region = Regions[Faker.RandomNumber.Next(0, 2)].Region,
                State = Faker.Enum.Random<OrderState>(),
            });

        foreach (var c in orders)
            Orders[(int)c.Id] = c;
    }

}
