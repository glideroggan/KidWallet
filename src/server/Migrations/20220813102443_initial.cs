using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoneTasks",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoneTasks", x => new { x.TaskId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "SpendingAccounts",
                columns: table => new
                {
                    SpendingAccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Balance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpendingAccounts", x => x.SpendingAccountId);
                });

            migrationBuilder.CreateTable(
                name: "Reserves",
                columns: table => new
                {
                    ReserveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    OwnerAccountSpendingAccountId = table.Column<int>(type: "int", nullable: false),
                    DestAccountSpendingAccountId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileImg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    SpendingAccountId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_SpendingAccounts_SpendingAccountId",
                        column: x => x.SpendingAccountId,
                        principalTable: "SpendingAccounts",
                        principalColumn: "SpendingAccountId");
                });

            migrationBuilder.CreateTable(
                name: "AccountHistories",
                columns: table => new
                {
                    TransactionDataId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceAccountId = table.Column<int>(type: "int", nullable: false),
                    SourceAccountType = table.Column<int>(type: "int", nullable: false),
                    DestAccountId = table.Column<int>(type: "int", nullable: false),
                    DestAccountType = table.Column<int>(type: "int", nullable: false),
                    Funds = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountHistories", x => x.TransactionDataId);
                    table.ForeignKey(
                        name: "FK_AccountHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MessageType = table.Column<int>(type: "int", nullable: false),
                    IdentifierId = table.Column<int>(type: "int", nullable: true),
                    Target = table.Column<int>(type: "int", nullable: false),
                    TargetName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SenderUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "SavingAccounts",
                columns: table => new
                {
                    SavingsAccountEntityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DepositFunds = table.Column<int>(type: "int", nullable: false),
                    CalculatedFunds = table.Column<int>(type: "int", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingAccounts", x => x.SavingsAccountEntityId);
                    table.ForeignKey(
                        name: "FK_SavingAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    SpecificUserId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImgUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payout = table.Column<int>(type: "int", nullable: false),
                    NotBefore = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayInTheWeek = table.Column<long>(type: "bigint", nullable: false),
                    Week = table.Column<bool>(type: "bit", nullable: false),
                    EveryOtherWeek = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Once = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.TaskId);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_SpecificUserId",
                        column: x => x.SpecificUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Tasks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountHistories_UserId",
                table: "AccountHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderUserId",
                table: "Notifications",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserves_DestAccountSpendingAccountId",
                table: "Reserves",
                column: "DestAccountSpendingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserves_OwnerAccountSpendingAccountId",
                table: "Reserves",
                column: "OwnerAccountSpendingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingAccounts_UserId",
                table: "SavingAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SpecificUserId",
                table: "Tasks",
                column: "SpecificUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId",
                table: "Tasks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SpendingAccountId",
                table: "Users",
                column: "SpendingAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountHistories");

            migrationBuilder.DropTable(
                name: "DoneTasks");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Reserves");

            migrationBuilder.DropTable(
                name: "SavingAccounts");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "SpendingAccounts");
        }
    }
}
