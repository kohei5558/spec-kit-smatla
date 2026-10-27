using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddKanjiProblems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Choices",
                table: "Problems",
                type: "TEXT",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectText",
                table: "Problems",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentText",
                table: "LearningRecords",
                type: "TEXT",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Choices",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "CorrectText",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "StudentText",
                table: "LearningRecords");
        }
    }
}
