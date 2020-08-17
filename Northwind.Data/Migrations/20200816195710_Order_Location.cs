using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Northwind.Data.Migrations
{
    public partial class Order_Location : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Shippers_ShipVia",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Freight",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShipAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShipCity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShipCountry",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShipPostalCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShipRegion",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "ShipVia",
                table: "Orders",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ShippedDate",
                table: "Orders",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "RequiredDate",
                table: "Orders",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "OrderDate",
                table: "Orders",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Orders",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Orders",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Orders",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_LocationId",
                table: "Orders",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Locations_LocationId",
                table: "Orders",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Shippers_ShipVia",
                table: "Orders",
                column: "ShipVia",
                principalTable: "Shippers",
                principalColumn: "ShipperId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Locations_LocationId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Shippers_ShipVia",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_LocationId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "ShipVia",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ShippedDate",
                table: "Orders",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequiredDate",
                table: "Orders",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset));

            migrationBuilder.AlterColumn<DateTime>(
                name: "OrderDate",
                table: "Orders",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset));

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Freight",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShipAddress",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipCity",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipCountry",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipPostalCode",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipRegion",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Shippers_ShipVia",
                table: "Orders",
                column: "ShipVia",
                principalTable: "Shippers",
                principalColumn: "ShipperId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
