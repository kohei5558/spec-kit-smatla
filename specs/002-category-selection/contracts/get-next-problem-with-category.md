# API Contract: Get Next Problem with Category Filter

**Endpoint**: `GET /api/problems/next`  
**Feature**: 002-category-selection  
**Date**: 2026-01-18

## Overview

既存の問題取得エンドポイントを拡張し、計算カテゴリでフィルタリング可能にします。後方互換性を維持するため、category パラメータはオプショナルです。

## Request

### URL

```
GET /api/problems/next
```

### Query Parameters

| Parameter          | Type           | Required | Default | Description                                                                                                 |
| ------------------ | -------------- | -------- | ------- | ----------------------------------------------------------------------------------------------------------- |
| `studentId`        | integer        | Yes      | -       | 児童の ID                                                                                                   |
| `category`         | string         | No       | null    | 計算カテゴリ（"Addition", "Subtraction", "Multiplication", "Division"）。省略時は全カテゴリからランダム出題 |
| `excludeRecentIds` | array[integer] | No       | []      | 最近解いた問題 ID のリスト（重複出題を防ぐ）                                                                |

### Valid Category Values

| Value            | 日本語 | 説明                 |
| ---------------- | ------ | -------------------- |
| `Addition`       | 足し算 | CalculationType = 0  |
| `Subtraction`    | 引き算 | CalculationType = 1  |
| `Multiplication` | 掛け算 | CalculationType = 2  |
| `Division`       | 割り算 | CalculationType = 3  |
| _(省略)_         | すべて | カテゴリフィルタなし |

### Example Requests

#### 足し算のみを取得

```http
GET /api/problems/next?studentId=1&category=Addition&excludeRecentIds=10,20,30
```

#### すべてのカテゴリからランダム取得（既存の動作）

```http
GET /api/problems/next?studentId=1&excludeRecentIds=10,20,30
```

または明示的に category を省略:

```http
GET /api/problems/next?studentId=1
```

---

## Response

### Success Response

**Status Code**: `200 OK`

**Headers**:

```
Content-Type: application/json
```

**Body**:

```json
{
  "success": true,
  "data": {
    "id": 123,
    "question": "12 + 34 = ?",
    "difficultyLevel": 3,
    "calculationTypeText": "Addition"
  },
  "message": null
}
```

### Response Schema

| Field                      | Type       | Description                      |
| -------------------------- | ---------- | -------------------------------- |
| `success`                  | boolean    | リクエストが成功したか           |
| `data`                     | ProblemDto | 問題データ（成功時）             |
| `data.id`                  | integer    | 問題 ID                          |
| `data.question`            | string     | 問題文（例: "12 + 34 = ?"）      |
| `data.difficultyLevel`     | integer    | 難易度（1-10）                   |
| `data.calculationTypeText` | string     | 計算種類の文字列表現             |
| `message`                  | string?    | エラーメッセージ（エラー時のみ） |

---

## Error Responses

### 400 Bad Request - Invalid Student ID

```json
{
  "success": false,
  "data": null,
  "message": "Valid student ID is required."
}
```

**Trigger**: studentId が 0 以下

---

### 400 Bad Request - Invalid Category

```json
{
  "success": false,
  "data": null,
  "message": "Invalid category value. Allowed values: Addition, Subtraction, Multiplication, Division."
}
```

**Trigger**: category パラメータの値が無効（例: "InvalidCategory"）

**Note**: ASP.NET Core のモデルバインディングが自動的に検証

---

### 404 Not Found - No Available Problems

```json
{
  "success": false,
  "data": null,
  "message": "No available problems found."
}
```

**Trigger**:

- 指定されたカテゴリに問題が存在しない
- すべての問題が excludeRecentIds に含まれている

---

### 404 Not Found - Student Not Found

```json
{
  "success": false,
  "data": null,
  "message": "Student with ID 999 not found."
}
```

**Trigger**: 指定された studentId が存在しない

---

## Business Rules

### Category Filtering Logic

1. **category が指定されている場合**:

   - `WHERE CalculationType = category` で問題を絞り込み
   - excludeRecentIds に含まれない問題からランダムに 1 問選択

2. **category が省略されている場合（null）**:
   - すべてのカテゴリから問題を取得（既存の動作）
   - excludeRecentIds に含まれない問題からランダムに 1 問選択

### Duplicate Problem Prevention

- excludeRecentIds パラメータは引き続き機能
- カテゴリフィルタリングと組み合わせて使用可能
- 同一カテゴリ内で最近解いた問題を除外できる

---

## Backward Compatibility

✅ **完全に後方互換性あり**

既存のクライアントは引き続き category パラメータなしで API を呼び出せます：

```http
GET /api/problems/next?studentId=1
```

この呼び出しは、従来通りすべてのカテゴリからランダムに問題を返します。

---

## Implementation Notes

### Controller Changes

```csharp
[HttpGet("next")]
public async Task<ActionResult<ApiResponse<ProblemDto>>> GetNext(
    [FromQuery] int studentId,
    [FromQuery] CalculationType? category = null,  // 新規パラメータ
    [FromQuery] List<int>? excludeRecentIds = null)
{
    if (studentId <= 0)
    {
        return BadRequest(ApiResponse<ProblemDto>.ErrorResponse(
            "Valid student ID is required."));
    }

    try
    {
        var problem = await _problemService.GetNextProblemAsync(
            studentId, category, excludeRecentIds);

        if (problem == null)
        {
            return NotFound(ApiResponse<ProblemDto>.ErrorResponse(
                "No available problems found."));
        }

        var problemDto = new ProblemDto
        {
            Id = problem.Id,
            Question = problem.Question,
            DifficultyLevel = problem.DifficultyLevel,
            CalculationTypeText = problem.CalculationType.ToString()
        };

        return Ok(ApiResponse<ProblemDto>.SuccessResponse(problemDto));
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(ApiResponse<ProblemDto>.ErrorResponse(ex.Message));
    }
}
```

### Service Changes

```csharp
public interface IProblemService
{
    Task<Problem?> GetNextProblemAsync(
        int studentId,
        CalculationType? category = null,  // 新規パラメータ
        List<int>? excludeRecentIds = null);

    // 既存のメソッドは変更なし
    Task<(bool IsCorrect, int PointsEarned, bool LeveledUp, Level? NewLevel)>
        SubmitAnswerAsync(int studentId, int problemId, int answer);
}
```

---

## Testing Scenarios

| Test Case                 | studentId | category       | excludeRecentIds | Expected Result                     |
| ------------------------- | --------- | -------------- | ---------------- | ----------------------------------- |
| 足し算のみ取得            | 1         | Addition       | []               | Addition の問題を返す               |
| 引き算のみ取得            | 1         | Subtraction    | []               | Subtraction の問題を返す            |
| すべて取得（明示的 null） | 1         | null           | []               | 任意のカテゴリの問題を返す          |
| すべて取得（省略）        | 1         | _(省略)_       | []               | 任意のカテゴリの問題を返す          |
| 除外リスト併用            | 1         | Multiplication | [10,20]          | ID 10,20 以外の Multiplication 問題 |
| 無効な studentId          | 0         | Addition       | []               | 400 Bad Request                     |
| 問題が存在しない          | 1         | Addition       | [全問題 ID]      | 404 Not Found                       |

---

## Performance Considerations

- **Index**: CalculationType にインデックスが存在するため、カテゴリフィルタリングは高速
- **Query Complexity**: O(log n) - インデックススキャン
- **Expected Latency**: < 100ms（研究フェーズの目標値）
- **Cache**: 問題データは変更頻度が低いため、将来的にキャッシング追加を検討可能

---

## Version History

| Version | Date       | Changes                                |
| ------- | ---------- | -------------------------------------- |
| 1.0     | 2026-01-18 | 初版リリース - category パラメータ追加 |
