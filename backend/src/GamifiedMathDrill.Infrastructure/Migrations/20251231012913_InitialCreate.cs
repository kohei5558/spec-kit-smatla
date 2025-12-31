using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Levels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LevelNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiredCorrectAnswers = table.Column<int>(type: "INTEGER", nullable: false),
                    MinDifficulty = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxDifficulty = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Problems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Question = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CorrectAnswer = table.Column<int>(type: "INTEGER", nullable: false),
                    CalculationType = table.Column<int>(type: "INTEGER", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RequiredPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rewards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CurrentLevelId = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    TotalPoints = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ConsecutiveDays = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    TotalProblems = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CorrectAnswers = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Levels_CurrentLevelId",
                        column: x => x.CurrentLevelId,
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProblemId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    BonusPoints = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 20),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyChallenges_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AcquiredRewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    RewardId = table.Column<int>(type: "INTEGER", nullable: false),
                    PointsSpent = table.Column<int>(type: "INTEGER", nullable: false),
                    AcquiredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcquiredRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcquiredRewards_Rewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "Rewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcquiredRewards_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProblemId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeSpentSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningRecords_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningRecords_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcquiredRewards_AcquiredAt",
                table: "AcquiredRewards",
                column: "AcquiredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AcquiredRewards_RewardId",
                table: "AcquiredRewards",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_AcquiredRewards_StudentId",
                table: "AcquiredRewards",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallenges_IsActive",
                table: "DailyChallenges",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallenges_ProblemId",
                table: "DailyChallenges",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallenges_TargetDate",
                table: "DailyChallenges",
                column: "TargetDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningRecords_AnsweredAt",
                table: "LearningRecords",
                column: "AnsweredAt");

            migrationBuilder.CreateIndex(
                name: "IX_LearningRecords_ProblemId",
                table: "LearningRecords",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningRecords_StudentId",
                table: "LearningRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningRecords_StudentId_AnsweredAt",
                table: "LearningRecords",
                columns: new[] { "StudentId", "AnsweredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Levels_LevelNumber",
                table: "Levels",
                column: "LevelNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Problems_CalculationType",
                table: "Problems",
                column: "CalculationType");

            migrationBuilder.CreateIndex(
                name: "IX_Problems_DifficultyLevel",
                table: "Problems",
                column: "DifficultyLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Rewards_Category",
                table: "Rewards",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Rewards_RequiredPoints",
                table: "Rewards",
                column: "RequiredPoints");

            migrationBuilder.CreateIndex(
                name: "IX_Students_CurrentLevelId",
                table: "Students",
                column: "CurrentLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_LastLoginAt",
                table: "Students",
                column: "LastLoginAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcquiredRewards");

            migrationBuilder.DropTable(
                name: "DailyChallenges");

            migrationBuilder.DropTable(
                name: "LearningRecords");

            migrationBuilder.DropTable(
                name: "Rewards");

            migrationBuilder.DropTable(
                name: "Problems");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Levels");
        }
    }
}
