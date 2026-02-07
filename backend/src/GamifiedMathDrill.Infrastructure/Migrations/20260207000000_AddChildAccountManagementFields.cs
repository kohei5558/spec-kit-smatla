using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChildAccountManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add GradeLevel column to AspNetUsers
            migrationBuilder.AddColumn<int>(
                name: "GradeLevel",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            // Create PresetAvatars table
            migrationBuilder.CreateTable(
                name: "PresetAvatars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresetAvatars", x => x.Id);
                });

            // Create index on DisplayOrder
            migrationBuilder.CreateIndex(
                name: "IX_PresetAvatars_DisplayOrder",
                table: "PresetAvatars",
                column: "DisplayOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop PresetAvatars table
            migrationBuilder.DropTable(
                name: "PresetAvatars");

            // Drop GradeLevel column from AspNetUsers
            migrationBuilder.DropColumn(
                name: "GradeLevel",
                table: "AspNetUsers");
        }
    }
}
