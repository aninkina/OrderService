using FluentMigrator;

namespace Ozon.Route256.Practice.OrdersService.Dal.Migrations;

[Migration(0, "Init")]
public class InitialMigration : Migration
{
    public override void Down()
    {
        Delete.Table("orders");
        Delete.Table("regions");
        Delete.Table("customers");
        Delete.Table("addresses");

    }

    public override void Up()
    {
        var res = Schema.Table("orders").Exists();
        if (res) { return; }
        Create.Table("orders")
                .WithColumn("id").AsInt64().NotNullable().PrimaryKey()
                .WithColumn("count").AsInt32().NotNullable()
                .WithColumn("price").AsDecimal().NotNullable()
                .WithColumn("weight").AsInt32().NotNullable()
                .WithColumn("source").AsInt32().NotNullable()
                .WithColumn("state").AsInt32().NotNullable()
                .WithColumn("start_time").AsDateTime().NotNullable()
                .WithColumn("region").AsString().NotNullable()
                .WithColumn("customer_id").AsInt32().NotNullable();

        Create.Table("regions")
            .WithColumn("name").AsString().NotNullable().PrimaryKey()
            .WithColumn("latitude").AsDouble().NotNullable()
            .WithColumn("longitude").AsDouble().NotNullable();

        Create.Table("addresses")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("region").AsString().NotNullable()
                .WithColumn("city").AsString().NotNullable()
                .WithColumn("street").AsString().NotNullable()
                .WithColumn("building").AsString().NotNullable()
                .WithColumn("apartment").AsString().NotNullable()
                .WithColumn("latitude").AsDouble().NotNullable()
                .WithColumn("longitude").AsDouble().NotNullable();

        Create.Table("customers")
            .WithColumn("id").AsInt32().NotNullable().PrimaryKey()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("phone_number").AsString().NotNullable()
            .WithColumn("address_id").AsInt32().NotNullable();

        Create.ForeignKey("FK_Order_Region")
            .FromTable("orders").ForeignColumn("region")
            .ToTable("regions").PrimaryColumn("name");

        Create.ForeignKey("FK_Order_Customer")
            .FromTable("orders").ForeignColumn("customer_id")
            .ToTable("customers").PrimaryColumn("id");

        Create.ForeignKey("FK_Customer_Address")
            .FromTable("customers").ForeignColumn("address_id")
            .ToTable("addresses").PrimaryColumn("id");
    }
}
