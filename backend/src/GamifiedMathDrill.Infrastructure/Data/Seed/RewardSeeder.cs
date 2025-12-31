using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Infrastructure.Data.Seed;

/// <summary>
/// 景品のシードデータ
/// </summary>
public static class RewardSeeder
{
    public static List<Reward> GetRewards()
    {
        return new List<Reward>
        {
            // バッジ (10種類)
            new Reward { Id = 1, Name = "銅メダル", Description = "最初の一歩！", RequiredPoints = 50, Category = RewardCategory.Badge, ImageUrl = "/images/rewards/bronze-medal.png" },
            new Reward { Id = 2, Name = "銀メダル", Description = "順調な成長！", RequiredPoints = 100, Category = RewardCategory.Badge, ImageUrl = "/images/rewards/silver-medal.png" },
            new Reward { Id = 3, Name = "金メダル", Description = "素晴らしい！", RequiredPoints = 200, Category = RewardCategory.Badge, ImageUrl = "/images/rewards/gold-medal.png" },
            new Reward { Id = 4, Name = "プラチナメダル", Description = "最高の栄誉！", RequiredPoints = 500, Category = RewardCategory.Badge, ImageUrl = "/images/rewards/platinum-medal.png" },
            new Reward { Id = 5, Name = "星バッジ", Description = "きらきら輝く！", RequiredPoints = 150, Category = RewardCategory.Badge, ImageUrl = "/images/rewards/star-badge.png" },
            new Reward { Id = 6, Name = "ハートバッジ", Description = "愛と努力の証", RequiredPoints = 150, Category = RewardCategory.Badge, ImageUrl = "/images/rewards/heart-badge.png" },
            new Reward { Id = 7, Name = "雷バッジ", Description = "スピードマスター", RequiredPoints = 250, Category = RewardCategory.Badge, ImageUrl = "/images/rewards/thunder-badge.png" },
            new Reward { Id = 8, Name = "王冠バッジ", Description = "計算王の証", RequiredPoints = 400, Category = RewardCategory.Badge, ImageUrl = "/images/rewards/crown-badge.png" },
            new Reward { Id = 9, Name = "虹バッジ", Description = "全ての計算マスター", RequiredPoints = 600, Category = RewardCategory.Badge, ImageUrl = "/images/rewards/rainbow-badge.png" },
            new Reward { Id = 10, Name = "伝説バッジ", Description = "伝説の計算名人", RequiredPoints = 1000, Category = RewardCategory.Badge, ImageUrl = "/images/rewards/legend-badge.png" },

            // アバター (10種類)
            new Reward { Id = 11, Name = "青いアバター", Description = "クールなブルー", RequiredPoints = 80, Category = RewardCategory.Avatar, ImageUrl = "/images/avatars/blue-avatar.png" },
            new Reward { Id = 12, Name = "赤いアバター", Description = "情熱のレッド", RequiredPoints = 80, Category = RewardCategory.Avatar, ImageUrl = "/images/avatars/red-avatar.png" },
            new Reward { Id = 13, Name = "緑のアバター", Description = "自然のグリーン", RequiredPoints = 80, Category = RewardCategory.Avatar, ImageUrl = "/images/avatars/green-avatar.png" },
            new Reward { Id = 14, Name = "黄色いアバター", Description = "元気なイエロー", RequiredPoints = 80, Category = RewardCategory.Avatar, ImageUrl = "/images/avatars/yellow-avatar.png" },
            new Reward { Id = 15, Name = "紫のアバター", Description = "神秘のパープル", RequiredPoints = 120, Category = RewardCategory.Avatar, ImageUrl = "/images/avatars/purple-avatar.png" },
            new Reward { Id = 16, Name = "ピンクのアバター", Description = "可愛いピンク", RequiredPoints = 120, Category = RewardCategory.Avatar, ImageUrl = "/images/avatars/pink-avatar.png" },
            new Reward { Id = 17, Name = "オレンジのアバター", Description = "明るいオレンジ", RequiredPoints = 120, Category = RewardCategory.Avatar, ImageUrl = "/images/avatars/orange-avatar.png" },
            new Reward { Id = 18, Name = "虹色アバター", Description = "七色の輝き", RequiredPoints = 300, Category = RewardCategory.Avatar, ImageUrl = "/images/avatars/rainbow-avatar.png" },
            new Reward { Id = 19, Name = "金色アバター", Description = "ゴールデンスター", RequiredPoints = 400, Category = RewardCategory.Avatar, ImageUrl = "/images/avatars/gold-avatar.png" },
            new Reward { Id = 20, Name = "ダイヤモンドアバター", Description = "最高のきらめき", RequiredPoints = 800, Category = RewardCategory.Avatar, ImageUrl = "/images/avatars/diamond-avatar.png" },

            // キャラクター (10種類)
            new Reward { Id = 21, Name = "子猫のミーちゃん", Description = "可愛い子猫", RequiredPoints = 100, Category = RewardCategory.Character, ImageUrl = "/images/characters/cat.png" },
            new Reward { Id = 22, Name = "子犬のポチ", Description = "元気な子犬", RequiredPoints = 100, Category = RewardCategory.Character, ImageUrl = "/images/characters/dog.png" },
            new Reward { Id = 23, Name = "ウサギのピョン太", Description = "ぴょんぴょんウサギ", RequiredPoints = 150, Category = RewardCategory.Character, ImageUrl = "/images/characters/rabbit.png" },
            new Reward { Id = 24, Name = "パンダのパンちゃん", Description = "もふもふパンダ", RequiredPoints = 200, Category = RewardCategory.Character, ImageUrl = "/images/characters/panda.png" },
            new Reward { Id = 25, Name = "ペンギンのペンペン", Description = "おしゃれペンギン", RequiredPoints = 200, Category = RewardCategory.Character, ImageUrl = "/images/characters/penguin.png" },
            new Reward { Id = 26, Name = "フクロウのホーホー", Description = "賢いフクロウ", RequiredPoints = 250, Category = RewardCategory.Character, ImageUrl = "/images/characters/owl.png" },
            new Reward { Id = 27, Name = "ライオンのレオ", Description = "勇敢なライオン", RequiredPoints = 350, Category = RewardCategory.Character, ImageUrl = "/images/characters/lion.png" },
            new Reward { Id = 28, Name = "ドラゴンのリュウ", Description = "伝説のドラゴン", RequiredPoints = 500, Category = RewardCategory.Character, ImageUrl = "/images/characters/dragon.png" },
            new Reward { Id = 29, Name = "フェニックスのフェニ", Description = "不死鳥の輝き", RequiredPoints = 700, Category = RewardCategory.Character, ImageUrl = "/images/characters/phoenix.png" },
            new Reward { Id = 30, Name = "ユニコーンのユニ", Description = "幻の一角獣", RequiredPoints = 1000, Category = RewardCategory.Character, ImageUrl = "/images/characters/unicorn.png" }
        };
    }
}
