# Data Model: Gamified Math Drill App

**Date**: 2025-12-30  
**Phase**: 1 - Design & Contracts  
**Purpose**: エンティティの詳細設計とリレーションシップの定義

---

## Entity Relationship Diagram

```
┌─────────────────┐
│     Student     │
│─────────────────│
│ Id (PK)         │
│ Name            │──┐
│ CurrentLevelId  │  │
│ TotalPoints     │  │
│ ConsecutiveDays │  │
│ TotalProblems   │  │
│ CorrectAnswers  │  │
│ CreatedAt       │  │
│ LastLoginAt     │  │
└─────────────────┘  │
         │           │
         │ 1         │
         │           │
         │ *         │
         ▼           │
┌──────────────────┐ │
│ LearningRecord   │ │
│──────────────────│ │
│ Id (PK)          │ │
│ StudentId (FK)   │ │
│ ProblemId (FK)   │ │
│ IsCorrect        │ │
│ TimeSpentSeconds │ │
│ AnsweredAt       │ │
└──────────────────┘ │
         │           │
         │ *         │
         │           │
         │ 1         │
         ▼           │
┌─────────────────┐  │
│    Problem      │  │
│─────────────────│  │
│ Id (PK)         │  │
│ Question        │  │
│ CorrectAnswer   │  │
│ Calculation-    │  │
│   Type          │  │
│ DifficultyLevel │  │
└─────────────────┘  │
                     │
         ┌───────────┘
         │ 1
         │
         │ *
         ▼
┌──────────────────┐
│ AcquiredReward   │
│──────────────────│
│ Id (PK)          │
│ StudentId (FK)   │
│ RewardId (FK)    │
│ PointsSpent      │
│ AcquiredAt       │
└──────────────────┘
         │
         │ *
         │
         │ 1
         ▼
┌─────────────────┐
│     Reward      │
│─────────────────│
│ Id (PK)         │
│ Name            │
│ Description     │
│ RequiredPoints  │
│ Category        │
│ ImageUrl        │
└─────────────────┘

┌──────────────────┐
│ DailyChallenge   │
│──────────────────│
│ Id (PK)          │
│ ProblemId (FK)   │
│ TargetDate       │
│ BonusPoints      │
│ IsActive         │
└──────────────────┘

┌─────────────────┐
│     Level       │
│─────────────────│
│ Id (PK)         │
│ LevelNumber     │
│ RequiredCorrect │
│ MinDifficulty   │
│ MaxDifficulty   │
└─────────────────┘
```

---

## Entity Definitions

### 1. Student（児童）

児童のプロフィールと学習進捗を管理するエンティティ。

| Field           | Type     | Description                  | Validation              |
| --------------- | -------- | ---------------------------- | ----------------------- |
| Id              | int/Guid | 主キー                       | PK, Auto-increment      |
| Name            | string   | 児童の名前（ニックネーム可） | Required, MaxLength(50) |
| CurrentLevelId  | int      | 現在のレベル（外部キー）     | FK to Level, Default=1  |
| TotalPoints     | int      | 獲得ポイント総数             | >=0, Default=0          |
| ConsecutiveDays | int      | 連続学習日数                 | >=0, Default=0          |
| TotalProblems   | int      | 解いた問題総数               | >=0, Default=0          |
| CorrectAnswers  | int      | 正解数                       | >=0, Default=0          |
| CreatedAt       | DateTime | アカウント作成日時           | Required, Default=Now   |
| LastLoginAt     | DateTime | 最終ログイン日時             | Nullable                |

**Relationships**:

- 1:N with LearningRecord（1 人の児童が複数の学習記録を持つ）
- 1:N with AcquiredReward（1 人の児童が複数の景品を獲得）
- N:1 with Level（複数の児童が同じレベルに属する）

**Indexes**:

- PRIMARY KEY on Id
- INDEX on CurrentLevelId
- INDEX on LastLoginAt (連続日数計算用)

---

### 2. Problem（問題）

計算問題の定義を管理するエンティティ。

| Field           | Type     | Description                                                 | Validation               |
| --------------- | -------- | ----------------------------------------------------------- | ------------------------ |
| Id              | int/Guid | 主キー                                                      | PK, Auto-increment       |
| Question        | string   | 問題文（例: "23 + 45 = ?"）                                 | Required, MaxLength(100) |
| CorrectAnswer   | int      | 正解の数値                                                  | Required                 |
| CalculationType | enum     | 計算種類（Addition, Subtraction, Multiplication, Division） | Required                 |
| DifficultyLevel | int      | 難易度（1-10）                                              | Required, Range(1, 10)   |

**Enum: CalculationType**:

- `Addition` = 0 (足し算)
- `Subtraction` = 1 (引き算)
- `Multiplication` = 2 (掛け算)
- `Division` = 3 (割り算)

**Relationships**:

- 1:N with LearningRecord（1 つの問題が複数の学習記録に関連）
- 1:N with DailyChallenge（1 つの問題が複数の日のチャレンジに使用可能）

**Indexes**:

- PRIMARY KEY on Id
- INDEX on CalculationType (種類別検索用)
- INDEX on DifficultyLevel (レベル別問題抽出用)

**Notes**:

- 問題はシードデータとして事前に 500-1000 問用意
- 動的生成も可能だが、Phase 1 では静的データで実装

---

### 3. LearningRecord（学習記録）

児童が問題を解いた履歴を記録するエンティティ。

| Field            | Type     | Description                     | Validation              |
| ---------------- | -------- | ------------------------------- | ----------------------- |
| Id               | int/Guid | 主キー                          | PK, Auto-increment      |
| StudentId        | int/Guid | 児童 ID（外部キー）             | FK to Student, Required |
| ProblemId        | int/Guid | 問題 ID（外部キー）             | FK to Problem, Required |
| IsCorrect        | bool     | 正誤（true=正解, false=不正解） | Required                |
| TimeSpentSeconds | int      | 所要時間（秒）                  | >=0, Nullable           |
| AnsweredAt       | DateTime | 解答日時                        | Required, Default=Now   |

**Relationships**:

- N:1 with Student（複数の記録が 1 人の児童に属する）
- N:1 with Problem（複数の記録が 1 つの問題に関連）

**Indexes**:

- PRIMARY KEY on Id
- INDEX on StudentId (児童別検索用)
- INDEX on AnsweredAt (日別・週別集計用)
- COMPOSITE INDEX on (StudentId, AnsweredAt) (パフォーマンス最適化)

**Notes**:

- AnsweredAt を基に日別・週別の学習記録を集計（FR-006）
- IsCorrect を基に正答率を計算

---

### 4. Reward（景品）

交換可能な景品の定義を管理するエンティティ。

| Field          | Type     | Description                            | Validation               |
| -------------- | -------- | -------------------------------------- | ------------------------ |
| Id             | int/Guid | 主キー                                 | PK, Auto-increment       |
| Name           | string   | 景品名（例: "金メダルバッジ"）         | Required, MaxLength(100) |
| Description    | string   | 説明                                   | MaxLength(500)           |
| RequiredPoints | int      | 必要ポイント                           | >0, Required             |
| Category       | enum     | カテゴリー（Badge, Avatar, Character） | Required                 |
| ImageUrl       | string   | 画像 URL                               | MaxLength(500), Nullable |

**Enum: RewardCategory**:

- `Badge` = 0 (バッジ)
- `Avatar` = 1 (アバター)
- `Character` = 2 (キャラクター)

**Relationships**:

- 1:N with AcquiredReward（1 つの景品が複数回交換される）

**Indexes**:

- PRIMARY KEY on Id
- INDEX on RequiredPoints (ポイント範囲検索用)
- INDEX on Category (カテゴリー別表示用)

**Notes**:

- シードデータとして 20-30 種類の景品を用意
- RequiredPoints は 50, 100, 200, 500 など段階的に設定

---

### 5. AcquiredReward（獲得景品）

児童が交換した景品の記録を管理するエンティティ。

| Field       | Type     | Description         | Validation              |
| ----------- | -------- | ------------------- | ----------------------- |
| Id          | int/Guid | 主キー              | PK, Auto-increment      |
| StudentId   | int/Guid | 児童 ID（外部キー） | FK to Student, Required |
| RewardId    | int/Guid | 景品 ID（外部キー） | FK to Reward, Required  |
| PointsSpent | int      | 消費ポイント        | >0, Required            |
| AcquiredAt  | DateTime | 獲得日時            | Required, Default=Now   |

**Relationships**:

- N:1 with Student（複数の獲得景品が 1 人の児童に属する）
- N:1 with Reward（複数の獲得景品が 1 つの景品タイプに関連）

**Indexes**:

- PRIMARY KEY on Id
- INDEX on StudentId (児童別検索用)
- INDEX on AcquiredAt (獲得履歴表示用)

**Business Rules**:

- 交換時に Student.TotalPoints から PointsSpent を減算
- トランザクション処理で整合性を保証（Student 更新と AcquiredReward 挿入をアトミックに）

---

### 6. DailyChallenge（デイリーチャレンジ）

その日限定の特別問題を管理するエンティティ。

| Field       | Type     | Description          | Validation              |
| ----------- | -------- | -------------------- | ----------------------- |
| Id          | int/Guid | 主キー               | PK, Auto-increment      |
| ProblemId   | int/Guid | 問題 ID（外部キー）  | FK to Problem, Required |
| TargetDate  | Date     | 対象日（YYYY-MM-DD） | Required, Unique        |
| BonusPoints | int      | ボーナスポイント     | >=0, Default=20         |
| IsActive    | bool     | 有効フラグ           | Default=true            |

**Relationships**:

- N:1 with Problem（複数のチャレンジが 1 つの問題を参照可能）

**Indexes**:

- PRIMARY KEY on Id
- UNIQUE INDEX on TargetDate (1 日 1 チャレンジ)
- INDEX on IsActive (アクティブなチャレンジの検索用)

**Business Rules**:

- 毎日 0 時（UTC or JST）に新しいチャレンジを自動生成
- 過去のチャレンジは IsActive=false に更新
- 正解時は通常ポイント + BonusPoints を付与

---

### 7. Level（レベル）

児童の習熟度レベルを管理するエンティティ。

| Field           | Type     | Description                      | Validation         |
| --------------- | -------- | -------------------------------- | ------------------ |
| Id              | int/Guid | 主キー                           | PK, Auto-increment |
| LevelNumber     | int      | レベル番号（1, 2, 3...）         | Required, Unique   |
| RequiredCorrect | int      | 次のレベルに進むための必要正解数 | >0, Required       |
| MinDifficulty   | int      | 出題する問題の最小難易度         | Range(1, 10)       |
| MaxDifficulty   | int      | 出題する問題の最大難易度         | Range(1, 10)       |

**Relationships**:

- 1:N with Student（複数の児童が同じレベルに属する）

**Indexes**:

- PRIMARY KEY on Id
- UNIQUE INDEX on LevelNumber
- INDEX on LevelNumber (レベル順序検索用)

**Business Rules**:

- 児童が RequiredCorrect 問連続正解すると次のレベルへ昇格
- レベルが上がると出題される問題の難易度が上昇（MinDifficulty, MaxDifficulty）
- シードデータとして 10 レベル程度を用意（例: Level 1: 難易度 1-3, Level 5: 難易度 5-7, Level 10: 難易度 8-10）

---

## Database Migrations

EF Core Migrations を使用してデータベーススキーマを管理します。

**Migration Strategy**:

1. **Initial Migration**: すべてのエンティティとリレーションシップを定義
2. **Seed Data Migration**: 問題、景品、レベルの初期データを投入
3. **Index Optimization Migration**: パフォーマンス最適化のためのインデックス追加

**Commands**:

```bash
# Migration 作成
dotnet ef migrations add InitialCreate --project GamifiedMathDrill.Infrastructure

# データベース更新
dotnet ef database update --project GamifiedMathDrill.Infrastructure

# Seed データ追加
dotnet ef migrations add SeedData --project GamifiedMathDrill.Infrastructure
```

---

## Data Validation Rules

すべてのエンティティは以下の検証ルールに従います：

1. **Required Fields**: Null 不可フィールドは必ず値を持つ
2. **Range Validation**: 数値フィールドは指定範囲内（例: DifficultyLevel は 1-10）
3. **String Length**: 文字列フィールドは最大長を指定（例: Name は MaxLength(50)）
4. **Foreign Key Integrity**: 外部キーは参照先のエンティティが存在することを保証
5. **Business Rules**: トランザクション単位でビジネスルールを適用（例: ポイント減算時のチェック）

**Validation Implementation**:

- **Data Annotations**: エンティティクラスに `[Required]`, `[Range]`, `[MaxLength]` などを適用
- **Fluent API**: `DbContext.OnModelCreating` でリレーションシップと制約を定義
- **Service Layer Validation**: ビジネスロジックレイヤーで複雑な検証を実施

---

## Next Steps

data-model.md の完成により、次のステップに進みます：

1. **contracts/**: REST API エンドポイント定義（次の成果物）
2. **quickstart.md**: 開発環境セットアップ手順
3. **Constitution Check（再評価）**: データモデルが憲法に準拠しているか確認
