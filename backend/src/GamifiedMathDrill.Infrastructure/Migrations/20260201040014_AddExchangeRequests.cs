using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExchangeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    RewardId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ParentNote = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RequiredPoints = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRequests_Rewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "Rewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeRequests_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRequests_RequestedAt",
                table: "ExchangeRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRequests_RewardId",
                table: "ExchangeRequests",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRequests_Status",
                table: "ExchangeRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRequests_StudentId",
                table: "ExchangeRequests",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRequests");
        }
    }
}
