# Quickstart: Gamified Math Drill App

**Date**: 2025-12-30  
**Purpose**: 開発環境のセットアップ手順とローカル実行ガイド

---

## Prerequisites

開発を開始する前に、以下のツールをインストールしてください：

### Required

- **.NET 8.0 SDK** ([ダウンロード](https://dotnet.microsoft.com/download/dotnet/8.0))

  ```bash
  dotnet --version  # 8.0.x であることを確認
  ```

- **Node.js 20.x LTS** (Blazor WASM ビルドツール用)

  ```bash
  node --version  # v20.x.x であることを確認
  npm --version
  ```

- **Git** (バージョン管理)
  ```bash
  git --version
  ```

### Recommended

- **Visual Studio 2022** (Community Edition 以上) または **Visual Studio Code**

  - VS Code の場合: C# Dev Kit 拡張機能をインストール

- **PostgreSQL 16.x** (本番環境用 - 開発環境では SQLite を使用)

- **Docker Desktop** (コンテナ化されたデータベース実行用 - オプション)

---

## Getting Started

### 1. リポジトリのクローン

```bash
git clone https://github.com/[your-org]/gamified-math-drill.git
cd gamified-math-drill
git checkout 001-gamified-math-drill
```

---

### 2. バックエンドのセットアップ

#### 2.1 依存関係のインストール

```bash
cd backend/src/GamifiedMathDrill.Api
dotnet restore
```

#### 2.2 データベースのセットアップ（SQLite - 開発環境）

```bash
# appsettings.Development.json を確認
cat appsettings.Development.json

# 出力例:
# {
#   "ConnectionStrings": {
#     "DefaultConnection": "Data Source=gamifiedmathdr ill.db"
#   }
# }

# EF Core Migrations を実行
dotnet ef database update --project ../GamifiedMathDrill.Infrastructure

# シードデータを投入（問題、景品、レベル）
dotnet run --seed
```

#### 2.3 バックエンドの起動

```bash
dotnet run
# または dotnet watch run（ホットリロード有効）

# 起動後、ブラウザで Swagger UI を確認:
# http://localhost:5000/swagger
```

---

### 3. フロントエンドのセットアップ

#### 3.1 依存関係のインストール

```bash
cd ../../frontend
dotnet restore
```

#### 3.2 appsettings の設定

`wwwroot/appsettings.json` を編集し、API ベース URL を設定：

```json
{
  "ApiBaseUrl": "http://localhost:5000/api"
}
```

#### 3.3 フロントエンドの起動

```bash
dotnet run
# または dotnet watch run

# 起動後、ブラウザで以下にアクセス:
# http://localhost:5001
```

---

## Project Structure

```
gamified-math-drill/
├── backend/
│   ├── src/
│   │   ├── GamifiedMathDrill.Api/           # Web API エントリーポイント
│   │   ├── GamifiedMathDrill.Core/          # ビジネスロジック
│   │   └── GamifiedMathDrill.Infrastructure/  # データアクセス
│   └── tests/
│       ├── GamifiedMathDrill.Tests.Unit/
│       ├── GamifiedMathDrill.Tests.Integration/
│       └── GamifiedMathDrill.Tests.E2E/
├── frontend/
│   ├── src/
│   │   ├── components/                      # UI コンポーネント
│   │   ├── pages/                           # ページコンポーネント
│   │   └── services/                        # API クライアント
│   └── tests/
└── shared/
    └── contracts/                           # API契約ドキュメント
```

---

## Running Tests

### Unit Tests

```bash
# Backend unit tests
cd backend/tests/GamifiedMathDrill.Tests.Unit
dotnet test

# すべてのテストを実行（詳細出力）
dotnet test --logger "console;verbosity=detailed"
```

### Integration Tests

```bash
cd backend/tests/GamifiedMathDrill.Tests.Integration
dotnet test
```

### E2E Tests (Playwright)

```bash
# Playwright のインストール（初回のみ）
cd backend/tests/GamifiedMathDrill.Tests.E2E
pwsh bin/Debug/net8.0/playwright.ps1 install

# E2E テストの実行
dotnet test

# ヘッドフルモードで実行（ブラウザを表示）
dotnet test -- Playwright.LaunchOptions.Headless=false
```

---

## Database Management

### Migrations

```bash
# 新しい Migration を作成
cd backend/src/GamifiedMathDrill.Infrastructure
dotnet ef migrations add [MigrationName] --startup-project ../GamifiedMathDrill.Api

# Migration を適用
dotnet ef database update --startup-project ../GamifiedMathDrill.Api

# Migration を元に戻す
dotnet ef database update [PreviousMigrationName] --startup-project ../GamifiedMathDrill.Api

# Migration を削除
dotnet ef migrations remove --startup-project ../GamifiedMathDrill.Api
```

### Seed Data

```bash
# シードデータを再投入（データベースをリセット）
cd backend/src/GamifiedMathDrill.Api
dotnet run --seed --force
```

---

## Environment Variables

### Development

`backend/src/GamifiedMathDrill.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=gamifiedmathdrill.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Production

環境変数で設定（Azure App Service, AWS Elastic Beanstalk など）：

```bash
export ConnectionStrings__DefaultConnection="Host=your-db-host;Database=gamifiedmathdrill;Username=your-user;Password=your-password"
export ASPNETCORE_ENVIRONMENT=Production
```

---

## Common Tasks

### 新しい Controller の追加

```bash
cd backend/src/GamifiedMathDrill.Api/Controllers
# [ControllerName]Controller.cs を作成
```

### 新しい Service の追加

```bash
cd backend/src/GamifiedMathDrill.Core/Services
# I[ServiceName].cs (interface) を作成
# [ServiceName].cs (implementation) を作成

# DI に登録（Program.cs）:
# builder.Services.AddScoped<I[ServiceName], [ServiceName]>();
```

### 新しい Entity の追加

```bash
cd backend/src/GamifiedMathDrill.Core/Models
# [EntityName].cs を作成

# DbContext に追加:
# cd ../../../GamifiedMathDrill.Infrastructure/Data
# ApplicationDbContext.cs に DbSet を追加

# Migration を作成:
dotnet ef migrations add Add[EntityName] --startup-project ../../GamifiedMathDrill.Api
```

---

## Troubleshooting

### 問題: `dotnet ef` コマンドが見つからない

**解決策**:

```bash
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

### 問題: データベース接続エラー

**解決策**:

```bash
# 接続文字列を確認
cat appsettings.Development.json

# データベースファイルの権限を確認（SQLite）
ls -la gamifiedmathdrill.db

# データベースをリセット
rm gamifiedmathdrill.db
dotnet ef database update
```

### 問題: フロントエンドが API に接続できない

**解決策**:

```bash
# CORS 設定を確認（Program.cs）
# バックエンドの URL を確認（フロントエンド appsettings.json）
# ブラウザの開発者ツールでネットワークエラーを確認
```

### 問題: Blazor WASM が遅い（開発環境）

**解決策**:

```bash
# リリースモードでビルド
dotnet build -c Release

# または AOT（Ahead-of-Time）コンパイルを有効化
# frontend.csproj に追加:
# <RunAOTCompilation>true</RunAOTCompilation>
```

---

## Next Steps

開発環境のセットアップが完了したら：

1. **API ドキュメント**: http://localhost:5000/swagger を確認
2. **アプリを起動**: http://localhost:5001 でフロントエンドにアクセス
3. **テスト実行**: `dotnet test` ですべてのテストを実行
4. **タスクリスト**: `specs/001-gamified-math-drill/tasks.md` を確認（Phase 2 で作成）
5. **憲法遵守**: 実装中は `.specify/memory/constitution.md` の原則に従う

---

## Resources

- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core/)
- [Blazor WebAssembly Documentation](https://learn.microsoft.com/aspnet/core/blazor/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/ef/core/)
- [Playwright for .NET](https://playwright.dev/dotnet/)
- [MudBlazor Components](https://mudblazor.com/)

---

**Happy Coding! 🎉**
