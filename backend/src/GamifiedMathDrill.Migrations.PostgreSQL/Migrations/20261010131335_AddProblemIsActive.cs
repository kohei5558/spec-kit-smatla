using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddProblemIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Problems",
                type: "boolean",
                nullable: false,
                // 既存の問題はいったん使う状態にする（起動時に ProblemSynchronizer が最新のセットに合わせる、011）
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Problems");
        }
    }
}
