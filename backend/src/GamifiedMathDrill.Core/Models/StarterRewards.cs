namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 新しい家庭に用意する初期景品（実物景品）。保護者の登録時に家庭ごとにコピーされる
/// </summary>
public static class StarterRewards
{
    /// <summary>
    /// 指定した家庭の初期景品を新しく作る（ID は DB で採番される）
    /// </summary>
    public static List<Reward> CreateFor(string parentId)
    {
        var rewards = Templates();
        foreach (var reward in rewards)
        {
            reward.ParentId = parentId;
            reward.CreatedBy = parentId;
            // SQLite など rowversion を自動生成しない DB 向けに初期化
            reward.RowVersion = Guid.NewGuid().ToByteArray();
        }
        return rewards;
    }

    private static List<Reward> Templates()
    {
        return new List<Reward>
        {
            // お菓子カテゴリー
            new Reward { Name = "うまい棒 チーズ味", Description = "定番の駄菓子", RequiredPoints = 10, Category = RewardCategory.Snack, ImageUrl = "/images/rewards/umaibo-cheese.jpg", Stock = 20, IsPhysical = true, IsActive = true },
            new Reward { Name = "チロルチョコ", Description = "小さなチョコレート", RequiredPoints = 15, Category = RewardCategory.Snack, ImageUrl = "/images/rewards/tirol.jpg", Stock = 15, IsPhysical = true, IsActive = true },
            new Reward { Name = "ブラックサンダー", Description = "ザクザク食感のチョコバー", RequiredPoints = 20, Category = RewardCategory.Snack, ImageUrl = "/images/rewards/black-thunder.jpg", Stock = 10, IsPhysical = true, IsActive = true },
            new Reward { Name = "ポテロング", Description = "細長いポテトチップス", RequiredPoints = 25, Category = RewardCategory.Snack, ImageUrl = "/images/rewards/potelong.jpg", Stock = 12, IsPhysical = true, IsActive = true },
            new Reward { Name = "キャベツ太郎", Description = "キャベツ風味のスナック", RequiredPoints = 10, Category = RewardCategory.Snack, ImageUrl = "/images/rewards/cabbage-taro.jpg", Stock = 18, IsPhysical = true, IsActive = true },

            // カードカテゴリー
            new Reward { Name = "ポケモンカード 1パック", Description = "ランダム5枚入り", RequiredPoints = 100, Category = RewardCategory.Card, ImageUrl = "/images/rewards/pokemon-card-pack.jpg", Stock = 5, IsPhysical = true, IsActive = true },
            new Reward { Name = "遊戯王カード 1パック", Description = "ランダム5枚入り", RequiredPoints = 100, Category = RewardCategory.Card, ImageUrl = "/images/rewards/yugioh-card-pack.jpg", Stock = 5, IsPhysical = true, IsActive = true },
            new Reward { Name = "デュエルマスターズカード 1パック", Description = "ランダム5枚入り", RequiredPoints = 100, Category = RewardCategory.Card, ImageUrl = "/images/rewards/duelmasters-card-pack.jpg", Stock = 5, IsPhysical = true, IsActive = true },

            // おもちゃカテゴリー
            new Reward { Name = "ミニカー", Description = "かっこいいミニカー", RequiredPoints = 150, Category = RewardCategory.Toy, ImageUrl = "/images/rewards/minicar.jpg", Stock = 3, IsPhysical = true, IsActive = true },
            new Reward { Name = "スーパーボール", Description = "よく弾むボール", RequiredPoints = 30, Category = RewardCategory.Toy, ImageUrl = "/images/rewards/superball.jpg", Stock = 10, IsPhysical = true, IsActive = true },
            new Reward { Name = "けん玉", Description = "伝統的な日本のおもちゃ", RequiredPoints = 200, Category = RewardCategory.Toy, ImageUrl = "/images/rewards/kendama.jpg", Stock = 2, IsPhysical = true, IsActive = true },
            new Reward { Name = "ヨーヨー", Description = "技が楽しめるヨーヨー", RequiredPoints = 180, Category = RewardCategory.Toy, ImageUrl = "/images/rewards/yoyo.jpg", Stock = 3, IsPhysical = true, IsActive = true },

            // 文房具カテゴリー
            new Reward { Name = "キャラクター鉛筆 3本セット", Description = "人気キャラクターの鉛筆", RequiredPoints = 50, Category = RewardCategory.Stationery, ImageUrl = "/images/rewards/pencil-set.jpg", Stock = 8, IsPhysical = true, IsActive = true },
            new Reward { Name = "消しゴム", Description = "かわいい消しゴム", RequiredPoints = 30, Category = RewardCategory.Stationery, ImageUrl = "/images/rewards/eraser.jpg", Stock = 12, IsPhysical = true, IsActive = true },
            new Reward { Name = "シールセット", Description = "キラキラシール50枚", RequiredPoints = 60, Category = RewardCategory.Stationery, ImageUrl = "/images/rewards/sticker-set.jpg", Stock = 10, IsPhysical = true, IsActive = true },
            new Reward { Name = "ノート", Description = "かわいいノート", RequiredPoints = 80, Category = RewardCategory.Stationery, ImageUrl = "/images/rewards/notebook.jpg", Stock = 6, IsPhysical = true, IsActive = true },

            // 本カテゴリー
            new Reward { Name = "学習漫画", Description = "楽しく学べる漫画", RequiredPoints = 300, Category = RewardCategory.Book, ImageUrl = "/images/rewards/study-manga.jpg", Stock = 2, IsPhysical = true, IsActive = true },
            new Reward { Name = "絵本", Description = "面白い絵本", RequiredPoints = 250, Category = RewardCategory.Book, ImageUrl = "/images/rewards/picture-book.jpg", Stock = 3, IsPhysical = true, IsActive = true },

            // その他カテゴリー
            new Reward { Name = "キーホルダー", Description = "かわいいキーホルダー", RequiredPoints = 120, Category = RewardCategory.Other, ImageUrl = "/images/rewards/keychain.jpg", Stock = 5, IsPhysical = true, IsActive = true },
            new Reward { Name = "バッジ", Description = "缶バッジ", RequiredPoints = 40, Category = RewardCategory.Other, ImageUrl = "/images/rewards/badge.jpg", Stock = 15, IsPhysical = true, IsActive = true },
        };
    }
}
