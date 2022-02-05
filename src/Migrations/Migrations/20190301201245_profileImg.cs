using Microsoft.EntityFrameworkCore.Migrations;

namespace core.Migrations
{
    public partial class profileImg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FriendPoints",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImg",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FriendPoints",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileImg",
                table: "Users");
        }
    }
}
