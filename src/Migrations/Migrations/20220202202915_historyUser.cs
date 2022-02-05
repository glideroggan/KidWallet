using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace core.Migrations
{
    public partial class historyUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AccountHistories_UserId",
                table: "AccountHistories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountHistories_Users_UserId",
                table: "AccountHistories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountHistories_Users_UserId",
                table: "AccountHistories");

            migrationBuilder.DropIndex(
                name: "IX_AccountHistories_UserId",
                table: "AccountHistories");
        }
    }
}
