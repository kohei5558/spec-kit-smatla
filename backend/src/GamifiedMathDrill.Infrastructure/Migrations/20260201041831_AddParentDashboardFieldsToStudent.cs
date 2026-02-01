using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParentDashboardFieldsToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AccuracyRate",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentUserId",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalProblemsCompleted",
                table: "Students",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccuracyRate",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentUserId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "TotalProblemsCompleted",
                table: "Students");
        }
    }
}
