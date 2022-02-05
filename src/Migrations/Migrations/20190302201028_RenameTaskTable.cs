using Microsoft.EntityFrameworkCore.Migrations;

namespace core.Migrations
{
    public partial class RenameTaskTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TaskEntity",
                newName: "TaskEntitys");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TaskEntitys",
                newName: "TaskEntity");
        }
    }
}
