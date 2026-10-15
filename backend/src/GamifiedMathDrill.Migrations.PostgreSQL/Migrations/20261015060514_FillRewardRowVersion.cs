using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class FillRewardRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RowVersion が [Timestamp] だったため、これまで DB に保存されず空のままだった。空の景品に値を入れて編集できるようにする
            migrationBuilder.Sql("UPDATE \"Rewards\" SET \"RowVersion\" = decode(md5(random()::text || \"Id\"::text), 'hex') WHERE \"RowVersion\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 入れた値は残しても害がないため、元に戻さない
        }
    }
}
