using Microsoft.EntityFrameworkCore.Migrations;

namespace core.Migrations
{
    public partial class task_schema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DayInTheWeek",
                table: "tasks",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "EveryOtherWeek",
                table: "tasks",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayInTheWeek",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "EveryOtherWeek",
                table: "tasks");
        }
    }
}
