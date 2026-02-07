# Data Model: Authentication UI Pages

**Date**: 2026年2月7日  
**Feature**: [spec.md](spec.md) | [plan.md](plan.md) | [research.md](research.md)

## Overview

このドキュメントでは、004-auth-ui-pagesで必要なデータモデルとデータベーススキーマを定義します。

---

## 1. Entity: ApplicationUser (既存の更新)

既存の`ApplicationUser`エンティティは変更不要ですが、ASP.NET Core Identityの標準フィールドを確認します。

### Fields

| Field              | Type            | Description                | Constraints               |
| ------------------ | --------------- | -------------------------- | ------------------------- |
| Id                 | string          | ユーザーID（GUID）         | PK, NOT NULL              |
| Email              | string          | メールアドレス             | UNIQUE, NOT NULL, MAX 256 |
| NormalizedEmail    | string          | 正規化されたメールアドレス | INDEXED                   |
| EmailConfirmed     | bool            | メール確認済みフラグ       | DEFAULT false             |
| PasswordHash       | string          | ハッシュ化されたパスワード | NOT NULL                  |
| SecurityStamp      | string          | セキュリティスタンプ       | NOT NULL                  |
| ConcurrencyStamp   | string          | 並行制御用                 | NOT NULL                  |
| DisplayName        | string          | 表示名                     | NOT NULL, MAX 50          |
| UserName           | string          | ユーザー名（メールと同じ） | UNIQUE, NOT NULL          |
| NormalizedUserName | string          | 正規化されたユーザー名     | INDEXED                   |
| AccessFailedCount  | int             | ログイン失敗回数           | DEFAULT 0                 |
| LockoutEnabled     | bool            | ロックアウト有効フラグ     | DEFAULT true              |
| LockoutEnd         | DateTimeOffset? | ロックアウト終了時刻       | NULLABLE                  |

### Indexes

- `IX_Users_NormalizedEmail` (NormalizedEmail)
- `IX_Users_NormalizedUserName` (NormalizedUserName)

### Notes

- ASP.NET Core Identityの標準フィールドを使用
- `EmailConfirmed`は将来のメール確認機能用（Phase 2以降）
- `DisplayName`は既存カスタムフィールド（保護者の表示名）

---

## 2. Entity: PasswordResetToken (新規)

パスワードリセットトークンの管理用エンティティ。ASP.NET Core Identityのトークン機能と併用。

### Fields

| Field     | Type      | Description                      | Constraints             |
| --------- | --------- | -------------------------------- | ----------------------- |
| Id        | int       | トークンID                       | PK, IDENTITY            |
| UserId    | string    | ユーザーID（ApplicationUser.Id） | FK, NOT NULL, INDEXED   |
| TokenHash | string    | ハッシュ化されたトークン値       | NOT NULL, UNIQUE        |
| CreatedAt | DateTime  | トークン作成日時                 | NOT NULL                |
| ExpiresAt | DateTime  | トークン有効期限                 | NOT NULL                |
| IsUsed    | bool      | 使用済みフラグ                   | DEFAULT false           |
| UsedAt    | DateTime? | 使用日時                         | NULLABLE                |
| IpAddress | string?   | リクエスト元IPアドレス           | NULLABLE, MAX 45 (IPv6) |

### Relationships

- **ApplicationUser** (1:N): 1人のユーザーは複数のリセットトークンを持つ

### Indexes

- `IX_PasswordResetTokens_UserId` (UserId)
- `IX_PasswordResetTokens_TokenHash` (TokenHash, UNIQUE)
- `IX_PasswordResetTokens_ExpiresAt` (ExpiresAt) - 期限切れトークンのクリーンアップ用

### Validation Rules

- `ExpiresAt`は`CreatedAt`の24時間後
- `IsUsed`がtrueの場合、`UsedAt`は必須
- `TokenHash`はSHA256ハッシュ（64文字）

### Entity Class

```csharp
public class PasswordResetToken
{
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    [Required]
    [StringLength(64)] // SHA256 hash
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime? UsedAt { get; set; }

    [StringLength(45)] // IPv6 max length
    public string? IpAddress { get; set; }
}
```

---

## 3. Database Migration

### Migration: AddPasswordResetTokens

```csharp
public partial class AddPasswordResetTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PasswordResetTokens",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false),
                TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_PasswordResetTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PasswordResetTokens_UserId",
            table: "PasswordResetTokens",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_PasswordResetTokens_TokenHash",
            table: "PasswordResetTokens",
            column: "TokenHash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PasswordResetTokens_ExpiresAt",
            table: "PasswordResetTokens",
            column: "ExpiresAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PasswordResetTokens");
    }
}
```

### Migration Command

```bash
cd backend/src/GamifiedMathDrill.Api
dotnet ef migrations add AddPasswordResetTokens --project ../GamifiedMathDrill.Infrastructure
dotnet ef database update
```

---

## 4. DbContext Configuration

### ApplicationDbContext.cs Update

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Existing DbSets...

    // NEW: Password reset tokens
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Existing configurations...

        // NEW: PasswordResetToken configuration
        builder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TokenHash)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
        });
    }
}
```

---

## 5. DTOs (Data Transfer Objects)

### RegisterRequest.cs (新規)

```csharp
namespace GamifiedMathDrill.Api.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "表示名は必須です")]
    [StringLength(50, ErrorMessage = "表示名は50文字以内で入力してください")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "パスワードは必須です")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "パスワードは8文字以上100文字以内で入力してください")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
        ErrorMessage = "パスワードは大文字、小文字、数字、記号を各1文字以上含む必要があります")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "パスワード確認は必須です")]
    [Compare(nameof(Password), ErrorMessage = "パスワードが一致しません")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

### RegisterResponse.cs (新規)

```csharp
namespace GamifiedMathDrill.Api.DTOs;

public class RegisterResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? UserId { get; set; }
}
```

### ForgotPasswordRequest.cs (新規)

```csharp
namespace GamifiedMathDrill.Api.DTOs;

public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    public string Email { get; set; } = string.Empty;
}
```

### ForgotPasswordResponse.cs (新規)

```csharp
namespace GamifiedMathDrill.Api.DTOs;

public class ForgotPasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "メールアドレスが登録されている場合、パスワードリセットのメールをお送りしました。";
}
```

### ResetPasswordRequest.cs (新規)

```csharp
namespace GamifiedMathDrill.Api.DTOs;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "トークンは必須です")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "新しいパスワードは必須です")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "パスワードは8文字以上100文字以内で入力してください")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
        ErrorMessage = "パスワードは大文字、小文字、数字、記号を各1文字以上含む必要があります")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "パスワード確認は必須です")]
    [Compare(nameof(NewPassword), ErrorMessage = "パスワードが一致しません")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

### ResetPasswordResponse.cs (新規)

```csharp
namespace GamifiedMathDrill.Api.DTOs;

public class ResetPasswordResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
```

### LoginRequest.cs (既存の更新)

```csharp
namespace GamifiedMathDrill.Api.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "パスワードは必須です")]
    public string Password { get; set; } = string.Empty;

    // NEW: Remember me flag
    public bool RememberMe { get; set; } = false;
}
```

### LoginResponse.cs (既存の更新)

```csharp
namespace GamifiedMathDrill.Api.DTOs;

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? UserId { get; set; }
    public string? DisplayName { get; set; }
    public string? Role { get; set; }

    // NEW: Token expiration info
    public DateTime? ExpiresAt { get; set; }

    public string? ErrorMessage { get; set; }
}
```

---

## 6. Repository Pattern

### IPasswordResetTokenRepository.cs (新規)

```csharp
namespace GamifiedMathDrill.Core.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);
    Task<PasswordResetToken?> GetByIdAsync(int id);
    Task<List<PasswordResetToken>> GetActiveTokensForUserAsync(string userId);
    Task<PasswordResetToken> CreateAsync(PasswordResetToken token);
    Task UpdateAsync(PasswordResetToken token);
    Task<int> DeleteExpiredTokensAsync();
}
```

### PasswordResetTokenRepository.cs (新規)

```csharp
namespace GamifiedMathDrill.Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordResetTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<PasswordResetToken?> GetByIdAsync(int id)
    {
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<PasswordResetToken>> GetActiveTokensForUserAsync(string userId)
    {
        return await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<PasswordResetToken> CreateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task UpdateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task<int> DeleteExpiredTokensAsync()
    {
        var expiredTokens = await _context.PasswordResetTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        _context.PasswordResetTokens.RemoveRange(expiredTokens);
        return await _context.SaveChangesAsync();
    }
}
```

---

## 7. Entity Relationships Diagram

```
┌─────────────────────────┐
│   ApplicationUser       │
│  (ASP.NET Identity)     │
├─────────────────────────┤
│ Id (PK)                 │
│ Email (UNIQUE)          │
│ PasswordHash            │
│ DisplayName             │
│ EmailConfirmed          │
│ ...                     │
└───────────┬─────────────┘
            │
            │ 1:N
            │
            ▼
┌─────────────────────────┐
│ PasswordResetToken      │
├─────────────────────────┤
│ Id (PK)                 │
│ UserId (FK)             │◄──┐
│ TokenHash (UNIQUE)      │   │
│ CreatedAt               │   │
│ ExpiresAt               │   │
│ IsUsed                  │   │
│ UsedAt                  │   │
│ IpAddress               │   │
└─────────────────────────┘   │
                              │
                        Foreign Key
```

---

## 8. Data Validation Summary

### Email Validation

- Format: RFC 5322準拠（ASP.NET Core標準）
- Max length: 256文字
- Uniqueness: データベースレベルで保証

### Password Validation

- Length: 8-100文字
- Complexity:
  - 小文字: 1文字以上
  - 大文字: 1文字以上
  - 数字: 1文字以上
  - 記号: 1文字以上 (@$!%\*?&)
- Hashing: ASP.NET Core Identity (PBKDF2)

### Display Name Validation

- Max length: 50文字
- Required: Yes
- Allowed characters: 制限なし（Unicode対応）

### Token Validation

- Format: SHA256ハッシュ（64文字）
- Uniqueness: データベースレベルで保証
- Expiration: 24時間
- Single use: IsUsedフラグで制御

---

## Summary

このデータモデルでは以下を定義しました：

1. **既存エンティティ**: `ApplicationUser`（ASP.NET Core Identity標準フィールドを使用）
2. **新規エンティティ**: `PasswordResetToken`（トークン管理用）
3. **DTOs**: 6個のリクエスト/レスポンスDTO
4. **Repository**: `IPasswordResetTokenRepository`と実装
5. **Migration**: `AddPasswordResetTokens`マイグレーション
6. **Validation**: メール、パスワード、表示名、トークンのバリデーションルール

次のフェーズでAPI契約（OpenAPI/Swagger仕様）を定義します。
