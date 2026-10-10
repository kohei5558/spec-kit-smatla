using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GamifiedMathDrill.Infrastructure.Data.Seed;

/// <summary>
/// ユーザーの初期データ（保護者・子供アカウント）
/// </summary>
public static class UserSeeder
{
    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        // 子供のPINはログイン時にハッシュで照合されるため、ハッシュ化して保存する
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();

        // 保護者アカウント
        var parentEmail = "parent@example.com";
        var existingParent = await userManager.FindByEmailAsync(parentEmail);

        if (existingParent == null)
        {
            var parent = new ApplicationUser
            {
                UserName = parentEmail,
                Email = parentEmail,
                DisplayName = "田中太郎",
                Role = UserRole.Parent,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(parent, "Parent123!");
            if (result.Succeeded)
            {
                // デモ家庭の初期景品
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                context.Rewards.AddRange(StarterRewards.CreateFor(parent.Id));
                await context.SaveChangesAsync();

                Console.WriteLine($"Created parent user: {parentEmail}");

                // 子供アカウント1
                var child1 = new ApplicationUser
                {
                    UserName = $"{parent.Id}_child1",
                    Email = $"{parent.Id}_child1@example.com",
                    DisplayName = "花子",
                    Role = UserRole.Child,
                    ParentId = parent.Id,
                    AvatarUrl = "/images/avatars/girl1.png",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                child1.PIN = passwordHasher.HashPassword(child1, "1234");
                await userManager.CreateAsync(child1, "Child123!@#");
                Console.WriteLine($"Created child user: {child1.DisplayName} (PIN: 1234)");

                // 子供アカウント2
                var child2 = new ApplicationUser
                {
                    UserName = $"{parent.Id}_child2",
                    Email = $"{parent.Id}_child2@example.com",
                    DisplayName = "次郎",
                    Role = UserRole.Child,
                    ParentId = parent.Id,
                    AvatarUrl = "/images/avatars/boy1.png",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                child2.PIN = passwordHasher.HashPassword(child2, "5678");
                await userManager.CreateAsync(child2, "Child456!@#");
                Console.WriteLine($"Created child user: {child2.DisplayName} (PIN: 5678)");
            }
            else
            {
                Console.WriteLine($"Failed to create parent user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"Parent user already exists: {parentEmail}");
        }
    }
}
