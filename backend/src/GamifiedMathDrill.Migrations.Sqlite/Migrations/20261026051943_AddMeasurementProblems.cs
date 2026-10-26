using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementProblems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnswerUnits",
                table: "Problems",
                type: "TEXT",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerUnits",
                table: "Problems");
        }
    }
}
