using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixLearningRecordSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeSpentSeconds",
                table: "LearningRecords");

            migrationBuilder.RenameColumn(
                name: "AnsweredAt",
                table: "LearningRecords",
                newName: "SolvedAt");

            migrationBuilder.RenameIndex(
                name: "IX_LearningRecords_StudentId_AnsweredAt",
                table: "LearningRecords",
                newName: "IX_LearningRecords_StudentId_SolvedAt");

            migrationBuilder.RenameIndex(
                name: "IX_LearningRecords_AnsweredAt",
                table: "LearningRecords",
                newName: "IX_LearningRecords_SolvedAt");

            migrationBuilder.AddColumn<int>(
                name: "PointsEarned",
                table: "LearningRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StudentAnswer",
                table: "LearningRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeTakenSeconds",
                table: "LearningRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PointsEarned",
                table: "LearningRecords");

            migrationBuilder.DropColumn(
                name: "StudentAnswer",
                table: "LearningRecords");

            migrationBuilder.DropColumn(
                name: "TimeTakenSeconds",
                table: "LearningRecords");

            migrationBuilder.RenameColumn(
                name: "SolvedAt",
                table: "LearningRecords",
                newName: "AnsweredAt");

            migrationBuilder.RenameIndex(
                name: "IX_LearningRecords_StudentId_SolvedAt",
                table: "LearningRecords",
                newName: "IX_LearningRecords_StudentId_AnsweredAt");

            migrationBuilder.RenameIndex(
                name: "IX_LearningRecords_SolvedAt",
                table: "LearningRecords",
                newName: "IX_LearningRecords_AnsweredAt");

            migrationBuilder.AddColumn<int>(
                name: "TimeSpentSeconds",
                table: "LearningRecords",
                type: "INTEGER",
                nullable: true);
        }
    }
}
