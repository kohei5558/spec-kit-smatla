# Data Model: 問題カテゴリ選択機能

**Feature**: 002-category-selection | **Date**: 2026-01-18
**Purpose**: Phase 1 - データ構造、状態管理、エンティティ関係を定義

## Entities

### 既存エンティティ（変更なし）

#### CalculationType (Enum)

既存の列挙型。変更不要。

| 値  | 名前           | 説明   |
| --- | -------------- | ------ |
| 0   | Addition       | 足し算 |
| 1   | Subtraction    | 引き算 |
| 2   | Multiplication | 掛け算 |
| 3   | Division       | 割り算 |

**Location**: `backend/src/GamifiedMathDrill.Core/Models/CalculationType.cs`

---

#### Problem (既存エンティティ)

既存のエンティティ。CalculationType プロパティが既に存在。変更不要。

| プロパティ      | 型              | 説明                                 |
| --------------- | --------------- | ------------------------------------ |
| Id              | int             | 問題 ID（主キー）                    |
| Question        | string          | 問題文                               |
| CorrectAnswer   | int             | 正解                                 |
| CalculationType | CalculationType | 計算種類（**既にインデックスあり**） |
| DifficultyLevel | int             | 難易度（1-10）                       |

**Location**: `backend/src/GamifiedMathDrill.Core/Models/Problem.cs`

---

#### LearningRecord (既存エンティティ)

既存のエンティティ。Problem への参照経由で CalculationType を取得可能。変更不要。

| プロパティ  | 型       | 説明                     |
| ----------- | -------- | ------------------------ |
| Id          | int      | 学習記録 ID（主キー）    |
| StudentId   | int      | 児童 ID                  |
| ProblemId   | int      | 問題 ID（外部キー）      |
| IsCorrect   | bool     | 正解したか               |
| SubmittedAt | DateTime | 回答日時                 |
| Problem     | Problem  | ナビゲーションプロパティ |

**Location**: `backend/src/GamifiedMathDrill.Core/Models/LearningRecord.cs`

**Note**: カテゴリ別統計は `LearningRecord.Problem.CalculationType` で集計

---

### 新規エンティティ（フロントエンドのみ）

#### CategorySelectionState (クライアント側の状態)

セッション中のカテゴリ選択状態を管理。データベースには保存しない。

| プロパティ       | 型               | 説明                            |
| ---------------- | ---------------- | ------------------------------- |
| SelectedCategory | CalculationType? | 選択中のカテゴリ（null=すべて） |
| LastUpdated      | DateTime         | 最終更新日時（デバッグ用）      |

**Storage**: `sessionStorage` (Key: `category-selection`)

**JSON Example**:

```json
{
  "selectedCategory": "Addition",
  "lastUpdated": "2026-01-18T10:30:00Z"
}
```

または「すべて」選択時:

```json
{
  "selectedCategory": null,
  "lastUpdated": "2026-01-18T10:30:00Z"
}
```

**Location**: `frontend/GamifiedMathDrill.Client/Services/CategoryStateService.cs` で管理

---

## State Transitions

### カテゴリ選択状態のライフサイクル

```
[初期状態: 未選択]
       ↓
[ホーム画面でカテゴリ選択]
       ↓
[sessionStorage に保存]
       ↓
[問題画面に遷移] ──────┐
       ↓                │
[カテゴリ保持したまま問題を連続で解く]
       ↑                │
       └────────────────┘
       ↓
[ホーム画面に戻る] → [再選択可能]
       ↓
[タブを閉じる] → [状態クリア]
```

### バリデーション規則

| 状態             | 遷移先           | 検証                                   |
| ---------------- | ---------------- | -------------------------------------- |
| 未選択           | 問題画面         | ❌ 遷移を防止、メッセージ表示          |
| 未選択           | ホーム画面       | ✅ 許可                                |
| カテゴリ選択済み | 問題画面         | ✅ 許可、選択状態を問題取得 API に渡す |
| カテゴリ選択済み | ホーム画面       | ✅ 許可、再選択可能                    |
| カテゴリ選択済み | ページ再読み込み | ✅ sessionStorage から復元             |

---

## Data Flow

### カテゴリ選択フロー

```
[ホーム画面]
    │
    ├─ 児童がカテゴリをクリック
    │
    ↓
[CategoryStateService.SetCategory(type)]
    │
    ├─ sessionStorage に保存
    │
    ↓
[ProblemPage に遷移（NavigationManager）]
    │
    ├─ CategoryStateService.GetCategory() で選択状態を取得
    │
    ↓
[ProblemApiClient.GetNextProblem(studentId, category)]
    │
    ├─ GET /api/problems/next?studentId=1&category=Addition
    │
    ↓
[ProblemsController.GetNext(studentId, category)]
    │
    ├─ IProblemService.GetNextProblemAsync(studentId, category, excludeIds)
    │
    ↓
[ProblemService: カテゴリでフィルタリング]
    │
    ├─ category != null ならば WHERE CalculationType = category
    │
    ↓
[Problem を返す]
    │
    ↓
[ProblemPage で問題表示（カテゴリ名+アイコン付き）]
```

### カテゴリ別統計取得フロー

```
[ResultsPage 表示]
    │
    ├─ 4つのカテゴリ + "すべて" の統計を取得
    │
    ↓
[並列で5回のAPI呼び出し]
    │
    ├─ GET /api/learning-records?calculationType=Addition
    ├─ GET /api/learning-records?calculationType=Subtraction
    ├─ GET /api/learning-records?calculationType=Multiplication
    ├─ GET /api/learning-records?calculationType=Division
    └─ GET /api/learning-records (全体)
    │
    ↓
[集計結果を画面に表示]
    │
    ├─ カテゴリ別の正答率、取り組み回数
    ├─ グラフまたはテーブルで視覚化（MudChart または MudDataGrid）
```

---

## Validation Rules

### フロントエンド（Home.razor）

```csharp
private async Task StartProblem()
{
    var category = await CategoryStateService.GetCategory();

    if (category == null && !allowUnselectedCategory)
    {
        // 未選択の場合
        Snackbar.Add("カテゴリを選んでね！", Severity.Warning);
        return; // 遷移しない
    }

    // 遷移を許可
    Navigation.NavigateTo("/problem");
}
```

### バックエンド（ProblemsController）

```csharp
[HttpGet("next")]
public async Task<ActionResult<ApiResponse<ProblemDto>>> GetNext(
    [FromQuery] int studentId,
    [FromQuery] CalculationType? category = null,  // null は「すべて」
    [FromQuery] List<int>? excludeRecentIds = null)
{
    // category が null でも有効（既存の動作を維持）
    var problem = await _problemService.GetNextProblemAsync(
        studentId, category, excludeRecentIds);

    // ... 残りの処理
}
```

---

## Database Schema Impact

**変更なし** - 既存のスキーマで対応可能

### 既存のインデックス（確認済み）

```sql
-- ApplicationDbContext.cs で定義済み
CREATE INDEX IX_Problems_CalculationType ON Problems(CalculationType);
```

このインデックスにより、カテゴリフィルタリングクエリのパフォーマンスが最適化されます。

---

## API Contract Extensions

### Modified Endpoint

**GET /api/problems/next**

**Before**:

```
GET /api/problems/next?studentId=1&excludeRecentIds=10,20,30
```

**After** (後方互換性あり):

```
GET /api/problems/next?studentId=1&category=Addition&excludeRecentIds=10,20,30
```

**Parameters**:

- `studentId` (int, required): 児童 ID
- `category` (CalculationType?, optional): フィルタするカテゴリ（省略時は「すべて」）
- `excludeRecentIds` (List<int>?, optional): 除外する問題 ID のリスト

**Response** (変更なし):

```json
{
  "success": true,
  "data": {
    "id": 123,
    "question": "12 + 34 = ?",
    "difficultyLevel": 3,
    "calculationTypeText": "Addition"
  }
}
```

---

## Summary

| 項目             | 詳細                                              |
| ---------------- | ------------------------------------------------- |
| 新規テーブル     | なし                                              |
| 既存テーブル変更 | なし                                              |
| 新規インデックス | なし（既存の CalculationType インデックスを活用） |
| 状態管理         | sessionStorage（フロントエンドのみ）              |
| API 変更         | 1 エンドポイント拡張（後方互換性あり）            |
| データ移行       | 不要                                              |

**Complexity**: 低 - 既存のデータ構造を最大限活用
