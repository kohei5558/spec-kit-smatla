using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Migrations.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class UseFluentEmojiImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 初期景品・アバターの画像を Fluent Emoji（画面側 wwwroot の PNG）にする。既存の DB のパスも合わせる
            migrationBuilder.Sql("UPDATE \"Rewards\" SET \"ImageUrl\" = REPLACE(\"ImageUrl\", '.jpg', '.png') WHERE \"ImageUrl\" LIKE '/images/rewards/%.jpg';");
            foreach (var table in new[] { "AspNetUsers", "Students" })
            {
                migrationBuilder.Sql($"UPDATE \"{table}\" SET \"AvatarUrl\" = '/avatars/rabbit.png' WHERE \"AvatarUrl\" = '/images/avatars/girl1.png';");
                migrationBuilder.Sql($"UPDATE \"{table}\" SET \"AvatarUrl\" = '/avatars/lion.png' WHERE \"AvatarUrl\" = '/images/avatars/boy1.png';");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 画像のパスの置き換えだけなので、元に戻す必要はない
        }
    }
}
