using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TowerScaffolding.Migrations
{
    public partial class RowVersionInitialize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Task",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Project",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Customer",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "LeadingHand",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DayWork",
                rowVersion: true,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "LeadingHand");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DayWork");
        }
    }
}
