# Tasks: 保護者管理画面と実物景品交換システム

**Input**: Design documents from `/specs/003-parent-admin-rewards/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/  
**Feature Branch**: `003-parent-admin-rewards`

**Tests**: Tests are NOT included in this feature (no TDD requirement specified)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `- [ ] [ID] [P?] [Story?] Description with file path`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4, US5)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: ブランチ作成、既存データ削除、NuGetパッケージ追加

**Estimated Time**: 2 hours

- [ ] T001 Create branch `003-parent-admin-rewards` from main
- [ ] T002 Delete existing virtual reward data (Rewards table) using SQL or EF migration
- [ ] T003 Delete existing exchange history data (AcquiredRewards table)
- [ ] T004 [P] Add NuGet package Microsoft.AspNetCore.Identity.EntityFrameworkCore to backend/src/GamifiedMathDrill.Infrastructure/GamifiedMathDrill.Infrastructure.csproj
- [ ] T005 [P] Add NuGet package Microsoft.AspNetCore.Authentication.JwtBearer to backend/src/GamifiedMathDrill.Api/GamifiedMathDrill.Api.csproj
- [ ] T006 [P] Add NuGet package System.IdentityModel.Tokens.Jwt to backend/src/GamifiedMathDrill.Api/GamifiedMathDrill.Api.csproj
- [ ] T007 Add JWT configuration to backend/src/GamifiedMathDrill.Api/appsettings.Development.json (SecretKey, Issuer, Audience, ExpiryMinutes)
- [ ] T008 Create uploads directory at frontend/GamifiedMathDrill.Client/wwwroot/uploads/rewards/
- [ ] T009 Update .gitignore to exclude wwwroot/uploads/

**Checkpoint**: Environment ready - Foundation phase can begin

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: ASP.NET Core Identity統合、認証基盤の実装。全てのUser Storyで必要な基盤。

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

**Estimated Time**: 14 hours

### Identity Framework Setup

- [ ] T010 [P] Create UserRole enum in backend/src/GamifiedMathDrill.Core/Models/UserRole.cs (Parent=0, Child=1)
- [ ] T011 Create ApplicationUser class extending IdentityUser in backend/src/GamifiedMathDrill.Infrastructure/Identity/ApplicationUser.cs (Role, PIN, ParentId, DisplayName, AvatarUrl, CreatedAt, IsActive)
- [ ] T012 Update ApplicationDbContext to extend IdentityDbContext<ApplicationUser> in backend/src/GamifiedMathDrill.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T013 Create EF migration for Identity tables using `dotnet ef migrations add AddIdentityTables`
- [ ] T014 Apply migration to database using `dotnet ef database update`

### Authentication Services

- [ ] T015 [P] Create LoginRequest DTO in backend/src/GamifiedMathDrill.Api/DTOs/LoginRequest.cs (Email, Password)
- [ ] T016 [P] Create LoginResponse DTO in backend/src/GamifiedMathDrill.Api/DTOs/LoginResponse.cs (Token, UserId, DisplayName, Role, ParentId, ExpiresAt)
- [ ] T017 [P] Create ChildLoginRequest DTO in backend/src/GamifiedMathDrill.Api/DTOs/ChildLoginRequest.cs (ChildId, PIN)
- [ ] T018 Create IAuthService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IAuthService.cs (LoginAsync, ChildLoginAsync, GenerateJwtToken)
- [ ] T019 Create AuthService implementation in backend/src/GamifiedMathDrill.Core/Services/AuthService.cs (JWT generation, PIN verification using UserManager)
- [ ] T020 Create AuthController in backend/src/GamifiedMathDrill.Api/Controllers/AuthController.cs (POST /api/auth/login, POST /api/auth/child-login)

### Program.cs Configuration

- [ ] T021 Configure ASP.NET Core Identity in backend/src/GamifiedMathDrill.Api/Program.cs (AddIdentity<ApplicationUser, IdentityRole>)
- [ ] T022 Configure JWT authentication in backend/src/GamifiedMathDrill.Api/Program.cs (AddAuthentication, AddJwtBearer with validation parameters)
- [ ] T023 Add authentication middleware to request pipeline in backend/src/GamifiedMathDrill.Api/Program.cs (UseAuthentication, UseAuthorization)

### Seed Data

- [ ] T024 Create UserSeeder class in backend/src/GamifiedMathDrill.Infrastructure/Data/Seed/UserSeeder.cs (default parent with email "parent@example.com", 2 child accounts with PINs)
- [ ] T025 Update ApplicationDbContext to call UserSeeder in OnModelCreating or separate seed method

**Checkpoint**: Foundation ready - User Story 4 (Authentication) and all other user stories can now begin

---

## Phase 3: User Story 4 - 保護者と子供のアカウントでログインする (Priority: P1) 🎯 MVP

**Goal**: 保護者・子供の認証フローを実装し、ロールベースのアクセス制御を確立

**Independent Test**: 保護者がメール・パスワードでログインし管理画面にアクセスできる。子供がPINでログインし問題解答画面にアクセスできる。子供は管理画面にアクセスできない。

**Estimated Time**: 12 hours

### Backend Models & DTOs (US4)

- [ ] T026 [P] [US4] Create ErrorResponse DTO in backend/src/GamifiedMathDrill.Api/DTOs/ErrorResponse.cs (Error, Message, Details)

### Frontend Services (US4)

- [ ] T027 [P] [US4] Create UserRole enum in frontend/GamifiedMathDrill.Client/Models/UserRole.cs (Parent, Child - sync with backend)
- [ ] T028 [P] [US4] Create LoginRequest model in frontend/GamifiedMathDrill.Client/Models/LoginRequest.cs
- [ ] T029 [P] [US4] Create LoginResponse model in frontend/GamifiedMathDrill.Client/Models/LoginResponse.cs
- [ ] T030 [P] [US4] Create ChildLoginRequest model in frontend/GamifiedMathDrill.Client/Models/ChildLoginRequest.cs
- [ ] T031 Create TokenService in frontend/GamifiedMathDrill.Client/Services/TokenService.cs (SaveToken, GetToken, RemoveToken using localStorage or sessionStorage)
- [ ] T032 Create AuthService in frontend/GamifiedMathDrill.Client/Services/AuthService.cs (LoginAsync calling POST /api/auth/login, ChildLoginAsync calling POST /api/auth/child-login)

### Frontend Pages (US4)

- [ ] T033 [P] [US4] Create Login.razor page in frontend/GamifiedMathDrill.Client/Pages/Login.razor (parent login form with email/password, MudTextField, MudButton)
- [ ] T034 [P] [US4] Create ChildLogin.razor page in frontend/GamifiedMathDrill.Client/Pages/ChildLogin.razor (child selection with avatar/name buttons, PIN input dialog)
- [ ] T035 Create AuthGuard component in frontend/GamifiedMathDrill.Client/Components/AuthGuard.razor (redirect to /login if not authenticated)
- [ ] T036 Create RoleBasedLayout component in frontend/GamifiedMathDrill.Client/Components/RoleBasedLayout.razor (different navigation for Parent vs Child role)

### Frontend Navigation & Layout (US4)

- [ ] T037 [US4] Update MainLayout.razor in frontend/GamifiedMathDrill.Client/Layout/MainLayout.razor (integrate RoleBasedLayout, show logout button)
- [ ] T038 [US4] Update App.razor in frontend/GamifiedMathDrill.Client/App.razor (add authentication state provider if needed)

**Checkpoint**: User Story 4 complete - Authentication system functional, Parent and Child can log in with different credentials

---

## Phase 4: User Story 1 - 保護者が実物景品を登録・管理する (Priority: P1) 🎯 MVP

**Goal**: 保護者が実物景品をCRUDできる機能を実装。画像アップロード、在庫管理、楽観的ロック対応。

**Independent Test**: 保護者がログイン後、景品を新規登録（画像含む）、編集、削除でき、データベースに反映される。子供画面で登録した景品が表示される。

**Estimated Time**: 18 hours

### Backend Models (US1)

- [ ] T039 [P] [US1] Update RewardCategory enum in backend/src/GamifiedMathDrill.Core/Models/RewardCategory.cs (remove Badge/Avatar/Character, add Snack=0, Card=1, Toy=2, Stationery=3, Book=4, Other=5)
- [ ] T040 [US1] Update Reward model in backend/src/GamifiedMathDrill.Core/Models/Reward.cs (add Stock int?, IsPhysical bool, CreatedBy string, CreatedAt DateTime, UpdatedAt DateTime?, IsActive bool, RowVersion byte[], remove Category references to old enums)
- [ ] T041 [US1] Create EF migration for Reward table schema changes using `dotnet ef migrations add UpdateRewardForPhysicalItems`
- [ ] T042 [US1] Apply migration to database using `dotnet ef database update`
- [ ] T043 [US1] Update RewardSeeder in backend/src/GamifiedMathDrill.Infrastructure/Data/Seed/RewardSeeder.cs (seed 10-20 physical rewards: dagashi, Pokemon cards, toys with images, stock, categories)

### Backend DTOs (US1)

- [ ] T044 [P] [US1] Update RewardDto in backend/src/GamifiedMathDrill.Api/DTOs/RewardDto.cs (add Stock, IsPhysical, ImageUrl, CreatedBy, CreatedAt, UpdatedAt, IsActive, RowVersion)
- [ ] T045 [P] [US1] Create CreateRewardRequest DTO in backend/src/GamifiedMathDrill.Api/DTOs/CreateRewardRequest.cs (Name, Description, RequiredPoints, Category, IsPhysical, Stock, IFormFile Image)
- [ ] T046 [P] [US1] Create UpdateRewardRequest DTO in backend/src/GamifiedMathDrill.Api/DTOs/UpdateRewardRequest.cs (Name, Description, RequiredPoints, Category, IsPhysical, Stock, IFormFile? Image, byte[] RowVersion)

### Backend Services (US1)

- [ ] T047 [P] [US1] Create IImageStorageService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IImageStorageService.cs (SaveImageAsync, DeleteImageAsync)
- [ ] T048 [US1] Create ImageStorageService implementation in backend/src/GamifiedMathDrill.Core/Services/ImageStorageService.cs (save to wwwroot/uploads/rewards/ with GUID filename, validate MIME type and size 5MB max)
- [ ] T049 [US1] Update IRewardService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IRewardService.cs (add CreateRewardAsync, UpdateRewardAsync, DeleteRewardAsync methods)
- [ ] T050 [US1] Update RewardService in backend/src/GamifiedMathDrill.Core/Services/RewardService.cs (implement CRUD with image storage, optimistic locking using RowVersion, catch DbUpdateConcurrencyException)

### Backend Controllers (US1)

- [ ] T051 [US1] Update RewardsController in backend/src/GamifiedMathDrill.Api/Controllers/RewardsController.cs (add POST /api/rewards [Authorize(Roles="Parent")], PUT /api/rewards/{id} [Authorize(Roles="Parent")], DELETE /api/rewards/{id} [Authorize(Roles="Parent")])
- [ ] T052 [US1] Update GET /api/rewards endpoint in backend/src/GamifiedMathDrill.Api/Controllers/RewardsController.cs (filter by IsActive=true for Child role, return all for Parent role)

### Frontend Models (US1)

- [x] T053 [P] [US1] Update RewardDto model in frontend/GamifiedMathDrill.Client/Models/RewardDto.cs (add Stock, IsPhysical, ImageUrl, CreatedBy, CreatedAt, UpdatedAt, IsActive)
- [x] T054 [P] [US1] Update RewardCategory enum in frontend/GamifiedMathDrill.Client/Models/RewardCategory.cs (sync with backend: Snack, Card, Toy, Stationery, Book, Other)

### Frontend Services (US1)

- [x] T055 [US1] Update RewardApiClient in frontend/GamifiedMathDrill.Client/Services/RewardApiClient.cs (add CreateRewardAsync with multipart/form-data, UpdateRewardAsync, DeleteRewardAsync methods with JWT token in headers)

### Frontend Components (US1)

- [x] T056 [P] [US1] Create ImageUploader component in frontend/GamifiedMathDrill.Client/Components/ImageUploader.razor (IBrowserFile input, preview, size validation 5MB max, MudFileUpload)

### Frontend Pages (US1)

- [x] T057 [US1] Create RewardManagement.razor page in frontend/GamifiedMathDrill.Client/Pages/RewardManagement.razor (reward list with MudTable, add/edit/delete buttons, [Authorize(Roles="Parent")])
- [x] T058 [US1] Create RewardForm.razor page in frontend/GamifiedMathDrill.Client/Pages/RewardForm.razor (form for create/update reward with ImageUploader, MudTextField for name/description/points/stock, MudSelect for category)
- [x] T059 [US1] Update Home.razor in frontend/GamifiedMathDrill.Client/Pages/Home.razor (display physical rewards with images from API, show stock count, hide out-of-stock items for children)

**Checkpoint**: User Story 1 complete - Parents can manage physical rewards with images, children can see available rewards

---

## Phase 5: User Story 2 - 子供が実物景品の交換を申請する (Priority: P1) 🎯 MVP

**Goal**: 子供がポイントで景品を申請できる機能を実装。ポイント・在庫チェック、申請状態管理。

**Independent Test**: 子供がログイン後、十分なポイントと在庫がある景品を選択し、交換申請が完了する。申請一覧で「申請中」ステータスが表示される。

**Estimated Time**: 14 hours

### Backend Models (US2)

- [ ] T060 [P] [US2] Create ExchangeStatus enum in backend/src/GamifiedMathDrill.Core/Models/ExchangeStatus.cs (Pending=0, Approved=1, Rejected=2, Cancelled=3)
- [ ] T061 [US2] Create ExchangeRequest model (rename from AcquiredReward) in backend/src/GamifiedMathDrill.Core/Models/ExchangeRequest.cs (Id, StudentId, RewardId, Status, RequestedAt, ApprovedAt, RejectedAt, CancelledAt, ApprovedBy, RejectionReason, ParentNote)
- [ ] T062 [US2] Create EF migration to rename AcquiredRewards to ExchangeRequests and add new columns using `dotnet ef migrations add RenameToExchangeRequests`
- [ ] T063 [US2] Apply migration to database using `dotnet ef database update`

### Backend DTOs (US2)

- [ ] T064 [P] [US2] Create ExchangeRequestDto in backend/src/GamifiedMathDrill.Api/DTOs/ExchangeRequestDto.cs (Id, StudentId, StudentName, RewardId, RewardName, RewardImageUrl, RequiredPoints, Status, RequestedAt, ApprovedAt, RejectedAt, CancelledAt, ApprovedBy, ApproverName, RejectionReason, ParentNote)
- [ ] T065 [P] [US2] Create CreateExchangeRequestRequest DTO in backend/src/GamifiedMathDrill.Api/DTOs/CreateExchangeRequestRequest.cs (RewardId)

### Backend Services (US2)

- [ ] T066 [P] [US2] Create IExchangeRequestService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IExchangeRequestService.cs (CreateRequestAsync, GetRequestsByStudentAsync, CancelRequestAsync)
- [ ] T067 [US2] Create ExchangeRequestService in backend/src/GamifiedMathDrill.Core/Services/ExchangeRequestService.cs (implement CreateRequestAsync with transaction: check points, check stock, decrement stock with optimistic lock, create ExchangeRequest with Status=Pending)
- [ ] T068 [US2] Implement GetRequestsByStudentAsync in backend/src/GamifiedMathDrill.Core/Services/ExchangeRequestService.cs (query by StudentId with Include Reward, order by RequestedAt DESC)
- [ ] T069 [US2] Implement CancelRequestAsync in backend/src/GamifiedMathDrill.Core/Services/ExchangeRequestService.cs (set Status=Cancelled, increment stock back, validate Status=Pending only)

### Backend Repositories (US2)

- [ ] T070 [P] [US2] Create IExchangeRequestRepository interface in backend/src/GamifiedMathDrill.Core/Interfaces/IExchangeRequestRepository.cs (standard CRUD methods)
- [ ] T071 [US2] Create ExchangeRequestRepository in backend/src/GamifiedMathDrill.Infrastructure/Repositories/ExchangeRequestRepository.cs (implement CRUD with EF Core)

### Backend Controllers (US2)

- [ ] T072 [US2] Create ExchangeRequestsController in backend/src/GamifiedMathDrill.Api/Controllers/ExchangeRequestsController.cs (POST /api/exchange-requests [Authorize(Roles="Child")], GET /api/exchange-requests/my [Authorize(Roles="Child")], PUT /api/exchange-requests/{id}/cancel [Authorize(Roles="Child")])

### Frontend Models (US2)

- [ ] T073 [P] [US2] Create ExchangeStatus enum in frontend/GamifiedMathDrill.Client/Models/ExchangeStatus.cs (Pending, Approved, Rejected, Cancelled)
- [ ] T074 [P] [US2] Create ExchangeRequestDto model in frontend/GamifiedMathDrill.Client/Models/ExchangeRequestDto.cs (sync with backend DTO)
- [ ] T075 [P] [US2] Create CreateExchangeRequestRequest model in frontend/GamifiedMathDrill.Client/Models/CreateExchangeRequestRequest.cs

### Frontend Services (US2)

- [ ] T076 [US2] Create ExchangeRequestApiClient in frontend/GamifiedMathDrill.Client/Services/ExchangeRequestApiClient.cs (CreateRequestAsync, GetMyRequestsAsync, CancelRequestAsync with JWT token in headers)

### Frontend Pages (US2)

- [ ] T077 [US2] Update Home.razor in frontend/GamifiedMathDrill.Client/Pages/Home.razor (add exchange request button on reward cards, show confirmation dialog with points/stock check before submitting)
- [ ] T078 [US2] Update AcquiredRewardsPage.razor (rename to ExchangeRequestsPage) in frontend/GamifiedMathDrill.Client/Pages/AcquiredRewardsPage.razor (display Status badge with color coding: Pending=yellow, Approved=green, Rejected=red, Cancelled=gray, show cancel button for Pending status)

**Checkpoint**: User Story 2 complete - Children can request reward exchanges, see their request status

---

## Phase 6: User Story 3 - 保護者が交換申請を承認・却下する (Priority: P1) 🎯 MVP

**Goal**: 保護者が交換申請を承認・却下できる機能を実装。承認時のポイント消費、却下時の在庫返却。

**Independent Test**: 保護者がログイン後、未承認申請一覧を確認し、申請を承認するとポイントが消費され在庫が減る。却下すると在庫が戻る。

**Estimated Time**: 12 hours

### Backend DTOs (US3)

- [ ] T079 [P] [US3] Create ApproveRequestRequest DTO in backend/src/GamifiedMathDrill.Api/DTOs/ApproveRequestRequest.cs (ParentNote optional)
- [ ] T080 [P] [US3] Create RejectRequestRequest DTO in backend/src/GamifiedMathDrill.Api/DTOs/RejectRequestRequest.cs (Reason required, ParentNote optional)

### Backend Services (US3)

- [ ] T081 [US3] Update IExchangeRequestService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IExchangeRequestService.cs (add ApproveRequestAsync, RejectRequestAsync)
- [ ] T082 [US3] Implement ApproveRequestAsync in backend/src/GamifiedMathDrill.Core/Services/ExchangeRequestService.cs (transaction: validate Status=Pending, recheck student points, deduct points, set Status=Approved, set ApprovedAt/ApprovedBy, commit transaction)
- [ ] T083 [US3] Implement RejectRequestAsync in backend/src/GamifiedMathDrill.Core/Services/ExchangeRequestService.cs (validate Status=Pending, increment stock back, set Status=Rejected, set RejectedAt/RejectionReason, no point deduction since not deducted on request)

### Backend Controllers (US3)

- [ ] T084 [US3] Update ExchangeRequestsController in backend/src/GamifiedMathDrill.Api/Controllers/ExchangeRequestsController.cs (add PUT /api/exchange-requests/{id}/approve [Authorize(Roles="Parent")], PUT /api/exchange-requests/{id}/reject [Authorize(Roles="Parent")])

### Frontend Services (US3)

- [x] T085 [US3] Update ExchangeRequestApiClient in frontend/GamifiedMathDrill.Client/Services/ExchangeRequestApiClient.cs (add ApproveRequestAsync, RejectRequestAsync methods)

### Frontend Pages (US3)

- [x] T086 [P] [US3] Create ExchangeRequestList.razor page in frontend/GamifiedMathDrill.Client/Pages/ExchangeRequestList.razor (list all requests with MudTable, filter by Status, show child name/reward/status/date, [Authorize(Roles="Parent")])
- [x] T087 [US3] Create ExchangeRequestDetail.razor page in frontend/GamifiedMathDrill.Client/Pages/ExchangeRequestDetail.razor (display full request details, approve button with confirmation, reject button with reason input dialog, [Authorize(Roles="Parent")])

**Checkpoint**: User Story 3 complete - Parents can approve/reject exchange requests, points and stock are correctly managed

---

## Phase 7: User Story 5 - 保護者がダッシュボードで子供の学習状況を確認する (Priority: P2)

**Goal**: 保護者ダッシュボードで未承認申請数、子供一覧、学習統計を表示。30秒ポーリングで未承認数を更新。

**Independent Test**: 保護者がログイン後、ダッシュボードで子供の学習状況、未承認申請数を確認できる。未承認バッジが30秒ごとに更新される。

**Estimated Time**: 14 hours

### Backend DTOs (US5)

- [x] T088 [P] [US5] Create DashboardSummaryDto in backend/src/GamifiedMathDrill.Api/DTOs/DashboardSummaryDto.cs (PendingRequestsCount, Children list, RecentRequests, TotalRewardsCreated, ActiveRewardsCount)
- [x] T089 [P] [US5] Create ChildDto in backend/src/GamifiedMathDrill.Api/DTOs/ChildDto.cs (Id, DisplayName, AvatarUrl, TotalPoints, PendingRequestsCount, TotalProblemsCompleted, AccuracyRate, CreatedAt, IsActive)
- [x] T090 [P] [US5] Create ParentStatisticsDto in backend/src/GamifiedMathDrill.Api/DTOs/ParentStatisticsDto.cs (TotalChildren, TotalProblemsCompleted, TotalPointsEarned, TotalExchangesApproved, MonthlyStats, CategoryBreakdown, RewardCategoryBreakdown)

### Backend Services (US5)

- [x] T091 [P] [US5] Create IParentDashboardService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IParentDashboardService.cs (GetDashboardSummaryAsync, GetPendingRequestsAsync, GetChildrenAsync, GetStatisticsAsync)
- [x] T092 [US5] Create ParentDashboardService in backend/src/GamifiedMathDrill.Core/Services/ParentDashboardService.cs (query pending requests count, children with their points/stats, recent requests with Include navigation)

### Backend Controllers (US5)

- [x] T093 [US5] Create ParentDashboardController in backend/src/GamifiedMathDrill.Api/Controllers/ParentDashboardController.cs (GET /api/parent/dashboard [Authorize(Roles="Parent")], GET /api/parent/pending-requests [Authorize(Roles="Parent")], GET /api/parent/children [Authorize(Roles="Parent")], GET /api/parent/statistics [Authorize(Roles="Parent")])

### Frontend Services (US5)

- [x] T094 [P] [US5] Create PollingService in frontend/GamifiedMathDrill.Client/Services/PollingService.cs (30-second interval polling for pending requests count, IDisposable for cleanup)
- [x] T095 [US5] Create ParentDashboardApiClient in frontend/GamifiedMathDrill.Client/Services/ParentDashboardApiClient.cs (GetDashboardSummaryAsync, GetPendingRequestsAsync, GetChildrenAsync, GetStatisticsAsync)

### Frontend Components (US5)

- [x] T096 [US5] Create PendingRequestsBadge component in frontend/GamifiedMathDrill.Client/Components/PendingRequestsBadge.razor (display pending count with MudBadge, integrate PollingService for auto-refresh every 30 seconds)

### Frontend Pages (US5)

- [x] T097 [US5] Create ParentDashboard.razor page in frontend/GamifiedMathDrill.Client/Pages/ParentDashboard.razor (display summary cards: pending requests with PendingRequestsBadge, children list with stats, recent requests, link to full lists, [Authorize(Roles="Parent")])
- [x] T098 [US5] Update MainLayout.razor in frontend/GamifiedMathDrill.Client/Layout/MainLayout.razor (add PendingRequestsBadge to parent navigation menu)

**Checkpoint**: User Story 5 complete - Parents can see dashboard with real-time pending request updates, child learning statistics

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: 全User Storyに影響する改善、ドキュメント、セキュリティ強化

**Estimated Time**: 8 hours

- [ ] T099 [P] Add authorization checks to all API endpoints (verify correct [Authorize] attributes with roles)
- [ ] T100 [P] Add input validation to all DTOs using Data Annotations (Required, StringLength, Range attributes)
- [ ] T101 [P] Add error handling middleware to backend/src/GamifiedMathDrill.Api/Middleware/ for consistent error responses
- [ ] T102 [P] Add logging to all service methods using ILogger for troubleshooting
- [ ] T103 Update README.md in repository root with setup instructions for authentication (JWT SecretKey configuration, default user credentials)
- [ ] T104 Update quickstart.md in specs/003-parent-admin-rewards/ with validation checklist
- [ ] T105 [P] Add CORS configuration to backend/src/GamifiedMathDrill.Api/Program.cs for production (restrict to specific frontend domain)
- [ ] T106 [P] Add security headers to API responses (X-Content-Type-Options, X-Frame-Options, etc.)
- [ ] T107 Verify all image uploads are validated for MIME type and size across all upload points
- [ ] T108 Test optimistic locking behavior with concurrent reward updates (manual or automated concurrency test)
- [ ] T109 Review and test all edge cases from spec.md (stock management, point insufficiency, image upload failures, multi-device login, child deletion, request cancellation)
- [ ] T110 Run full E2E test suite covering all 5 user stories

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - User Story 4 (Authentication) establishes user roles (T026-T038)
  - User Story 1 (Reward Management) uses authentication (T039-T059)
  - User Story 2 (Exchange Request) depends on US1 rewards existing (T060-T078)
  - User Story 3 (Approval) depends on US2 requests existing (T079-T087)
  - User Story 5 (Dashboard) depends on US1, US2, US3 data (T088-T098)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 4 (P1 - Authentication)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 1 (P1 - Reward Management)**: Can start after US4 (needs authentication for [Authorize] attributes)
- **User Story 2 (P1 - Exchange Request)**: Can start after US4, should complete after US1 (needs rewards to exist for meaningful testing)
- **User Story 3 (P1 - Approval)**: Can start after US4, should complete after US2 (needs requests to approve)
- **User Story 5 (P2 - Dashboard)**: Can start after US4, should complete after US1/US2/US3 (needs data to display)

### Within Each User Story

- Models and enums first (can be parallel)
- Services before controllers
- DTOs before services and controllers
- Frontend models before frontend services
- Frontend services before frontend pages
- Components before pages that use them

### Parallel Opportunities

- **Setup (Phase 1)**: T004, T005, T006 (NuGet packages) can run in parallel
- **Foundational (Phase 2)**: T010 (UserRole), T015-T017 (DTOs) can run in parallel after T011-T014 complete
- **User Story 4**: T026, T027-T030, T033, T034 can run in parallel
- **User Story 1**: T039, T044-T046, T047, T053-T054, T056 can run in parallel initially
- **User Story 2**: T060, T064-T065, T070, T073-T075 can run in parallel initially
- **User Story 3**: T079-T080, T086 can run in parallel initially
- **User Story 5**: T088-T090, T091, T094-T095 can run in parallel initially
- **Polish (Phase 8)**: T099-T102, T105-T107 can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all parallel tasks for User Story 1 models/DTOs:
Task T039: Update RewardCategory enum
Task T044: Update RewardDto
Task T045: Create CreateRewardRequest DTO
Task T046: Create UpdateRewardRequest DTO
Task T047: Create IImageStorageService interface
Task T053: Update RewardDto model (frontend)
Task T054: Update RewardCategory enum (frontend)
Task T056: Create ImageUploader component

# After above complete, continue with dependent tasks:
Task T040: Update Reward model (needs T039 enum)
Task T041-T042: Create and apply migration (needs T040)
Task T048: Create ImageStorageService (needs T047 interface)
Task T049-T050: Update IRewardService and RewardService (needs T040, T048)
Task T051-T052: Update RewardsController (needs T049-T050)
Task T055: Update RewardApiClient (needs backend endpoints)
Task T057-T059: Create frontend pages (needs T055 API client, T056 component)
```

---

## Implementation Strategy

### MVP First (User Stories 4, 1, 2, 3 - All P1)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 4 (Authentication) - Required for all other stories
4. Complete Phase 4: User Story 1 (Reward Management) - Parent can add rewards
5. Complete Phase 5: User Story 2 (Exchange Request) - Child can request exchanges
6. Complete Phase 6: User Story 3 (Approval) - Parent can approve/reject
7. **STOP and VALIDATE**: Test core approval workflow end-to-end
8. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational + US4 → Authentication ready
2. Add User Story 1 → Test independently → Parents can manage rewards
3. Add User Story 2 → Test independently → Children can request exchanges
4. Add User Story 3 → Test independently → Parents can approve requests (Full MVP!)
5. Add User Story 5 → Test independently → Enhanced parent dashboard
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 4 (Authentication) - MUST complete first
3. After US4 completes:
   - Developer A: User Story 1 (Reward Management)
   - Developer B: User Story 2 (Exchange Request) - can start backend models in parallel
   - Developer C: User Story 5 (Dashboard) - can start DTOs/interfaces in parallel
4. After US1 completes:
   - Developer B: Continue User Story 2 (now has rewards to work with)
5. After US2 completes:
   - Developer C: User Story 3 (Approval) - now has requests to approve
6. Stories complete and integrate independently

---

## Total Task Count & Estimated Time

- **Phase 1 (Setup)**: 9 tasks, 2 hours
- **Phase 2 (Foundational)**: 16 tasks, 14 hours
- **Phase 3 (US4 - Authentication)**: 13 tasks, 12 hours
- **Phase 4 (US1 - Reward Management)**: 21 tasks, 18 hours
- **Phase 5 (US2 - Exchange Request)**: 19 tasks, 14 hours
- **Phase 6 (US3 - Approval)**: 9 tasks, 12 hours
- **Phase 7 (US5 - Dashboard)**: 12 tasks, 14 hours
- **Phase 8 (Polish)**: 12 tasks, 8 hours

**Total**: 111 tasks, ~94 hours (~12-15 business days at 6-8 hours/day)

---

## Notes

- [P] tasks = different files, no dependencies within the same phase
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- US4 (Authentication) MUST be completed before any other user story can function properly
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Tests are not included per feature specification (no TDD requirement)
- Security validation in Phase 8 is critical due to authentication and authorization features
