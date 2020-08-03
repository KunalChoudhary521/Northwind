using Microsoft.EntityFrameworkCore.Migrations;

namespace Northwind.Data.Migrations
{
    public partial class Customer_Location : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Customers");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Customers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_LocationId",
                table: "Customers",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Locations_LocationId",
                table: "Customers",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Locations_LocationId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_LocationId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Customers",
                type: "text",
                nullable: true);
        }
    }
}
