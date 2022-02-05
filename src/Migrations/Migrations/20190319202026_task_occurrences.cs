using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace core.Migrations
{
    public partial class task_occurrences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Day",
                table: "tasks",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NotBefore",
                table: "tasks",
                nullable: false,
                type: "datetime2",
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<bool>(
                name: "Week",
                table: "tasks",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "NotBefore",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "Week",
                table: "tasks");
        }
    }
}
