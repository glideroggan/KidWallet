using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace core.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavingAccounts",
                columns: table => new
                {
                    SavingsAccountEntityId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    DepositFunds = table.Column<int>(nullable: false),
                    CalculatedFunds = table.Column<int>(nullable: false),
                    ReleaseDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingAccounts", x => x.SavingsAccountEntityId);
                });

            migrationBuilder.CreateTable(
                name: "SpendingAccounts",
                columns: table => new
                {
                    SpendingAccountId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Balance = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpendingAccounts", x => x.SpendingAccountId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SpendingAccountId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Role = table.Column<int>(nullable: false),
                    ParentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => new { x.UserId, x.SpendingAccountId });
                    table.UniqueConstraint("AK_Users_UserId", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "AccountHistories",
                columns: table => new
                {
                    TransactionDataId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(nullable: false),
                    SavingsAccountEntityId = table.Column<int>(nullable: true),
                    SpendingAccountId = table.Column<int>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    SourceAccountId = table.Column<int>(nullable: false),
                    SourceAccountType = table.Column<int>(nullable: false),
                    DestAccountId = table.Column<int>(nullable: false),
                    DestAccountType = table.Column<int>(nullable: false),
                    Funds = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountHistories", x => x.TransactionDataId);
                    table.ForeignKey(
                        name: "FK_AccountHistories_SavingAccounts_SavingsAccountEntityId",
                        column: x => x.SavingsAccountEntityId,
                        principalTable: "SavingAccounts",
                        principalColumn: "SavingsAccountEntityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountHistories_SpendingAccounts_SpendingAccountId",
                        column: x => x.SpendingAccountId,
                        principalTable: "SpendingAccounts",
                        principalColumn: "SpendingAccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountHistories_SavingsAccountEntityId",
                table: "AccountHistories",
                column: "SavingsAccountEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountHistories_SpendingAccountId",
                table: "AccountHistories",
                column: "SpendingAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountHistories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "SavingAccounts");

            migrationBuilder.DropTable(
                name: "SpendingAccounts");
        }
    }
}
