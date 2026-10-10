using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GamifiedMathDrill.Infrastructure.Services;

/// <summary>
/// 子供アカウント管理サービス実装
/// </summary>
public class ChildAccountService : IChildAccountService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IMemoryCache _cache;
    private readonly IPresetAvatarRepository _avatarRepository;

    private const int MaxChildAccounts = 10;
    private const int MaxPinAttempts = 3;
    private static readonly TimeSpan PinLockoutDuration = TimeSpan.FromMinutes(5);

    public ChildAccountService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IMemoryCache cache,
        IPresetAvatarRepository avatarRepository)
    {
        _context = context;
        _userManager = userManager;
        _passwordHasher = passwordHasher;
        _cache = cache;
        _avatarRepository = avatarRepository;
    }

    /// <summary>
    /// 保護者の子供アカウント一覧を取得
    /// </summary>
    public async Task<List<ChildAccountDto>> ListAsync(string parentId)
    {
        var children = await _context.Users
            .Where(u => u.ParentId == parentId && u.Role == UserRole.Child)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        return children.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 子供アカウント詳細を取得
    /// </summary>
    public async Task<ChildAccountDto?> GetAsync(string childId, string parentId)
    {
        // parentIdが空の場合は子供自身のログイン（IsActiveチェックのみ）
        var query = _context.Users.Where(u => u.Id == childId && u.Role == UserRole.Child);
        
        if (!string.IsNullOrEmpty(parentId))
        {
            query = query.Where(u => u.ParentId == parentId);
        }

        var child = await query.FirstOrDefaultAsync();

        return child != null ? MapToDto(child) : null;
    }

    /// <summary>
    /// 子供アカウント詳細（学習統計含む）を取得
    /// </summary>
    public async Task<ChildAccountDto?> GetDetailAsync(string childId, string parentId)
    {
        var dto = await GetAsync(childId, parentId);
        if (dto == null) return null;

        dto.LearningStats = await GetLearningStatsAsync(childId);
        return dto;
    }

    /// <summary>
    /// 子供アカウントを作成
    /// </summary>
    public async Task<ChildAccountDto> CreateAsync(string parentId, ChildAccountCreateDto dto)
    {
        // 上限チェック
        var existingCount = await _context.Users
            .CountAsync(u => u.ParentId == parentId && u.Role == UserRole.Child);
        
        if (existingCount >= MaxChildAccounts)
        {
            throw new InvalidOperationException($"子供アカウントは{MaxChildAccounts}件までです");
        }

        // 重複名チェック
        var nameExists = await _context.Users
            .AnyAsync(u => u.ParentId == parentId && u.Role == UserRole.Child && u.DisplayName == dto.Name);
        
        if (nameExists)
        {
            throw new InvalidOperationException("同じ名前の子供アカウントが既に存在します");
        }

        // プリセットアバター存在チェック
        var avatar = await _avatarRepository.GetByIdAsync(dto.PresetAvatarId);
        if (avatar == null)
        {
            throw new InvalidOperationException("指定されたアバターが見つかりません");
        }

        // ApplicationUser作成
        var userName = Guid.NewGuid().ToString();
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = $"{userName}@child.local", // 子供アカウントにはダミーメールアドレス
            DisplayName = dto.Name,
            Role = UserRole.Child,
            ParentId = parentId,
            GradeLevel = dto.GradeLevel,
            AvatarUrl = $"/avatars/{avatar.FileName}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // PINをハッシュ化して保存
        user.PIN = _passwordHasher.HashPassword(user, dto.PIN);

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"アカウント作成に失敗しました: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Student作成（1:1関係）
        var student = new Student
        {
            Name = user.DisplayName,
            ParentUserId = user.Id,
            AvatarUrl = user.AvatarUrl,
            // 学年に応じたレベルから始める（例: 3年生はレベル5）
            CurrentLevelId = GradeStartLevel.ForGrade(dto.GradeLevel)
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        // 子供アカウントに学習者を紐付ける（未設定だと初回ログイン時に別の学習者が作られてしまう）
        user.StudentId = student.Id;
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    /// <summary>
    /// 子供アカウントを更新
    /// </summary>
    public async Task<ChildAccountDto> UpdateAsync(string childId, string parentId, ChildAccountUpdateDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == childId && u.ParentId == parentId && u.Role == UserRole.Child);

        if (user == null)
        {
            throw new InvalidOperationException("子供アカウントが見つかりません");
        }

        // 重複名チェック（自分以外）
        var nameExists = await _context.Users
            .AnyAsync(u => u.ParentId == parentId && u.Role == UserRole.Child && u.DisplayName == dto.Name && u.Id != childId);
        
        if (nameExists)
        {
            throw new InvalidOperationException("同じ名前の子供アカウントが既に存在します");
        }

        // プリセットアバター存在チェック
        var avatar = await _avatarRepository.GetByIdAsync(dto.PresetAvatarId);
        if (avatar == null)
        {
            throw new InvalidOperationException("指定されたアバターが見つかりません");
        }

        // 更新
        user.DisplayName = dto.Name;
        user.GradeLevel = dto.GradeLevel;
        user.AvatarUrl = $"/avatars/{avatar.FileName}";

        // PIN変更
        if (!string.IsNullOrEmpty(dto.NewPIN))
        {
            user.PIN = _passwordHasher.HashPassword(user, dto.NewPIN);
        }

        await _userManager.UpdateAsync(user);

        // Student同期更新（子供アカウントに紐付いた学習者）
        var student = user.StudentId == null ? null : await _context.Students.FindAsync(user.StudentId.Value);
        if (student != null)
        {
            student.Name = user.DisplayName;
            student.AvatarUrl = user.AvatarUrl;

            // 学年を上げて始めるレベルが今より高くなったら上げる（下げることはしない）
            var startLevel = GradeStartLevel.ForGrade(dto.GradeLevel);
            if (student.CurrentLevelId < startLevel)
            {
                student.CurrentLevelId = startLevel;
                student.CorrectAnswers = 0; // 新しいレベルの連続正解数を数え直す
            }
            await _context.SaveChangesAsync();
        }

        return MapToDto(user);
    }

    /// <summary>
    /// 子供アカウントを一時停止
    /// </summary>
    public async Task SuspendAsync(string childId, string parentId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == childId && u.ParentId == parentId && u.Role == UserRole.Child);

        if (user == null)
        {
            throw new InvalidOperationException("子供アカウントが見つかりません");
        }

        user.IsActive = false;
        await _userManager.UpdateAsync(user);
    }

    /// <summary>
    /// 子供アカウントを再開
    /// </summary>
    public async Task ActivateAsync(string childId, string parentId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == childId && u.ParentId == parentId && u.Role == UserRole.Child);

        if (user == null)
        {
            throw new InvalidOperationException("子供アカウントが見つかりません");
        }

        user.IsActive = true;
        await _userManager.UpdateAsync(user);
    }

    /// <summary>
    /// 子供アカウントを削除
    /// </summary>
    public async Task DeleteAsync(string childId, string parentId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == childId && u.ParentId == parentId && u.Role == UserRole.Child);

        if (user == null)
        {
            throw new InvalidOperationException("子供アカウントが見つかりません");
        }

        // Student削除（連鎖削除でLearningRecordsも削除される）。
        // 紐付いた学習者に加え、以前の不具合で作成時に作られた未使用の学習者（ParentUserId に子供のIDが入っている）も消す
        var students = await _context.Students
            .Where(s => s.Id == user.StudentId || s.ParentUserId == user.Id)
            .ToListAsync();
        _context.Students.RemoveRange(students);

        await _userManager.DeleteAsync(user);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 子供の学習統計を取得
    /// </summary>
    public async Task<ChildLearningStatsDto> GetLearningStatsAsync(string childId)
    {
        // 子供アカウントに紐付いた学習者（問題の回答やポイントが記録される方）
        var studentId = await _context.Users
            .Where(u => u.Id == childId)
            .Select(u => u.StudentId)
            .FirstOrDefaultAsync();
        var student = studentId == null ? null : await _context.Students.FindAsync(studentId.Value);

        if (student == null)
        {
            return new ChildLearningStatsDto();
        }

        // 過去7日間のアクティビティ
        var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-6);
        var recentActivity = await _context.LearningRecords
            .Where(r => r.StudentId == student.Id && r.SolvedAt >= sevenDaysAgo)
            .GroupBy(r => r.SolvedAt.Date)
            .Select(g => new DailyActivity
            {
                Date = g.Key,
                ProblemsCount = g.Count()
            })
            .OrderBy(a => a.Date)
            .ToListAsync();

        // 回答数・正解数は学習記録から数える（Student.CorrectAnswers はレベルアップ用の連続正解数のため使わない）
        var answered = await _context.LearningRecords.CountAsync(r => r.StudentId == student.Id);
        var correct = await _context.LearningRecords.CountAsync(r => r.StudentId == student.Id && r.IsCorrect);
        var accuracyRate = answered > 0
            ? Math.Round((decimal)correct / answered * 100, 1)
            : 0;

        return new ChildLearningStatsDto
        {
            TotalProblems = answered,
            CorrectAnswers = correct,
            AccuracyRate = accuracyRate,
            TotalPoints = student.TotalPoints,
            ConsecutiveDays = student.ConsecutiveDays,
            LastStudyDate = student.LastLoginAt,
            RecentActivity = recentActivity
        };
    }

    /// <summary>
    /// 子供アカウントのPINを検証
    /// </summary>
    public async Task<bool> VerifyPinAsync(string childId, string pin)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == childId && u.Role == UserRole.Child);

        if (user == null || user.PIN == null)
        {
            return false;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PIN, pin);
        var isSuccess = result == PasswordVerificationResult.Success;
        
        // 検証成功時は失敗カウンターをクリア
        if (isSuccess)
        {
            var attemptsKey = $"pin_attempts_{childId}";
            _cache.Remove(attemptsKey);
        }
        
        return isSuccess;
    }

    /// <summary>
    /// PINロックアウト状態を確認
    /// </summary>
    public Task<bool> IsLockedOutAsync(string childId)
    {
        var lockoutKey = $"pin_lockout_{childId}";
        return Task.FromResult(_cache.TryGetValue(lockoutKey, out _));
    }

    /// <summary>
    /// PIN失敗回数を記録
    /// </summary>
    public Task RecordFailedPinAttemptAsync(string childId)
    {
        var attemptsKey = $"pin_attempts_{childId}";
        var lockoutKey = $"pin_lockout_{childId}";

        // 初回は1、以降は既存値+1（以前はGetOrCreate直後にも加算しており、初回が2回分と数えられていた）
        var attempts = _cache.TryGetValue(attemptsKey, out int current) ? current + 1 : 1;
        _cache.Set(attemptsKey, attempts, TimeSpan.FromMinutes(5));

        if (attempts >= MaxPinAttempts)
        {
            _cache.Set(lockoutKey, true, PinLockoutDuration);
            _cache.Remove(attemptsKey);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// PIN失敗カウンターをクリア（ログイン成功時）
    /// </summary>
    public Task ClearFailedPinAttemptsAsync(string childId)
    {
        var attemptsKey = $"pin_attempts_{childId}";
        _cache.Remove(attemptsKey);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 同じPINを使用している兄弟がいるか確認
    /// </summary>
    public async Task<bool> HasDuplicatePinAsync(string parentId, string pin, string? excludeChildId = null)
    {
        var siblings = await _context.Users
            .Where(u => u.ParentId == parentId && u.Role == UserRole.Child && u.Id != excludeChildId)
            .ToListAsync();

        foreach (var sibling in siblings)
        {
            if (sibling.PIN != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(sibling, sibling.PIN, pin);
                if (result == PasswordVerificationResult.Success)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 子供アカウントにStudentIdを設定
    /// </summary>
    public async Task UpdateStudentIdAsync(string childId, int studentId)
    {
        var user = await _context.Users.FindAsync(childId);
        if (user == null || user.Role != UserRole.Child)
        {
            throw new InvalidOperationException("Child account not found");
        }

        user.StudentId = studentId;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// ApplicationUserをDTOにマッピング
    /// </summary>
    private static ChildAccountDto MapToDto(ApplicationUser user)
    {
        return new ChildAccountDto
        {
            Id = user.Id,
            Name = user.DisplayName,
            GradeLevel = user.GradeLevel ?? 1,
            AvatarUrl = user.AvatarUrl ?? string.Empty,
            IsActive = user.IsActive,
            StudentId = user.StudentId ?? 0,  // StudentIdを追加
            CreatedAt = user.CreatedAt
        };
    }
}
