# Gamified Math Drill - Backend

ASP.NET Core 8.0で実装されたゲーミフィケーション要素を持つ算数ドリルアプリケーションのバックエンドAPI。

## 主な機能

### 1. 問題生成と回答

- **カテゴリ別問題取得**: 足し算、引き算、掛け算、割り算のカテゴリを指定して問題を取得
- **適応的難易度調整**: 学習者の履歴に基づいて難易度を自動調整
- **リアルタイム採点**: 回答送信後、即座に正誤判定とポイント付与

### 2. 学習記録と統計

- **学習履歴管理**: すべての学習記録を保存・参照
- **カテゴリ別統計**: 各計算カテゴリごとの正答率、問題数、獲得ポイントを集計
- **期間フィルタリング**: 日付範囲を指定して統計を取得

### 3. ポイントとリワード

- **ポイント獲得**: 問題正解時にポイントを付与
- **景品交換**: 貯めたポイントで景品と交換
- **獲得履歴**: 交換した景品の履歴を管理

### 4. デイリーチャレンジ

- **毎日の課題**: 日替わりで特別な問題セットを自動生成
- **ボーナスポイント**: 通常問題より高いポイントを獲得可能

## API エンドポイント

### 問題 (Problems)

#### GET `/api/problems/next`

次の問題を取得します。

**クエリパラメータ:**

- `studentId` (int, 必須): 学習者ID
- `calculationType` (string, オプション): 計算カテゴリ
  - `Addition`: 足し算
  - `Subtraction`: 引き算
  - `Multiplication`: 掛け算
  - `Division`: 割り算

**レスポンス例:**

```json
{
  "id": 123,
  "operand1": 15,
  "operand2": 7,
  "operator": "+",
  "calculationType": "Addition",
  "difficulty": 2
}
```

**パフォーマンス:**

- カテゴリ指定時: < 100ms
- カテゴリ未指定時: < 50ms

#### POST `/api/problems/answer`

回答を送信して採点します。

**リクエストボディ:**

```json
{
  "studentId": 1,
  "problemId": 123,
  "answer": 22
}
```

**レスポンス例:**

```json
{
  "isCorrect": true,
  "correctAnswer": 22,
  "pointsEarned": 10,
  "totalPoints": 150,
  "message": "正解！"
}
```

### 学習記録 (Learning Records)

#### GET `/api/learning-records`

学習記録の一覧を取得します。

**クエリパラメータ:**

- `studentId` (int, 必須): 学習者ID
- `startDate` (datetime, オプション): 開始日
- `endDate` (datetime, オプション): 終了日
- `calculationType` (string, オプション): 計算カテゴリでフィルタ
- `pageNumber` (int, デフォルト: 1): ページ番号
- `pageSize` (int, デフォルト: 10): ページサイズ

**レスポンス例:**

```json
{
  "records": [
    {
      "id": 456,
      "studentId": 1,
      "problemId": 123,
      "operand1": 15,
      "operand2": 7,
      "operator": "+",
      "calculationType": "Addition",
      "userAnswer": 22,
      "correctAnswer": 22,
      "isCorrect": true,
      "pointsEarned": 10,
      "completedAt": "2024-01-15T10:30:00Z"
    }
  ],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 10
}
```

#### GET `/api/learning-records/statistics`

学習統計を取得します。

**クエリパラメータ:**

- `studentId` (int, 必須): 学習者ID
- `startDate` (datetime, オプション): 開始日
- `endDate` (datetime, オプション): 終了日
- `calculationType` (string, オプション): 特定カテゴリの統計のみ取得

**レスポンス例:**

```json
{
  "totalProblems": 150,
  "correctAnswers": 135,
  "averageAccuracy": 90.0,
  "totalPoints": 1350,
  "problemsByType": {
    "Addition": 40,
    "Subtraction": 35,
    "Multiplication": 45,
    "Division": 30
  },
  "accuracyByType": {
    "Addition": 95.0,
    "Subtraction": 88.6,
    "Multiplication": 91.1,
    "Division": 86.7
  }
}
```

**パフォーマンス:**

- カテゴリ指定時: < 200ms
- 全体統計: < 300ms

### デイリーチャレンジ (Daily Challenges)

#### GET `/api/daily-challenges/today`

今日のデイリーチャレンジを取得します。

**クエリパラメータ:**

- `studentId` (int, 必須): 学習者ID

#### POST `/api/daily-challenges/answer`

デイリーチャレンジの回答を送信します。

### 景品 (Rewards)

#### GET `/api/rewards`

利用可能な景品一覧を取得します。

#### POST `/api/rewards/exchange`

ポイントを使って景品と交換します。

## アーキテクチャ

### プロジェクト構成

```
backend/
├── src/
│   ├── GamifiedMathDrill.Api/          # Web APIレイヤー
│   │   ├── Controllers/                # APIコントローラー
│   │   ├── DTOs/                      # データ転送オブジェクト
│   │   ├── Middleware/                # カスタムミドルウェア
│   │   └── Services/                  # APIレイヤー固有のサービス
│   ├── GamifiedMathDrill.Core/        # ドメインレイヤー
│   │   ├── Models/                    # ドメインエンティティ
│   │   ├── Interfaces/                # リポジトリインターフェース
│   │   └── Services/                  # ドメインサービス
│   └── GamifiedMathDrill.Infrastructure/  # インフラストラクチャレイヤー
│       ├── Data/                      # データベースコンテキスト
│       ├── Repositories/              # リポジトリ実装
│       ├── Jobs/                      # バックグラウンドジョブ
│       └── Migrations/                # EF Core マイグレーション
└── tests/
    ├── GamifiedMathDrill.Tests.Unit/         # ユニットテスト
    └── GamifiedMathDrill.Tests.Integration/  # 統合テスト
```

### 設計パターン

- **Clean Architecture**: ドメイン中心の設計、依存関係の逆転
- **Repository Pattern**: データアクセスの抽象化
- **Dependency Injection**: サービスライフタイム管理
- **CQRS (簡易版)**: 読み取りと書き込みの分離

## 技術スタック

- **.NET 8.0**: 最新のLTSバージョン
- **ASP.NET Core Web API**: RESTful API実装
- **Entity Framework Core 8.0**: ORM
- **SQLite** (開発): 軽量なローカルデータベース
- **PostgreSQL** (本番): スケーラブルなリレーショナルデータベース
- **Serilog**: 構造化ログ
- **xUnit + FluentAssertions**: テストフレームワーク

## セットアップ

### 前提条件

- .NET 8.0 SDK以上
- Visual Studio 2022 / VS Code / Rider

### インストール

1. リポジトリをクローン

```bash
git clone <repository-url>
cd backend
```

2. 依存パッケージの復元

```bash
dotnet restore
```

3. データベースのマイグレーション実行

```bash
cd src/GamifiedMathDrill.Api
dotnet ef database update
```

4. アプリケーションの起動

```bash
dotnet run
```

APIは `https://localhost:7000` で起動します。

### 開発環境での設定

`appsettings.Development.json` でローカル設定をカスタマイズできます：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=gamifiedmathdrill.db"
  }
}
```

## テスト実行

### すべてのテストを実行

```bash
dotnet test
```

### パフォーマンステストのみ実行

```bash
dotnet test --filter Category=Performance
```

### 特定のテストクラスを実行

```bash
dotnet test --filter FullyQualifiedName~CategoryPerformanceTests
```

## カテゴリ選択機能の実装詳細

### データフロー

1. **フロントエンド**: ユーザーがカテゴリを選択
2. **セッション保存**: `CategoryStateService` がsessionStorageに保存
3. **API呼び出し**: `calculationType` パラメータ付きで `/api/problems/next` を呼び出し
4. **フィルタリング**: `ProblemService` が指定カテゴリの問題を検索
5. **問題表示**: カテゴリに一致した問題を返却

### パフォーマンス最適化

- **インデックス**: `CalculationType` カラムにインデックスを設定
- **キャッシング**: 頻繁にアクセスされる問題セットをメモリキャッシュ
- **非同期処理**: すべてのデータベースクエリは非同期実行
- **ページング**: 大量データは常にページングして取得

### エラーハンドリング

カテゴリ指定時に問題が見つからない場合:

```json
{
  "title": "問題が見つかりません",
  "status": 404,
  "detail": "選択したカテゴリ（掛け算）の問題が見つかりませんでした。別のカテゴリを選択してください。"
}
```

### バックワード互換性

- `calculationType` パラメータはオプションのため、既存のAPIクライアントは影響を受けない
- パラメータ未指定時は全カテゴリからランダムに問題を選択（従来の動作）

## ログ

Serilogを使用した構造化ログを実装。ログは以下に出力されます：

- **コンソール**: 開発時の即時確認
- **ファイル**: `logs/log-YYYYMMDD.txt`

重要なイベント：

- API呼び出し（リクエスト/レスポンス）
- エラーとスタックトレース
- パフォーマンスメトリクス
- カテゴリ別問題フィルタリング結果

## 今後の改善予定

- [ ] 認証・認可の実装 (JWT)
- [ ] 親アカウント機能
- [ ] リアルタイムランキング
- [ ] より高度な適応的学習アルゴリズム
- [ ] GraphQL対応

## ライセンス

MIT License
