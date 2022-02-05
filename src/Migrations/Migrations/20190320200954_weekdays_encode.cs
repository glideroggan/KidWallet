using Microsoft.EntityFrameworkCore.Migrations;

namespace core.Migrations
{
    public partial class weekdays_encode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: "tasks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Day",
                table: "tasks",
                nullable: false,
                defaultValue: false);
        }
    }
}
