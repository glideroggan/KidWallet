using Microsoft.EntityFrameworkCore.Migrations;

namespace core.Migrations
{
    public partial class bindTask2Kid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpecificUserId",
                table: "tasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecificUserId",
                table: "tasks");
        }
    }
}
