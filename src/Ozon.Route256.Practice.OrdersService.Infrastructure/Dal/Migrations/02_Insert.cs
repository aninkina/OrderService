using FluentMigrator;
using Ozon.Route256.Practice.OrdersService.Dal.Common;
using Ozon.Route256.Practice.OrdersService.Protos.OrdersProto;

namespace Ozon.Route256.Practice.OrdersService.Dal.Migrations;


[Migration(1, "Add regions")]
public class InsertMigration : Migration
{
    public override void Down()
    {
        Delete.FromTable("orders").AllRows();
        Delete.FromTable("customers").AllRows();
        Delete.FromTable("regions").AllRows();
        Delete.FromTable("addresses").AllRows();
    }

    public override void Up()
    {
        InsertRegion("Moscow", latitude1: 56.050941, longitude1: 37.362532);
        InsertRegion("StPetersburg", latitude1: 60.240494, longitude1: 30.333441);
        InsertRegion("Novosibirsk", latitude1: 55.035503, longitude1: 83.239251);


        Insert.IntoTable("addresses").Row(
            new
            {
                id = 777,
                region = "AddressRegion",
                city = "TYumen",
                street = "30 let",
                building = "14 ",
                apartment = "24",
                latitude = 54.65,
                longitude = 65.56
            });

        Insert.IntoTable("customers").Row(
            new
            {
                id = 20202,
                name = "First Second",
                phone_number = "MyNumber",
                address_id = 777
            });

        Insert.IntoTable("orders").Row(
            new
            {
                id = 10101,
                count = 1,
                price = 10.10,
                weight = 10,
                source = (int)OrderSource.WebSite,
                state = (int)OrderState.Created,
                start_time = DateTime.Now,
                region = "Moscow",
                customer_id = 20202
            });
    }

    private void InsertRegion(string name1, double latitude1, double longitude1)
    {
        Insert.IntoTable("regions").Row(
            new
            {
                name = name1,
                latitude = latitude1,
                longitude = longitude1
            });
    }
}
