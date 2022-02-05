using Microsoft.EntityFrameworkCore.Migrations;

namespace core.Migrations
{
    public partial class Changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountHistories_SavingAccounts_SavingsAccountEntityId",
                table: "AccountHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountHistories_SpendingAccounts_SpendingAccountId",
                table: "AccountHistories");

            migrationBuilder.DropIndex(
                name: "IX_AccountHistories_SavingsAccountEntityId",
                table: "AccountHistories");

            migrationBuilder.DropIndex(
                name: "IX_AccountHistories_SpendingAccountId",
                table: "AccountHistories");

            migrationBuilder.DropColumn(
                name: "SavingsAccountEntityId",
                table: "AccountHistories");

            migrationBuilder.DropColumn(
                name: "SpendingAccountId",
                table: "AccountHistories");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "AccountHistories",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "AccountHistories",
                newName: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AccountHistories",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "AccountHistories",
                newName: "Date");

            migrationBuilder.AddColumn<int>(
                name: "SavingsAccountEntityId",
                table: "AccountHistories",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpendingAccountId",
                table: "AccountHistories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountHistories_SavingsAccountEntityId",
                table: "AccountHistories",
                column: "SavingsAccountEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountHistories_SpendingAccountId",
                table: "AccountHistories",
                column: "SpendingAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountHistories_SavingAccounts_SavingsAccountEntityId",
                table: "AccountHistories",
                column: "SavingsAccountEntityId",
                principalTable: "SavingAccounts",
                principalColumn: "SavingsAccountEntityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountHistories_SpendingAccounts_SpendingAccountId",
                table: "AccountHistories",
                column: "SpendingAccountId",
                principalTable: "SpendingAccounts",
                principalColumn: "SpendingAccountId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
