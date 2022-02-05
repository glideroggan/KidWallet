using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace core.Migrations
{
    public partial class SpecificUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SpecificUserId",
                table: "Tasks",
                column: "SpecificUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_SpecificUserId",
                table: "Tasks",
                column: "SpecificUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_SpecificUserId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_SpecificUserId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "SpecificUserId",
                table: "Tasks");
        }
    }
}
