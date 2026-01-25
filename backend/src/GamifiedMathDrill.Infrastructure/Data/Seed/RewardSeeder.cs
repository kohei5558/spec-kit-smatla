using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Infrastructure.Data.Seed;

/// <summary>
/// 景品の初期データ（実物景品）
/// </summary>
public static class RewardSeeder
{
    public static List<Reward> GetRewards()
    {
        return new List<Reward>
        {
            // お菓子カテゴリー
            new Reward { Id = 1, Name = "うまい棒 チーズ味", Description = "定番の駄菓子", RequiredPoints = 10, Category = RewardCategory.Snack, ImageUrl = "/images/rewards/umaibo-cheese.jpg", Stock = 20, IsPhysical = true, IsActive = true },
            new Reward { Id = 2, Name = "チロルチョコ", Description = "小さなチョコレート", RequiredPoints = 15, Category = RewardCategory.Snack, ImageUrl = "/images/rewards/tirol.jpg", Stock = 15, IsPhysical = true, IsActive = true },
            new Reward { Id = 3, Name = "ブラックサンダー", Description = "ザクザク食感のチョコバー", RequiredPoints = 20, Category = RewardCategory.Snack, ImageUrl = "/images/rewards/black-thunder.jpg", Stock = 10, IsPhysical = true, IsActive = true },
            new Reward { Id = 4, Name = "ポテロング", Description = "細長いポテトチップス", RequiredPoints = 25, Category = RewardCategory.Snack, ImageUrl = "/images/rewards/potelong.jpg", Stock = 12, IsPhysical = true, IsActive = true },
            new Reward { Id = 5, Name = "キャベツ太郎", Description = "キャベツ風味のスナック", RequiredPoints = 10, Category = RewardCategory.Snack, ImageUrl = "/images/rewards/cabbage-taro.jpg", Stock = 18, IsPhysical = true, IsActive = true },

            // カードカテゴリー
            new Reward { Id = 6, Name = "ポケモンカード 1パック", Description = "ランダム5枚入り", RequiredPoints = 100, Category = RewardCategory.Card, ImageUrl = "/images/rewards/pokemon-card-pack.jpg", Stock = 5, IsPhysical = true, IsActive = true },
            new Reward { Id = 7, Name = "遊戯王カード 1パック", Description = "ランダム5枚入り", RequiredPoints = 100, Category = RewardCategory.Card, ImageUrl = "/images/rewards/yugioh-card-pack.jpg", Stock = 5, IsPhysical = true, IsActive = true },
            new Reward { Id = 8, Name = "デュエルマスターズカード 1パック", Description = "ランダム5枚入り", RequiredPoints = 100, Category = RewardCategory.Card, ImageUrl = "/images/rewards/duelmasters-card-pack.jpg", Stock = 5, IsPhysical = true, IsActive = true },

            // おもちゃカテゴリー
            new Reward { Id = 9, Name = "ミニカー", Description = "かっこいいミニカー", RequiredPoints = 150, Category = RewardCategory.Toy, ImageUrl = "/images/rewards/minicar.jpg", Stock = 3, IsPhysical = true, IsActive = true },
            new Reward { Id = 10, Name = "スーパーボール", Description = "よく弾むボール", RequiredPoints = 30, Category = RewardCategory.Toy, ImageUrl = "/images/rewards/superball.jpg", Stock = 10, IsPhysical = true, IsActive = true },
            new Reward { Id = 11, Name = "けん玉", Description = "伝統的な日本のおもちゃ", RequiredPoints = 200, Category = RewardCategory.Toy, ImageUrl = "/images/rewards/kendama.jpg", Stock = 2, IsPhysical = true, IsActive = true },
            new Reward { Id = 12, Name = "ヨーヨー", Description = "技が楽しめるヨーヨー", RequiredPoints = 180, Category = RewardCategory.Toy, ImageUrl = "/images/rewards/yoyo.jpg", Stock = 3, IsPhysical = true, IsActive = true },

            // 文房具カテゴリー
            new Reward { Id = 13, Name = "キャラクター鉛筆 3本セット", Description = "人気キャラクターの鉛筆", RequiredPoints = 50, Category = RewardCategory.Stationery, ImageUrl = "/images/rewards/pencil-set.jpg", Stock = 8, IsPhysical = true, IsActive = true },
            new Reward { Id = 14, Name = "消しゴム", Description = "かわいい消しゴム", RequiredPoints = 30, Category = RewardCategory.Stationery, ImageUrl = "/images/rewards/eraser.jpg", Stock = 12, IsPhysical = true, IsActive = true },
            new Reward { Id = 15, Name = "シールセット", Description = "キラキラシール50枚", RequiredPoints = 60, Category = RewardCategory.Stationery, ImageUrl = "/images/rewards/sticker-set.jpg", Stock = 10, IsPhysical = true, IsActive = true },
            new Reward { Id = 16, Name = "ノート", Description = "かわいいノート", RequiredPoints = 80, Category = RewardCategory.Stationery, ImageUrl = "/images/rewards/notebook.jpg", Stock = 6, IsPhysical = true, IsActive = true },

            // 本カテゴリー
            new Reward { Id = 17, Name = "学習漫画", Description = "楽しく学べる漫画", RequiredPoints = 300, Category = RewardCategory.Book, ImageUrl = "/images/rewards/study-manga.jpg", Stock = 2, IsPhysical = true, IsActive = true },
            new Reward { Id = 18, Name = "絵本", Description = "面白い絵本", RequiredPoints = 250, Category = RewardCategory.Book, ImageUrl = "/images/rewards/picture-book.jpg", Stock = 3, IsPhysical = true, IsActive = true },

            // その他カテゴリー
            new Reward { Id = 19, Name = "キーホルダー", Description = "かわいいキーホルダー", RequiredPoints = 120, Category = RewardCategory.Other, ImageUrl = "/images/rewards/keychain.jpg", Stock = 5, IsPhysical = true, IsActive = true },
            new Reward { Id = 20, Name = "バッジ", Description = "缶バッジ", RequiredPoints = 40, Category = RewardCategory.Other, ImageUrl = "/images/rewards/badge.jpg", Stock = 15, IsPhysical = true, IsActive = true },
        };
    }
}
