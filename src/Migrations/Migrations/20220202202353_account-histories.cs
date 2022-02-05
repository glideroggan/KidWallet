using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace core.Migrations
{
    public partial class accounthistories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            // migrationBuilder.CreateTable(
            //     name: "AccountHistories",
            //     columns: table => new
            //     {
            //         TransactionDataId = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         UserId = table.Column<int>(type: "int", nullable: false),
            //         CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //         SourceAccountId = table.Column<int>(type: "int", nullable: false),
            //         SourceAccountType = table.Column<int>(type: "int", nullable: false),
            //         DestAccountId = table.Column<int>(type: "int", nullable: false),
            //         DestAccountType = table.Column<int>(type: "int", nullable: false),
            //         Funds = table.Column<int>(type: "int", nullable: false),
            //         Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_AccountHistories", x => x.TransactionDataId);
            //     });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountHistories");
        }
    }
}
