# Quick Start: 保護者管理画面と実物景品交換システム

**Feature**: 003-parent-admin-rewards | **Branch**: `003-parent-admin-rewards`

## Overview

このドキュメントは、開発者が本機能の実装を開始するための最小限の情報を提供します。

### What's New?

- 🔐 **ASP.NET Core Identity**: 保護者・子供のマルチユーザー認証（JWT）
- 🎁 **実物景品管理**: 駄菓子、ポケモンカード等の実物アイテム（画像アップロード対応）
- ✅ **承認フロー**: 子供が申請 → 保護者が承認・却下 → 実物を渡す
- 📊 **保護者ダッシュボード**: 未承認申請、子供一覧、統計情報（30 秒ポーリング）
- 🔒 **在庫管理**: 楽観的ロック（RowVersion）による競合制御

### Key Changes

| 変更内容                             | 影響範囲                      | 破壊的変更 |
| ------------------------------------ | ----------------------------- | ---------- |
| `Student` → `ApplicationUser`        | 全 Student 参照、認証システム | ✅         |
| 仮想アイテム削除                     | Reward シードデータ           | ✅         |
| `AcquiredReward` → `ExchangeRequest` | 交換システム、Status 追加     | ✅         |
| Reward に Stock, ImagePath 追加      | 景品管理、画像保存処理        | ⚠️         |

---

## Prerequisites

- .NET 8.0 SDK
- SQLite（開発用）または PostgreSQL（本番用）
- Node.js（フロントエンドビルド用）
- Visual Studio 2022 または VS Code（推奨）

---

## Quick Setup (5 分)

### 1. ブランチ作成

```bash
git checkout -b 003-parent-admin-rewards
```

### 2. NuGet パッケージ追加

```bash
cd backend/src/GamifiedMathDrill.Api
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
```

### 3. 環境変数設定（JWT 秘密鍵）

**appsettings.Development.json**:

```json
{
  "Jwt": {
    "SecretKey": "YOUR_SUPER_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG!!!",
    "Issuer": "GamifiedMathDrill",
    "Audience": "GamifiedMathDrill.Client",
    "ExpiryMinutes": 1440
  }
}
```

**⚠️ 重要**: `SecretKey` は本番環境で必ず環境変数または Key Vault で管理すること。

### 4. 既存データ削除（テスト環境のみ）

```bash
cd backend/src/GamifiedMathDrill.Api
dotnet ef database update 0  # データベースをリセット
dotnet ef database update    # 最新マイグレーションを適用
```

または：

```bash
rm backend/src/GamifiedMathDrill.Api/app.db  # SQLite の場合
```

---

## Architecture Overview

### Data Model

```
ApplicationUser (ASP.NET Core Identity)
├── Role: Parent
│   ├── Email, PasswordHash
│   ├── Children: [ApplicationUser]
│   └── CreatedRewards: [Reward]
└── Role: Child
    ├── PIN (4桁ハッシュ)
    ├── ParentId → Parent
    └── ExchangeRequests: [ExchangeRequest]

Reward
├── Stock (nullable)
├── ImagePath (string)
├── IsPhysical (bool)
├── CreatedBy → ApplicationUser (Parent)
└── RowVersion (楽観的ロック)

ExchangeRequest (旧 AcquiredReward)
├── Status (Pending/Approved/Rejected/Cancelled)
├── RequestedAt, ApprovedAt, RejectedAt
├── ApprovedBy → ApplicationUser (Parent)
├── RejectionReason (string)
└── StudentId → ApplicationUser (Child)
```

### API Endpoints

| Method | Endpoint                            | Role   | 説明                   |
| ------ | ----------------------------------- | ------ | ---------------------- |
| POST   | /api/auth/login                     | -      | 保護者ログイン         |
| POST   | /api/auth/child-login               | -      | 子供ログイン           |
| GET    | /api/rewards                        | Both   | 景品一覧取得           |
| POST   | /api/rewards                        | Parent | 景品登録（画像含む）   |
| PUT    | /api/rewards/{id}                   | Parent | 景品更新               |
| DELETE | /api/rewards/{id}                   | Parent | 景品削除（論理削除）   |
| POST   | /api/exchange-requests              | Child  | 交換申請               |
| GET    | /api/exchange-requests/my           | Child  | 自分の申請一覧         |
| PUT    | /api/exchange-requests/{id}/cancel  | Child  | 申請キャンセル         |
| PUT    | /api/exchange-requests/{id}/approve | Parent | 申請承認               |
| PUT    | /api/exchange-requests/{id}/reject  | Parent | 申請却下               |
| GET    | /api/parent/dashboard               | Parent | ダッシュボードサマリー |
| GET    | /api/parent/pending-requests        | Parent | 未承認申請一覧         |

詳細は [contracts/](contracts/) を参照。

---

## Key Files to Create/Modify

### Backend (新規作成)

| ファイル                                        | 説明                           |
| ----------------------------------------------- | ------------------------------ |
| `Infrastructure/Identity/ApplicationUser.cs`    | IdentityUser 拡張              |
| `Core/Models/UserRole.cs`                       | Parent/Child enum              |
| `Core/Models/ExchangeRequest.cs`                | 交換申請エンティティ           |
| `Core/Models/ExchangeStatus.cs`                 | Pending/Approved/Rejected enum |
| `Core/Services/AuthService.cs`                  | JWT 生成、PIN 検証             |
| `Core/Services/ImageStorageService.cs`          | 画像保存・削除処理             |
| `Core/Services/ExchangeRequestService.cs`       | 申請・承認・却下ロジック       |
| `Api/Controllers/AuthController.cs`             | 認証 API                       |
| `Api/Controllers/ExchangeRequestsController.cs` | 交換申請 API                   |
| `Api/Controllers/ParentDashboardController.cs`  | 保護者ダッシュボード API       |

### Backend (修正)

| ファイル                                      | 変更内容                                      |
| --------------------------------------------- | --------------------------------------------- |
| `Infrastructure/Data/ApplicationDbContext.cs` | → `IdentityDbContext<ApplicationUser>` に変更 |
| `Core/Models/Reward.cs`                       | Stock, ImagePath, CreatedBy, RowVersion 追加  |
| `Core/Models/RewardCategory.cs`               | Badge/Avatar 削除、Snack/Card/Toy 追加        |
| `Infrastructure/Data/Seed/RewardSeeder.cs`    | 実物景品データに変更                          |
| `Api/Program.cs`                              | Identity, JWT 認証設定追加                    |

### Frontend (新規作成)

| ファイル                                | 説明                           |
| --------------------------------------- | ------------------------------ |
| `Pages/Login.razor`                     | 保護者ログイン画面             |
| `Pages/ChildLogin.razor`                | 子供ログイン画面               |
| `Pages/ParentDashboard.razor`           | 保護者ダッシュボード           |
| `Pages/RewardManagement.razor`          | 景品管理画面                   |
| `Pages/RewardForm.razor`                | 景品登録・編集フォーム         |
| `Pages/ExchangeRequestList.razor`       | 交換申請一覧                   |
| `Pages/ExchangeRequestDetail.razor`     | 申請詳細・承認画面             |
| `Services/AuthService.cs`               | 認証 API 呼び出し              |
| `Services/TokenService.cs`              | JWT 保存・取得                 |
| `Services/ExchangeRequestApiClient.cs`  | 交換申請 API クライアント      |
| `Services/PollingService.cs`            | 30 秒ポーリング                |
| `Components/ImageUploader.razor`        | 画像アップロードコンポーネント |
| `Components/PendingRequestsBadge.razor` | 未承認バッジ                   |

### Frontend (修正)

| ファイル                          | 変更内容              |
| --------------------------------- | --------------------- |
| `Pages/Home.razor`                | 実物景品用 UI         |
| `Pages/AcquiredRewardsPage.razor` | Status バッジ表示追加 |
| `Layout/MainLayout.razor`         | Role 別ナビゲーション |

---

## Development Workflow

### Phase 0: 環境準備（2h）

1. ブランチ作成
2. 既存データ削除（仮想アイテム）
3. NuGet パッケージ追加
4. JWT 設定（appsettings.json）

### Phase 1: Identity 統合（6h）

1. ApplicationUser, UserRole 作成
2. ApplicationDbContext → IdentityDbContext 変更
3. マイグレーション作成・実行
4. UserSeeder 作成（デフォルト保護者・子供）
5. Program.cs に Identity 設定追加

**確認方法**:

```bash
dotnet ef migrations add AddIdentity
dotnet ef database update
```

### Phase 2: 認証 API（8h）

1. AuthService 作成（JWT 生成、PIN 検証）
2. AuthController 作成（/api/auth/login, /child-login）
3. 単体テスト作成
4. Postman/curl でテスト

**確認方法**:

```bash
curl -X POST http://localhost:5242/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"parent@example.com","password":"Test@1234"}'
```

### Phase 3: Reward 拡張（4h）

1. Reward モデル拡張（Stock, ImagePath, RowVersion 追加）
2. マイグレーション作成・実行
3. RewardSeeder 修正（実物景品データ）

### Phase 4: 景品管理 API（10h）

1. ImageStorageService 作成
2. RewardService 拡張（CRUD + 画像保存）
3. RewardsController 拡張（POST/PUT/DELETE）
4. 楽観的ロック処理（DbUpdateConcurrencyException）
5. 単体・統合テスト

**確認方法**:

```bash
curl -X POST http://localhost:5242/api/rewards \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "name=うまい棒" \
  -F "requiredPoints=10" \
  -F "category=Snack" \
  -F "isPhysical=true" \
  -F "stock=50" \
  -F "image=@umaibo.jpg"
```

### Phase 5-7: ExchangeRequest（18h）

1. ExchangeRequest, ExchangeStatus 作成
2. ExchangeRequestService 実装（申請・承認・却下）
3. ExchangeRequestsController 実装
4. ParentDashboardController 実装
5. テスト（単体・統合・同時申請）

### Phase 8-11: フロントエンド（30h）

1. 認証画面（ログイン、子供ログイン）
2. 景品管理画面（CRUD + 画像アップロード）
3. 交換申請画面（子供）
4. 保護者ダッシュボード（未承認一覧、承認・却下）
5. ポーリング実装（30 秒間隔）

### Phase 12: テスト・バグ修正（8h）

1. E2E テスト（Playwright）
2. パフォーマンステスト（NFR 検証）
3. セキュリティレビュー
4. ドキュメント更新

---

## Testing Strategy

### Unit Tests

```csharp
// AuthService のテスト例
[Fact]
public async Task LoginAsync_ValidCredentials_ReturnsJwtToken()
{
    // Arrange
    var authService = new AuthService(_userManager, _jwtSettings);

    // Act
    var result = await authService.LoginAsync("parent@example.com", "Test@1234");

    // Assert
    Assert.NotNull(result.Token);
    Assert.Equal("Parent", result.Role);
}
```

### Integration Tests

```csharp
// 景品登録のテスト例
[Fact]
public async Task CreateReward_WithImage_SavesImageAndReturnsDto()
{
    // Arrange
    var client = _factory.CreateClient();
    var formData = new MultipartFormDataContent
    {
        { new StringContent("うまい棒"), "name" },
        { new StringContent("10"), "requiredPoints" },
        { new ByteArrayContent(File.ReadAllBytes("test.jpg")), "image", "test.jpg" }
    };

    // Act
    var response = await client.PostAsync("/api/rewards", formData);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var reward = await response.Content.ReadFromJsonAsync<RewardDto>();
    Assert.NotNull(reward.ImageUrl);
    Assert.True(File.Exists($"wwwroot{reward.ImageUrl}"));
}
```

### E2E Tests (Playwright)

```csharp
[Test]
public async Task ParentApprovalFlow_ApproveRequest_UpdatesStatus()
{
    // 1. 子供ログイン → 景品申請
    await Page.GotoAsync("/child-login");
    await Page.ClickAsync("text=花子");
    await Page.FillAsync("input[type=password]", "1234");
    await Page.ClickAsync("button:has-text('ログイン')");
    await Page.ClickAsync("text=ポケモンカード");
    await Page.ClickAsync("button:has-text('交換申請')");

    // 2. 保護者ログイン → 承認
    await Page.GotoAsync("/login");
    await Page.FillAsync("input[type=email]", "parent@example.com");
    await Page.FillAsync("input[type=password]", "Test@1234");
    await Page.ClickAsync("button:has-text('ログイン')");
    await Page.ClickAsync("text=未承認申請");
    await Page.ClickAsync("button:has-text('承認')");

    // 3. 検証
    await Expect(Page.Locator("text=承認しました")).ToBeVisibleAsync();
}
```

---

## Security Checklist

- ✅ JWT SecretKey を環境変数で管理
- ✅ パスワードは bcrypt でハッシュ化（ASP.NET Core Identity デフォルト）
- ✅ PIN も bcrypt でハッシュ化
- ✅ 画像アップロード時に MIME type 検証
- ✅ ファイルサイズ制限（5MB）
- ✅ [Authorize] 属性で API 保護
- ✅ Role-based authorization（Parent/Child）
- ✅ トークンの有効期限設定（24 時間）
- ✅ HTTPS 必須（本番環境）
- ✅ CORS 設定（本番では特定ドメインのみ許可）

---

## Common Issues & Solutions

### 1. JWT トークンが無効

**症状**: 401 Unauthorized エラー

**解決策**:

- SecretKey が 32 文字以上か確認
- Issuer, Audience が appsettings.json と一致するか確認
- トークンの有効期限を確認

### 2. 画像アップロードが失敗

**症状**: 500 Internal Server Error

**解決策**:

- `wwwroot/uploads/rewards/` ディレクトリが存在するか確認
- 書き込み権限があるか確認（Linux/Mac の場合は `chmod 755`）
- ファイルサイズが 5MB 以下か確認

### 3. 在庫が負数になる

**症状**: 同時申請時に在庫がマイナス

**解決策**:

- Reward.RowVersion が設定されているか確認
- DbUpdateConcurrencyException を catch して再試行

### 4. マイグレーション失敗

**症状**: Entity Framework のマイグレーションでエラー

**解決策**:

```bash
dotnet ef database drop  # データベース削除
dotnet ef migrations remove  # マイグレーション削除
dotnet ef migrations add InitialCreate  # 再作成
dotnet ef database update
```

---

## Next Steps

1. ✅ [spec.md](spec.md) を読む（機能要件の理解）
2. ✅ [research.md](research.md) を読む（技術的な決定事項）
3. ✅ [data-model.md](data-model.md) を読む（データ構造の理解）
4. ✅ [plan.md](plan.md) を読む（実装フェーズの詳細）
5. ⏳ Phase 0 開始（環境準備）
6. ⏳ Phase 1 開始（Identity 統合）

---

## References

- [ASP.NET Core Identity Documentation](https://learn.microsoft.com/ja-jp/aspnet/core/security/authentication/identity)
- [JWT Authentication in ASP.NET Core](https://learn.microsoft.com/ja-jp/aspnet/core/security/authentication/jwt-authn)
- [Entity Framework Core Concurrency](https://learn.microsoft.com/ja-jp/ef/core/saving/concurrency)
- [Blazor WebAssembly Security](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/security/webassembly/)

---

## Contact

質問・不明点は以下に連絡してください：

- **Spec**: [spec.md](spec.md)
- **API Contracts**: [contracts/](contracts/)
- **Implementation Plan**: [plan.md](plan.md)
