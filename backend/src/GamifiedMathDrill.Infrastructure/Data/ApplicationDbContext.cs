using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Data;

/// <summary>
/// アプリケーションのデータベースコンテキスト
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Problem> Problems => Set<Problem>();
    public DbSet<LearningRecord> LearningRecords => Set<LearningRecord>();
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<AcquiredReward> AcquiredRewards => Set<AcquiredReward>();
    public DbSet<DailyChallenge> DailyChallenges => Set<DailyChallenge>();
    public DbSet<Level> Levels => Set<Level>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Student
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalPoints).HasDefaultValue(0);
            entity.Property(e => e.ConsecutiveDays).HasDefaultValue(0);
            entity.Property(e => e.TotalProblems).HasDefaultValue(0);
            entity.Property(e => e.CorrectAnswers).HasDefaultValue(0);
            entity.Property(e => e.CurrentLevelId).HasDefaultValue(1);
            
            entity.HasIndex(e => e.CurrentLevelId);
            entity.HasIndex(e => e.LastLoginAt);

            entity.HasOne(e => e.CurrentLevel)
                .WithMany(l => l.Students)
                .HasForeignKey(e => e.CurrentLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Problem
        modelBuilder.Entity<Problem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Question).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CorrectAnswer).IsRequired();
            entity.Property(e => e.CalculationType).IsRequired();
            entity.Property(e => e.DifficultyLevel).IsRequired();
            
            entity.HasIndex(e => e.CalculationType);
            entity.HasIndex(e => e.DifficultyLevel);
        });

        // LearningRecord
        modelBuilder.Entity<LearningRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsCorrect).IsRequired();
            entity.Property(e => e.StudentAnswer).IsRequired();
            entity.Property(e => e.PointsEarned).IsRequired();
            entity.Property(e => e.TimeTakenSeconds).IsRequired();
            entity.Property(e => e.SolvedAt).IsRequired();
            
            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.SolvedAt);
            entity.HasIndex(e => new { e.StudentId, e.SolvedAt });

            entity.HasOne(e => e.Student)
                .WithMany(s => s.LearningRecords)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Problem)
                .WithMany(p => p.LearningRecords)
                .HasForeignKey(e => e.ProblemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Reward
        modelBuilder.Entity<Reward>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RequiredPoints).IsRequired();
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            
            entity.HasIndex(e => e.RequiredPoints);
            entity.HasIndex(e => e.Category);
        });

        // AcquiredReward
        modelBuilder.Entity<AcquiredReward>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PointsSpent).IsRequired();
            entity.Property(e => e.AcquiredAt).IsRequired();
            
            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.AcquiredAt);

            entity.HasOne(e => e.Student)
                .WithMany(s => s.AcquiredRewards)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Reward)
                .WithMany(r => r.AcquiredRewards)
                .HasForeignKey(e => e.RewardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DailyChallenge
        modelBuilder.Entity<DailyChallenge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetDate).IsRequired();
            entity.Property(e => e.BonusPoints).HasDefaultValue(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasIndex(e => e.TargetDate).IsUnique();
            entity.HasIndex(e => e.IsActive);

            entity.HasOne(e => e.Problem)
                .WithMany(p => p.DailyChallenges)
                .HasForeignKey(e => e.ProblemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Level
        modelBuilder.Entity<Level>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LevelNumber).IsRequired();
            entity.Property(e => e.RequiredCorrectAnswers).IsRequired();
            entity.Property(e => e.MinDifficulty).IsRequired();
            entity.Property(e => e.MaxDifficulty).IsRequired();
            
            entity.HasIndex(e => e.LevelNumber).IsUnique();
        });
    }
}
