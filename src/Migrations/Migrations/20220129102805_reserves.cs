using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace core.Migrations
{
    public partial class reserves : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reserves",
                columns: table => new
                {
                    ReserveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    OwnerAccountSpendingAccountId = table.Column<int>(type: "int", nullable: false),
                    DestAccountSpendingAccountId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: DateTime.UtcNow),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reserves", x => x.ReserveId);
                    table.ForeignKey(
                        name: "FK_Reserves_SpendingAccounts_DestAccountSpendingAccountId",
                        column: x => x.DestAccountSpendingAccountId,
                        principalTable: "SpendingAccounts",
                        principalColumn: "SpendingAccountId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Reserves_SpendingAccounts_OwnerAccountSpendingAccountId",
                        column: x => x.OwnerAccountSpendingAccountId,
                        principalTable: "SpendingAccounts",
                        principalColumn: "SpendingAccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reserves_DestAccountSpendingAccountId",
                table: "Reserves",
                column: "DestAccountSpendingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserves_OwnerAccountSpendingAccountId",
                table: "Reserves",
                column: "OwnerAccountSpendingAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reserves");
        }
    }
}
