# API Contracts: Gamified Math Drill App

**Date**: 2025-12-30  
**Phase**: 1 - Design & Contracts  
**Purpose**: REST API エンドポイントの定義

---

## Base URL

- **Development**: `http://localhost:5000/api`
- **Production**: `https://[domain]/api`

---

## Authentication

**Current Phase**: シングルユーザー専用のため、認証は簡易実装

- **Student ID**: リクエストヘッダーまたはクエリパラメータで `X-Student-Id` を送信
- **Future Enhancement**: 保護者向け管理画面では JWT 認証を追加予定

---

## API Endpoints

### 1. Student API

#### GET `/students/{id}`

児童のプロフィールと学習統計を取得します。

**Response 200**:

```json
{
  "id": 1,
  "name": "太郎",
  "currentLevel": {
    "levelNumber": 3,
    "minDifficulty": 3,
    "maxDifficulty": 5
  },
  "totalPoints": 450,
  "consecutiveDays": 7,
  "totalProblems": 120,
  "correctAnswers": 95,
  "accuracyRate": 0.79,
  "lastLoginAt": "2025-12-30T10:30:00Z"
}
```

---

#### POST `/students`

新しい児童アカウントを作成します（初回セットアップ時）。

**Request**:

```json
{
  "name": "太郎"
}
```

**Response 201**:

```json
{
  "id": 1,
  "name": "太郎",
  "currentLevel": {
    "levelNumber": 1,
    "minDifficulty": 1,
    "maxDifficulty": 3
  },
  "totalPoints": 0,
  "consecutiveDays": 0
}
```

---

#### PATCH `/students/{id}/login`

児童のログインを記録し、連続学習日数を更新します。

**Request**: (Body なし)

**Response 200**:

```json
{
  "consecutiveDays": 8,
  "encouragementMessage": "すごい！8日連続だよ！"
}
```

---

### 2. Problem API

#### GET `/problems/next`

児童の現在のレベルに応じた次の問題を取得します。

**Query Parameters**:

- `studentId` (required): 児童 ID
- `excludeRecentIds` (optional): 最近解いた問題の ID リスト（カンマ区切り）

**Response 200**:

```json
{
  "id": 42,
  "question": "23 + 45 = ?",
  "calculationType": "Addition",
  "difficultyLevel": 3
}
```

---

#### POST `/problems/{id}/answer`

問題への回答を送信し、正誤判定とポイント付与を行います。

**Request**:

```json
{
  "studentId": 1,
  "userAnswer": 68,
  "timeSpentSeconds": 25
}
```

**Response 200 (正解)**:

```json
{
  "isCorrect": true,
  "correctAnswer": 68,
  "pointsEarned": 10,
  "totalPoints": 460,
  "message": "正解！よくできました！",
  "levelUp": false
}
```

**Response 200 (不正解)**:

```json
{
  "isCorrect": false,
  "correctAnswer": 68,
  "pointsEarned": 0,
  "totalPoints": 450,
  "message": "不正解。正しい答えは 68 です。",
  "levelUp": false
}
```

**Response 200 (レベルアップ)**:

```json
{
  "isCorrect": true,
  "correctAnswer": 68,
  "pointsEarned": 10,
  "totalPoints": 460,
  "message": "正解！レベルアップ！",
  "levelUp": true,
  "newLevel": {
    "levelNumber": 4,
    "minDifficulty": 4,
    "maxDifficulty": 6
  }
}
```

---

### 3. Learning Records API

#### GET `/learning-records`

児童の学習履歴を取得します。

**Query Parameters**:

- `studentId` (required): 児童 ID
- `startDate` (optional): 開始日（YYYY-MM-DD）
- `endDate` (optional): 終了日（YYYY-MM-DD）
- `calculationType` (optional): 計算種類でフィルタ

**Response 200**:

```json
{
  "records": [
    {
      "id": 501,
      "problem": {
        "id": 42,
        "question": "23 + 45 = ?",
        "calculationType": "Addition"
      },
      "isCorrect": true,
      "timeSpentSeconds": 25,
      "answeredAt": "2025-12-30T10:35:00Z"
    }
  ],
  "totalRecords": 120,
  "page": 1,
  "pageSize": 20
}
```

---

#### GET `/learning-records/statistics`

日別・週別・計算種類別の統計を取得します。

**Query Parameters**:

- `studentId` (required): 児童 ID
- `groupBy` (required): `daily` | `weekly` | `calculationType`
- `startDate` (optional): 開始日
- `endDate` (optional): 終了日

**Response 200 (daily)**:

```json
{
  "statistics": [
    {
      "date": "2025-12-30",
      "totalProblems": 15,
      "correctAnswers": 12,
      "accuracyRate": 0.8,
      "averageTimeSeconds": 28
    },
    {
      "date": "2025-12-29",
      "totalProblems": 18,
      "correctAnswers": 14,
      "accuracyRate": 0.78,
      "averageTimeSeconds": 32
    }
  ]
}
```

**Response 200 (calculationType)**:

```json
{
  "statistics": [
    {
      "calculationType": "Addition",
      "totalProblems": 40,
      "correctAnswers": 35,
      "accuracyRate": 0.875
    },
    {
      "calculationType": "Subtraction",
      "totalProblems": 35,
      "correctAnswers": 25,
      "accuracyRate": 0.714
    },
    {
      "calculationType": "Multiplication",
      "totalProblems": 30,
      "correctAnswers": 22,
      "accuracyRate": 0.733
    },
    {
      "calculationType": "Division",
      "totalProblems": 15,
      "correctAnswers": 13,
      "accuracyRate": 0.867
    }
  ]
}
```

---

### 4. Rewards API

#### GET `/rewards`

交換可能な景品一覧を取得します。

**Query Parameters**:

- `category` (optional): カテゴリーでフィルタ（`Badge` | `Avatar` | `Character`）
- `maxPoints` (optional): 指定ポイント以下の景品のみ表示

**Response 200**:

```json
{
  "rewards": [
    {
      "id": 1,
      "name": "金メダルバッジ",
      "description": "すごい！金メダルを獲得！",
      "requiredPoints": 100,
      "category": "Badge",
      "imageUrl": "/images/rewards/gold-medal.png"
    },
    {
      "id": 2,
      "name": "にこにこアバター",
      "description": "笑顔のアバター",
      "requiredPoints": 200,
      "category": "Avatar",
      "imageUrl": "/images/rewards/smile-avatar.png"
    }
  ]
}
```

---

#### POST `/rewards/{id}/exchange`

ポイントを消費して景品と交換します。

**Request**:

```json
{
  "studentId": 1
}
```

**Response 200 (成功)**:

```json
{
  "success": true,
  "reward": {
    "id": 1,
    "name": "金メダルバッジ",
    "category": "Badge"
  },
  "pointsSpent": 100,
  "remainingPoints": 350,
  "message": "おめでとう！金メダルバッジを獲得しました！"
}
```

**Response 400 (ポイント不足)**:

```json
{
  "success": false,
  "error": "InsufficientPoints",
  "message": "ポイントが不足しています。あと 50 ポイント必要です。",
  "requiredPoints": 100,
  "currentPoints": 50
}
```

---

#### GET `/rewards/acquired`

児童が獲得した景品一覧を取得します。

**Query Parameters**:

- `studentId` (required): 児童 ID

**Response 200**:

```json
{
  "acquiredRewards": [
    {
      "id": 101,
      "reward": {
        "id": 1,
        "name": "金メダルバッジ",
        "category": "Badge",
        "imageUrl": "/images/rewards/gold-medal.png"
      },
      "pointsSpent": 100,
      "acquiredAt": "2025-12-28T15:20:00Z"
    }
  ],
  "totalCount": 3
}
```

---

### 5. Daily Challenge API

#### GET `/daily-challenges/today`

今日のデイリーチャレンジ問題を取得します。

**Response 200**:

```json
{
  "id": 5,
  "problem": {
    "id": 123,
    "question": "78 - 39 = ?",
    "calculationType": "Subtraction",
    "difficultyLevel": 5
  },
  "bonusPoints": 20,
  "targetDate": "2025-12-30"
}
```

**Response 404 (チャレンジなし)**:

```json
{
  "message": "今日のチャレンジはありません。"
}
```

---

#### POST `/daily-challenges/{id}/answer`

デイリーチャレンジへの回答を送信します。

**Request**:

```json
{
  "studentId": 1,
  "userAnswer": 39,
  "timeSpentSeconds": 45
}
```

**Response 200 (正解)**:

```json
{
  "isCorrect": true,
  "correctAnswer": 39,
  "pointsEarned": 30,
  "totalPoints": 480,
  "message": "正解！今日のチャレンジクリア！ボーナスポイント +20",
  "bonusPointsEarned": 20
}
```

---

## Error Responses

すべてのエンドポイントで共通のエラーレスポンス形式を使用します。

**400 Bad Request**:

```json
{
  "error": "ValidationError",
  "message": "入力が不正です。",
  "details": {
    "userAnswer": "数値を入力してください。"
  }
}
```

**404 Not Found**:

```json
{
  "error": "NotFound",
  "message": "指定されたリソースが見つかりません。"
}
```

**500 Internal Server Error**:

```json
{
  "error": "InternalError",
  "message": "サーバーエラーが発生しました。しばらくしてからもう一度お試しください。"
}
```

---

## Rate Limiting

児童の学習体験を保護するため、以下のレート制限を適用します：

- **問題取得**: 1 秒あたり最大 5 リクエスト
- **回答送信**: 1 秒あたり最大 3 リクエスト
- **景品交換**: 1 分あたり最大 10 リクエスト

**Response 429 (Too Many Requests)**:

```json
{
  "error": "RateLimitExceeded",
  "message": "リクエストが多すぎます。少し待ってからもう一度お試しください。",
  "retryAfter": 30
}
```

---

## Next Steps

API 契約の定義により、次のステップに進みます：

1. **quickstart.md**: 開発環境セットアップ手順（次の成果物）
2. **Constitution Check（再評価）**: API 設計が憲法に準拠しているか確認
3. **OpenAPI/Swagger Specification**: Swashbuckle.AspNetCore で自動生成
