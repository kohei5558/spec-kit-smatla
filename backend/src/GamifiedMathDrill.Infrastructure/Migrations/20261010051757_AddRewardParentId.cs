using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardParentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentId",
                table: "Rewards",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rewards_ParentId",
                table: "Rewards",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rewards_ParentId",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Rewards");
        }
    }
}
