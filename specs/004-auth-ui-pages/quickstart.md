# Quickstart Guide: Authentication UI Pages

**Date**: 2026年2月7日  
**Feature**: [spec.md](spec.md) | [plan.md](plan.md) | [research.md](research.md) | [data-model.md](data-model.md)

## Overview

このガイドでは、004-auth-ui-pages機能の開発環境セットアップと実装手順を説明します。

---

## Prerequisites

開始前に以下がインストールされていることを確認してください：

- **.NET SDK 8.0+**: [ダウンロード](https://dotnet.microsoft.com/download)
- **PostgreSQL 14+**: [ダウンロード](https://www.postgresql.org/download/)
- **Node.js 18+** (フロントエンドツール用): [ダウンロード](https://nodejs.org/)
- **Git**: [ダウンロード](https://git-scm.com/)
- **VS Code** (推奨): [ダウンロード](https://code.visualstudio.com/)

### 推奨 VS Code 拡張機能

- C# Dev Kit
- C# Extensions
- PostgreSQL (Chris Kolkman)
- REST Client

---

## 1. Repository Setup

### Clone and Branch

```bash
# リポジトリをクローン（まだの場合）
git clone <repository-url>
cd spec-kit-smatla

# feature ブランチに切り替え
git checkout 004-auth-ui-pages

# 最新の変更を取得
git pull origin 004-auth-ui-pages
```

---

## 2. Database Setup

### PostgreSQL データベース作成

```bash
# PostgreSQL に接続
psql -U postgres

# データベース作成
CREATE DATABASE gamified_math_drill;

# ユーザー作成と権限付与
CREATE USER gmdduser WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE gamified_math_drill TO gmdduser;

# 接続確認
\c gamified_math_drill
\q
```

### 接続文字列設定

```bash
# backend/src/GamifiedMathDrill.Api/appsettings.Development.json を編集
cd backend/src/GamifiedMathDrill.Api
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=gamified_math_drill;Username=gmdduser;Password=your_secure_password"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "GamifiedMathDrill",
    "Audience": "GamifiedMathDrillClient",
    "SessionExpiryMinutes": 60,
    "RememberMeExpiryMinutes": 43200
  },
  "Email": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": "587",
    "Username": "your-email@example.com",
    "Password": "your-email-password",
    "FromAddress": "noreply@gamifiedmathdrill.com"
  },
  "AppUrl": "http://localhost:5000"
}
```

### マイグレーション実行

```bash
# Infrastructure プロジェクトからマイグレーション作成
cd backend/src/GamifiedMathDrill.Api
dotnet ef migrations add AddPasswordResetTokens --project ../GamifiedMathDrill.Infrastructure

# データベースに適用
dotnet ef database update

# マイグレーション確認
dotnet ef migrations list
```

---

## 3. Backend Development

### ソリューションビルド

```bash
cd backend
dotnet restore
dotnet build
```

### 新規ファイル作成

#### 1. Entity: PasswordResetToken

```bash
# ファイル作成
touch src/GamifiedMathDrill.Core/Models/PasswordResetToken.cs
```

[data-model.md](data-model.md#2-entity-passwordresettoken-新規)を参照して実装。

#### 2. DTOs

```bash
# DTOファイル作成
cd src/GamifiedMathDrill.Api/DTOs
touch RegisterRequest.cs
touch RegisterResponse.cs
touch ForgotPasswordRequest.cs
touch ForgotPasswordResponse.cs
touch ResetPasswordRequest.cs
touch ResetPasswordResponse.cs
```

[data-model.md](data-model.md#5-dtos-data-transfer-objects)を参照して実装。

#### 3. Repository

```bash
# Repository作成
touch src/GamifiedMathDrill.Core/Interfaces/IPasswordResetTokenRepository.cs
touch src/GamifiedMathDrill.Infrastructure/Repositories/PasswordResetTokenRepository.cs
```

[data-model.md](data-model.md#6-repository-pattern)を参照して実装。

#### 4. Email Service

```bash
# Email Service作成
touch src/GamifiedMathDrill.Core/Interfaces/IEmailService.cs
touch src/GamifiedMathDrill.Infrastructure/Services/EmailService.cs
```

[research.md](research.md#6-メール送信サービスの選択)を参照して実装。

#### 5. Rate Limit Middleware

```bash
# Middleware作成
touch src/GamifiedMathDrill.Api/Middleware/RateLimitMiddleware.cs
```

[research.md](research.md#4-ブルートフォース攻撃対策とレート制限)を参照して実装。

#### 6. AuthController 更新

```bash
# 既存のAuthControllerを編集
code src/GamifiedMathDrill.Api/Controllers/AuthController.cs
```

以下の新規エンドポイントを追加：

- `POST /api/auth/register`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `GET /api/auth/validate-reset-token`

`POST /api/auth/login`エンドポイントを更新（rememberMeパラメータ対応）

#### 7. AuthService 更新

```bash
# 既存のAuthServiceを編集
code src/GamifiedMathDrill.Infrastructure/Services/AuthService.cs
```

以下のメソッドを追加：

- `RegisterAsync(RegisterRequest request)`
- `SendPasswordResetEmailAsync(string email)`
- `ResetPasswordAsync(ResetPasswordRequest request)`
- `ValidateResetTokenAsync(string token, string email)`

`LoginAsync`メソッドを更新（rememberMeフラグ対応）

#### 8. DI 登録 (Program.cs)

```bash
code src/GamifiedMathDrill.Api/Program.cs
```

```csharp
// Services registration
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Middleware registration
app.UseMiddleware<RateLimitMiddleware>();
```

### バックエンド実行

```bash
cd backend/src/GamifiedMathDrill.Api
dotnet run
```

ブラウザで Swagger UIを確認: `http://localhost:5000/swagger`

---

## 4. Frontend Development

### 新規コンポーネント作成

```bash
cd frontend/GamifiedMathDrill.Client
```

#### 1. Register.razor

```bash
touch Pages/Register.razor
```

```razor
@page "/register"
@using GamifiedMathDrill.Client.Services
@inject AuthService AuthService
@inject NavigationManager Navigation

<MudContainer MaxWidth="MaxWidth.Small" Class="mt-8">
    <MudCard>
        <MudCardHeader>
            <MudText Typo="Typo.h5">新規アカウント作成</MudText>
        </MudCardHeader>
        <MudCardContent>
            <MudTextField @bind-Value="email" Label="メールアドレス"
                          Variant="Variant.Outlined" Required="true"
                          InputType="InputType.Email" />
            <MudTextField @bind-Value="displayName" Label="表示名"
                          Variant="Variant.Outlined" Required="true"
                          Class="mt-4" />
            <MudTextField @bind-Value="password" Label="パスワード"
                          InputType="InputType.Password" Variant="Variant.Outlined"
                          Required="true" Class="mt-4" />
            <MudTextField @bind-Value="confirmPassword" Label="パスワード確認"
                          InputType="InputType.Password" Variant="Variant.Outlined"
                          Required="true" Class="mt-4" />

            @if (!string.IsNullOrEmpty(errorMessage))
            {
                <MudAlert Severity="Severity.Error" Class="mt-4">@errorMessage</MudAlert>
            }
        </MudCardContent>
        <MudCardActions>
            <MudButton Variant="Variant.Filled" Color="Color.Primary"
                       OnClick="HandleRegister" FullWidth="true" Disabled="@isLoading">
                @if (isLoading)
                {
                    <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                }
                else
                {
                    <span>アカウント作成</span>
                }
            </MudButton>
        </MudCardActions>
        <MudCardActions>
            <MudText Typo="Typo.body2">
                既にアカウントをお持ちですか？
                <MudLink Href="/login">ログイン</MudLink>
            </MudText>
        </MudCardActions>
    </MudCard>
</MudContainer>

@code {
    private string email = string.Empty;
    private string displayName = string.Empty;
    private string password = string.Empty;
    private string confirmPassword = string.Empty;
    private string? errorMessage;
    private bool isLoading = false;

    private async Task HandleRegister()
    {
        // Implementation
    }
}
```

#### 2. ForgotPassword.razor

```bash
touch Pages/ForgotPassword.razor
```

#### 3. ResetPassword.razor

```bash
touch Pages/ResetPassword.razor
```

#### 4. AuthService 更新

```bash
code Services/AuthService.cs
```

以下のメソッドを追加：

- `RegisterAsync(RegisterRequest request)`
- `ForgotPasswordAsync(string email)`
- `ResetPasswordAsync(ResetPasswordRequest request)`
- `ValidateResetTokenAsync(string token, string email)`

`LoginAsync`メソッドを更新（rememberMeフラグ対応）

#### 5. TokenService 更新

```bash
code Services/TokenService.cs
```

`SaveTokenAsync`メソッドを更新（rememberMeフラグでストレージ切り替え）

### フロントエンド実行

```bash
cd frontend/GamifiedMathDrill.Client
dotnet run
```

ブラウザで確認: `http://localhost:5001`

---

## 5. Testing

### Backend Unit Tests

```bash
cd backend/tests/GamifiedMathDrill.Tests.Unit
dotnet test
```

### Backend Integration Tests

```bash
cd backend/tests/GamifiedMathDrill.Tests.Integration
dotnet test
```

### Frontend Component Tests (bUnit)

```bash
# bUnit テストプロジェクト作成（初回のみ）
cd frontend
dotnet new xunit -n GamifiedMathDrill.Client.Tests
cd GamifiedMathDrill.Client.Tests
dotnet add package bunit
dotnet add reference ../GamifiedMathDrill.Client/GamifiedMathDrill.Client.csproj

# テスト実行
dotnet test
```

---

## 6. API Testing with REST Client

VS Code の REST Client 拡張機能を使用してAPIをテストします。

### test-auth-api.http

```bash
touch backend/test-auth-api.http
```

```http
### Register New Parent
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "displayName": "Test Parent",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!"
}

### Login without Remember Me
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "SecurePass123!",
  "rememberMe": false
}

### Login with Remember Me
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "SecurePass123!",
  "rememberMe": true
}

### Forgot Password
POST http://localhost:5000/api/auth/forgot-password
Content-Type: application/json

{
  "email": "test@example.com"
}

### Validate Reset Token
GET http://localhost:5000/api/auth/validate-reset-token?token=CfDJ8N...&email=test@example.com

### Reset Password
POST http://localhost:5000/api/auth/reset-password
Content-Type: application/json

{
  "email": "test@example.com",
  "token": "CfDJ8N...",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

---

## 7. Development Workflow

### 日々の開発フロー

```bash
# 1. ブランチを最新に更新
git pull origin 004-auth-ui-pages

# 2. バックエンド起動（ターミナル1）
cd backend/src/GamifiedMathDrill.Api
dotnet watch run

# 3. フロントエンド起動（ターミナル2）
cd frontend/GamifiedMathDrill.Client
dotnet watch run

# 4. コード変更を監視（自動リロード有効）

# 5. テスト実行（ターミナル3）
cd backend/tests/GamifiedMathDrill.Tests.Unit
dotnet watch test

# 6. 変更をコミット
git add .
git commit -m "feat: implement register page UI"
git push origin 004-auth-ui-pages
```

### デバッグ

#### VS Code デバッグ設定 (.vscode/launch.json)

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Backend API",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build-backend",
      "program": "${workspaceFolder}/backend/src/GamifiedMathDrill.Api/bin/Debug/net8.0/GamifiedMathDrill.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/backend/src/GamifiedMathDrill.Api",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    {
      "name": "Frontend Client",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build-frontend",
      "program": "${workspaceFolder}/frontend/GamifiedMathDrill.Client/bin/Debug/net8.0/GamifiedMathDrill.Client.dll",
      "args": [],
      "cwd": "${workspaceFolder}/frontend/GamifiedMathDrill.Client",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ],
  "compounds": [
    {
      "name": "Full Stack",
      "configurations": ["Backend API", "Frontend Client"]
    }
  ]
}
```

---

## 8. Common Issues & Solutions

### Issue 1: マイグレーション失敗

**症状**: `dotnet ef database update` でエラー

**解決策**:

```bash
# EF Core ツールがインストールされているか確認
dotnet tool list -g

# インストールされていない場合
dotnet tool install --global dotnet-ef

# プロジェクトパスを確認
cd backend/src/GamifiedMathDrill.Api
dotnet ef database update --project ../GamifiedMathDrill.Infrastructure
```

### Issue 2: SMTP メール送信失敗

**症状**: パスワードリセットメールが送信されない

**解決策**:

```bash
# 開発環境ではログのみに記録（メール送信スキップ）
# appsettings.Development.json に以下を追加
{
  "Email": {
    "EnableSending": false  // 開発環境ではメール送信を無効化
  }
}
```

### Issue 3: CORS エラー

**症状**: フロントエンドからAPIへのリクエストがブロックされる

**解決策**:

```csharp
// Program.cs に CORS 設定を追加
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5001", "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

app.UseCors("AllowFrontend");
```

### Issue 4: JWT トークンが認識されない

**症状**: 認証が必要なエンドポイントで401エラー

**解決策**:

```bash
# JWT 設定を確認
# appsettings.Development.json の Jwt:Secret が32文字以上であることを確認
# フロントエンドでトークンがヘッダーに含まれているか確認
# Browser Dev Tools > Network > Request Headers > Authorization: Bearer <token>
```

---

## 9. Next Steps

1. **Phase 2: Implementation**: [plan.md](plan.md)を参照して実装を開始
2. **Testing**: テストケースを作成（[spec.md](spec.md)の受け入れ基準を参照）
3. **Code Review**: 実装完了後、プルリクエストを作成
4. **Documentation**: README.md や API ドキュメントを更新

---

## Resources

- [ASP.NET Core Identity Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [MudBlazor Components](https://mudblazor.com/components/)
- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Feature Specification](spec.md)
- [Technical Research](research.md)
- [Data Model](data-model.md)
- [API Contracts](contracts/auth-api.yaml)
