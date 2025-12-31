# Tasks: Gamified Math Drill App

**Input**: Design documents from `/specs/001-gamified-math-drill/`  
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [data-model.md](data-model.md), [contracts/api.md](contracts/api.md)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

**Tests**: Tests are NOT included as they were not explicitly requested in the feature specification.

---

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create backend project structure with Clean Architecture (GamifiedMathDrill.Api, GamifiedMathDrill.Core, GamifiedMathDrill.Infrastructure)
- [ ] T002 Initialize .NET 8.0 solution with dependencies (ASP.NET Core, EF Core, xUnit, Moq)
- [ ] T003 [P] Create frontend Blazor WebAssembly project structure
- [ ] T004 [P] Configure linting, formatting tools (.editorconfig, omnisharp.json)
- [ ] T005 [P] Setup CI/CD pipeline configuration (GitHub Actions or similar)
- [ ] T006 Configure HTTPS and HSTS middleware in backend/src/GamifiedMathDrill.Api/Program.cs
- [ ] T007 Setup CORS policy for frontend-backend communication in backend/src/GamifiedMathDrill.Api/Program.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T008 Define all entity models in backend/src/GamifiedMathDrill.Core/Models/ (Student, Problem, LearningRecord, Reward, AcquiredReward, DailyChallenge, Level)
- [ ] T009 Create ApplicationDbContext with DbSets in backend/src/GamifiedMathDrill.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T010 Configure entity relationships and constraints using Fluent API in ApplicationDbContext.OnModelCreating
- [ ] T011 Create initial EF Core migration with all entities in backend/src/GamifiedMathDrill.Infrastructure/Migrations/
- [ ] T012 [P] Create seed data migration for Problems (500-1000 math problems for grade 3) in backend/src/GamifiedMathDrill.Infrastructure/Migrations/
- [ ] T013 [P] Create seed data migration for Rewards (20-30 rewards: badges, avatars, characters) in backend/src/GamifiedMathDrill.Infrastructure/Migrations/
- [ ] T014 [P] Create seed data migration for Levels (10 levels with difficulty progression) in backend/src/GamifiedMathDrill.Infrastructure/Migrations/
- [ ] T015 Implement IEncryptionService interface and AES-256 encryption service in backend/src/GamifiedMathDrill.Infrastructure/Services/EncryptionService.cs
- [ ] T016 Configure Data Protection API for session tokens in backend/src/GamifiedMathDrill.Api/Program.cs
- [ ] T017 Setup connection strings for SQLite (dev) and PostgreSQL (prod) in appsettings.json and appsettings.Production.json
- [ ] T018 Implement global error handling middleware in backend/src/GamifiedMathDrill.Api/Middleware/ErrorHandlingMiddleware.cs
- [ ] T019 Configure structured logging (Serilog or similar) in backend/src/GamifiedMathDrill.Api/Program.cs
- [ ] T020 Implement rate limiting middleware in backend/src/GamifiedMathDrill.Api/Middleware/RateLimitingMiddleware.cs
- [ ] T021 Setup Swagger/OpenAPI documentation in backend/src/GamifiedMathDrill.Api/Program.cs
- [ ] T022 Create base API response models (SuccessResponse, ErrorResponse) in backend/src/GamifiedMathDrill.Core/Models/Responses/
- [ ] T023 Implement repository interfaces in backend/src/GamifiedMathDrill.Core/Interfaces/ (IStudentRepository, IProblemRepository, ILearningRecordRepository, IRewardRepository, IAcquiredRewardRepository, IDailyChallengeRepository, ILevelRepository)
- [ ] T024 Implement repository implementations in backend/src/GamifiedMathDrill.Infrastructure/Repositories/
- [ ] T025 Configure dependency injection for all services and repositories in backend/src/GamifiedMathDrill.Api/Program.cs
- [ ] T026 [P] Setup MudBlazor UI library in frontend/Program.cs
- [ ] T027 [P] Create base layout and navigation components in frontend/src/components/Layout/
- [ ] T028 [P] Configure API base URL in frontend/wwwroot/appsettings.json
- [ ] T029 [P] Create API client service base class in frontend/src/services/ApiClientBase.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - 計算問題を解いてポイントを獲得する (Priority: P1) 🎯 MVP

**Goal**: 児童が計算問題を解いて正解するとポイントを獲得できる。問題表示、回答判定、ポイント付与の基本サイクルを実装。

**Independent Test**: アプリを起動し、問題を表示し、回答を送信し、正解/不正解の判定とポイント付与が正しく動作することを確認。

### Implementation for User Story 1

- [ ] T030 [P] [US1] Create StudentService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IStudentService.cs
- [ ] T031 [P] [US1] Create ProblemService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IProblemService.cs
- [ ] T032 [US1] Implement StudentService with GetById, Create, UpdateLogin methods in backend/src/GamifiedMathDrill.Core/Services/StudentService.cs
- [ ] T033 [US1] Implement ProblemService with GetNextProblem, SubmitAnswer methods in backend/src/GamifiedMathDrill.Core/Services/ProblemService.cs
- [ ] T034 [US1] Implement level-up logic in ProblemService (check consecutive correct answers, update level)
- [ ] T035 [P] [US1] Create StudentController with GET /students/{id}, POST /students, PATCH /students/{id}/login endpoints in backend/src/GamifiedMathDrill.Api/Controllers/StudentController.cs
- [ ] T036 [P] [US1] Create ProblemController with GET /problems/next, POST /problems/{id}/answer endpoints in backend/src/GamifiedMathDrill.Api/Controllers/ProblemController.cs
- [ ] T037 [US1] Add validation for answer submission (non-negative points, valid student/problem IDs) in ProblemController
- [ ] T038 [US1] Implement transaction handling for answer submission (atomic update of Student points and LearningRecord creation)
- [ ] T039 [P] [US1] Create ProblemPage Blazor component in frontend/src/pages/ProblemPage.razor
- [ ] T040 [P] [US1] Create ProblemDisplay component (shows question, input field) in frontend/src/components/Problem/ProblemDisplay.razor
- [ ] T041 [P] [US1] Create AnswerFeedback component (shows correct/incorrect message, points earned) in frontend/src/components/Problem/AnswerFeedback.razor
- [ ] T042 [US1] Create ProblemApiClient service in frontend/src/services/ProblemApiClient.cs
- [ ] T043 [US1] Implement problem fetch and answer submission logic in ProblemPage
- [ ] T044 [US1] Add "Next Problem" button navigation logic in ProblemPage
- [ ] T045 [US1] Implement excludeRecentIds query parameter to prevent duplicate problems in ProblemApiClient
- [ ] T046 [US1] Add visual feedback animations (correct: green checkmark, incorrect: red cross) in AnswerFeedback component
- [ ] T047 [US1] Display level-up notification when student advances to next level in ProblemPage

**Checkpoint**: User Story 1 is fully functional and independently testable.児童は問題を解いてポイントを獲得できる。

---

## Phase 4: User Story 2 - 過去の学習結果を確認する (Priority: P2)

**Goal**: 児童や保護者が学習履歴、正答率、ポイント総数を視覚的に確認できる。日別・週別・計算種類別の統計を表示。

**Independent Test**: 過去の LearningRecord データを基に、結果画面に統計とグラフが正しく表示されることを確認。

### Implementation for User Story 2

- [ ] T048 [P] [US2] Create LearningRecordService interface in backend/src/GamifiedMathDrill.Core/Interfaces/ILearningRecordService.cs
- [ ] T049 [US2] Implement LearningRecordService with GetRecords, GetStatistics methods in backend/src/GamifiedMathDrill.Core/Services/LearningRecordService.cs
- [ ] T050 [US2] Implement statistics aggregation logic (daily, weekly, by calculation type) in LearningRecordService
- [ ] T051 [P] [US2] Create LearningRecordController with GET /learning-records, GET /learning-records/statistics endpoints in backend/src/GamifiedMathDrill.Api/Controllers/LearningRecordController.cs
- [ ] T052 [US2] Add filtering by date range and calculation type in LearningRecordController
- [ ] T053 [US2] Implement pagination for learning records list in LearningRecordController
- [ ] T054 [P] [US2] Create ResultsPage Blazor component in frontend/src/pages/ResultsPage.razor
- [ ] T055 [P] [US2] Create StatisticsSummary component (displays total problems, accuracy, points) in frontend/src/components/Results/StatisticsSummary.razor
- [ ] T056 [P] [US2] Create StatisticsChart component (bar chart for daily/weekly/type statistics) using Chart.js or similar in frontend/src/components/Results/StatisticsChart.razor
- [ ] T057 [P] [US2] Create LearningRecordList component (table of past records) in frontend/src/components/Results/LearningRecordList.razor
- [ ] T058 [US2] Create LearningRecordApiClient service in frontend/src/services/LearningRecordApiClient.cs
- [ ] T059 [US2] Implement date range picker for filtering in ResultsPage
- [ ] T060 [US2] Implement calculation type filter tabs (All, Addition, Subtraction, Multiplication, Division) in ResultsPage
- [ ] T061 [US2] Add visual indicators for accuracy rate (green >80%, yellow 60-80%, red <60%) in StatisticsSummary
- [ ] T062 [US2] Implement responsive layout for charts (mobile-friendly) in StatisticsChart

**Checkpoint**: User Story 2 is fully functional. 児童と保護者は学習結果を詳細に確認できる。

---

## Phase 5: User Story 4 - 飽きない工夫で継続学習する (Priority: P2)

**Goal**: 連続学習日数の記録、日替わりチャレンジ問題、励ましメッセージで学習継続を促進。

**Independent Test**: 連続ログイン記録、デイリーチャレンジ問題の表示、励ましメッセージ表示が独立して動作。

**Note**: US4 is implemented before US3 because it's P2 (higher priority than US3's P3)

### Implementation for User Story 4

- [ ] T063 [P] [US4] Create DailyChallengeService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IDailyChallengeService.cs
- [ ] T064 [US4] Implement DailyChallengeService with GetTodayChallenge, SubmitChallengeAnswer methods in backend/src/GamifiedMathDrill.Core/Services/DailyChallengeService.cs
- [ ] T065 [US4] Implement background job for daily challenge generation (create new challenge at midnight) in backend/src/GamifiedMathDrill.Infrastructure/Jobs/DailyChallengeJob.cs
- [ ] T066 [US4] Implement consecutive days calculation logic in StudentService.UpdateLogin
- [ ] T067 [US4] Implement encouragement message generation based on streak days in StudentService
- [ ] T068 [P] [US4] Create DailyChallengeController with GET /daily-challenges/today, POST /daily-challenges/{id}/answer endpoints in backend/src/GamifiedMathDrill.Api/Controllers/DailyChallengeController.cs
- [ ] T069 [US4] Add bonus points logic for daily challenge correct answers in DailyChallengeService
- [ ] T070 [P] [US4] Create HomePage Blazor component with welcome message and stats in frontend/src/pages/HomePage.razor
- [ ] T071 [P] [US4] Create ConsecutiveDaysDisplay component (shows streak with visual indicator) in frontend/src/components/Home/ConsecutiveDaysDisplay.razor
- [ ] T072 [P] [US4] Create EncouragementMessage component (displays motivational messages) in frontend/src/components/Home/EncouragementMessage.razor
- [ ] T073 [P] [US4] Create DailyChallengeCard component (highlights today's challenge) in frontend/src/components/Home/DailyChallengeCard.razor
- [ ] T074 [US4] Create DailyChallengeApiClient service in frontend/src/services/DailyChallengeApiClient.cs
- [ ] T075 [US4] Implement login tracking on app startup in HomePage (call PATCH /students/{id}/login)
- [ ] T076 [US4] Display encouragement message based on consecutive days on HomePage
- [ ] T077 [US4] Add "Start Today's Challenge" button navigation to DailyChallengeCard
- [ ] T078 [US4] Implement achievement badge display for milestones (7 days, 14 days, 30 days streaks) in HomePage
- [ ] T079 [US4] Add "Welcome back!" message for returning users (lastLoginAt > 1 day ago) in HomePage

**Checkpoint**: User Story 4 is fully functional. 児童は毎日継続して学習する動機づけを得る。

---

## Phase 6: User Story 3 - ポイントを使って景品と交換する (Priority: P3)

**Goal**: 児童がポイントを消費して景品（バッジ、アバター、キャラクター）と交換できる。ポイント不足時は適切なメッセージを表示。

**Independent Test**: 景品一覧表示、ポイント残高チェック、交換トランザクション、獲得景品一覧表示が独立して動作。

### Implementation for User Story 3

- [ ] T080 [P] [US3] Create RewardService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IRewardService.cs
- [ ] T081 [US3] Implement RewardService with GetRewards, ExchangeReward, GetAcquiredRewards methods in backend/src/GamifiedMathDrill.Core/Services/RewardService.cs
- [ ] T082 [US3] Implement point validation logic (check sufficient points before exchange) in RewardService
- [ ] T083 [US3] Implement transaction handling for reward exchange (atomic update of Student points and AcquiredReward creation) in RewardService
- [ ] T084 [P] [US3] Create RewardController with GET /rewards, POST /rewards/{id}/exchange, GET /rewards/acquired endpoints in backend/src/GamifiedMathDrill.Api/Controllers/RewardController.cs
- [ ] T085 [US3] Add filtering by category and maxPoints query parameters in RewardController
- [ ] T086 [US3] Implement InsufficientPoints error response (400) with required points details in RewardController
- [ ] T087 [P] [US3] Create RewardsPage Blazor component in frontend/src/pages/RewardsPage.razor
- [ ] T088 [P] [US3] Create RewardList component (grid of available rewards with images) in frontend/src/components/Rewards/RewardList.razor
- [ ] T089 [P] [US3] Create RewardCard component (displays reward name, description, required points, image) in frontend/src/components/Rewards/RewardCard.razor
- [ ] T090 [P] [US3] Create ExchangeConfirmationDialog component (modal for exchange confirmation) in frontend/src/components/Rewards/ExchangeConfirmationDialog.razor
- [ ] T091 [P] [US3] Create AcquiredRewardsPage Blazor component in frontend/src/pages/AcquiredRewardsPage.razor
- [ ] T092 [P] [US3] Create AcquiredRewardGallery component (displays owned rewards) in frontend/src/components/Rewards/AcquiredRewardGallery.razor
- [ ] T093 [US3] Create RewardApiClient service in frontend/src/services/RewardApiClient.cs
- [ ] T094 [US3] Implement reward filtering by category tabs (All, Badges, Avatars, Characters) in RewardsPage
- [ ] T095 [US3] Implement exchange flow with confirmation dialog in RewardsPage
- [ ] T096 [US3] Display current points balance prominently in RewardsPage
- [ ] T097 [US3] Show "Insufficient Points" error message when clicking on unaffordable rewards in RewardCard
- [ ] T098 [US3] Add success animation/message after successful exchange in RewardsPage
- [ ] T099 [US3] Disable exchange button for rewards that exceed current points in RewardCard

**Checkpoint**: User Story 3 is fully functional. 児童はポイントを使って景品と交換できる。

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final refinements, error handling improvements, performance optimization

- [ ] T100 [P] Add loading spinners/skeletons for all API calls across all pages
- [ ] T101 [P] Implement offline detection and display appropriate message in frontend (FR-018)
- [ ] T102 [P] Add error boundary components for graceful error handling in frontend
- [ ] T103 [P] Optimize database queries with appropriate indexes (verify all indexes from data-model.md are created)
- [ ] T104 [P] Implement caching for frequently accessed data (problems, rewards) using In-Memory Cache
- [ ] T105 [P] Add application monitoring and metrics (Application Insights or Prometheus)
- [ ] T106 [P] Create user-friendly 404 Not Found page in frontend
- [ ] T107 [P] Create user-friendly 500 Internal Server Error page in frontend
- [ ] T108 [P] Implement responsive design testing for mobile devices (Chrome DevTools)
- [ ] T109 [P] Add accessibility improvements (ARIA labels, keyboard navigation) for all components
- [ ] T110 [P] Perform security audit (HTTPS enforcement, CSRF protection, input validation)
- [ ] T111 [P] Optimize Blazor WASM bundle size (enable AOT compilation if needed)
- [ ] T112 [P] Add performance testing (load test with k6 or JMeter for 100 concurrent users)
- [ ] T113 [P] Document API endpoints with detailed examples in Swagger UI
- [ ] T114 [P] Create deployment scripts for Azure App Service or AWS Elastic Beanstalk
- [ ] T115 [P] Setup production database (PostgreSQL) with backups and encryption
- [ ] T116 [P] Configure environment-specific settings (appsettings.Production.json)

**Checkpoint**: Application is production-ready with polish, performance, and security enhancements.

---

## Dependencies & Execution Order

### Story Completion Order (for MVP delivery)

1. **Phase 1-2** (Setup + Foundation): MUST complete first - all stories depend on this
2. **Phase 3** (US1 - P1): Core MVP functionality - 問題解答とポイント獲得
3. **Phase 4** (US2 - P2) OR **Phase 5** (US4 - P2): Can be done in either order after US1
4. **Phase 6** (US3 - P3): Reward exchange - can be done last
5. **Phase 7** (Polish): Final refinements

### Critical Path

```
Setup (T001-T007) → Foundational (T008-T029) → US1 (T030-T047) → US2/US4 (parallel) → US3 (T080-T099) → Polish (T100-T116)
```

### Parallel Execution Opportunities

**Within Phase 2 (Foundational)**:

- T012, T013, T014 (seed data migrations) can run in parallel after T011
- T026, T027, T028, T029 (frontend setup) can run in parallel with backend tasks T008-T025

**Within Phase 3 (US1)**:

- T030, T031 (service interfaces) can be created in parallel
- T035, T036 (controllers) can be created in parallel after T032-T034
- T039, T040, T041 (Blazor components) can be created in parallel

**Within Phase 4 (US2)**:

- T054, T055, T056, T057 (Blazor components) can be created in parallel after T048-T053

**Within Phase 5 (US4)**:

- T070, T071, T072, T073 (Blazor components) can be created in parallel after T063-T069

**Within Phase 6 (US3)**:

- T087, T088, T089, T090, T091, T092 (Blazor components) can be created in parallel after T080-T086

**Within Phase 7 (Polish)**:

- Most tasks (T100-T116) can run in parallel as they are independent improvements

---

## Implementation Strategy

### MVP Scope (Minimum Viable Product)

**Phase 1-3 only** (Setup + Foundation + US1):

- 児童は問題を解いてポイントを獲得できる
- レベルアップ機能が動作する
- 基本的な計算ドリルとして機能する

**Recommended MVP**: Phase 1-5 (includes US1, US2, US4):

- 問題解答 + 学習結果確認 + 継続学習促進
- 保護者が進捗を確認できる
- デイリーチャレンジで毎日の動機づけ

**Full Feature Set**: All Phases (includes US3):

- 景品交換機能を追加
- 完全なゲーミフィケーション体験

### Incremental Delivery

Each User Story phase delivers a complete, testable, and demonstrable increment:

1. **After Phase 3** (US1): "問題を解いてポイントを獲得できるアプリ"として動作
2. **After Phase 4** (US2): "学習結果を確認できる"機能が追加される
3. **After Phase 5** (US4): "継続学習を促進する"工夫が加わる
4. **After Phase 6** (US3): "景品と交換できる"ゲーム要素が完成
5. **After Phase 7** (Polish): 本番環境にデプロイ可能な品質に到達

---

## Task Count Summary

- **Phase 1 (Setup)**: 7 tasks
- **Phase 2 (Foundational)**: 22 tasks (BLOCKING - must complete first)
- **Phase 3 (US1 - P1)**: 18 tasks
- **Phase 4 (US2 - P2)**: 15 tasks
- **Phase 5 (US4 - P2)**: 17 tasks
- **Phase 6 (US3 - P3)**: 20 tasks
- **Phase 7 (Polish)**: 17 tasks

**Total Tasks**: 116

**Parallel Opportunities**: ~40% of tasks can be executed in parallel within their phases

---

## Notes

- All file paths follow Clean Architecture structure (Api, Core, Infrastructure layers)
- Frontend uses Blazor WebAssembly with component-based architecture
- Each User Story is independently testable and can be demoed separately
- Tests are NOT included as they were not requested in the specification
- Constitution compliance: All tasks align with the 5 core principles (User-Centric, Privacy, Spec-First, Test-Driven, Continuous Learning)

---

**Next Steps**: Begin implementation with Phase 1 (Setup). Refer to [quickstart.md](quickstart.md) for development environment setup.
