# Tasks: Authentication UI Pages

**Feature Branch**: `004-auth-ui-pages`  
**Input**: Design documents from `/specs/004-auth-ui-pages/`  
**Prerequisites**: ✅ plan.md, ✅ spec.md, ✅ research.md, ✅ data-model.md, ✅ contracts/

**Tests**: Tests are included as this is a critical authentication feature requiring TDD approach per Constitution Principle IV.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

---

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1=Login, US2=Password Reset, US3=Registration)
- All paths are relative to repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and environment setup

- [x] T001 Verify .NET 8.0 SDK installation and PostgreSQL database connectivity
- [x] T002 Create feature branch `004-auth-ui-pages` and verify checkout
- [x] T003 [P] Run `dotnet restore` for backend solution in backend/GamifiedMathDrill.sln
- [x] T004 [P] Run `dotnet restore` for frontend project in frontend/GamifiedMathDrill.Client

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Database & Entities

- [x] T005 Create PasswordResetToken entity in backend/src/GamifiedMathDrill.Core/Models/PasswordResetToken.cs
- [x] T006 Create IPasswordResetTokenRepository interface in backend/src/GamifiedMathDrill.Core/Interfaces/IPasswordResetTokenRepository.cs
- [x] T007 Implement PasswordResetTokenRepository in backend/src/GamifiedMathDrill.Infrastructure/Repositories/PasswordResetTokenRepository.cs
- [x] T008 Update ApplicationDbContext to add PasswordResetTokens DbSet in backend/src/GamifiedMathDrill.Infrastructure/Data/ApplicationDbContext.cs
- [x] T009 Generate EF Core migration: `dotnet ef migrations add AddPasswordResetTokens` from backend/src/GamifiedMathDrill.Api
- [x] T010 Apply migration to database: `dotnet ef database update` from backend/src/GamifiedMathDrill.Api

### Shared DTOs

- [x] T011 [P] Create RegisterRequest.cs in backend/src/GamifiedMathDrill.Api/DTOs/RegisterRequest.cs
- [x] T012 [P] Create RegisterResponse.cs in backend/src/GamifiedMathDrill.Api/DTOs/RegisterResponse.cs
- [x] T013 [P] Create ForgotPasswordRequest.cs in backend/src/GamifiedMathDrill.Api/DTOs/ForgotPasswordRequest.cs
- [x] T014 [P] Create ForgotPasswordResponse.cs in backend/src/GamifiedMathDrill.Api/DTOs/ForgotPasswordResponse.cs
- [x] T015 [P] Create ResetPasswordRequest.cs in backend/src/GamifiedMathDrill.Api/DTOs/ResetPasswordRequest.cs
- [x] T016 [P] Create ResetPasswordResponse.cs in backend/src/GamifiedMathDrill.Api/DTOs/ResetPasswordResponse.cs
- [x] T017 Update LoginRequest.cs to add RememberMe property in backend/src/GamifiedMathDrill.Api/DTOs/LoginRequest.cs
- [x] T018 Update LoginResponse.cs to add ExpiresAt property in backend/src/GamifiedMathDrill.Api/DTOs/LoginResponse.cs

### Email Service Infrastructure

- [x] T019 Create IEmailService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IEmailService.cs
- [x] T020 Implement EmailService with SMTP support in backend/src/GamifiedMathDrill.Infrastructure/Services/EmailService.cs
- [x] T021 Add Email configuration section to backend/src/GamifiedMathDrill.Api/appsettings.Development.json

### Rate Limiting Infrastructure

- [x] T022 Create RateLimitMiddleware in backend/src/GamifiedMathDrill.Api/Middleware/RateLimitMiddleware.cs
- [x] T023 Register RateLimitMiddleware in backend/src/GamifiedMathDrill.Api/Program.cs

### Dependency Injection Setup

- [x] T024 Register IPasswordResetTokenRepository and PasswordResetTokenRepository in backend/src/GamifiedMathDrill.Api/Program.cs
- [x] T025 Register IEmailService and EmailService in backend/src/GamifiedMathDrill.Api/Program.cs
- [x] T026 Update JWT configuration to support SessionExpiryMinutes and RememberMeExpiryMinutes in backend/src/GamifiedMathDrill.Api/appsettings.json

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Parent Login with Session Persistence (Priority: P1) 🎯 MVP

**Goal**: 保護者がメールアドレスとパスワードでログインし、「ログイン状態を保持する」オプションを選択できる

**Independent Test**: ログイン画面でメールアドレス、パスワードを入力し、チェックボックスで保持オプションを選択してログインすると、トークンが適切なストレージに保存され、ダッシュボードにリダイレクトされる

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T027 [P] [US1] Create LoginTests.cs for basic login flow in backend/tests/GamifiedMathDrill.Tests.Integration/LoginTests.cs
- [x] T028 [P] [US1] Add test case for successful login with rememberMe=false in backend/tests/GamifiedMathDrill.Tests.Integration/LoginTests.cs
- [x] T029 [P] [US1] Add test case for successful login with rememberMe=true in backend/tests/GamifiedMathDrill.Tests.Integration/LoginTests.cs
- [x] T030 [P] [US1] Add test case for invalid credentials login failure in backend/tests/GamifiedMathDrill.Tests.Integration/LoginTests.cs
- [x] T031 [P] [US1] Add test case for JWT token expiry validation (60 min vs 30 days) in backend/tests/GamifiedMathDrill.Tests.Integration/LoginTests.cs

### Backend Implementation for User Story 1

- [x] T032 [US1] Update IAuthService interface to add rememberMe parameter to LoginAsync in backend/src/GamifiedMathDrill.Core/Interfaces/IAuthService.cs
- [x] T033 [US1] Update AuthService.LoginAsync to support rememberMe and adjust JWT expiry in backend/src/GamifiedMathDrill.Infrastructure/Services/AuthService.cs
- [x] T034 [US1] Update AuthController.Login endpoint to accept rememberMe parameter in backend/src/GamifiedMathDrill.Api/Controllers/AuthController.cs
- [x] T035 [US1] Add logging for login attempts (success/failure) in backend/src/GamifiedMathDrill.Infrastructure/Services/AuthService.cs
- [x] T036 [US1] Verify RateLimitMiddleware applies to /api/auth/login endpoint in backend/src/GamifiedMathDrill.Api/Middleware/RateLimitMiddleware.cs

### Frontend Implementation for User Story 1

- [x] T037 [P] [US1] Update TokenService to support session vs local storage based on rememberMe in frontend/GamifiedMathDrill.Client/Services/TokenService.cs
- [x] T038 [US1] Update AuthService.LoginAsync to pass rememberMe parameter in frontend/GamifiedMathDrill.Client/Services/AuthService.cs
- [x] T039 [US1] Update Login.razor to add "ログイン状態を保持する" MudCheckBox in frontend/GamifiedMathDrill.Client/Pages/Login.razor
- [x] T040 [US1] Update Login.razor to pass rememberMe state to AuthService.LoginAsync in frontend/GamifiedMathDrill.Client/Pages/Login.razor
- [x] T041 [US1] Add visual feedback for login in progress (MudProgressCircular) in frontend/GamifiedMathDrill.Client/Pages/Login.razor

### Frontend Tests for User Story 1

- [ ] T042 [P] [US1] Create bUnit test for Login.razor component rendering in frontend/tests (create if not exists)
- [ ] T043 [P] [US1] Create bUnit test for rememberMe checkbox interaction in frontend/tests

**Checkpoint**: User Story 1 complete - Parent can login with session persistence option

---

## Phase 4: User Story 2 - Password Reset Flow (Priority: P2)

**Goal**: 保護者がパスワードを忘れた場合、メールでリセットリンクを受信し、新しいパスワードを設定できる

**Independent Test**: 「パスワードを忘れた」リンクをクリックし、メールアドレスを入力してリセットリンクを受信し、新しいパスワードを設定できることを確認

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T044 [P] [US2] Create PasswordResetTests.cs in backend/tests/GamifiedMathDrill.Tests.Integration/PasswordResetTests.cs
- [x] T045 [P] [US2] Add test case for forgot password request (valid email) in backend/tests/GamifiedMathDrill.Tests.Integration/PasswordResetTests.cs
- [x] T046 [P] [US2] Add test case for forgot password request (non-existent email) returns success in backend/tests/GamifiedMathDrill.Tests.Integration/PasswordResetTests.cs
- [x] T047 [P] [US2] Add test case for validate reset token (valid token) in backend/tests/GamifiedMathDrill.Tests.Integration/PasswordResetTests.cs
- [x] T048 [P] [US2] Add test case for validate reset token (expired token) in backend/tests/GamifiedMathDrill.Tests.Integration/PasswordResetTests.cs
- [x] T049 [P] [US2] Add test case for reset password (valid token + new password) in backend/tests/GamifiedMathDrill.Tests.Integration/PasswordResetTests.cs
- [x] T050 [P] [US2] Add test case for reset password (used token) fails in backend/tests/GamifiedMathDrill.Tests.Integration/PasswordResetTests.cs

### Backend Implementation for User Story 2

- [x] T051 [US2] Add SendPasswordResetEmailAsync method to IAuthService in backend/src/GamifiedMathDrill.Core/Interfaces/IAuthService.cs
- [x] T052 [US2] Add ValidateResetTokenAsync method to IAuthService in backend/src/GamifiedMathDrill.Core/Interfaces/IAuthService.cs
- [x] T053 [US2] Add ResetPasswordAsync method to IAuthService in backend/src/GamifiedMathDrill.Core/Interfaces/IAuthService.cs
- [x] T054 [US2] Implement SendPasswordResetEmailAsync in AuthService (generate token, save to DB, send email) in backend/src/GamifiedMathDrill.Infrastructure/Services/AuthService.cs
- [x] T055 [US2] Implement ValidateResetTokenAsync in AuthService (check token exists, not expired, not used) in backend/src/GamifiedMathDrill.Infrastructure/Services/AuthService.cs
- [x] T056 [US2] Implement ResetPasswordAsync in AuthService (validate token, update password, mark token as used) in backend/src/GamifiedMathDrill.Infrastructure/Services/AuthService.cs
- [x] T057 [US2] Add POST /api/auth/forgot-password endpoint to AuthController in backend/src/GamifiedMathDrill.Api/Controllers/AuthController.cs
- [x] T058 [US2] Add GET /api/auth/validate-reset-token endpoint to AuthController in backend/src/GamifiedMathDrill.Api/Controllers/AuthController.cs
- [x] T059 [US2] Add POST /api/auth/reset-password endpoint to AuthController in backend/src/GamifiedMathDrill.Api/Controllers/AuthController.cs
- [x] T060 [US2] Add error handling and logging for all password reset operations in backend/src/GamifiedMathDrill.Infrastructure/Services/AuthService.cs

### Frontend Models for User Story 2

- [x] T061 [P] [US2] Create ForgotPasswordRequest.cs in frontend/GamifiedMathDrill.Client/Models/ForgotPasswordRequest.cs
- [x] T062 [P] [US2] Create ForgotPasswordResponse.cs in frontend/GamifiedMathDrill.Client/Models/ForgotPasswordResponse.cs
- [x] T063 [P] [US2] Create ResetPasswordRequest.cs in frontend/GamifiedMathDrill.Client/Models/ResetPasswordRequest.cs
- [x] T064 [P] [US2] Create ResetPasswordResponse.cs in frontend/GamifiedMathDrill.Client/Models/ResetPasswordResponse.cs

### Frontend Services for User Story 2

- [x] T065 [US2] Add ForgotPasswordAsync method to AuthService in frontend/GamifiedMathDrill.Client/Services/AuthService.cs
- [x] T066 [US2] Add ValidateResetTokenAsync method to AuthService in frontend/GamifiedMathDrill.Client/Services/AuthService.cs
- [x] T067 [US2] Add ResetPasswordAsync method to AuthService in frontend/GamifiedMathDrill.Client/Services/AuthService.cs

### Frontend Pages for User Story 2

- [x] T068 [P] [US2] Create ForgotPassword.razor page with email input form in frontend/GamifiedMathDrill.Client/Pages/ForgotPassword.razor
- [x] T069 [P] [US2] Add MudBlazor components (MudTextField, MudButton, MudAlert) to ForgotPassword.razor in frontend/GamifiedMathDrill.Client/Pages/ForgotPassword.razor
- [x] T070 [P] [US2] Add client-side validation for email format in ForgotPassword.razor in frontend/GamifiedMathDrill.Client/Pages/ForgotPassword.razor
- [x] T071 [P] [US2] Connect ForgotPassword.razor to AuthService.ForgotPasswordAsync in frontend/GamifiedMathDrill.Client/Pages/ForgotPassword.razor
- [x] T072 [P] [US2] Add success message display in ForgotPassword.razor in frontend/GamifiedMathDrill.Client/Pages/ForgotPassword.razor
- [x] T073 [P] [US2] Create ResetPassword.razor page with token validation on load in frontend/GamifiedMathDrill.Client/Pages/ResetPassword.razor
- [x] T074 [P] [US2] Add expired token error UI to ResetPassword.razor in frontend/GamifiedMathDrill.Client/Pages/ResetPassword.razor
- [x] T075 [P] [US2] Add password reset form (new password + confirm password) to ResetPassword.razor in frontend/GamifiedMathDrill.Client/Pages/ResetPassword.razor
- [x] T076 [P] [US2] Add client-side password validation (strength requirements) to ResetPassword.razor in frontend/GamifiedMathDrill.Client/Pages/ResetPassword.razor
- [x] T077 [P] [US2] Connect ResetPassword.razor to AuthService.ResetPasswordAsync in frontend/GamifiedMathDrill.Client/Pages/ResetPassword.razor
- [x] T078 [US2] Update Login.razor to add "パスワードを忘れた" link to /forgot-password in frontend/GamifiedMathDrill.Client/Pages/Login.razor

### Frontend Tests for User Story 2

- [ ] T079 [P] [US2] Create bUnit test for ForgotPassword.razor rendering and submission in frontend/tests
- [ ] T080 [P] [US2] Create bUnit test for ResetPassword.razor with valid token in frontend/tests
- [ ] T081 [P] [US2] Create bUnit test for ResetPassword.razor with expired token in frontend/tests

**Checkpoint**: User Story 2 complete - Password reset flow fully functional

---

## Phase 5: User Story 3 - Parent Account Creation (Priority: P3)

**Goal**: 新しい保護者がアカウントを作成し、システムを使い始めることができる

**Independent Test**: 新規登録画面で必要な情報（メールアドレス、表示名、パスワード）を入力して登録ボタンをクリックすると、アカウントが作成され、自動ログイン状態でダッシュボードにリダイレクトされる

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T082 [P] [US3] Create AuthRegisterTests.cs in backend/tests/GamifiedMathDrill.Tests.Integration/AuthRegisterTests.cs
- [x] T083 [P] [US3] Add test case for successful registration with valid data in backend/tests/GamifiedMathDrill.Tests.Integration/AuthRegisterTests.cs
- [x] T084 [P] [US3] Add test case for registration with duplicate email fails in backend/tests/GamifiedMathDrill.Tests.Integration/AuthRegisterTests.cs
- [x] T085 [P] [US3] Add test case for registration with weak password fails in backend/tests/GamifiedMathDrill.Tests.Integration/AuthRegisterTests.cs
- [x] T086 [P] [US3] Add test case for registration with mismatched passwords fails in backend/tests/GamifiedMathDrill.Tests.Integration/AuthRegisterTests.cs
- [x] T087 [P] [US3] Add test case for registration auto-login (JWT token returned) in backend/tests/GamifiedMathDrill.Tests.Integration/AuthRegisterTests.cs

### Backend Implementation for User Story 3

- [x] T088 [US3] Add RegisterAsync method to IAuthService in backend/src/GamifiedMathDrill.Core/Interfaces/IAuthService.cs
- [x] T089 [US3] Implement RegisterAsync in AuthService (validate, create user, auto-login) in backend/src/GamifiedMathDrill.Infrastructure/Services/AuthService.cs
- [x] T090 [US3] Add POST /api/auth/register endpoint to AuthController in backend/src/GamifiedMathDrill.Api/Controllers/AuthController.cs
- [x] T091 [US3] Add email uniqueness check in RegisterAsync in backend/src/GamifiedMathDrill.Infrastructure/Services/AuthService.cs
- [x] T092 [US3] Add password strength validation in RegisterAsync in backend/src/GamifiedMathDrill.Infrastructure/Services/AuthService.cs
- [x] T093 [US3] Add logging for registration attempts (success/failure) in backend/src/GamifiedMathDrill.Infrastructure/Services/AuthService.cs

### Frontend Models for User Story 3

- [x] T094 [P] [US3] Create RegisterRequest.cs in frontend/GamifiedMathDrill.Client/Models/RegisterRequest.cs
- [x] T095 [P] [US3] Create RegisterResponse.cs in frontend/GamifiedMathDrill.Client/Models/RegisterResponse.cs

### Frontend Services for User Story 3

- [x] T096 [US3] Add RegisterAsync method to AuthService in frontend/GamifiedMathDrill.Client/Services/AuthService.cs

### Frontend Pages for User Story 3

- [x] T097 [P] [US3] Create Register.razor page with registration form in frontend/GamifiedMathDrill.Client/Pages/Register.razor
- [x] T098 [P] [US3] Add MudBlazor components (MudTextField, MudButton, MudAlert) to Register.razor in frontend/GamifiedMathDrill.Client/Pages/Register.razor
- [x] T099 [P] [US3] Add client-side validation for all fields (email, displayName, password) in Register.razor in frontend/GamifiedMathDrill.Client/Pages/Register.razor
- [x] T100 [P] [US3] Add password strength indicator UI in Register.razor in frontend/GamifiedMathDrill.Client/Pages/Register.razor
- [x] T101 [P] [US3] Add confirm password matching validation in Register.razor in frontend/GamifiedMathDrill.Client/Pages/Register.razor
- [x] T102 [P] [US3] Connect Register.razor to AuthService.RegisterAsync in frontend/GamifiedMathDrill.Client/Pages/Register.razor
- [x] T103 [P] [US3] Handle auto-login after successful registration in Register.razor in frontend/GamifiedMathDrill.Client/Pages/Register.razor
- [x] T104 [US3] Update Login.razor to add "新規登録" link to /register in frontend/GamifiedMathDrill.Client/Pages/Login.razor

### Frontend Tests for User Story 3

- [ ] T105 [P] [US3] Create bUnit test for Register.razor rendering in frontend/tests
- [ ] T106 [P] [US3] Create bUnit test for Register.razor form validation in frontend/tests
- [ ] T107 [P] [US3] Create bUnit test for Register.razor successful submission in frontend/tests

**Checkpoint**: User Story 3 complete - Account creation flow fully functional

---

## Phase 6: Rate Limiting & Security (Cross-Cutting)

**Purpose**: Implement security measures across all authentication endpoints

- [x] T108 Create RateLimitTests.cs in backend/tests/GamifiedMathDrill.Tests.Integration/RateLimitTests.cs
- [x] T109 Add test case for rate limit on login after 5 failed attempts in backend/tests/GamifiedMathDrill.Tests.Integration/RateLimitTests.cs
- [x] T110 Add test case for rate limit reset after 15 minutes in backend/tests/GamifiedMathDrill.Tests.Integration/RateLimitTests.cs
- [x] T111 Verify RateLimitMiddleware applies to /api/auth/register endpoint in backend/src/GamifiedMathDrill.Api/Middleware/RateLimitMiddleware.cs
- [x] T112 Verify RateLimitMiddleware applies to /api/auth/forgot-password endpoint in backend/src/GamifiedMathDrill.Api/Middleware/RateLimitMiddleware.cs
- [x] T113 Add rate limit error message (429 Too Many Requests) handling in all frontend auth pages in frontend/GamifiedMathDrill.Client/Pages/

---

## Phase 7: Integration & End-to-End Testing

**Purpose**: Verify complete user journeys work end-to-end

- [x] T114 Create E2E test for full registration → login flow in backend/tests/GamifiedMathDrill.Tests.Integration/E2EAuthFlowTests.cs
- [x] T115 Create E2E test for login → forgot password → reset → login flow in backend/tests/GamifiedMathDrill.Tests.Integration/E2EAuthFlowTests.cs
- [x] T116 Create E2E test for remember me functionality (session vs local storage) in backend/tests/GamifiedMathDrill.Tests.Integration/E2EAuthFlowTests.cs
- [x] T117 Verify AuthenticationHelper.cs works with new register endpoint in backend/tests/GamifiedMathDrill.Tests.Integration/Helpers/AuthenticationHelper.cs
- [x] T118 Run all backend tests: `dotnet test` from backend/ directory
- [x] T119 Run all frontend tests (if bUnit tests created) from frontend/ directory

---

## Phase 8: Polish & Documentation

**Purpose**: Finalize UI/UX, documentation, and deployment readiness

### UI/UX Polish

- [x] T120 [P] Add responsive design verification for Login.razor (mobile/tablet/desktop) in frontend/GamifiedMathDrill.Client/Pages/Login.razor
- [x] T121 [P] Add responsive design verification for Register.razor in frontend/GamifiedMathDrill.Client/Pages/Register.razor
- [x] T122 [P] Add responsive design verification for ForgotPassword.razor in frontend/GamifiedMathDrill.Client/Pages/ForgotPassword.razor
- [x] T123 [P] Add responsive design verification for ResetPassword.razor in frontend/GamifiedMathDrill.Client/Pages/ResetPassword.razor
- [x] T124 [P] Add loading states (MudProgressCircular) to all submit buttons in frontend/GamifiedMathDrill.Client/Pages/
- [x] T125 [P] Verify all error messages are user-friendly and localized (Japanese) in frontend/GamifiedMathDrill.Client/Pages/

### API Documentation

- [ ] T126 [P] Update Swagger annotations for new endpoints in backend/src/GamifiedMathDrill.Api/Controllers/AuthController.cs
- [ ] T127 [P] Add XML documentation comments to all new DTOs in backend/src/GamifiedMathDrill.Api/DTOs/
- [ ] T128 [P] Verify OpenAPI spec matches contracts/auth-api.yaml in backend/src/GamifiedMathDrill.Api/

### Developer Documentation

- [ ] T129 Update README.md with new authentication features in repository root
- [ ] T130 Update quickstart.md with any final setup steps in specs/004-auth-ui-pages/quickstart.md
- [ ] T131 Create CHANGELOG.md entry for 004-auth-ui-pages feature in repository root

### Deployment Preparation

- [x] T132 Verify all environment variables documented in appsettings.json in backend/src/GamifiedMathDrill.Api/
- [x] T133 Create deployment checklist (database migration, email config, JWT secrets) in specs/004-auth-ui-pages/
- [x] T134 Run security audit (password hashing, HTTPS, rate limiting, token expiry) across all new code
- [ ] T135 Run performance testing for login/register endpoints (<5s response time) in backend/tests/

---

## Phase 9: Code Review & Merge

**Purpose**: Final quality gates before merging to main

- [x] T136 [P] Run `dotnet format` on all backend C# code in backend/
- [x] T137 Fix any linter warnings or errors in backend code
- [x] T138 Verify all tests pass: `dotnet test` in backend/
- [ ] T139 Create pull request from `004-auth-ui-pages` to main branch
- [ ] T140 Address code review feedback
- [ ] T141 Squash and merge to main branch after approval

---

## Dependencies & Execution Strategy

### Dependency Graph (User Story Completion Order)

```
Phase 1 (Setup)
     ↓
Phase 2 (Foundational) ← MUST COMPLETE FIRST
     ↓
     ├─→ Phase 3 (US1: Login) ← MVP - Highest Priority
     │
     ├─→ Phase 4 (US2: Password Reset) ← Can start after Phase 2
     │
     └─→ Phase 5 (US3: Registration) ← Can start after Phase 2

All above complete
     ↓
Phase 6 (Rate Limiting) ← Applies to all stories
     ↓
Phase 7 (Integration Testing)
     ↓
Phase 8 (Polish & Documentation)
     ↓
Phase 9 (Code Review & Merge)
```

### Parallel Execution Opportunities

**After Phase 2 completes**, the following can run in parallel:

1. **US1 Team** (T027-T043): Implement login with session persistence
2. **US2 Team** (T044-T081): Implement password reset flow
3. **US3 Team** (T082-T107): Implement registration flow

Within each user story, tasks marked with **[P]** can run in parallel:

- **US1**: T027-T031 (tests), T037 (TokenService) and T042-T043 (bUnit tests)
- **US2**: T044-T050 (tests), T061-T064 (models), T068-T077 (pages), T079-T081 (bUnit tests)
- **US3**: T082-T087 (tests), T094-T095 (models), T097-T104 (pages), T105-T107 (bUnit tests)

**Phase 8 (Polish)** tasks T120-T128 can mostly run in parallel

### Suggested MVP Scope

For fastest value delivery, implement **ONLY Phase 3 (US1: Login with Session Persistence)** first:

- Tasks: T001-T026 (Foundation) + T027-T043 (US1)
- Total: ~43 tasks
- Estimated time: 12-16 hours
- Delivers: Core login functionality with session management

Then incrementally add:

- **Iteration 2**: US2 (Password Reset) - 38 tasks, ~10-14 hours
- **Iteration 3**: US3 (Registration) - 26 tasks, ~8-12 hours

---

## Implementation Strategy

1. **TDD Approach**: Write tests first for each user story (Constitution Principle IV)
2. **Independent Stories**: Each user story can be developed and tested independently after Phase 2
3. **Incremental Delivery**: Deploy US1 as MVP, then add US2 and US3 in subsequent releases
4. **Parallel Development**: After Phase 2, multiple developers can work on different stories simultaneously

---

## Summary

- **Total Tasks**: 141
- **Setup & Foundation**: 26 tasks (T001-T026)
- **User Story 1 (Login)**: 17 tasks (T027-T043)
- **User Story 2 (Password Reset)**: 38 tasks (T044-T081)
- **User Story 3 (Registration)**: 26 tasks (T082-T107)
- **Cross-Cutting (Security)**: 6 tasks (T108-T113)
- **Integration Testing**: 6 tasks (T114-T119)
- **Polish & Docs**: 16 tasks (T120-T135)
- **Code Review**: 6 tasks (T136-T141)

**Estimated Total Time**: 40-55 hours (with parallelization: 30-40 hours)

**Critical Path**: Phase 1 → Phase 2 → Phase 3 (US1) → Phase 6 → Phase 7 → Phase 8 → Phase 9

**Parallel Opportunities**:

- Phase 2: 8 parallel tasks (T011-T016, T019-T020)
- Phase 3-5: 3 user stories can be developed in parallel
- Phase 8: Most polish tasks can run in parallel

All tasks follow the required checklist format with Task ID, optional [P] and [Story] markers, and specific file paths.
