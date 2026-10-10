using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddDivisionWithRemainder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CorrectRemainder",
                table: "Problems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentRemainder",
                table: "LearningRecords",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectRemainder",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "StudentRemainder",
                table: "LearningRecords");
        }
    }
}
