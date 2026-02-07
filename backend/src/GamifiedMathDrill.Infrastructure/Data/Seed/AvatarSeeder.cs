using GamifiedMathDrill.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Data.Seed;

/// <summary>
/// プリセットアバターのシードデータ
/// </summary>
public static class AvatarSeeder
{
    /// <summary>
    /// プリセットアバターをシードする
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 既にデータがある場合はスキップ
        if (await context.PresetAvatars.AnyAsync())
        {
            return;
        }

        var avatars = new List<PresetAvatar>
        {
            new() { Name = "猫", FileName = "cat.png", DisplayOrder = 1 },
            new() { Name = "犬", FileName = "dog.png", DisplayOrder = 2 },
            new() { Name = "パンダ", FileName = "panda.png", DisplayOrder = 3 },
            new() { Name = "ライオン", FileName = "lion.png", DisplayOrder = 4 },
            new() { Name = "ウサギ", FileName = "rabbit.png", DisplayOrder = 5 },
            new() { Name = "クマ", FileName = "bear.png", DisplayOrder = 6 },
            new() { Name = "トラ", FileName = "tiger.png", DisplayOrder = 7 },
            new() { Name = "キリン", FileName = "giraffe.png", DisplayOrder = 8 },
            new() { Name = "ゾウ", FileName = "elephant.png", DisplayOrder = 9 },
            new() { Name = "サル", FileName = "monkey.png", DisplayOrder = 10 },
            new() { Name = "ペンギン", FileName = "penguin.png", DisplayOrder = 11 },
            new() { Name = "コアラ", FileName = "koala.png", DisplayOrder = 12 },
            new() { Name = "カエル", FileName = "frog.png", DisplayOrder = 13 },
            new() { Name = "リス", FileName = "squirrel.png", DisplayOrder = 14 },
            new() { Name = "キツネ", FileName = "fox.png", DisplayOrder = 15 }
        };

        context.PresetAvatars.AddRange(avatars);
        await context.SaveChangesAsync();
    }
}
