using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GamifiedMathDrill.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentLevels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    CalculationType = table.Column<int>(type: "integer", nullable: false),
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentLevels_Levels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentLevels_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentLevels_LevelId",
                table: "StudentLevels",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLevels_StudentId_CalculationType",
                table: "StudentLevels",
                columns: new[] { "StudentId", "CalculationType" },
                unique: true);

            // 今のレベルと連続正解数を、既存の5種類（たし算・ひき算・かけ算・わり算・あまりのあるわり算）に写す。列を消す前に行う（014）
            migrationBuilder.Sql(@"INSERT INTO ""StudentLevels"" (""StudentId"", ""CalculationType"", ""LevelId"", ""CorrectAnswers"")
SELECT s.""Id"", t.v, s.""CurrentLevelId"", s.""CorrectAnswers""
FROM ""Students"" s
CROSS JOIN (SELECT 0 AS v UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4) t;");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Levels_CurrentLevelId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_CurrentLevelId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CorrectAnswers",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CurrentLevelId",
                table: "Students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CorrectAnswers",
                table: "Students",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentLevelId",
                table: "Students",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Students_CurrentLevelId",
                table: "Students",
                column: "CurrentLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Levels_CurrentLevelId",
                table: "Students",
                column: "CurrentLevelId",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 元に戻すときは、5種類のうちいちばん高いレベルを1つのレベルにする（連続正解数は0）
            migrationBuilder.Sql(@"UPDATE ""Students"" SET ""CurrentLevelId"" = COALESCE((SELECT MAX(l.""LevelId"") FROM ""StudentLevels"" l WHERE l.""StudentId"" = ""Students"".""Id"" AND l.""CalculationType"" <= 4), 1);");

            migrationBuilder.DropTable(
                name: "StudentLevels");
        }
    }
}
