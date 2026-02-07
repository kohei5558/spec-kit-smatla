# Data Model: 子供アカウント管理画面

**Feature**: 005-child-account-management  
**Phase**: 1 - Data Model & Contracts  
**Date**: 2026年2月7日

## Entity Relationship Diagram

```
┌──────────────────────┐
│  ApplicationUser     │
│  (ASP.NET Identity)  │
├──────────────────────┤
│ Id (PK)              │ ← 保護者アカウント
│ Email                │
│ PasswordHash         │
│ Role = Parent        │
│ DisplayName          │
│ CreatedAt            │
└──────────────────────┘
         │ 1
         │ ParentId
         │
         │ N
┌──────────────────────┐       ┌─────────────────────┐
│  ApplicationUser     │───────│  PresetAvatar       │
│  (子供アカウント)     │   N:1 ├─────────────────────┤
├──────────────────────┤       │ Id (PK)             │
│ Id (PK)              │       │ Name                │
│ Role = Child         │       │ FileName            │
│ ParentId (FK)        │       │ DisplayOrder        │
│ DisplayName          │       └─────────────────────┘
│ PIN (hashed)         │
│ AvatarUrl            │ ← "preset:{PresetAvatar.Id}"
│ GradeLevel (1-6)     │
│ IsActive             │ ← true=有効, false=停止
│ CreatedAt            │
└──────────────────────┘
         │ 1:1 (ParentUserId)
         │
         ▼
┌──────────────────────┐
│  Student             │
├──────────────────────┤
│ Id (PK)              │
│ Name                 │ ← ApplicationUser.DisplayNameと同期
│ ParentUserId (FK)    │ ← ApplicationUser.Idを参照
│ TotalPoints          │
│ TotalProblems        │
│ CorrectAnswers       │
│ ConsecutiveDays      │
│ LastLoginAt          │
│ AvatarUrl            │ ← ApplicationUser.AvatarUrlと同期
└──────────────────────┘
         │ 1
         │
         │ N
┌──────────────────────┐
│  LearningRecord      │
├──────────────────────┤
│ Id (PK)              │
│ StudentId (FK)       │
│ ProblemId            │
│ IsCorrect            │
│ CreatedAt            │
└──────────────────────┘
```

---

## Entity Specifications

### 1. ApplicationUser (既存拡張)

**Purpose**: ASP.NET Core Identityのユーザーモデル。保護者と子供アカウントの両方を表現。

| Field        | Type            | Constraints                      | Description                      |
| ------------ | --------------- | -------------------------------- | -------------------------------- |
| Id           | string (PK)     | Required                         | ASP.NET Identity標準ID（GUID）   |
| Email        | string          | Unique, Required (保護者のみ)    | 保護者用メールアドレス           |
| PasswordHash | string          | Required (保護者のみ)            | 保護者用パスワード（ハッシュ化） |
| Role         | UserRole (enum) | Required                         | Parent or Child                  |
| DisplayName  | string          | Required, MaxLength(50)          | 表示名（子供の場合は名前）       |
| PIN          | string?         | Optional, 4桁数字（ハッシュ化）  | 子供用PINコード（保護者はnull）  |
| ParentId     | string? (FK)    | Required (子供のみ)              | 親ApplicationUser.Id             |
| AvatarUrl    | string?         | Optional, MaxLength(200)         | "preset:{id}" 形式またはURL      |
| GradeLevel   | int?            | Range(1, 6), Required (子供のみ) | 学年（1〜6年生）                 |
| IsActive     | bool            | Required, Default=true           | アカウント状態（停止フラグ）     |
| CreatedAt    | DateTime        | Required                         | 作成日時（UTC）                  |

**New Fields**:

- `GradeLevel`: 子供の学年管理
- `IsActive`: 一時停止機能（User Story 4）

**Validation Rules**:

```csharp
// 子供アカウント作成時
if (Role == UserRole.Child)
{
    // PIN必須
    if (string.IsNullOrEmpty(PIN))
        throw new ValidationException("PINは必須です");

    // 学年必須
    if (!GradeLevel.HasValue || GradeLevel < 1 || GradeLevel > 6)
        throw new ValidationException("学年は1〜6年生で指定してください");

    // ParentId必須
    if (string.IsNullOrEmpty(ParentId))
        throw new ValidationException("親アカウントが指定されていません");

    // Email不要（nullまたは空）
    Email = null;
}
```

---

### 2. PresetAvatar (新規)

**Purpose**: システムが提供するプリセットアバター画像のマスタデータ。

| Field        | Type     | Constraints              | Description                  |
| ------------ | -------- | ------------------------ | ---------------------------- |
| Id           | int (PK) | Required, Identity       | アバターID                   |
| Name         | string   | Required, MaxLength(50)  | 表示名（例：「猫」「犬」）   |
| FileName     | string   | Required, MaxLength(100) | ファイル名（例：cat-01.png） |
| DisplayOrder | int      | Required                 | 表示順序                     |
| IsActive     | bool     | Required, Default=true   | 有効/無効フラグ              |

**Seed Data Example**:

```csharp
new PresetAvatar { Id = 1, Name = "猫", FileName = "cat-01.png", DisplayOrder = 1 },
new PresetAvatar { Id = 2, Name = "犬", FileName = "dog-01.png", DisplayOrder = 2 },
new PresetAvatar { Id = 3, Name = "パンダ", FileName = "panda-01.png", DisplayOrder = 3 },
new PresetAvatar { Id = 4, Name = "うさぎ", FileName = "rabbit-01.png", DisplayOrder = 4 },
new PresetAvatar { Id = 5, Name = "ライオン", FileName = "lion-01.png", DisplayOrder = 5 },
// ... 10〜20種類
```

**File Structure**:

```
frontend/wwwroot/avatars/
├── cat-01.png
├── dog-01.png
├── panda-01.png
├── rabbit-01.png
├── lion-01.png
└── ... (10〜20 files)
```

---

### 3. Student (既存、同期保持)

**Purpose**: 子供の学習データを管理。ApplicationUserとは1:1の関係。

**既存フィールド**:

- Id, Name, ParentUserId, TotalPoints, TotalProblems, CorrectAnswers, ConsecutiveDays, LastLoginAt, AvatarUrl

**同期ルール**:

- ApplicationUser作成時にStudentレコードも自動作成
- DisplayName変更 → Student.Name更新
- AvatarUrl変更 → Student.AvatarUrl更新
- IsActive=false → Studentには影響なし（学習データは保持）

**Synchronization Logic**:

```csharp
// ChildAccountService.CreateAsync()
var user = new ApplicationUser { ... };
await _userManager.CreateAsync(user);

var student = new Student
{
    Name = user.DisplayName,
    ParentUserId = user.Id,
    AvatarUrl = user.AvatarUrl
};
await _context.Students.AddAsync(student);
```

---

## Value Objects & DTOs

### ChildAccountCreateDto

```csharp
public class ChildAccountCreateDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; }

    [Required, Range(1, 6)]
    public int GradeLevel { get; set; }

    [Required]
    public int PresetAvatarId { get; set; }

    [Required, StringLength(4, MinimumLength = 4)]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "PINは4桁の数字で入力してください")]
    public string PIN { get; set; }
}
```

### ChildAccountUpdateDto

```csharp
public class ChildAccountUpdateDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; }

    [Required, Range(1, 6)]
    public int GradeLevel { get; set; }

    [Required]
    public int PresetAvatarId { get; set; }

    // PIN変更時のみ入力
    [StringLength(4, MinimumLength = 4)]
    [RegularExpression(@"^\d{4}$")]
    public string? NewPIN { get; set; }
}
```

### ChildAccountDto (Response)

```csharp
public class ChildAccountDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int GradeLevel { get; set; }
    public string AvatarUrl { get; set; } // "/avatars/cat-01.png"
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // 学習統計サマリー（詳細表示用）
    public ChildLearningStatsDto? LearningStats { get; set; }
}
```

### ChildLearningStatsDto

```csharp
public class ChildLearningStatsDto
{
    public int TotalProblems { get; set; }
    public int CorrectAnswers { get; set; }
    public decimal AccuracyRate { get; set; } // パーセント表示
    public int TotalPoints { get; set; }
    public int ConsecutiveDays { get; set; }
    public DateTime? LastStudyDate { get; set; }

    // 過去7日間のアクティビティ
    public List<DailyActivity> RecentActivity { get; set; }
}

public class DailyActivity
{
    public DateTime Date { get; set; }
    public int ProblemsCount { get; set; }
}
```

---

## Database Migrations

### Migration 1: AddChildAccountManagementFields

```csharp
public partial class AddChildAccountManagementFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ApplicationUserにGradeLevel追加
        migrationBuilder.AddColumn<int>(
            name: "GradeLevel",
            table: "AspNetUsers",
            nullable: true);

        // ApplicationUserにIsActive追加（デフォルトtrue）
        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "AspNetUsers",
            nullable: false,
            defaultValue: true);

        // PresetAvatarテーブル作成
        migrationBuilder.CreateTable(
            name: "PresetAvatars",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(maxLength: 50, nullable: false),
                FileName = table.Column<string>(maxLength: 100, nullable: false),
                DisplayOrder = table.Column<int>(nullable: false),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PresetAvatars", x => x.Id);
            });

        // PresetAvatarデータシード（AvatarSeeder経由）
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PresetAvatars");
        migrationBuilder.DropColumn(name: "GradeLevel", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "IsActive", table: "AspNetUsers");
    }
}
```

---

## State Transitions

### 子供アカウントのライフサイクル

```
   [作成]
     │
     ▼
┌─────────┐  停止操作   ┌─────────┐
│ Active  │─────────────>│ Inactive│
│(有効)   │             │(停止中) │
└─────────┘<─────────────└─────────┘
     ▲        再開操作        │
     │                       │
     └───────[削除]──────────┘
                │
                ▼
           (物理削除)
```

**State Rules**:

- **Active**: 子供がログイン可能、学習データ記録
- **Inactive**: ログイン不可、学習データ保持、保護者は閲覧可能
- **削除**: ApplicationUserとStudentを物理削除、LearningRecord等も連鎖削除

---

## Indexing Strategy

```sql
-- 子供アカウント一覧表示の高速化
CREATE INDEX IX_AspNetUsers_ParentId_Role ON AspNetUsers(ParentId, Role)
WHERE Role = 'Child';

-- PresetAvatar表示順ソート
CREATE INDEX IX_PresetAvatars_DisplayOrder ON PresetAvatars(DisplayOrder)
WHERE IsActive = true;

-- 学習統計の集計高速化（既存インデックス活用）
-- IX_Students_ParentUserId は既存想定
-- IX_LearningRecords_StudentId_CreatedAt は既存想定
```

---

## Data Validation Summary

| Validation Rule  | Location | Error Message                                                  |
| ---------------- | -------- | -------------------------------------------------------------- |
| PIN 4桁数字      | DTO      | "PINは4桁の数字で入力してください"                             |
| 学年 1〜6        | DTO      | "学年は1〜6年生で指定してください"                             |
| 名前必須         | DTO      | "名前は必須です"                                               |
| 重複名前チェック | Service  | "同じ名前の子供アカウントが既に存在します"                     |
| アカウント上限   | Service  | "子供アカウントの上限（10件）に達しています"                   |
| PINロックアウト  | Service  | "PINの入力に3回失敗しました。5分後に再試行してください"        |
| 停止中ログイン   | Service  | "このアカウントは現在使用できません。保護者に相談してください" |

---

## Performance Considerations

1. **子供アカウント一覧取得**: ParentId+Roleの複合インデックスで最適化
2. **学習統計集計**: 初回実装ではリアルタイム集計、パフォーマンス問題があればマテリアライズドビュー検討
3. **PINロックアウト**: IMemoryCacheで高速アクセス、5分後自動削除
4. **アバター画像**: 静的ファイル配信、ブラウザキャッシュ活用

---

**Status**: ✅ Data model completed. Proceed to API contracts definition.
