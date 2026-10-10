# Gamified Math Drill - Backend

ASP.NET Core 10で実装されたゲーミフィケーション要素を持つ算数ドリルアプリケーションのバックエンドAPI。

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
- `category` (string, オプション): 計算カテゴリ
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
│   ├── GamifiedMathDrill.Infrastructure/  # インフラストラクチャレイヤー
│   │   ├── Data/                      # データベースコンテキスト・シード
│   │   ├── Repositories/              # リポジトリ実装
│   │   └── Jobs/                      # バックグラウンドジョブ
│   ├── GamifiedMathDrill.Migrations.Sqlite/      # EF Core マイグレーション（開発用 SQLite）
│   └── GamifiedMathDrill.Migrations.PostgreSQL/  # EF Core マイグレーション（本番用 PostgreSQL）
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

- **.NET 10**: LTSバージョン（2028年11月までサポート）
- **ASP.NET Core Web API**: RESTful API実装
- **Entity Framework Core 10**: ORM
- **SQLite** (開発): 軽量なローカルデータベース
- **PostgreSQL** (本番): スケーラブルなリレーショナルデータベース
- **Serilog**: 構造化ログ
- **xUnit + FluentAssertions**: テストフレームワーク

## セットアップ

### 前提条件

- .NET 10 SDK以上
- Visual Studio 2022 / VS Code / Rider

### インストール

初回の準備（開発用の秘密鍵の登録）とフロントエンドと合わせた起動手順は、[リポジトリ直下の README](../README.md) を参照してください。

1. データベース

マイグレーションはアプリの起動時に自動で適用されます（開発は SQLite、本番は PostgreSQL）。
DB の種類は設定 `DatabaseProvider`（`Sqlite` / `PostgreSQL`）で切り替えられます。
本番では環境変数 `ConnectionStrings__DefaultConnection` と `Jwt__SecretKey` を設定してください。

2. アプリケーションの起動（リポジトリ直下で実行）

```bash
dotnet run --project backend/src/GamifiedMathDrill.Api --launch-profile http
```

APIは `http://localhost:5242` で起動します（Swagger: `http://localhost:5242/swagger`）。

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

## デバッグ

### VS Codeでのデバッグ

1. **ブレークポイントの設定**
   - VS Codeでソースコードファイルを開き、行番号の左側をクリックしてブレークポイントを設定

2. **デバッグ構成**
   - `.vscode/launch.json` でデバッグ構成を確認（自動生成されます）

   ```json
   {
     "version": "0.2.0",
     "configurations": [
       {
         "name": ".NET Core Launch (web)",
         "type": "coreclr",
         "request": "launch",
         "preLaunchTask": "build",
         "program": "${workspaceFolder}/src/GamifiedMathDrill.Api/bin/Debug/net10.0/GamifiedMathDrill.Api.dll",
         "args": [],
         "cwd": "${workspaceFolder}/src/GamifiedMathDrill.Api",
         "stopAtEntry": false,
         "serverReadyAction": {
           "action": "openExternally",
           "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
         },
         "env": {
           "ASPNETCORE_ENVIRONMENT": "Development"
         }
       }
     ]
   }
   ```

3. **デバッグ開始**
   - F5キーを押すか、「実行とデバッグ」パネルから「.NET Core Launch (web)」を選択
   - アプリケーションが起動し、ブレークポイントで実行が停止します

### Visual Studioでのデバッグ

1. `GamifiedMathDrill.sln` をVisual Studioで開く
2. `GamifiedMathDrill.Api` をスタートアッププロジェクトに設定
3. F5キーでデバッグ開始

### ログを使用したデバッグ

アプリケーションはSerilogを使用して詳細なログを出力します。

**ログファイルの場所:**

- `src/GamifiedMathDrill.Api/logs/log-YYYYMMDD.txt`
- `src/GamifiedMathDrill.Api/api.log` (最新のログ)

**ログレベルの変更:**

`appsettings.Development.json` でログレベルを調整：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "GamifiedMathDrill": "Debug"
    }
  }
}
```

**ログの確認:**

```bash
# リアルタイムでログを監視
tail -f src/GamifiedMathDrill.Api/api.log

# 最新50行を表示
tail -50 src/GamifiedMathDrill.Api/api.log

# エラーログのみ抽出
grep -i "error" src/GamifiedMathDrill.Api/api.log
```

### データベースのデバッグ

**SQLiteデータベースの確認:**

```bash
# データベースに接続
cd src/GamifiedMathDrill.Api
sqlite3 gamifiedmathdrill.db

# テーブル一覧
.tables

# テーブル構造の確認
.schema AspNetUsers

# データの確認
SELECT * FROM AspNetUsers;

# 終了
.quit
```

**データベースのリセット:**

```bash
# データベースファイルを削除
rm src/GamifiedMathDrill.Api/gamifiedmathdrill.db

# アプリを起動すると、マイグレーションとデモデータが自動で作り直される
```

### APIエンドポイントのデバッグ

**HTTPクライアントファイルの使用:**

VS Code REST Client拡張機能をインストールして、`GamifiedMathDrill.Api.http` を使用：

```bash
# REST Client拡張機能のインストール
code --install-extension humao.rest-client
```

`GamifiedMathDrill.Api.http` ファイルで「Send Request」をクリック

**curlコマンドでのテスト:**

```bash
# 保護者ログイン
curl -X POST http://localhost:5242/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"parent@example.com","password":"Parent123!"}'

# トークンを使用してAPI呼び出し
curl -X GET http://localhost:5242/api/rewards \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### よくあるデバッグシナリオ

**問題: データベースマイグレーションエラー**

```bash
# マイグレーションの状態確認（リポジトリ直下で実行）
dotnet tool restore
ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations list \
  --project backend/src/GamifiedMathDrill.Migrations.Sqlite \
  --startup-project backend/src/GamifiedMathDrill.Api

# 開発用 DB を作り直す（ファイルを消してアプリを再起動）
rm backend/src/GamifiedMathDrill.Api/gamifiedmathdrill.db*
```

マイグレーションの追加手順は リポジトリ直下の CLAUDE.md を参照してください（SQLite と PostgreSQL の両方に追加が必要です）。

**問題: ポートが既に使用されている**

```bash
# 使用中のプロセスを確認（macOS）
lsof -i :5242
# Windows（PowerShell）の場合: Get-NetTCPConnection -LocalPort 5242

# プロセスを終了
kill -9 <PID>
```

**問題: 認証エラー**

- JWT設定が正しいか確認
- トークンの有効期限を確認
- ログファイルで詳細なエラーメッセージを確認

**問題: NullReferenceException**

- ブレークポイントを設定して変数の値を確認
- ログで例外のスタックトレースを確認
- Dependency Injectionのサービス登録を確認

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

- [x] 認証・認可の実装 (JWT) - ✅ 完了 (Feature 003)
- [x] 親アカウント機能 - ✅ 完了 (Feature 003)
- [ ] リアルタイムランキング
- [ ] より高度な適応的学習アルゴリズム
- [ ] GraphQL対応

## 認証・認可

### セットアップ

#### JWT設定

JWT 署名キーはリポジトリに含めません。開発環境ではリポジトリ直下で次を実行し、User Secrets に登録します：

```bash
./scripts/setup-dev-secrets.sh
```

本番環境では環境変数で渡します（`Jwt__SecretKey`、`ConnectionStrings__DefaultConnection` など）。
Issuer・Audience・有効期限は `appsettings.Development.json` の `Jwt` セクションで設定します。

**重要**: 本番環境では環境変数または Azure Key Vault 等のシークレット管理サービスを使用してください。

#### デフォルトユーザー

開発環境（`ASPNETCORE_ENVIRONMENT=Development`）の初回起動時のみ、以下のテストユーザーが自動作成されます（本番では作成されません）：

**保護者アカウント:**

- Email: `parent@example.com`
- Password: `Parent123!`
- Role: `Parent`

**子供アカウント（上記保護者の子供）:**

- `花子`（PIN: `1234`）
- `次郎`（PIN: `5678`）

子供ログイン画面は、保護者が「子供アカウント管理」画面で「この端末を子供用に登録する」を押した端末でのみ使えます。

### 認証フロー

1. **保護者ログイン**: POST `/api/auth/login`

   ```json
   {
     "email": "parent@example.com",
     "password": "Parent123!"
   }
   ```

2. **子供用端末の登録**（保護者ログイン中）: POST `/api/devices` → レスポンスの `token` を端末に保存

   ```json
   { "name": "リビングのタブレット" }
   ```

3. **子供一覧の取得**（子供ログイン画面）: GET `/api/devices/current/children`（ヘッダー `X-Device-Token` 必須）

4. **子供ログイン**: POST `/api/auth/child/login`（ヘッダー `X-Device-Token` 必須。他家庭の子供は 404）

   ```json
   {
     "ChildId": "子供アカウントID",
     "PIN": "1234"
   }
   ```

5. **レスポンス**:

   ```json
   {
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
     "userId": "user-guid",
     "userName": "parent@example.com",
     "role": "Parent",
     "parentId": null,
     "expiresAt": "2026-02-01T12:00:00Z"
   }
   ```

4. **API呼び出し**: `Authorization: Bearer {token}` ヘッダーを含める

### ロールベース認可

- **Parent**: 景品管理、交換申請の承認/却下、ダッシュボード閲覧
- **Child**: 問題を解く、景品一覧閲覧、交換申請

### エンドポイント認可

| エンドポイント                           | 必要なロール  |
| ---------------------------------------- | ------------- |
| GET `/api/rewards`                       | Parent, Child |
| POST `/api/rewards`                      | Parent        |
| GET `/api/exchangerequests/my`           | Child         |
| POST `/api/exchangerequests`             | Child         |
| PUT `/api/exchangerequests/{id}/approve` | Parent        |
| PUT `/api/exchangerequests/{id}/reject`  | Parent        |
| GET `/api/parent/dashboard`              | Parent        |

## ライセンス

MIT License
