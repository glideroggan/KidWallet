using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace core.Migrations
{
    public partial class messagechanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReserveId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Notifications",
                newName: "IdentifierId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdentifierId",
                table: "Notifications",
                newName: "TaskId");

            migrationBuilder.AddColumn<int>(
                name: "ReserveId",
                table: "Notifications",
                type: "int",
                nullable: true);
        }
    }
}
