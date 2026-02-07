# Research: 子供アカウント管理画面

**Feature**: 005-child-account-management  
**Phase**: 0 - Research & Decisions  
**Date**: 2026年2月7日

## Research Tasks

このドキュメントは、仕様書のTechnical Contextセクションで「NEEDS CLARIFICATION」とマークされた項目、および重要な技術選択の調査結果をまとめたものです。

---

## 1. PINハッシュ化とロックアウト実装

### Decision: ASP.NET Core IdentityのPasswordHasherを使用

**Rationale**:

- 既存のApplicationUserでASP.NET Core Identityを使用しており、PasswordHasher<TUser>が利用可能
- BCryptやPBKDF2を自前で実装するより、フレームワーク標準のセキュリティベストプラクティスに従う
- PINは4桁数字だが、ハッシュ化により平文保存を回避

**Implementation Approach**:

```csharp
// ApplicationUser.PIN のハッシュ化
var hasher = new PasswordHasher<ApplicationUser>();
user.PIN = hasher.HashPassword(user, plainTextPin);

// 検証
var result = hasher.VerifyHashedPassword(user, user.PIN, inputPin);
```

**Alternatives Considered**:

- **BCrypt**: 独自ライブラリ必要、ASP.NET Core標準ではない
- **SHA256**: 単純ハッシュはレインボーテーブル攻撃に脆弱
- **平文保存**: セキュリティリスク高、憲法違反

### Decision: PINロックアウトはメモリキャッシュで実装

**Rationale**:

- 5分間の一時的なロックアウトのため、永続化不要
- IMemoryCacheを使用してログイン失敗回数と最終失敗時刻を記録
- スケーラビリティが必要になればRedisに移行可能

**Implementation Approach**:

```csharp
// キャッシュキー: "pin_lockout:{userId}"
var lockoutKey = $"pin_lockout:{userId}";
var failedAttempts = _cache.Get<int>(lockoutKey);

if (failedAttempts >= 3)
{
    var lockoutTime = _cache.Get<DateTime>($"{lockoutKey}_time");
    if (DateTime.UtcNow < lockoutTime.AddMinutes(5))
        throw new AccountLockedException();
}
```

**Alternatives Considered**:

- **DBテーブル保存**: オーバーヘッド高、5分間の一時データには不適
- **Redis**: 小規模アプリには過剰、将来の拡張オプションとして保持

---

## 2. プリセットアバター管理

### Decision: DBマスタテーブル + 静的ファイル配信

**Rationale**:

- 10〜20種類のプリセットアバター画像を`wwwroot/avatars/`に配置
- `PresetAvatar`テーブルでID、ファイル名、表示名を管理
- ApplicationUser.AvatarUrlにはプリセットID（例：`preset:cat-01`）を保存
- 画像URL生成時に`/avatars/cat-01.png`に変換

**Implementation Approach**:

```csharp
public class PresetAvatar
{
    public int Id { get; set; }
    public string Name { get; set; } // "猫"
    public string FileName { get; set; } // "cat-01.png"
    public int DisplayOrder { get; set; }
}
```

**Seeding**:

```csharp
// AvatarSeeder.cs
modelBuilder.Entity<PresetAvatar>().HasData(
    new PresetAvatar { Id = 1, Name = "猫", FileName = "cat-01.png", DisplayOrder = 1 },
    new PresetAvatar { Id = 2, Name = "犬", FileName = "dog-01.png", DisplayOrder = 2 },
    // ... 10〜20種類
);
```

**Alternatives Considered**:

- **CDN/クラウドストレージ**: 小規模アプリには過剰、コスト増
- **Base64埋め込み**: ページサイズ肥大化、キャッシュ効率悪い
- **カスタムアップロード**: 仕様で明示的に除外（不適切画像リスク回避）

---

## 3. 学年管理とApplicationUser拡張

### Decision: ApplicationUserにGradeLevelプロパティ追加

**Rationale**:

- 既存のApplicationUserは子供アカウントを表現（Role=Child, PIN設定済み）
- 学年は1〜6年生の整数値（int）で保存
- バリデーション: [Range(1, 6)]属性で制約

**Implementation Approach**:

```csharp
public class ApplicationUser : IdentityUser
{
    public UserRole Role { get; set; }
    public string? PIN { get; set; } // ハッシュ化済み
    public string? ParentId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; } // "preset:cat-01" 形式

    // 新規追加
    [Range(1, 6)]
    public int? GradeLevel { get; set; } // 1〜6年生
    public bool IsActive { get; set; } = true; // 一時停止フラグ

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**Alternatives Considered**:

- **別テーブル（ChildProfile）**: 1:1関係の追加テーブルは過剰、ApplicationUser拡張で十分
- **Enumで学年管理**: 整数値の方がシンプル、将来の拡張（中学生対応）も容易

---

## 4. 子供ログイン画面のカード形式UI実装

### Decision: Blazorコンポーネント + CSS Grid

**Rationale**:

- `ChildAccountCard.razor`を再利用可能なコンポーネントとして実装
- CSS Gridでレスポンシブ対応（モバイル1列、タブレット2列、PC3〜4列）
- アバター画像 + 名前 + タップ可能なカード

**Implementation Approach**:

```razor
<!-- ChildAccountCard.razor -->
<div class="child-card" @onclick="OnCardClick">
    <img src="/avatars/@Avatar" alt="@Name" class="avatar-image" />
    <h3>@Name</h3>
</div>

@code {
    [Parameter] public string Name { get; set; }
    [Parameter] public string Avatar { get; set; }
    [Parameter] public EventCallback OnCardClick { get; set; }
}
```

**CSS Grid Layout**:

```css
.child-cards-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 1rem;
}

.child-card {
  cursor: pointer;
  border: 2px solid #ccc;
  border-radius: 12px;
  padding: 1rem;
  text-align: center;
}
```

**Alternatives Considered**:

- **Flexbox**: Gridの方が均等配置が容易
- **サードパーティUIライブラリ**: 子供向けカスタムデザインには不向き

---

## 5. 学習統計データの集計

### Decision: 既存のStudentモデルから集計クエリで取得

**Rationale**:

- Studentモデルには既にTotalProblems、CorrectAnswers、ConsecutiveDays等が存在
- LearningRecordsテーブルから過去7日間のアクティビティを集計
- リアルタイム集計でパフォーマンス問題が発生すれば、将来的にマテリアライズドビューを検討

**Implementation Approach**:

```csharp
public async Task<ChildLearningStatsDto> GetLearningStatsAsync(string childUserId)
{
    var student = await _context.Students
        .Include(s => s.LearningRecords)
        .FirstOrDefaultAsync(s => s.ParentUserId == childUserId);

    var stats = new ChildLearningStatsDto
    {
        TotalProblems = student.TotalProblems,
        CorrectAnswers = student.CorrectAnswers,
        AccuracyRate = student.TotalProblems > 0
            ? (decimal)student.CorrectAnswers / student.TotalProblems * 100
            : 0,
        ConsecutiveDays = student.ConsecutiveDays,
        LastStudyDate = student.LastLoginAt,
        RecentActivity = student.LearningRecords
            .Where(lr => lr.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .GroupBy(lr => lr.CreatedAt.Date)
            .Select(g => new DailyActivity { Date = g.Key, ProblemsCount = g.Count() })
            .ToList()
    };

    return stats;
}
```

**Alternatives Considered**:

- **別の統計テーブル**: 現時点では不要、将来の最適化オプション
- **キャッシュ**: 学習状況は頻繁に更新されるためキャッシュ効果低い

---

## 6. 子供アカウント上限（10件）の実装

### Decision: サービス層でバリデーション

**Rationale**:

- ChildAccountService.CreateAsync()内で保護者の既存子供アカウント数をカウント
- 10件以上の場合はBusinessRuleViolationException（カスタム例外）をスロー
- フロントエンドでも事前チェック（UX向上）

**Implementation Approach**:

```csharp
public async Task<ChildAccountDto> CreateAsync(string parentUserId, ChildAccountCreateDto dto)
{
    var childCount = await _context.Users
        .CountAsync(u => u.ParentId == parentUserId && u.Role == UserRole.Child);

    if (childCount >= 10)
        throw new BusinessRuleViolationException("子供アカウントの上限（10件）に達しています");

    // アカウント作成処理
}
```

**Alternatives Considered**:

- **DB制約**: 複雑なチェック制約は保守性低い
- **無制限**: 仕様で上限10件が明記されている

---

## 7. PIN忘れ時のリセットフロー

### Decision: 編集画面でPINフィールドを直接変更

**Rationale**:

- 保護者はすでに認証済み（管理画面アクセス可能）
- EditChildAccountフォームでPINフィールドを表示し、新しいPINを入力
- 保存時にハッシュ化して更新
- 複雑なリセットフロー（メール送信等）は不要

**Implementation Approach**:

```razor
<!-- EditChildAccount.razor -->
<EditForm Model="model" OnValidSubmit="HandleSubmit">
    <InputText @bind-Value="model.Name" />
    <InputNumber @bind-Value="model.GradeLevel" />

    <!-- PIN変更 -->
    <label>新しいPIN（変更する場合のみ入力）</label>
    <InputText @bind-Value="model.NewPIN" type="password" maxlength="4" />

    <button type="submit">保存</button>
</EditForm>
```

**Alternatives Considered**:

- **一時PIN発行**: 小学生向けアプリには複雑すぎる
- **メールリセット**: 子供はメールアドレスを持たない前提

---

## Summary of Key Decisions

| 項目            | 選択技術・方式                        | 理由                                   |
| --------------- | ------------------------------------- | -------------------------------------- |
| PINハッシュ化   | ASP.NET Core IdentityのPasswordHasher | フレームワーク標準、既存認証基盤と統合 |
| PINロックアウト | IMemoryCache                          | 5分間の一時データ、永続化不要          |
| アバター管理    | 静的ファイル + PresetAvatarマスタ     | シンプル、カスタムアップロード不要     |
| 学年管理        | ApplicationUser.GradeLevelプロパティ  | 既存モデル拡張、1〜6の整数値           |
| カード形式UI    | Blazor + CSS Grid                     | レスポンシブ、子供に優しいビジュアル   |
| 学習統計        | Studentモデルから集計クエリ           | 既存データ活用、リアルタイム集計       |
| アカウント上限  | サービス層バリデーション（10件）      | ビジネスルール、例外スロー             |
| PIN忘れ対応     | 編集画面で直接変更                    | シンプル、保護者認証済み               |

---

## Open Questions / Future Considerations

1. **スケーラビリティ**: 将来的にRedis（ロックアウト）やマテリアライズドビュー（統計）が必要か？
   - **決定**: 現時点では不要。パフォーマンス問題が発生した時点で検討。

2. **アバター画像の追加方法**: 新しいプリセットを追加する際の運用フロー
   - **決定**: マイグレーションでAvatarSeederを更新。管理画面からの追加は将来的な拡張オプション。

3. **複数保護者対応**: 1家庭に複数の保護者アカウントがある場合の共有
   - **決定**: 現状は1保護者前提（ParentIdで1:N）。将来的にParentChildマッピングテーブルでM:N対応可能。

---

**Status**: ✅ All research tasks completed. Proceed to Phase 1 (Data Model & Contracts).
