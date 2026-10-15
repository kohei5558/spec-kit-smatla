using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class PerStudentDailyChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 全員共通だった以前のチャレンジは学習者に紐付けられないため消す（010、公開前のため）
            migrationBuilder.Sql("DELETE FROM \"DailyChallenges\";");

            migrationBuilder.DropIndex(
                name: "IX_DailyChallenges_IsActive",
                table: "DailyChallenges");

            migrationBuilder.DropIndex(
                name: "IX_DailyChallenges_TargetDate",
                table: "DailyChallenges");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DailyChallenges");

            migrationBuilder.AlterColumn<int>(
                name: "BonusPoints",
                table: "DailyChallenges",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "AnsweredAt",
                table: "DailyChallenges",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "DailyChallenges",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentAnswer",
                table: "DailyChallenges",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "DailyChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallenges_StudentId_TargetDate",
                table: "DailyChallenges",
                columns: new[] { "StudentId", "TargetDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyChallenges_Students_StudentId",
                table: "DailyChallenges",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyChallenges_Students_StudentId",
                table: "DailyChallenges");

            migrationBuilder.DropIndex(
                name: "IX_DailyChallenges_StudentId_TargetDate",
                table: "DailyChallenges");

            migrationBuilder.DropColumn(
                name: "AnsweredAt",
                table: "DailyChallenges");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "DailyChallenges");

            migrationBuilder.DropColumn(
                name: "StudentAnswer",
                table: "DailyChallenges");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "DailyChallenges");

            migrationBuilder.AlterColumn<int>(
                name: "BonusPoints",
                table: "DailyChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 20,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DailyChallenges",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallenges_IsActive",
                table: "DailyChallenges",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallenges_TargetDate",
                table: "DailyChallenges",
                column: "TargetDate",
                unique: true);
        }
    }
}
