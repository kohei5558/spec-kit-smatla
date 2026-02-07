# Implementation Plan: Authentication UI Pages

**Branch**: `004-auth-ui-pages` | **Date**: 2026年2月7日 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/004-auth-ui-pages/spec.md`

## Summary

保護者向けの認証UI（ログイン、パスワードリセット、新規登録）を実装します。既存の認証APIとBlazor WebAssemblyフロントエンドに統合し、ユーザーフレンドリーでセキュアな認証フローを提供します。主な要素：

- **ログイン画面**: メール/パスワード認証、「ログイン状態を保持する」オプション、IPベースのレート制限
- **パスワードリセット画面**: メール送信、リセットトークン検証、新パスワード設定
- **新規登録画面**: 最小限の情報（メールアドレス、表示名、パスワード）での保護者アカウント作成
- **セキュリティ**: レート制限、トークン有効期限管理、適切なエラーメッセージ

## Technical Context

**Language/Version**: C# / .NET 8.0  
**Primary Dependencies**:

- Backend: ASP.NET Core 8.0, ASP.NET Core Identity, JWT Bearer Authentication
- Frontend: Blazor WebAssembly 8.0, MudBlazor 7.15.0, Blazored.LocalStorage 4.3.0
  **Storage**: PostgreSQL (via Entity Framework Core)  
  **Testing**: xUnit (backend), bUnit (frontend Blazor components)  
  **Target Platform**: Web (オンライン必須アーキテクチャ)  
  **Project Type**: Web application (backend + frontend)  
  **Performance Goals**:
- ログイン処理 5秒以内
- 新規登録 3分以内（ユーザー操作含む）
- パスワードリセットメール送信 1分以内
  **Constraints**:
- セキュアなパスワード管理（ハッシュ化、強度検証）
- HTTPS必須
- JWT有効期限管理（セッション/長期保持の切り替え）
- IPベースのレート制限実装
  **Scale/Scope**:
- 3つのページ（Login, Password Reset, Register）
- 4-6つの新規バックエンドエンドポイント
- 既存の認証インフラに統合

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### I. User-Centric Design ✅ PASS

- 保護者向けの明確で直感的なUI設計
- エラーメッセージはユーザーフレンドリー
- モバイル/タブレット/デスクトップ対応（レスポンシブデザイン）
- アクセシビリティ基準（WCAG 2.1 AA）への配慮

### II. Data Privacy & Child Safety ✅ PASS

- 最小限の情報のみ収集（メールアドレス、表示名、パスワード）
- パスワードは常にハッシュ化して保存
- HTTPS必須（通信の暗号化）
- セキュアなJWT管理（有効期限、セキュアストレージ）
- 児童の個人情報は収集しない（保護者アカウントのみ）

### III. Specification-First Development ✅ PASS

- 詳細な仕様書作成済み（spec.md）
- 明確化プロセス完了（5つの質問に回答済み）
- すべての要件がテスト可能
- Edge caseと受入基準が定義済み

### IV. Test-Driven Quality ✅ PASS

- すべての機能要件に対応するテストケース定義
- 既存のテストインフラ活用（xUnit、統合テスト）
- パフォーマンス基準明記（5秒以内のログイン、1分以内のメール送信）

### V. Continuous Learning & Adaptation ✅ PASS

- ユーザーフィードバックに基づく改善可能な設計
- 将来的な拡張を考慮（MFA、ソーシャルログイン等はOut of Scope）
- セキュリティベストプラクティスの継続的適用

### Compliance: 技術スタック ✅ PASS

- ASP.NET Core 8.0使用
- C#プログラミング言語
- オンライン必須アーキテクチャ準拠
- 既存のバックエンド（ASP.NET Core Identity、JWT）に統合

**Overall Status**: ✅ ALL GATES PASSED - Ready for implementation

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

## Project Structure

### Documentation (this feature)

```text
specs/004-auth-ui-pages/
├── spec.md              # 機能仕様書（完了）
├── plan.md              # This file - 実装計画
├── research.md          # Phase 0 output - 技術調査
├── data-model.md        # Phase 1 output - データモデル
├── quickstart.md        # Phase 1 output - 開発者向けガイド
├── contracts/           # Phase 1 output - API契約
│   ├── auth-register.yml
│   ├── auth-forgot-password.yml
│   └── auth-reset-password.yml
├── checklists/
│   └── requirements.md  # 品質チェックリスト（完了）
└── tasks.md             # Phase 2 output - タスクリスト（/speckit.tasks）
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── GamifiedMathDrill.Api/
│   │   ├── Controllers/
│   │   │   └── AuthController.cs         # [UPDATE] 新規エンドポイント追加
│   │   ├── DTOs/
│   │   │   ├── RegisterRequest.cs        # [NEW] 新規登録リクエスト
│   │   │   ├── ForgotPasswordRequest.cs  # [NEW] パスワードリセット要求
│   │   │   └── ResetPasswordRequest.cs   # [NEW] パスワードリセット実行
│   │   └── Middleware/
│   │       └── RateLimitMiddleware.cs    # [NEW] レート制限ミドルウェア
│   ├── GamifiedMathDrill.Core/
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs           # [UPDATE] メソッド追加
│   │   │   └── IEmailService.cs          # [NEW] メール送信インターフェース
│   │   └── Models/
│   │       └── PasswordResetToken.cs     # [NEW] リセットトークンモデル
│   └── GamifiedMathDrill.Infrastructure/
│       ├── Services/
│       │   ├── AuthService.cs            # [UPDATE] 新規登録/リセット機能追加
│       │   └── EmailService.cs           # [NEW] メール送信サービス
│       ├── Data/
│       │   └── ApplicationDbContext.cs   # [UPDATE] PasswordResetToken追加
│       └── Migrations/                   # [NEW] マイグレーションファイル
└── tests/
    └── GamifiedMathDrill.Tests.Integration/
        ├── AuthRegisterTests.cs          # [NEW] 新規登録テスト
        ├── PasswordResetTests.cs         # [NEW] パスワードリセットテスト
        └── RateLimitTests.cs             # [NEW] レート制限テスト

frontend/
├── GamifiedMathDrill.Client/
│   ├── Pages/
│   │   ├── Login.razor                   # [UPDATE] ログイン状態保持オプション追加
│   │   ├── Register.razor                # [NEW] 新規登録ページ
│   │   ├── ForgotPassword.razor          # [NEW] パスワードリセット要求ページ
│   │   └── ResetPassword.razor           # [NEW] パスワードリセット実行ページ
│   ├── Services/
│   │   ├── AuthService.cs                # [UPDATE] 新規登録/リセットメソッド追加
│   │   └── TokenService.cs               # [UPDATE] 長期保持トークン対応
│   └── Models/
│       ├── RegisterRequest.cs            # [NEW] 新規登録リクエストモデル
│       ├── ForgotPasswordRequest.cs      # [NEW] パスワードリセット要求モデル
│       └── ResetPasswordRequest.cs       # [NEW] パスワードリセット実行モデル
```

**Structure Decision**: 既存のWeb applicationの構造（backend + frontend）を維持します。バックエンドは既存の認証インフラ（ASP.NET Core Identity、JWT）に統合し、フロントエンドは既存のBlazorページ、サービス、コンポーネントパターンに従います。

## Complexity Tracking

**No violations detected** - All Constitution gates passed without requiring justification.

---

## Phase 0: Research & Technical Decisions

**Status**: ✅ COMPLETE  
**Output**: [research.md](research.md)

### Research Tasks Completed

1. **セッション管理とJWTトークンの保持戦略**
   - Decision: ユーザー選択可能な保持期間（チェックボックスで切り替え）
   - Implementation: セッションストレージ（60分）vs ローカルストレージ（30日）

2. **パスワードリセットメール送信とエラーハンドリング**
   - Decision: 常に成功メッセージを返す（セキュリティ優先）
   - Implementation: メール送信失敗時もユーザーには成功を通知、内部ログに記録

3. **新規アカウント作成時の情報収集**
   - Decision: 最小限（メールアドレス、表示名、パスワードのみ）
   - Rationale: MVP原則、離脱率低減、プライバシー保護

4. **ブルートフォース攻撃対策とレート制限**
   - Decision: IPベースのレート制限（15分/5回失敗でブロック）
   - Implementation: RateLimitMiddleware実装

5. **パスワードリセットリンクの有効期限切れ処理**
   - Decision: 明確なエラーメッセージ + 再試行導線
   - Implementation: トークン検証エンドポイント追加

6. **メール送信サービスの選択**
   - Decision: IEmailSenderインターフェース実装（SMTP/SendGrid切り替え可能）
   - Implementation: EmailService with configurable provider

7. **フロントエンドUIライブラリとコンポーネント設計**
   - Decision: 既存のMudBlazorを活用
   - Components: MudTextField, MudButton, MudAlert, MudCheckBox

8. **パスワード強度検証**
   - Decision: サーバー/クライアント両方で検証
   - Requirements: 8文字以上、大文字・小文字・数字・記号各1文字以上

### All NEEDS CLARIFICATION Resolved

すべての技術的不明点が解決され、実装の準備が整いました。

---

## Phase 1: Design & Contracts

**Status**: ✅ COMPLETE  
**Outputs**:

- [data-model.md](data-model.md) - データモデルとエンティティ定義
- [contracts/auth-api.yaml](contracts/auth-api.yaml) - OpenAPI仕様
- [quickstart.md](quickstart.md) - 開発者向けクイックスタートガイド
- エージェントコンテキスト更新完了

### Data Model Summary

#### 新規エンティティ

1. **PasswordResetToken**
   - Fields: Id, UserId, TokenHash, CreatedAt, ExpiresAt, IsUsed, UsedAt, IpAddress
   - Relationships: ApplicationUser (1:N)
   - Indexes: UserId, TokenHash (UNIQUE), ExpiresAt

#### DTOs Created

- RegisterRequest, RegisterResponse
- ForgotPasswordRequest, ForgotPasswordResponse
- ResetPasswordRequest, ResetPasswordResponse
- LoginRequest (updated), LoginResponse (updated)

#### Repository Pattern

- IPasswordResetTokenRepository
- PasswordResetTokenRepository implementation

### API Contracts Summary

#### 新規エンドポイント

1. **POST /api/auth/register** - 新規保護者アカウント作成
2. **POST /api/auth/forgot-password** - パスワードリセットメール送信
3. **POST /api/auth/reset-password** - パスワードリセット実行
4. **GET /api/auth/validate-reset-token** - トークン検証

#### 既存エンドポイント更新

- **POST /api/auth/login** - rememberMeフラグ追加

### Database Migration

- Migration名: `AddPasswordResetTokens`
- テーブル: PasswordResetTokens
- 関連: ApplicationUser (FK)

---

## Phase 2: Implementation Breakdown

**Status**: 🔄 READY FOR EXECUTION  
**Next Command**: `/speckit.tasks` to generate detailed task breakdown

### Backend Implementation (Est. 8-12 hours)

#### Task Group 1: Core Infrastructure (3-4h)

1. **PasswordResetToken Entity & Migration**
   - Create entity model
   - Configure EF Core relationships
   - Generate and apply migration
   - Test: Verify database schema

2. **Repository Implementation**
   - Implement IPasswordResetTokenRepository
   - Add CRUD methods
   - Test: Unit tests for repository methods

3. **Email Service**
   - Implement IEmailService
   - Configure SMTP settings
   - Add password reset email template
   - Test: Mock email sending in unit tests

#### Task Group 2: Authentication Service Updates (2-3h)

4. **Register Method**
   - Add RegisterAsync to AuthService
   - Validate password strength
   - Check email uniqueness
   - Create ApplicationUser
   - Test: Integration tests for registration flow

5. **Password Reset Methods**
   - Add SendPasswordResetEmailAsync
   - Add ValidateResetTokenAsync
   - Add ResetPasswordAsync
   - Test: Integration tests for reset flow

6. **Login Method Update**
   - Add rememberMe parameter
   - Adjust JWT expiry based on flag
   - Test: Verify token expiry times

#### Task Group 3: API Endpoints (2-3h)

7. **AuthController Updates**
   - Add POST /api/auth/register endpoint
   - Add POST /api/auth/forgot-password endpoint
   - Add POST /api/auth/reset-password endpoint
   - Add GET /api/auth/validate-reset-token endpoint
   - Update POST /api/auth/login endpoint
   - Test: Integration tests for all endpoints

8. **Rate Limit Middleware**
   - Implement RateLimitMiddleware
   - Configure for /api/auth/login
   - Store failed attempts (in-memory)
   - Test: Simulate brute force attacks

#### Task Group 4: DTOs and Validation (1h)

9. **Create DTOs**
   - RegisterRequest, RegisterResponse
   - ForgotPasswordRequest, ForgotPasswordResponse
   - ResetPasswordRequest, ResetPasswordResponse
   - Update LoginRequest, LoginResponse
   - Test: Validation attribute tests

### Frontend Implementation (Est. 8-10 hours)

#### Task Group 5: Service Layer Updates (2-3h)

10. **AuthService Updates**
    - Add RegisterAsync method
    - Add ForgotPasswordAsync method
    - Add ResetPasswordAsync method
    - Add ValidateResetTokenAsync method
    - Update LoginAsync (rememberMe parameter)
    - Test: Unit tests with HttpClient mocks

11. **TokenService Updates**
    - Add rememberMe parameter to SaveTokenAsync
    - Implement session vs local storage logic
    - Add GetStorageType method
    - Test: Storage behavior tests

#### Task Group 6: UI Components (4-5h)

12. **Register.razor Page**
    - Create component structure
    - Add form with MudBlazor components
    - Implement client-side validation
    - Add error/success messaging
    - Connect to AuthService
    - Test: bUnit component tests

13. **ForgotPassword.razor Page**
    - Create component structure
    - Add email input form
    - Implement submission logic
    - Add success message display
    - Test: bUnit component tests

14. **ResetPassword.razor Page**
    - Create component structure
    - Parse token from query string
    - Validate token on load
    - Add password reset form
    - Handle expired token scenario
    - Test: bUnit component tests

15. **Login.razor Updates**
    - Add "ログイン状態を保持する" checkbox
    - Update login call with rememberMe flag
    - Test: Updated component tests

#### Task Group 7: Routing & Navigation (1h)

16. **Route Configuration**
    - Add /register route
    - Add /forgot-password route
    - Add /reset-password route
    - Update navigation links in Login page
    - Test: Navigation flow tests

### Testing (Est. 4-6 hours)

#### Task Group 8: Backend Tests (2-3h)

17. **Unit Tests**
    - AuthService method tests
    - Repository tests
    - Email service tests
    - DTO validation tests

18. **Integration Tests**
    - Full registration flow
    - Password reset flow
    - Rate limiting behavior
    - Token expiry scenarios

#### Task Group 9: Frontend Tests (2-3h)

19. **Component Tests (bUnit)**
    - Register.razor tests
    - ForgotPassword.razor tests
    - ResetPassword.razor tests
    - Updated Login.razor tests

20. **End-to-End Flow Tests**
    - Registration → Login flow
    - Forgot Password → Reset → Login flow
    - Remember Me functionality

### Documentation & Deployment (Est. 2-3 hours)

#### Task Group 10: Documentation (1-2h)

21. **API Documentation**
    - Update Swagger annotations
    - Add request/response examples
    - Document error codes

22. **Developer Documentation**
    - Update README.md
    - Add environment variable documentation
    - Create deployment checklist

#### Task Group 11: Code Review & QA (1h)

23. **Code Review Preparation**
    - Run all tests
    - Check code coverage
    - Run linter
    - Update CHANGELOG.md

**Total Estimated Time**: 22-31 hours

---

## Implementation Phases Summary

### ✅ Phase 0: Research (COMPLETE)

- 8 technical decisions documented
- All alternatives evaluated
- Implementation approaches defined

### ✅ Phase 1: Design & Contracts (COMPLETE)

- Data model with 1 new entity
- 6 new DTOs + 2 updated DTOs
- 4 new API endpoints + 1 updated endpoint
- Repository interface and implementation
- OpenAPI specification created
- Quickstart guide for developers

### 🔄 Phase 2: Implementation (READY)

- Backend: 9 task groups, 22-31 hours estimated
- Frontend: 7 task groups
- Testing: 2 task groups
- Documentation: 2 task groups

### ⏳ Phase 3: Tasks Breakdown (PENDING)

- Run `/speckit.tasks` to generate detailed task list
- Assign task priorities and dependencies
- Create GitHub issues/cards

---

## Dependencies & Integration Points

### Existing Components (DO NOT MODIFY)

- ApplicationUser entity (ASP.NET Core Identity)
- Child entity and child login flow
- JWT token generation infrastructure
- Blazored.LocalStorage package

### New Dependencies Required

- **System.Net.Mail** (built-in, for EmailService)
- **Optional**: SendGrid or other email provider SDK

### Integration Points

1. **AuthController**: Add new endpoints alongside existing Login/ChildLogin
2. **AuthService**: Extend with new methods (Register, ForgotPassword, ResetPassword)
3. **ApplicationDbContext**: Add PasswordResetTokens DbSet
4. **TokenService**: Update to support session vs local storage
5. **Program.cs**: Register new services and middleware

---

## Risk Assessment & Mitigation

### Security Risks

1. **User Enumeration via Email**
   - Mitigation: Always return success message regardless of account existence

2. **Brute Force Attacks**
   - Mitigation: IP-based rate limiting (15min/5 attempts)

3. **Token Replay Attacks**
   - Mitigation: Single-use tokens with IsUsed flag

4. **Weak Passwords**
   - Mitigation: Server + client validation (8+ chars, complexity requirements)

### Technical Risks

1. **Email Delivery Failures**
   - Mitigation: Log failures, don't expose to user, manual admin intervention if needed

2. **Token Expiry Edge Cases**
   - Mitigation: Clear error messages with re-request option

3. **Session Storage Limitations**
   - Mitigation: Fallback to localStorage if sessionStorage unavailable

### Performance Risks

1. **Database Query Performance**
   - Mitigation: Proper indexing on PasswordResetTokens (TokenHash, UserId, ExpiresAt)

2. **Rate Limit Memory Usage**
   - Mitigation: In-memory dictionary with periodic cleanup of old entries

---

## Success Criteria (from spec.md)

This implementation must satisfy all 8 success criteria from [spec.md](spec.md):

1. ✅ **SC-001**: 保護者が新規アカウントを作成し、メールアドレスとパスワードでログインできる
2. ✅ **SC-002**: パスワードを忘れた保護者がメールでリセットリンクを受信できる
3. ✅ **SC-003**: すべてのパスワードが安全にハッシュ化されて保存される
4. ✅ **SC-004**: 不正なログイン試行が制限される（レート制限）
5. ✅ **SC-005**: ログイン状態の保持オプションが提供され、選択に応じてセッション管理される
6. ✅ **SC-006**: すべての認証エンドポイントが適切なHTTPステータスコードとエラーメッセージを返す
7. ✅ **SC-007**: UIがモバイル、タブレット、デスクトップで正しく表示される（MudBlazor）
8. ✅ **SC-008**: ログインとパスワードリセットの処理時間が要求を満たす（5秒以内、1分以内）

---

## Next Steps

1. **Run `/speckit.tasks`** to generate detailed task breakdown with priorities
2. **Create GitHub issues** from task list
3. **Start implementation** following task order
4. **Write tests first** (TDD approach per Constitution Principle IV)
5. **Code review** after each task group completion
6. **Integration testing** after backend + frontend completion
7. **User acceptance testing** with stakeholders
8. **Deployment** to staging environment
9. **Production release** after final approval

---

## References

- [Feature Specification](spec.md)
- [Technical Research](research.md)
- [Data Model](data-model.md)
- [API Contracts](contracts/auth-api.yaml)
- [Quickstart Guide](quickstart.md)
- [Project Constitution](../../.specify/memory/constitution.md)
