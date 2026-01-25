# Data Model: 保護者管理画面と実物景品交換システム

**Feature**: 003-parent-admin-rewards | **Date**: 2026-01-25  
**Purpose**: Phase 1 - データ構造、状態管理、エンティティ関係を定義

## Entities

### 新規エンティティ

#### ApplicationUser (拡張)

ASP.NET Core Identity の IdentityUser を拡張し、保護者と子供の両方を管理。

| プロパティ   | 型       | 説明                                 | 必須 |
| ------------ | -------- | ------------------------------------ | ---- |
| Id           | string   | ユーザーID（主キー、GUID）           | ✓    |
| UserName     | string   | ユーザー名（メールまたは子供の名前） | ✓    |
| Email        | string   | メールアドレス（保護者のみ）         | △    |
| PasswordHash | string   | パスワードハッシュ（保護者のみ）     | △    |
| Role         | UserRole | ロール（Parent/Child）               | ✓    |
| PIN          | string?  | PIN（子供のみ、ハッシュ化、4桁）     |      |
| ParentId     | string?  | 保護者ID（子供の場合のみ、外部キー） |      |
| DisplayName  | string   | 表示名                               | ✓    |
| AvatarUrl    | string?  | アバター画像URL                      |      |
| CreatedAt    | DateTime | アカウント作成日時                   | ✓    |
| IsActive     | bool     | アクティブフラグ（論理削除用）       | ✓    |

**Navigation Properties**:

- `Parent`: ApplicationUser（保護者への参照、子供の場合のみ）
- `Children`: ICollection<ApplicationUser>（子供一覧、保護者の場合のみ）
- `LearningRecords`: ICollection<LearningRecord>（学習記録、子供の場合のみ）
- `ExchangeRequests`: ICollection<ExchangeRequest>（交換申請、子供の場合のみ）

**Location**: `backend/src/GamifiedMathDrill.Infrastructure/Identity/ApplicationUser.cs`

**Indexes**:

- `Email` (Unique, for Parent)
- `ParentId` (for Child)
- `Role`

---

#### UserRole (Enum)

ユーザーのロールを定義。

| 値  | 名前   | 説明   |
| --- | ------ | ------ |
| 0   | Parent | 保護者 |
| 1   | Child  | 子供   |

**Location**: `backend/src/GamifiedMathDrill.Core/Models/UserRole.cs`

---

#### Reward (拡張)

既存の Reward エンティティを拡張し、実物景品対応。

**既存プロパティ**:
| プロパティ | 型 | 説明 | 既存 |
|-----------------|-----------------|--------------------------------|------|
| Id | int | 景品ID（主キー） | ✓ |
| Name | string | 景品名 | ✓ |
| Description | string? | 説明 | ✓ |
| RequiredPoints | int | 必要ポイント | ✓ |
| Category | RewardCategory | カテゴリ（Badge/Avatar/etc） | ✓ |
| ImageUrl | string? | 画像URL | ✓ |

**新規プロパティ**:
| プロパティ | 型 | 説明 | 必須 |
|-----------------|-----------------|--------------------------------|------|
| Stock | int? | 在庫数（null=無制限） | |
| IsPhysical | bool | 実物景品フラグ | ✓ |
| CreatedBy | string | 作成者（保護者ID） | ✓ |
| CreatedAt | DateTime | 作成日時 | ✓ |
| UpdatedAt | DateTime? | 更新日時 | |
| IsActive | bool | 有効フラグ | ✓ |
| RowVersion | byte[] | 楽観的ロック用 | ✓ |

**Navigation Properties**:

- `Creator`: ApplicationUser（作成した保護者）
- `ExchangeRequests`: ICollection<ExchangeRequest>（交換申請一覧）

**Location**: `backend/src/GamifiedMathDrill.Core/Models/Reward.cs`

**Indexes**:

- `CreatedBy`
- `IsActive`
- `IsPhysical`

**Constraints**:

- `Stock >= 0` (CHECK constraint)
- `RequiredPoints > 0` (CHECK constraint)

---

#### RewardCategory (Enum - 更新)

景品カテゴリを実物景品向けに更新。

| 値  | 名前       | 説明               | 変更 |
| --- | ---------- | ------------------ | ---- |
| 0   | Snack      | 駄菓子             | 新規 |
| 1   | Card       | カード（ポケカ等） | 新規 |
| 2   | Toy        | おもちゃ           | 新規 |
| 3   | Stationery | 文房具             | 新規 |
| 4   | Book       | 本・漫画           | 新規 |
| 5   | Other      | その他             | 新規 |

**Note**: 既存の Badge/Avatar/Character は削除

**Location**: `backend/src/GamifiedMathDrill.Core/Models/RewardCategory.cs`

---

#### ExchangeRequest (AcquiredReward を拡張)

既存の AcquiredReward を ExchangeRequest にリネームし、申請・承認フロー対応。

**既存プロパティ**:
| プロパティ | 型 | 説明 | 既存 |
|-----------------|----------|--------------------------|------|
| Id | int | 申請ID（主キー） | ✓ |
| StudentId | int | 子供ID（外部キー） | ✓ |
| RewardId | int | 景品ID（外部キー） | ✓ |
| AcquiredAt | DateTime | 獲得日時（旧フィールド） | ✓ |

**新規/変更プロパティ**:
| プロパティ | 型 | 説明 | 必須 |
|------------------|------------------|--------------------------------|------|
| Status | ExchangeStatus | ステータス（Pending/Approved...）| ✓ |
| RequestedAt | DateTime | 申請日時（AcquiredAt をリネーム）| ✓ |
| ApprovedAt | DateTime? | 承認日時 | |
| RejectedAt | DateTime? | 却下日時 | |
| CancelledAt | DateTime? | キャンセル日時 | |
| ApprovedBy | string? | 承認者（保護者ID） | |
| RejectionReason | string? | 却下理由（最大500文字） | |
| ParentNote | string? | 保護者メモ | |

**Navigation Properties**:

- `Student`: ApplicationUser（子供）
- `Reward`: Reward（景品）
- `Approver`: ApplicationUser（承認した保護者）

**Location**: `backend/src/GamifiedMathDrill.Core/Models/ExchangeRequest.cs`

**Indexes**:

- `StudentId`
- `Status`
- `RequestedAt`
- Composite: `(Status, RequestedAt)` for dashboard queries

---

#### ExchangeStatus (Enum)

交換申請のステータス。

| 値  | 名前      | 説明                         |
| --- | --------- | ---------------------------- |
| 0   | Pending   | 申請中（初期状態）           |
| 1   | Approved  | 承認済み（実物渡し済み）     |
| 2   | Rejected  | 却下（ポイント返却済み）     |
| 3   | Cancelled | キャンセル（子供が取り消し） |

**Location**: `backend/src/GamifiedMathDrill.Core/Models/ExchangeStatus.cs`

---

#### Student (既存 - 拡張不要)

既存の Student エンティティは ApplicationUser に統合。

**Migration Note**:

- 既存の Student データを ApplicationUser（Child ロール）に移行
- StudentId を ApplicationUser.Id に置き換え
- LearningRecord.StudentId を string 型に変更

---

## State Transitions

### 交換申請のステータス遷移

```
[初期状態]
    ↓
[Pending] ──────────────┐
    │                  │
    │ 保護者が承認      │ 子供がキャンセル
    ↓                  ↓
[Approved]        [Cancelled]
    (終了)            (終了)

[Pending]
    │ 保護者が却下
    ↓
[Rejected]
    (終了)
```

**許可される遷移**:
| From | To | 条件 | アクション |
|-----------|------------|------------------------|---------------------------------|
| Pending | Approved | 保護者が承認 | ポイント消費、在庫減少 |
| Pending | Rejected | 保護者が却下 | （在庫は申請時に既に減少済み） |
| Pending | Cancelled | 子供がキャンセル | 在庫を戻す |
| Approved | - | 変更不可 | - |
| Rejected | - | 変更不可 | - |
| Cancelled | - | 変更不可 | - |

---

## Data Flow

### 認証フロー

```
[ログイン画面]
    │
    ├─ 保護者ログイン: Email + Password
    │   ↓
    │  [ASP.NET Core Identity で認証]
    │   ↓
    │  [JWT トークン発行（Role=Parent）]
    │   ↓
    │  [保護者ダッシュボード]
    │
    └─ 子供ログイン: 名前選択 + PIN
        ↓
       [カスタム認証（PIN 検証）]
        ↓
       [JWT トークン発行（Role=Child）]
        ↓
       [子供用ホーム画面]
```

---

### 景品登録フロー

```
[保護者ダッシュボード]
    ↓
[景品管理画面] → [新規景品追加]
    ↓
[景品フォーム入力]
    - 名前、説明、必要ポイント
    - 在庫数、カテゴリ
    - 画像アップロード（IBrowserFile）
    ↓
[POST /api/rewards]
    ↓
[RewardService.CreateReward()]
    - 画像を wwwroot/uploads/rewards/ に保存
    - Reward エンティティを DB に保存
    - CreatedBy に保護者ID を設定
    ↓
[景品一覧に表示]
```

---

### 交換申請フロー

```
[子供: 景品一覧画面]
    ↓
[景品カード選択]
    ↓
[バリデーション]
    - ポイント不足チェック
    - 在庫チェック
    ↓
[交換確認ダイアログ]
    ↓
[POST /api/exchange-requests]
    ↓
[ExchangeRequestService.CreateRequest()]
    - トランザクション開始
    - 在庫デクリメント（楽観的ロック）
    - ExchangeRequest を作成（Status=Pending）
    - トランザクションコミット
    ↓
[申請完了メッセージ]
    ↓
[申請一覧に表示（Status=申請中）]
```

---

### 承認フロー

```
[保護者: ダッシュボード]
    │
    ├─ 未承認バッジ表示（ポーリング: 30秒間隔）
    │
    ↓
[交換申請一覧]
    ↓
[申請詳細を選択]
    - 子供の名前
    - 景品名、必要ポイント
    - 申請日時
    ↓
[承認 / 却下 選択]
    │
    ├─ 承認
    │   ↓
    │  [PUT /api/exchange-requests/{id}/approve]
    │   ↓
    │  [ExchangeRequestService.ApproveRequest()]
    │   - トランザクション開始
    │   - ポイント再チェック
    │   - 子供のポイント消費
    │   - Status を Approved に変更
    │   - ApprovedAt, ApprovedBy を設定
    │   - トランザクションコミット
    │   ↓
    │  [承認完了メッセージ]
    │
    └─ 却下
        ↓
       [PUT /api/exchange-requests/{id}/reject]
        ↓
       [ExchangeRequestService.RejectRequest()]
        - Status を Rejected に変更
        - RejectedAt, RejectionReason を設定
        - 在庫を戻す
        ↓
       [却下完了メッセージ]
```

---

## Database Schema Changes

### Migration Steps

1. **Identity スキーマ追加**

   ```sql
   CREATE TABLE AspNetUsers (
       Id NVARCHAR(450) PRIMARY KEY,
       UserName NVARCHAR(256),
       Email NVARCHAR(256),
       PasswordHash NVARCHAR(MAX),
       Role INT NOT NULL,
       PIN NVARCHAR(256),
       ParentId NVARCHAR(450),
       DisplayName NVARCHAR(100),
       AvatarUrl NVARCHAR(500),
       CreatedAt DATETIME2 NOT NULL,
       IsActive BIT NOT NULL,
       FOREIGN KEY (ParentId) REFERENCES AspNetUsers(Id)
   );
   ```

2. **Rewards テーブル拡張**

   ```sql
   ALTER TABLE Rewards
   ADD Stock INT NULL,
       IsPhysical BIT NOT NULL DEFAULT 0,
       CreatedBy NVARCHAR(450) NOT NULL,
       CreatedAt DATETIME2 NOT NULL,
       UpdatedAt DATETIME2,
       IsActive BIT NOT NULL DEFAULT 1,
       RowVersion ROWVERSION;

   ALTER TABLE Rewards
   ADD CONSTRAINT FK_Rewards_AspNetUsers_CreatedBy
   FOREIGN KEY (CreatedBy) REFERENCES AspNetUsers(Id);

   ALTER TABLE Rewards
   ADD CONSTRAINT CK_Rewards_Stock CHECK (Stock IS NULL OR Stock >= 0);
   ```

3. **AcquiredRewards → ExchangeRequests リネーム・拡張**

   ```sql
   EXEC sp_rename 'AcquiredRewards', 'ExchangeRequests';

   ALTER TABLE ExchangeRequests
   ADD Status INT NOT NULL DEFAULT 0,
       RequestedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
       ApprovedAt DATETIME2,
       RejectedAt DATETIME2,
       CancelledAt DATETIME2,
       ApprovedBy NVARCHAR(450),
       RejectionReason NVARCHAR(500),
       ParentNote NVARCHAR(1000);

   EXEC sp_rename 'ExchangeRequests.AcquiredAt', 'RequestedAt', 'COLUMN';

   ALTER TABLE ExchangeRequests
   ADD CONSTRAINT FK_ExchangeRequests_AspNetUsers_ApprovedBy
   FOREIGN KEY (ApprovedBy) REFERENCES AspNetUsers(Id);
   ```

4. **Students → AspNetUsers データ移行**

   ```sql
   INSERT INTO AspNetUsers (Id, UserName, DisplayName, Role, CreatedAt, IsActive)
   SELECT
       NEWID(),
       Name,
       Name,
       1, -- Child
       GETUTCDATE(),
       1
   FROM Students;

   -- LearningRecords の StudentId を更新
   -- (一時的にマッピングテーブル経由)
   ```

5. **既存景品データ削除**

   ```sql
   DELETE FROM ExchangeRequests;  -- 既存の仮想アイテム交換記録
   DELETE FROM Rewards;            -- 既存の仮想アイテム

   -- 新しいシードデータ投入（駄菓子、ポケカなど）
   INSERT INTO Rewards (Name, Description, RequiredPoints, Category, IsPhysical, Stock, CreatedBy, CreatedAt, IsActive)
   VALUES
       ('うまい棒 コンソメ味', '定番の駄菓子', 10, 0, 1, 50, @DefaultParentId, GETUTCDATE(), 1),
       ('ポケモンカード 1パック', '拡張パック', 200, 1, 1, 10, @DefaultParentId, GETUTCDATE(), 1),
       -- ...
   ```

---

## Validation Rules

### Reward (景品)

| フィールド     | 検証                                      |
| -------------- | ----------------------------------------- |
| Name           | 必須、1-100文字                           |
| RequiredPoints | 必須、1以上                               |
| Stock          | 0以上（null許可）                         |
| ImageUrl       | 最大500文字、画像形式（.jpg, .png, .gif） |

### ExchangeRequest (交換申請)

| フィールド      | 検証                           |
| --------------- | ------------------------------ |
| StudentId       | 必須、存在する子供ユーザー     |
| RewardId        | 必須、存在する有効な景品       |
| Status          | 必須、有効な ExchangeStatus 値 |
| RejectionReason | 却下時は必須、最大500文字      |

### ApplicationUser (ユーザー)

| フィールド  | 検証                                |
| ----------- | ----------------------------------- |
| Email       | 保護者は必須、メール形式            |
| Password    | 保護者は必須、8文字以上、英数字混在 |
| PIN         | 子供は必須、4桁数字                 |
| ParentId    | 子供は必須、存在する保護者ユーザー  |
| DisplayName | 必須、1-100文字                     |

---

## Performance Considerations

### Indexes Strategy

| テーブル         | インデックス                      | 理由                             |
| ---------------- | --------------------------------- | -------------------------------- |
| AspNetUsers      | (Role, IsActive)                  | ユーザー一覧取得の高速化         |
| Rewards          | (IsActive, IsPhysical, CreatedBy) | 景品一覧・フィルタリングの高速化 |
| ExchangeRequests | (Status, RequestedAt DESC)        | 未承認申請一覧の高速化           |
| ExchangeRequests | (StudentId, Status)               | 子供ごとの申請履歴取得の高速化   |

### Query Optimization

**保護者ダッシュボードの未承認件数取得**:

```csharp
var pendingCount = await _context.ExchangeRequests
    .Where(r => r.Status == ExchangeStatus.Pending)
    .CountAsync();  // インデックス利用で高速化
```

**子供の申請履歴取得**:

```csharp
var requests = await _context.ExchangeRequests
    .Include(r => r.Reward)
    .Where(r => r.StudentId == studentId)
    .OrderByDescending(r => r.RequestedAt)
    .Take(20)
    .ToListAsync();  // Composite index (StudentId, Status) で最適化
```

---

## Security Considerations

| データ           | 脅威                           | 対策                                           |
| ---------------- | ------------------------------ | ---------------------------------------------- |
| パスワード       | 平文保存による漏洩             | bcrypt（ASP.NET Core Identity デフォルト）     |
| PIN              | 総当たり攻撃                   | ハッシュ化、ログイン試行回数制限               |
| JWT トークン     | XSS によるトークン盗難         | HttpOnly Cookie または LocalStorage + CSRF対策 |
| 画像アップロード | 悪意あるファイルのアップロード | ファイル拡張子・MIME タイプ検証、サイズ制限    |
| 在庫操作         | 同時更新による在庫マイナス     | 楽観的ロック（RowVersion）                     |
