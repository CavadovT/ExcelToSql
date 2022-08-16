using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExcelToSql.Data.Migrations
{
    public partial class InitialProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Examples",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Segment = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    Product = table.Column<string>(nullable: true),
                    DiscountBrand = table.Column<string>(nullable: true),
                    UnitsSold = table.Column<double>(nullable: false),
                    Manifactur = table.Column<double>(nullable: false),
                    SalePrice = table.Column<double>(nullable: false),
                    GrossSales = table.Column<double>(nullable: false),
                    Discounts = table.Column<double>(nullable: false),
                    Sales = table.Column<double>(nullable: false),
                    COGS = table.Column<double>(nullable: false),
                    Profit = table.Column<double>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examples", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Examples");
        }
    }
}
