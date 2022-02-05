using Microsoft.EntityFrameworkCore.Migrations;

namespace core.Migrations
{
    public partial class columnsForTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "tasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImgUrl",
                table: "tasks",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Payout",
                table: "tasks",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "ImgUrl",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "Payout",
                table: "tasks");
        }
    }
}
