# Implementation Plan: 保護者管理画面と実物景品交換システム

**Branch**: `003-parent-admin-rewards` | **Date**: 2026-01-25 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/003-parent-admin-rewards/spec.md`

## Summary

実物景品（駄菓子、ポケモンカード、おもちゃなど）の管理機能と、保護者による承認フロー付き交換システムを実装します。保護者が実物景品をマスター登録し、子供がポイントで交換申請、保護者が承認・却下する 3 段階のワークフローを構築します。既存の仮想アイテムシステムを全面的に再設計し、ASP.NET Core Identity によるマルチユーザー認証を導入します。

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**:

- ASP.NET Core 8.0, Entity Framework Core 8.0
- ASP.NET Core Identity 8.0（認証・認可）
- Blazor WebAssembly, MudBlazor 6.0+（UI components）
- System.IdentityModel.Tokens.Jwt（JWT トークン生成）

**Storage**:

- SQLite (development), PostgreSQL (production)
- 画像ファイル: Local filesystem (`wwwroot/uploads/rewards/`)

**Testing**: xUnit, Moq, Playwright for .NET (E2E tests)

**Target Platform**: Web application（保護者: PC/タブレット、子供: タブレット想定）

**Project Type**: Web (backend + frontend)

**Performance Goals**:

- 保護者ダッシュボード初期表示: 3 秒以内（NFR-002）
- 未承認申請の取得: 1 秒以内（NFR-003）
- 画像アップロード: 5MB まで、5 秒以内
- ポーリング間隔: 30 秒（過剰な API 負荷を防止）

**Constraints**:

- 既存の Student テーブルを ApplicationUser に統合（破壊的変更）
- 既存の仮想アイテムデータは削除（実物景品に置換）
- 保護者と子供で異なる認証方式（Email + Password vs 名前選択 + PIN）
- マルチテナント対応（保護者ごとに独立した子供・景品管理）
- 在庫管理に楽観的ロック（DbUpdateConcurrencyException 処理必須）

**Scale/Scope**:

- 想定ユーザー: 保護者 1〜数名、子供 1〜5 名/保護者
- 景品数: 10〜50 アイテム/保護者
- 同時申請: 最大 10 件/保護者（子供複数人が同時に申請）
- 影響を受けるコンポーネント:
  - 新規: 10+ ファイル（Identity 関連、Admin 画面、認証 API）
  - 修正: 8+ ファイル（Reward, ExchangeRequest, Student 削除）

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### Principle I: User-Centric Design ✅

- ✅ 実物景品による学習動機の向上（US1-US5、児童の要望に応える）
- ✅ 保護者による適切な監督機能（承認フロー、US3）
- ✅ 視覚的でわかりやすい UI（画像付き景品表示、FR-002）
- ✅ 子供の安全性（保護者承認なしでは実物受け取り不可、FR-007）

**Status**: PASS - 児童と保護者双方のニーズに対応

---

### Principle II: Data Privacy & Child Safety ✅

- ✅ 子供の個人情報は保護者のみアクセス可能（マルチテナント設計）
- ✅ パスワード・PIN のハッシュ化（ASP.NET Core Identity、bcrypt）
- ✅ JWT トークンのセキュアな保存（HttpOnly Cookie 推奨）
- ✅ 画像アップロードの検証（MIME type、ファイルサイズ制限）
- ✅ 実物景品の配布は保護者の監督下で実施

**Status**: PASS - プライバシーとセキュリティを重視

---

### Principle III: Specification-First Development ✅

- ✅ 詳細な仕様書作成済み（spec.md、5 つの User Story、13 の FR）
- ✅ 技術非依存な表現（実装詳細は research.md と data-model.md に分離）
- ✅ テスト可能な要件（6 つの Non-Functional Requirements で測定可能）
- ✅ Edge case 定義済み（8 つのシナリオ、エラー処理含む）
- ✅ Technical Notes で既存システムとの差異を明記

**Status**: PASS - 仕様書は実装前に完成

---

### Principle IV: Test-Driven Quality ✅

- ✅ 成功基準が測定可能（NFR-001〜NFR-006）
- ✅ User Story ごとの Acceptance Criteria 明確（各 2〜4 条件）
- ✅ セキュリティテスト要件あり（認証・認可、Edge Case #6-#8）
- ✅ パフォーマンステスト基準あり（3 秒、1 秒、NFR-002-003）

**Status**: PASS - TDD アプローチの基盤確立

---

### Principle V: Continuous Learning & Adaptation ✅

- ✅ 交換履歴の記録（ExchangeRequest、承認・却下理由保存）
- ✅ 保護者による分析可能（子供の興味・行動パターン把握）
- ✅ 景品のカテゴリ分類（統計分析の基盤、RewardCategory enum）
- ✅ ポイント消費履歴（学習効果の測定に活用可能）

**Status**: PASS - データ駆動型の改善を支援

---

### Technology Stack Compliance ✅

- ✅ ASP.NET Core 使用（憲法の必須制約）
- ✅ C# 12 / .NET 8.0
- ✅ Blazor WebAssembly（既存スタックを継続）
- ✅ Entity Framework Core（既存 ORM）
- ✅ ASP.NET Core Identity（.NET 標準の認証基盤）

**Status**: PASS - 技術スタックの一貫性維持

---

**Overall Gate Status**: ✅ **PASS** - Phase 0 研究に進む準備完了

**Post-Phase 1 Re-check**: ⏳ **PENDING**

- データモデル設計完了後に再評価
- API 契約設計完了後に再評価
- 実装計画詳細化後に最終判定

## Project Structure

### Documentation (this feature)

```text
specs/003-parent-admin-rewards/
├── spec.md              # 機能仕様書（完成）
├── research.md          # 技術調査（完成）
├── data-model.md        # データモデル設計（完成）
├── plan.md              # このファイル（作成中）
├── contracts/           # Phase 1 output（API契約定義、次に作成）
│   ├── auth.yaml        # 認証API（POST /api/auth/login, /api/auth/child-login）
│   ├── rewards.yaml     # 景品CRUD（GET/POST/PUT/DELETE /api/rewards）
│   ├── exchange-requests.yaml  # 交換申請（POST /api/exchange-requests, PUT /approve, /reject）
│   └── parent-dashboard.yaml   # ダッシュボード（GET /api/parent/children, /pending-requests）
├── tasks.md             # Phase 2 output（/speckit.tasks コマンド）
└── quickstart.md        # Phase 1 output（開発者向けクイックスタート、次に作成）
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── GamifiedMathDrill.Api/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs                   # 新規: 認証API
│   │   │   ├── RewardsController.cs                # 拡張: CRUD + 画像アップロード
│   │   │   ├── ExchangeRequestsController.cs       # 新規: 交換申請・承認API
│   │   │   └── ParentDashboardController.cs        # 新規: 保護者ダッシュボードAPI
│   │   ├── DTOs/
│   │   │   ├── LoginRequest.cs                     # 新規
│   │   │   ├── LoginResponse.cs                    # 新規
│   │   │   ├── ChildLoginRequest.cs                # 新規
│   │   │   ├── RewardDto.cs                        # 拡張: Stock, ImagePath追加
│   │   │   ├── CreateRewardRequest.cs              # 新規
│   │   │   ├── UpdateRewardRequest.cs              # 新規
│   │   │   ├── ExchangeRequestDto.cs               # 拡張: Status, Approval情報追加
│   │   │   ├── CreateExchangeRequestRequest.cs     # 新規
│   │   │   ├── ApproveRequestRequest.cs            # 新規
│   │   │   └── RejectRequestRequest.cs             # 新規
│   │   ├── Middleware/
│   │   │   └── JwtAuthenticationMiddleware.cs      # 新規: JWT検証
│   │   ├── appsettings.json                        # 修正: JWT設定追加
│   │   └── Program.cs                              # 修正: Identity, JWT設定
│   ├── GamifiedMathDrill.Core/
│   │   ├── Models/
│   │   │   ├── UserRole.cs                         # 新規: Parent/Child enum
│   │   │   ├── Reward.cs                           # 拡張: Stock, CreatedBy追加
│   │   │   ├── RewardCategory.cs                   # 修正: 仮想→実物カテゴリ
│   │   │   ├── ExchangeRequest.cs                  # 新規: AcquiredRewardからリネーム・拡張
│   │   │   ├── ExchangeStatus.cs                   # 新規: Pending/Approved/Rejected
│   │   │   └── Student.cs                          # 削除: ApplicationUserに統合
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs                     # 新規: 認証サービス
│   │   │   ├── IRewardService.cs                   # 拡張: 画像保存、在庫管理
│   │   │   ├── IExchangeRequestService.cs          # 新規: 申請・承認ロジック
│   │   │   └── IImageStorageService.cs             # 新規: 画像保存処理
│   │   └── Services/
│   │       ├── AuthService.cs                      # 新規: JWT生成、PIN検証
│   │       ├── RewardService.cs                    # 拡張
│   │       ├── ExchangeRequestService.cs           # 新規
│   │       └── ImageStorageService.cs              # 新規
│   └── GamifiedMathDrill.Infrastructure/
│       ├── Identity/
│       │   └── ApplicationUser.cs                  # 新規: IdentityUser拡張
│       ├── Data/
│       │   ├── ApplicationDbContext.cs             # 拡張: Identity, 新エンティティ追加
│       │   └── Seed/
│       │       ├── RewardSeeder.cs                 # 修正: 実物景品データ
│       │       └── UserSeeder.cs                   # 新規: デフォルト保護者・子供
│       ├── Migrations/
│       │   └── 20260125_AddIdentityAndPhysicalRewards.cs  # 新規: マイグレーション
│       └── Repositories/
│           ├── RewardRepository.cs                 # 拡張: 在庫管理メソッド
│           └── ExchangeRequestRepository.cs        # 新規
└── tests/
    ├── GamifiedMathDrill.Tests.Unit/
    │   ├── AuthServiceTests.cs                     # 新規
    │   ├── ExchangeRequestServiceTests.cs          # 新規
    │   └── ImageStorageServiceTests.cs             # 新規
    └── GamifiedMathDrill.Tests.Integration/
        ├── AuthIntegrationTests.cs                 # 新規
        ├── RewardCrudIntegrationTests.cs           # 新規
        ├── ExchangeRequestFlowTests.cs             # 新規
        └── ConcurrencyTests.cs                     # 新規: 在庫競合テスト

frontend/
├── GamifiedMathDrill.Client/
│   ├── Pages/
│   │   ├── Login.razor                             # 新規: 保護者ログイン
│   │   ├── ChildLogin.razor                        # 新規: 子供ログイン
│   │   ├── ParentDashboard.razor                   # 新規: 保護者ダッシュボード
│   │   ├── RewardManagement.razor                  # 新規: 景品管理画面
│   │   ├── RewardForm.razor                        # 新規: 景品登録・編集フォーム
│   │   ├── ExchangeRequestList.razor               # 新規: 交換申請一覧
│   │   ├── ExchangeRequestDetail.razor             # 新規: 申請詳細・承認画面
│   │   ├── ChildrenManagement.razor                # 新規: 子供管理画面
│   │   ├── AcquiredRewardsPage.razor               # 修正: Status表示追加
│   │   └── Home.razor                              # 修正: 実物景品用UI
│   ├── Components/
│   │   ├── AuthGuard.razor                         # 新規: 認証チェック
│   │   ├── RoleBasedLayout.razor                   # 新規: ロール別レイアウト
│   │   ├── PendingRequestsBadge.razor              # 新規: 未承認バッジ
│   │   └── ImageUploader.razor                     # 新規: 画像アップロード
│   ├── Services/
│   │   ├── AuthService.cs                          # 新規: 認証API呼び出し
│   │   ├── TokenService.cs                         # 新規: JWT保存・取得
│   │   ├── RewardApiClient.cs                      # 拡張: CRUD API
│   │   ├── ExchangeRequestApiClient.cs             # 新規
│   │   └── PollingService.cs                       # 新規: 30秒ポーリング
│   ├── Models/
│   │   ├── UserRole.cs                             # 新規: バックエンドと同期
│   │   ├── RewardDto.cs                            # 拡張
│   │   ├── ExchangeRequestDto.cs                   # 拡張
│   │   └── ExchangeStatus.cs                       # 新規
│   └── wwwroot/
│       └── uploads/
│           └── rewards/                            # 新規: 画像保存先
└── tests/
    └── E2E/
        ├── ParentAuthFlowTests.cs                  # 新規
        ├── ChildAuthFlowTests.cs                   # 新規
        ├── RewardManagementFlowTests.cs            # 新規
        └── ExchangeApprovalFlowTests.cs            # 新規

.gitignore                                          # 修正: wwwroot/uploads/ 追加
```

**Structure Decision**: 既存の 3 層アーキテクチャ（Api, Core, Infrastructure）を維持しつつ、Identity 機能を追加。

- Backend: 新規 20+ ファイル、修正 8+ ファイル
- Frontend: 新規 15+ ファイル、修正 3 ファイル
- 破壊的変更: Student テーブル削除、Reward/AcquiredReward スキーマ変更

## Complexity Tracking

**Status**: ⚠️ **NEEDS REVIEW** - 大規模な破壊的変更を含む

**Introduced Complexities**:

1. **ASP.NET Core Identity 導入**
   - 理由: マルチユーザー認証が必須（保護者と子供の分離）
   - 軽減策: .NET 標準ライブラリを使用（カスタム実装を避ける）
   - 影響範囲: 全 API エンドポイントに認証・認可が必要

2. **楽観的ロック（RowVersion）**
   - 理由: 在庫のマイナス防止（同時申請時の競合）
   - 軽減策: Entity Framework Core の標準機能を活用
   - 影響範囲: Reward の更新処理、DbUpdateConcurrencyException 処理

3. **JWT 認証**
   - 理由: Blazor WebAssembly は stateless（Cookie ベース認証不可）
   - 軽減策: 標準ライブラリ使用、HttpOnly Cookie でトークン保存
   - 影響範囲: 全 API リクエストにトークン付与

4. **画像アップロード・保存**
   - 理由: 実物景品の視覚的な魅力が重要
   - 軽減策: ローカルファイルシステム（wwwroot/uploads/）、S3 は将来対応
   - 影響範囲: RewardController、ImageStorageService、.gitignore

5. **破壊的スキーマ変更**
   - 理由: Student → ApplicationUser 統合、仮想アイテム削除
   - 軽減策: マイグレーションスクリプトでデータ移行、テスト環境で検証
   - 影響範囲: 既存の全 Student 参照、シードデータ

**Justification**: これらの複雑性は、以下の理由で**正当化**されます。

- 保護者による監督が児童の安全性に不可欠（憲法 Principle II）
- 実物景品が学習動機向上に必須（憲法 Principle I、児童の要望）
- マルチユーザー認証は将来のスケーラビリティに必須
- 在庫管理はビジネスロジックの中核（景品切れの防止）

**Alternative Considered**:

- 段階的リリース（認証のみ先行）→ 却下（保護者管理と一体で価値を発揮）
- Cookie ベース認証 → 却下（Blazor WebAssembly は stateless）
- 外部認証（Auth0 等）→ 却下（小規模アプリには過剰、コスト増）

**Constitution Exception**: ✅ **APPROVED**

- Principle I（ユーザー中心）を満たすための必要な複雑性
- Principle II（セキュリティ）を満たすための必要な複雑性

---

## Phase Completion Summary

### Phase 0: Research ✅ COMPLETE

- [research.md](research.md) 作成完了
- 6 つの技術的疑問を解決（認証、画像、在庫、ステータス、Blazor、通知）
- すべての決定に根拠・代替案・実装例を記載

### Phase 1: Design & Contracts ⏳ IN PROGRESS

- [data-model.md](data-model.md) 作成完了
- [quickstart.md](quickstart.md) 作成予定（次のステップ）
- [contracts/](contracts/) ディレクトリ作成予定（API 定義）
  - `auth.yaml`: 認証 API
  - `rewards.yaml`: 景品 CRUD API
  - `exchange-requests.yaml`: 交換申請・承認 API
  - `parent-dashboard.yaml`: ダッシュボード API

### Phase 2: Task Breakdown ❌ NOT STARTED

- [tasks.md](tasks.md) `/speckit.tasks` コマンドで生成予定
- 実装タスクの詳細化・依存関係・見積もり

---

## Implementation Phases

### Phase 0: 環境準備と既存データ削除

**Goal**: Identity 導入前の準備、既存仮想アイテムの削除

**Tasks**:

1. ブランチ作成: `003-parent-admin-rewards`
2. 既存データ削除
   - `Rewards` テーブルの全レコード削除（仮想アイテム 30 件）
   - `AcquiredRewards` テーブルの全レコード削除（交換履歴）
3. NuGet パッケージ追加
   - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
   - `Microsoft.AspNetCore.Authentication.JwtBearer`
   - `System.IdentityModel.Tokens.Jwt`
4. テスト環境でマイグレーション実行確認

**Success Criteria**:

- ビルド成功
- 既存テスト PASS（データ削除のみなので影響なし）
- マイグレーション実行エラーなし

**Estimated Time**: 2 時間

---

### Phase 1: ASP.NET Core Identity 統合

**Goal**: ApplicationUser、UserRole、認証基盤の実装

**Tasks**:

1. **Models**:
   - `ApplicationUser.cs` 作成（IdentityUser 拡張）
   - `UserRole.cs` enum 作成
2. **Infrastructure**:
   - `ApplicationDbContext` を `IdentityDbContext<ApplicationUser>` に変更
   - Identity テーブルのマイグレーション作成・実行
   - `UserSeeder.cs` 作成（デフォルト保護者・子供）
3. **Program.cs**:
   - `AddIdentity<ApplicationUser, IdentityRole>()` 設定
   - `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` 設定
   - JWT 秘密鍵、Issuer、Audience 設定（appsettings.json）
4. **Unit Tests**:
   - ApplicationUser 作成テスト
   - パスワードハッシュ化テスト

**Success Criteria**:

- Identity テーブルが DB に作成される（AspNetUsers, AspNetRoles 等）
- デフォルトユーザーがシードされる
- 単体テスト PASS

**Estimated Time**: 6 時間

---

### Phase 2: 認証 API 実装

**Goal**: 保護者・子供のログイン API、JWT 生成

**Tasks**:

1. **DTOs**:
   - `LoginRequest.cs`, `LoginResponse.cs`
   - `ChildLoginRequest.cs`
2. **Services**:
   - `AuthService.cs` 作成
     - `LoginAsync(string email, string password)` → JWT 生成
     - `ChildLoginAsync(string childId, string pin)` → JWT 生成（Role=Child）
     - `GenerateJwtToken(ApplicationUser user)` メソッド
3. **Controllers**:
   - `AuthController.cs` 作成
     - `POST /api/auth/login` (保護者)
     - `POST /api/auth/child-login` (子供)
     - `POST /api/auth/logout` (トークン無効化は将来対応)
4. **Unit Tests**:
   - 保護者ログイン成功・失敗テスト
   - 子供ログイン成功・失敗テスト
   - JWT トークン検証テスト
5. **Integration Tests**:
   - 認証 API の E2E テスト

**Success Criteria**:

- `/api/auth/login` で JWT トークンが返される
- `/api/auth/child-login` で PIN 検証が動作
- 無効な認証情報で 401 Unauthorized
- テスト PASS（単体・統合）

**Estimated Time**: 8 時間

---

### Phase 3: Reward モデル拡張とシードデータ

**Goal**: 実物景品対応の Reward エンティティ、シードデータ投入

**Tasks**:

1. **Models**:
   - `Reward.cs` 拡張（Stock, IsPhysical, CreatedBy, RowVersion 追加）
   - `RewardCategory.cs` enum 修正（Badge/Avatar 削除、Snack/Card/Toy 追加）
2. **Migrations**:
   - `Rewards` テーブルのカラム追加マイグレーション
   - `Stock >= 0` CHECK 制約追加
3. **Seed**:
   - `RewardSeeder.cs` 修正（実物景品 10〜20 件）
     - 例: うまい棒（10pt）、ポケモンカード（200pt）、レゴミニセット（500pt）
4. **Unit Tests**:
   - Reward モデルのバリデーションテスト

**Success Criteria**:

- マイグレーション成功
- 実物景品データが DB に投入される
- Stock の負数登録が失敗する（CHECK 制約）
- テスト PASS

**Estimated Time**: 4 時間

---

### Phase 4: 景品管理 API（CRUD + 画像アップロード）

**Goal**: 保護者が景品を登録・編集・削除できる API

**Tasks**:

1. **DTOs**:
   - `RewardDto.cs` 拡張
   - `CreateRewardRequest.cs`, `UpdateRewardRequest.cs`
2. **Services**:
   - `ImageStorageService.cs` 作成
     - `SaveImageAsync(IFormFile file)` → ファイルパス返却
     - `DeleteImageAsync(string filePath)` → ファイル削除
   - `RewardService.cs` 拡張
     - `CreateRewardAsync(CreateRewardRequest)` → 画像保存 + DB 登録
     - `UpdateRewardAsync(int id, UpdateRewardRequest)` → 楽観的ロック処理
     - `DeleteRewardAsync(int id)` → IsActive=false（論理削除）
     - `GetRewardsByCreatorAsync(string parentId)` → 保護者の景品一覧
3. **Controllers**:
   - `RewardsController.cs` 拡張
     - `POST /api/rewards` [Authorize(Roles="Parent")]
     - `PUT /api/rewards/{id}` [Authorize(Roles="Parent")]
     - `DELETE /api/rewards/{id}` [Authorize(Roles="Parent")]
     - `GET /api/rewards` [Authorize] → 子供は自分の保護者の景品のみ
4. **Unit Tests**:
   - 画像保存・削除テスト（モック IFormFile）
   - 景品 CRUD ロジックテスト
   - 楽観的ロック競合テスト（DbUpdateConcurrencyException）
5. **Integration Tests**:
   - 画像アップロード付き景品登録（multipart/form-data）
   - 景品更新時の競合処理

**Success Criteria**:

- 保護者が画像付き景品を登録できる
- 画像が `wwwroot/uploads/rewards/` に保存される
- 子供は自分の保護者の景品のみ取得できる
- 同時更新時に DbUpdateConcurrencyException 発生
- テスト PASS（単体・統合）

**Estimated Time**: 10 時間

---

### Phase 5: ExchangeRequest 実装（申請機能）

**Goal**: 子供がポイントで景品を申請できる機能

**Tasks**:

1. **Models**:
   - `ExchangeRequest.cs` 作成（AcquiredReward からリネーム）
   - `ExchangeStatus.cs` enum 作成
   - マイグレーション作成・実行
2. **DTOs**:
   - `ExchangeRequestDto.cs` 拡張
   - `CreateExchangeRequestRequest.cs`
3. **Services**:
   - `ExchangeRequestService.cs` 作成
     - `CreateRequestAsync(string studentId, int rewardId)`
       - ポイント確認（不足なら例外）
       - 在庫確認（0 なら例外）
       - トランザクション開始
       - 在庫デクリメント（楽観的ロック）
       - ExchangeRequest 作成（Status=Pending）
       - トランザクションコミット
     - `GetRequestsByStudentAsync(string studentId)` → 子供の申請履歴
     - `CancelRequestAsync(int requestId, string studentId)` → Status=Cancelled、在庫戻す
4. **Controllers**:
   - `ExchangeRequestsController.cs` 作成
     - `POST /api/exchange-requests` [Authorize(Roles="Child")]
     - `GET /api/exchange-requests/my` [Authorize(Roles="Child")] → 自分の申請一覧
     - `PUT /api/exchange-requests/{id}/cancel` [Authorize(Roles="Child")]
5. **Unit Tests**:
   - ポイント不足時の例外テスト
   - 在庫不足時の例外テスト
   - 申請成功時の在庫減少テスト
   - キャンセル時の在庫復元テスト
6. **Integration Tests**:
   - 申請フロー E2E テスト
   - 同時申請の競合テスト（複数子供が同じ景品申請）

**Success Criteria**:

- 子供が景品を申請できる
- ポイント・在庫が不足時はエラー
- 申請後に在庫が減る
- キャンセル時に在庫が戻る
- 同時申請時にロック処理が動作
- テスト PASS

**Estimated Time**: 8 時間

---

### Phase 6: 承認・却下 API 実装

**Goal**: 保護者が申請を承認・却下できる機能

**Tasks**:

1. **DTOs**:
   - `ApproveRequestRequest.cs`, `RejectRequestRequest.cs`
2. **Services**:
   - `ExchangeRequestService.cs` 拡張
     - `ApproveRequestAsync(int requestId, string parentId)`
       - トランザクション開始
       - Status=Pending 確認（既に承認済みなら例外）
       - 子供のポイント再確認（ポイント不足なら例外）
       - 子供のポイント消費
       - Status=Approved、ApprovedAt、ApprovedBy 設定
       - トランザクションコミット
     - `RejectRequestAsync(int requestId, string parentId, string reason)`
       - Status=Rejected、RejectedAt、RejectionReason 設定
       - 在庫を戻す（申請時に既に減少済み）
3. **Controllers**:
   - `ExchangeRequestsController.cs` 拡張
     - `PUT /api/exchange-requests/{id}/approve` [Authorize(Roles="Parent")]
     - `PUT /api/exchange-requests/{id}/reject` [Authorize(Roles="Parent")]
4. **Unit Tests**:
   - 承認時のポイント消費テスト
   - 承認済み申請の再承認防止テスト
   - 却下時の在庫復元テスト
5. **Integration Tests**:
   - 承認フロー E2E テスト
   - 却下フロー E2E テスト

**Success Criteria**:

- 保護者が申請を承認できる
- 承認時に子供のポイントが消費される
- 却下時に在庫が戻る
- 重複承認が防止される
- テスト PASS

**Estimated Time**: 6 時間

---

### Phase 7: 保護者ダッシュボード API 実装

**Goal**: 未承認申請の一覧取得、子供一覧取得

**Tasks**:

1. **DTOs**:
   - `ParentDashboardSummaryDto.cs`（未承認件数、子供一覧、最近の申請）
2. **Services**:
   - `ParentDashboardService.cs` 作成（オプション、Controller から直接 Repository 呼び出しも可）
     - `GetPendingRequestsAsync(string parentId)` → 未承認申請一覧
     - `GetChildrenAsync(string parentId)` → 保護者の子供一覧
3. **Controllers**:
   - `ParentDashboardController.cs` 作成
     - `GET /api/parent/dashboard` [Authorize(Roles="Parent")] → サマリー
     - `GET /api/parent/pending-requests` [Authorize(Roles="Parent")] → 未承認一覧
     - `GET /api/parent/children` [Authorize(Roles="Parent")] → 子供一覧
4. **Unit Tests**:
   - 未承認申請の取得テスト
   - 子供一覧の取得テスト
5. **Integration Tests**:
   - ダッシュボード API の E2E テスト

**Success Criteria**:

- 保護者が未承認申請数を取得できる
- 保護者が子供一覧を取得できる
- 他の保護者のデータにアクセスできない（認可テスト）
- テスト PASS

**Estimated Time**: 4 時間

---

### Phase 8: フロントエンド - 認証画面実装

**Goal**: 保護者ログイン、子供ログイン画面

**Tasks**:

1. **Services**:
   - `AuthService.cs` 作成（API 呼び出し）
   - `TokenService.cs` 作成（JWT の保存・取得・削除）
2. **Pages**:
   - `Login.razor` 作成（保護者ログイン）
     - Email, Password フォーム
     - ログイン成功 → JWT 保存 → `/parent/dashboard` にリダイレクト
   - `ChildLogin.razor` 作成（子供ログイン）
     - 子供一覧（ボタン or カード）→ PIN 入力
     - ログイン成功 → JWT 保存 → `/home` にリダイレクト
3. **Components**:
   - `AuthGuard.razor` 作成（未認証時にログイン画面にリダイレクト）
4. **Layout**:
   - `MainLayout.razor` 修正（Role に応じてナビゲーションメニュー切り替え）
5. **E2E Tests**:
   - Playwright: 保護者ログインフロー
   - Playwright: 子供ログインフロー

**Success Criteria**:

- 保護者がメール・パスワードでログインできる
- 子供が名前・PIN でログインできる
- 未認証時は `/login` にリダイレクトされる
- E2E テスト PASS

**Estimated Time**: 6 時間

---

### Phase 9: フロントエンド - 景品管理画面実装

**Goal**: 保護者が景品を追加・編集・削除できる画面

**Tasks**:

1. **Services**:
   - `RewardApiClient.cs` 拡張（CRUD API 呼び出し）
2. **Pages**:
   - `RewardManagement.razor` 作成
     - 景品一覧（MudTable）
     - 新規追加ボタン → RewardForm に遷移
     - 編集ボタン → RewardForm に遷移
     - 削除ボタン → 確認ダイアログ → DELETE API
   - `RewardForm.razor` 作成
     - 名前、説明、必要ポイント、在庫、カテゴリ入力
     - 画像アップロード（ImageUploader コンポーネント）
     - 保存ボタン → POST/PUT API
3. **Components**:
   - `ImageUploader.razor` 作成（IBrowserFile → multipart/form-data）
4. **E2E Tests**:
   - Playwright: 景品登録フロー（画像アップロード含む）
   - Playwright: 景品編集・削除フロー

**Success Criteria**:

- 保護者が景品を登録できる（画像アップロード含む）
- 保護者が景品を編集・削除できる
- 画像がプレビュー表示される
- E2E テスト PASS

**Estimated Time**: 8 時間

---

### Phase 10: フロントエンド - 交換申請画面実装（子供）

**Goal**: 子供が景品を申請できる画面

**Tasks**:

1. **Services**:
   - `ExchangeRequestApiClient.cs` 作成
2. **Pages**:
   - `Home.razor` 修正（景品一覧表示、実物景品に変更）
     - 景品カード（画像、名前、ポイント、在庫表示）
     - 交換ボタン → 確認ダイアログ → POST /api/exchange-requests
   - `AcquiredRewardsPage.razor` 修正
     - Status バッジ表示（申請中/承認済み/却下/キャンセル）
     - 申請中のアイテムにキャンセルボタン
3. **E2E Tests**:
   - Playwright: 景品申請フロー
   - Playwright: 申請キャンセルフロー

**Success Criteria**:

- 子供が景品を申請できる
- 申請後に Status が「申請中」と表示される
- ポイント不足・在庫不足時にエラーメッセージ
- E2E テスト PASS

**Estimated Time**: 6 時間

---

### Phase 11: フロントエンド - 保護者ダッシュボード実装

**Goal**: 保護者が申請を承認・却下できる画面

**Tasks**:

1. **Services**:
   - `PollingService.cs` 作成（30 秒ごとに未承認数取得）
2. **Pages**:
   - `ParentDashboard.razor` 作成
     - 未承認件数バッジ（PendingRequestsBadge コンポーネント、ポーリング）
     - 子供一覧（カード表示、ポイント、最近の申請）
     - 未承認申請一覧へのリンク
   - `ExchangeRequestList.razor` 作成
     - 申請一覧（MudTable、Status フィルタ）
     - 詳細ボタン → ExchangeRequestDetail に遷移
   - `ExchangeRequestDetail.razor` 作成
     - 子供の名前、景品名、必要ポイント、申請日時
     - 承認ボタン → 確認ダイアログ → PUT /approve
     - 却下ボタン → 理由入力ダイアログ → PUT /reject
   - `ChildrenManagement.razor` 作成（将来対応: 子供の追加・編集）
3. **Components**:
   - `PendingRequestsBadge.razor` 作成（未承認数表示、ポーリング）
4. **E2E Tests**:
   - Playwright: 申請承認フロー
   - Playwright: 申請却下フロー
   - Playwright: ポーリングによる未承認数更新

**Success Criteria**:

- 保護者が未承認申請一覧を確認できる
- 保護者が申請を承認・却下できる
- 未承認バッジが 30 秒ごとに更新される
- E2E テスト PASS

**Estimated Time**: 10 時間

---

### Phase 12: テスト・バグ修正・ドキュメント

**Goal**: 全機能の統合テスト、バグ修正、README 更新

**Tasks**:

1. **Integration Tests**:
   - 全フローの E2E テスト実行（保護者・子供の両視点）
   - パフォーマンステスト（NFR-002, NFR-003 検証）
2. **Bug Fixes**:
   - 発見されたバグの修正
   - エッジケースの追加テスト
3. **Documentation**:
   - README.md 更新（認証方法、初期ユーザー、環境変数）
   - quickstart.md 更新（セットアップ手順）
   - API ドキュメント生成（Swagger）
4. **Code Review**:
   - コードレビュー、リファクタリング
   - セキュリティレビュー（JWT、画像アップロード）

**Success Criteria**:

- 全テスト PASS（単体・統合・E2E）
- パフォーマンス基準を満たす（3 秒、1 秒）
- README が最新状態
- セキュリティ脆弱性なし

**Estimated Time**: 8 時間

---

## Total Estimated Time

| Phase    | タスク                                | 時間    |
| -------- | ------------------------------------- | ------- |
| 0        | 環境準備と既存データ削除              | 2h      |
| 1        | ASP.NET Core Identity 統合            | 6h      |
| 2        | 認証 API 実装                         | 8h      |
| 3        | Reward モデル拡張とシードデータ       | 4h      |
| 4        | 景品管理 API（CRUD + 画像）           | 10h     |
| 5        | ExchangeRequest 実装（申請）          | 8h      |
| 6        | 承認・却下 API 実装                   | 6h      |
| 7        | 保護者ダッシュボード API              | 4h      |
| 8        | フロントエンド - 認証画面             | 6h      |
| 9        | フロントエンド - 景品管理             | 8h      |
| 10       | フロントエンド - 交換申請（子供）     | 6h      |
| 11       | フロントエンド - 保護者ダッシュボード | 10h     |
| 12       | テスト・バグ修正・ドキュメント        | 8h      |
| **合計** |                                       | **86h** |

**推定期間**: 約 11〜15 営業日（1 日 6〜8 時間作業の場合）

---

## Risk Management

### High Risks

| リスク                             | 影響 | 確率 | 軽減策                                     |
| ---------------------------------- | ---- | ---- | ------------------------------------------ |
| マイグレーション失敗（データ損失） | 高   | 中   | テスト環境で事前検証、バックアップ取得     |
| JWT トークンの漏洩                 | 高   | 低   | HttpOnly Cookie 使用、HTTPS 必須           |
| 在庫競合による不整合               | 中   | 中   | 楽観的ロック、トランザクション、テスト強化 |
| 画像アップロードのセキュリティ     | 中   | 低   | MIME type 検証、ファイルサイズ制限         |

### Medium Risks

| リスク                           | 影響 | 確率 | 軽減策                                    |
| -------------------------------- | ---- | ---- | ----------------------------------------- |
| パフォーマンス劣化（ポーリング） | 中   | 中   | 30 秒間隔、キャッシュ活用                 |
| 既存機能の破壊（Student 削除）   | 中   | 中   | 回帰テスト、段階的リリース                |
| 子供の PIN 忘れ                  | 低   | 高   | 保護者による PIN リセット機能（将来対応） |

---

## Success Criteria

### Functional Success

- ✅ 保護者が景品を登録・編集・削除できる
- ✅ 子供が景品を申請できる
- ✅ 保護者が申請を承認・却下できる
- ✅ 未承認申請数がリアルタイムで更新される（30 秒ポーリング）
- ✅ 在庫が正しく管理される（同時申請時も）

### Non-Functional Success

- ✅ 保護者ダッシュボード初期表示 3 秒以内（NFR-002）
- ✅ 未承認申請取得 1 秒以内（NFR-003）
- ✅ 全テスト PASS（単体・統合・E2E）
- ✅ セキュリティ脆弱性なし（JWT、画像アップロード）

### User Acceptance

- ✅ 保護者が景品管理を容易に実施できる
- ✅ 子供が実物景品に興味を示す
- ✅ 承認フローが保護者の監督下で適切に機能する

---

## Notes

- **破壊的変更**: Student テーブル削除により、既存データのマイグレーションが必要
- **段階的リリース**: Phase 8（認証画面）完了時点で中間デモ可能
- **将来対応**:
  - 子供の追加・編集機能（保護者画面）
  - PIN リセット機能
  - 画像の S3 保存（現在はローカルファイルシステム）
  - 通知機能の WebSocket 化（現在はポーリング）
  - 交換履歴の CSV エクスポート

---

## Next Steps

1. ✅ `contracts/` ディレクトリに API 定義を作成（OpenAPI/Swagger YAML）
2. ✅ `quickstart.md` を作成（開発者向けセットアップ手順）
3. ⏳ `/speckit.tasks` コマンドで `tasks.md` を生成
4. ⏳ Phase 0 開始（既存データ削除、NuGet パッケージ追加）
