using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeProblems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnswerFormat",
                table: "Problems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ClockTimes",
                table: "Problems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // 既存のあまりのあるわり算の問題は「[ ] あまり [ ]」の答えの形にする（013）
            migrationBuilder.Sql(@"UPDATE ""Problems"" SET ""AnswerFormat"" = 1 WHERE ""CalculationType"" = 4;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerFormat",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "ClockTimes",
                table: "Problems");
        }
    }
}
