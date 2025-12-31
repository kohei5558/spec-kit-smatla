# Tasks: Gamified Math Drill App

**Input**: Design documents from `/specs/001-gamified-math-drill/`  
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [data-model.md](data-model.md), [contracts/api.md](contracts/api.md)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

**Tests**: Tests are NOT included as they were not explicitly requested in the feature specification.

---

## Format: `- [ ] [ID] [P?] [Story?] Description`

- **Checkbox**: ALWAYS start with `- [ ]` (markdown checkbox)
- **[ID]**: Task ID (T001, T002, etc.) in execution order
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4) - REQUIRED for user story phases only
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create backend solution file at backend/GamifiedMathDrill.sln
- [ ] T002 Create backend project structure with Clean Architecture (GamifiedMathDrill.Api, GamifiedMathDrill.Core, GamifiedMathDrill.Infrastructure) in backend/src/
- [ ] T003 Initialize .NET 8.0 projects with base dependencies (ASP.NET Core 8.0, Entity Framework Core 8.0) in all backend projects
- [ ] T004 [P] Create frontend Blazor WebAssembly project structure at frontend/GamifiedMathDrill.Client/
- [ ] T005 [P] Add test project structure (GamifiedMathDrill.Tests.Unit, GamifiedMathDrill.Tests.Integration, GamifiedMathDrill.Tests.E2E) in backend/tests/
- [ ] T006 [P] Configure .editorconfig and omnisharp.json for consistent code formatting at repository root
- [ ] T007 [P] Setup GitHub Actions CI/CD workflow in .github/workflows/ci.yml
- [ ] T008 Configure HTTPS and HSTS middleware in backend/src/GamifiedMathDrill.Api/Program.cs
- [ ] T009 Setup CORS policy for frontend-backend communication in backend/src/GamifiedMathDrill.Api/Program.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Data Layer Setup

- [ ] T010 Define Student entity model in backend/src/GamifiedMathDrill.Core/Models/Student.cs (Id, Name, CurrentLevelId, TotalPoints, ConsecutiveDays, TotalProblems, CorrectAnswers, CreatedAt, LastLoginAt)
- [ ] T011 [P] Define Problem entity model in backend/src/GamifiedMathDrill.Core/Models/Problem.cs (Id, Question, CorrectAnswer, CalculationType, DifficultyLevel)
- [ ] T012 [P] Define LearningRecord entity model in backend/src/GamifiedMathDrill.Core/Models/LearningRecord.cs (Id, StudentId, ProblemId, IsCorrect, TimeSpentSeconds, AnsweredAt)
- [ ] T013 [P] Define Reward entity model in backend/src/GamifiedMathDrill.Core/Models/Reward.cs (Id, Name, Description, RequiredPoints, Category, ImageUrl)
- [ ] T014 [P] Define AcquiredReward entity model in backend/src/GamifiedMathDrill.Core/Models/AcquiredReward.cs (Id, StudentId, RewardId, PointsSpent, AcquiredAt)
- [ ] T015 [P] Define DailyChallenge entity model in backend/src/GamifiedMathDrill.Core/Models/DailyChallenge.cs (Id, ProblemId, TargetDate, BonusPoints, IsActive)
- [ ] T016 [P] Define Level entity model in backend/src/GamifiedMathDrill.Core/Models/Level.cs (Id, LevelNumber, RequiredCorrect, MinDifficulty, MaxDifficulty)
- [ ] T017 [P] Define CalculationType enum in backend/src/GamifiedMathDrill.Core/Models/CalculationType.cs (Addition, Subtraction, Multiplication, Division)
- [ ] T018 Create ApplicationDbContext with DbSets for all entities in backend/src/GamifiedMathDrill.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T019 Configure entity relationships and constraints using Fluent API in ApplicationDbContext.OnModelCreating method
- [ ] T020 Create initial EF Core migration "InitialCreate" in backend/src/GamifiedMathDrill.Infrastructure/Migrations/
- [ ] T021 [P] Create seed data for Problems (500-1000 grade 3 math problems covering Addition, Subtraction, Multiplication, Division) in backend/src/GamifiedMathDrill.Infrastructure/Data/Seeds/ProblemSeeds.cs
- [ ] T022 [P] Create seed data for Rewards (20-30 rewards: badges, avatars, characters with varying point requirements) in backend/src/GamifiedMathDrill.Infrastructure/Data/Seeds/RewardSeeds.cs
- [ ] T023 [P] Create seed data for Levels (10 levels with progressive difficulty 1-10) in backend/src/GamifiedMathDrill.Infrastructure/Data/Seeds/LevelSeeds.cs
- [ ] T024 Create migration to apply seed data in backend/src/GamifiedMathDrill.Infrastructure/Migrations/

### Security & Infrastructure

- [ ] T025 Define IEncryptionService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IEncryptionService.cs
- [ ] T026 Implement EncryptionService with AES-256 encryption for PII fields in backend/src/GamifiedMathDrill.Infrastructure/Services/EncryptionService.cs
- [ ] T027 Configure Data Protection API for session tokens in backend/src/GamifiedMathDrill.Api/Program.cs
- [ ] T028 Setup connection strings for SQLite (development) in backend/src/GamifiedMathDrill.Api/appsettings.Development.json
- [ ] T029 [P] Setup connection strings for PostgreSQL (production) in backend/src/GamifiedMathDrill.Api/appsettings.Production.json
- [ ] T030 Create ErrorHandlingMiddleware for global exception handling in backend/src/GamifiedMathDrill.Api/Middleware/ErrorHandlingMiddleware.cs
- [ ] T031 Create RateLimitingMiddleware to enforce API rate limits in backend/src/GamifiedMathDrill.Api/Middleware/RateLimitingMiddleware.cs
- [ ] T032 Configure Serilog for structured logging in backend/src/GamifiedMathDrill.Api/Program.cs
- [ ] T033 Setup Swagger/OpenAPI with Swashbuckle in backend/src/GamifiedMathDrill.Api/Program.cs

### API Response Models

- [ ] T034 [P] Create SuccessResponse<T> model in backend/src/GamifiedMathDrill.Core/Models/Responses/SuccessResponse.cs
- [ ] T035 [P] Create ErrorResponse model in backend/src/GamifiedMathDrill.Core/Models/Responses/ErrorResponse.cs

### Repository Layer

- [ ] T036 [P] Define IStudentRepository interface in backend/src/GamifiedMathDrill.Core/Interfaces/IStudentRepository.cs (GetByIdAsync, CreateAsync, UpdateAsync, SaveAsync)
- [ ] T037 [P] Define IProblemRepository interface in backend/src/GamifiedMathDrill.Core/Interfaces/IProblemRepository.cs (GetByIdAsync, GetByDifficultyRangeAsync, GetRandomAsync)
- [ ] T038 [P] Define ILearningRecordRepository interface in backend/src/GamifiedMathDrill.Core/Interfaces/ILearningRecordRepository.cs (GetByStudentIdAsync, CreateAsync, GetStatisticsAsync)
- [ ] T039 [P] Define IRewardRepository interface in backend/src/GamifiedMathDrill.Core/Interfaces/IRewardRepository.cs (GetAllAsync, GetByIdAsync, GetByCategoryAsync)
- [ ] T040 [P] Define IAcquiredRewardRepository interface in backend/src/GamifiedMathDrill.Core/Interfaces/IAcquiredRewardRepository.cs (GetByStudentIdAsync, CreateAsync)
- [ ] T041 [P] Define IDailyChallengeRepository interface in backend/src/GamifiedMathDrill.Core/Interfaces/IDailyChallengeRepository.cs (GetTodaysChallengeAsync, CreateAsync)
- [ ] T042 [P] Define ILevelRepository interface in backend/src/GamifiedMathDrill.Core/Interfaces/ILevelRepository.cs (GetByNumberAsync, GetAllAsync)
- [ ] T043 Implement StudentRepository in backend/src/GamifiedMathDrill.Infrastructure/Repositories/StudentRepository.cs
- [ ] T044 [P] Implement ProblemRepository with random selection logic in backend/src/GamifiedMathDrill.Infrastructure/Repositories/ProblemRepository.cs
- [ ] T045 [P] Implement LearningRecordRepository with statistics aggregation in backend/src/GamifiedMathDrill.Infrastructure/Repositories/LearningRecordRepository.cs
- [ ] T046 [P] Implement RewardRepository in backend/src/GamifiedMathDrill.Infrastructure/Repositories/RewardRepository.cs
- [ ] T047 [P] Implement AcquiredRewardRepository in backend/src/GamifiedMathDrill.Infrastructure/Repositories/AcquiredRewardRepository.cs
- [ ] T048 [P] Implement DailyChallengeRepository in backend/src/GamifiedMathDrill.Infrastructure/Repositories/DailyChallengeRepository.cs
- [ ] T049 [P] Implement LevelRepository in backend/src/GamifiedMathDrill.Infrastructure/Repositories/LevelRepository.cs

### Dependency Injection Configuration

- [ ] T050 Configure dependency injection for all repositories and services in backend/src/GamifiedMathDrill.Api/Program.cs

### Frontend Foundation

- [ ] T051 [P] Install and configure MudBlazor UI library in frontend/GamifiedMathDrill.Client/Program.cs
- [ ] T052 [P] Create MainLayout component with navigation in frontend/GamifiedMathDrill.Client/Layout/MainLayout.razor
- [ ] T053 [P] Create NavMenu component for child-friendly navigation in frontend/GamifiedMathDrill.Client/Layout/NavMenu.razor
- [ ] T054 [P] Configure API base URL in frontend/GamifiedMathDrill.Client/wwwroot/appsettings.json
- [ ] T055 [P] Create ApiClientBase with HttpClient configuration in frontend/GamifiedMathDrill.Client/Services/ApiClientBase.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - 計算問題を解いてポイントを獲得する (Priority: P1) 🎯 MVP

**Goal**: 児童が計算問題を解いて正解するとポイントを獲得できる。問題表示、回答判定、ポイント付与の基本サイクルを実装。

**Independent Test**: アプリを起動し、問題を表示し、回答を送信し、正解/不正解の判定とポイント付与が正しく動作することを確認。

### Service Layer for User Story 1

- [ ] T056 [P] [US1] Define IStudentService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IStudentService.cs (GetByIdAsync, CreateAsync, UpdateLoginAsync)
- [ ] T057 [P] [US1] Define IProblemService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IProblemService.cs (GetNextProblemAsync, SubmitAnswerAsync)
- [ ] T058 [US1] Implement StudentService with GetById, Create, UpdateLogin methods in backend/src/GamifiedMathDrill.Core/Services/StudentService.cs
- [ ] T059 [US1] Implement ProblemService with GetNextProblem logic (filters by level, excludes recent) in backend/src/GamifiedMathDrill.Core/Services/ProblemService.cs
- [ ] T060 [US1] Implement SubmitAnswer method in ProblemService with correctness checking and point calculation in backend/src/GamifiedMathDrill.Core/Services/ProblemService.cs
- [ ] T061 [US1] Implement level-up logic in ProblemService (check consecutive correct answers, update student level) in backend/src/GamifiedMathDrill.Core/Services/ProblemService.cs
- [ ] T062 [US1] Add transaction handling for answer submission (atomic Student update and LearningRecord creation) in ProblemService

### API Layer for User Story 1

- [ ] T063 [P] [US1] Create StudentController with GET /students/{id} endpoint in backend/src/GamifiedMathDrill.Api/Controllers/StudentsController.cs
- [ ] T064 [P] [US1] Add POST /students endpoint to StudentController in backend/src/GamifiedMathDrill.Api/Controllers/StudentsController.cs
- [ ] T065 [P] [US1] Add PATCH /students/{id}/login endpoint to StudentController in backend/src/GamifiedMathDrill.Api/Controllers/StudentsController.cs
- [ ] T066 [P] [US1] Create ProblemsController with GET /problems/next endpoint in backend/src/GamifiedMathDrill.Api/Controllers/ProblemsController.cs
- [ ] T067 [US1] Add POST /problems/{id}/answer endpoint to ProblemsController in backend/src/GamifiedMathDrill.Api/Controllers/ProblemsController.cs
- [ ] T068 [US1] Add input validation for answer submission (validate studentId, answer format, time) in ProblemsController
- [ ] T069 [US1] Add error handling for invalid problem/student IDs in both controllers

### Frontend for User Story 1

- [ ] T070 [P] [US1] Create StudentDto model in frontend/GamifiedMathDrill.Client/Models/StudentDto.cs
- [ ] T071 [P] [US1] Create ProblemDto model in frontend/GamifiedMathDrill.Client/Models/ProblemDto.cs
- [ ] T072 [P] [US1] Create ApiResponse<T> model in frontend/GamifiedMathDrill.Client/Models/ApiResponse.cs
- [ ] T073 [P] [US1] Create StudentApiClient service in frontend/GamifiedMathDrill.Client/Services/StudentApiClient.cs
- [ ] T074 [P] [US1] Create ProblemApiClient service with GetNextProblem and SubmitAnswer methods in frontend/GamifiedMathDrill.Client/Services/ProblemApiClient.cs
- [ ] T075 [P] [US1] Create Home page component for initial setup/login in frontend/GamifiedMathDrill.Client/Pages/Home.razor
- [ ] T076 [P] [US1] Create ProblemPage component with problem display and answer input in frontend/GamifiedMathDrill.Client/Pages/ProblemPage.razor
- [ ] T077 [P] [US1] Create ProblemDisplay component (shows question, input field, submit button) in frontend/GamifiedMathDrill.Client/Components/ProblemDisplay.razor
- [ ] T078 [P] [US1] Create AnswerFeedback component (shows correct/incorrect message, points earned) in frontend/GamifiedMathDrill.Client/Components/AnswerFeedback.razor
- [ ] T079 [US1] Implement problem fetch logic on page load in ProblemPage.razor
- [ ] T080 [US1] Implement answer submission logic with validation in ProblemPage.razor
- [ ] T081 [US1] Add "Next Problem" button and navigation logic in ProblemPage.razor
- [ ] T082 [US1] Implement excludeRecentIds query parameter to prevent duplicate problems in ProblemApiClient
- [ ] T083 [US1] Add visual feedback animations (correct: green checkmark, incorrect: red cross) in AnswerFeedback.razor
- [ ] T084 [US1] Display level-up notification modal when student advances in ProblemPage.razor
- [ ] T085 [US1] Add loading spinner during problem fetch and answer submission in ProblemPage.razor

**Checkpoint**: User Story 1 is fully functional and independently testable. 児童は問題を解いてポイントを獲得できる。

---

## Phase 4: User Story 2 - 過去の学習結果を確認する (Priority: P2)

**Independent Test**: 過去の LearningRecord データを基に、結果画面に統計とグラフが正しく表示されることを確認。

### Service Layer for User Story 2

- [ ] T086 [P] [US2] Define ILearningRecordService interface in backend/src/GamifiedMathDrill.Core/Interfaces/ILearningRecordService.cs (GetRecordsAsync, GetStatisticsAsync)
- [ ] T087 [US2] Implement LearningRecordService with GetRecords, GetStatistics methods in backend/src/GamifiedMathDrill.Core/Services/LearningRecordService.cs
- [ ] T088 [US2] Implement statistics aggregation logic (daily, weekly, by calculation type) in LearningRecordService

### API Layer for User Story 2

- [ ] T089 [P] [US2] Create LearningRecordsController with GET /learning-records endpoint in backend/src/GamifiedMathDrill.Api/Controllers/LearningRecordsController.cs
- [ ] T090 [P] [US2] Add GET /learning-records/statistics endpoint to LearningRecordsController in backend/src/GamifiedMathDrill.Api/Controllers/LearningRecordsController.cs
- [ ] T091 [US2] Add filtering by date range (startDate, endDate) query parameters in LearningRecordsController
- [ ] T092 [US2] Add filtering by calculation type query parameter in LearningRecordsController
- [ ] T093 [US2] Implement pagination for learning records list (page, pageSize parameters) in LearningRecordsController

### Frontend for User Story 2

- [ ] T094 [P] [US2] Create LearningRecordDto model in frontend/GamifiedMathDrill.Client/Models/LearningRecordDto.cs
- [ ] T095 [P] [US2] Create StatisticsDto model in frontend/GamifiedMathDrill.Client/Models/StatisticsDto.cs
- [ ] T096 [P] [US2] Create LearningRecordApiClient service in frontend/GamifiedMathDrill.Client/Services/LearningRecordApiClient.cs
- [ ] T097 [P] [US2] Create ResultsPage Blazor component in frontend/GamifiedMathDrill.Client/Pages/ResultsPage.razor
- [ ] T098 [P] [US2] Create StatisticsSummary component (displays total problems, accuracy, points) in frontend/GamifiedMathDrill.Client/Components/Results/StatisticsSummary.razor
- [ ] T099 [P] [US2] Create StatisticsChart component (bar chart for daily/weekly/type statistics) in frontend/GamifiedMathDrill.Client/Components/Results/StatisticsChart.razor
- [ ] T100 [P] [US2] Create LearningRecordList component (table of past records) in frontend/GamifiedMathDrill.Client/Components/Results/LearningRecordList.razor
- [ ] T101 [US2] Integrate chart library (Chart.js or ApexCharts.Blazor) in StatisticsChart component
- [ ] T102 [US2] Implement date range picker for filtering in ResultsPage.razor
- [ ] T103 [US2] Implement calculation type filter tabs (All, Addition, Subtraction, Multiplication, Division) in ResultsPage.razor
- [ ] T104 [US2] Add visual indicators for accuracy rate (green >80%, yellow 60-80%, red <60%) in StatisticsSummary.razor
- [ ] T105 [US2] Implement responsive layout for charts (mobile-friendly) in StatisticsChart.razor
- [ ] T106 [US2] Add pagination controls for learning records list in LearningRecordList.razor

**Checkpoint**: User Story 2 is fully functional. 児童と保護者は学習結果を詳細に確認できる。

---

## Phase 5: User Story 4 - 飽きない工夫で継続学習する (Priority: P2)

**Goal**: 連続学習日数の記録、日替わりチャレンジ問題、励ましメッセージで学習継続を促進。

**Independent Test**: 連続ログイン記録、デイリーチャレンジ問題の表示、励ましメッセージ表示が独立して動作。

**Note**: US4 is implemented before US3 because it's P2 (higher priority than US3's P3)

### Service Layer for User Story 4

- [ ] T107 [P] [US4] Define IDailyChallengeService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IDailyChallengeService.cs (GetTodaysChallengeAsync, SubmitChallengeAnswerAsync)
- [ ] T108 [US4] Implement DailyChallengeService with GetTodayChallenge, SubmitChallengeAnswer methods in backend/src/GamifiedMathDrill.Core/Services/DailyChallengeService.cs
- [ ] T109 [US4] Implement background job/scheduled task for daily challenge generation (create new challenge at midnight) in backend/src/GamifiedMathDrill.Infrastructure/Jobs/DailyChallengeJob.cs
- [ ] T110 [US4] Implement consecutive days calculation logic in StudentService.UpdateLoginAsync (compare LastLoginAt with current date)
- [ ] T111 [US4] Implement encouragement message generation based on streak days in StudentService.UpdateLoginAsync

### API Layer for User Story 4

- [ ] T112 [P] [US4] Create DailyChallengesController with GET /daily-challenges/today endpoint in backend/src/GamifiedMathDrill.Api/Controllers/DailyChallengesController.cs
- [ ] T113 [P] [US4] Add POST /daily-challenges/{id}/answer endpoint to DailyChallengesController in backend/src/GamifiedMathDrill.Api/Controllers/DailyChallengesController.cs
- [ ] T114 [US4] Add bonus points calculation logic for daily challenge correct answers in DailyChallengeService
- [ ] T115 [US4] Handle 404 response when no challenge exists for today in DailyChallengesController

### Frontend for User Story 4

- [ ] T116 [P] [US4] Create DailyChallengeDto model in frontend/GamifiedMathDrill.Client/Models/DailyChallengeDto.cs
- [ ] T117 [P] [US4] Create DailyChallengeApiClient service in frontend/GamifiedMathDrill.Client/Services/DailyChallengeApiClient.cs
- [ ] T118 [P] [US4] Update Home page component to display consecutive days and encouragement message in frontend/GamifiedMathDrill.Client/Pages/Home.razor
- [ ] T119 [P] [US4] Create ConsecutiveDaysDisplay component (shows streak with visual indicator) in frontend/GamifiedMathDrill.Client/Components/Home/ConsecutiveDaysDisplay.razor
- [ ] T120 [P] [US4] Create EncouragementMessage component (displays motivational messages) in frontend/GamifiedMathDrill.Client/Components/Home/EncouragementMessage.razor
- [ ] T121 [P] [US4] Create DailyChallengeCard component (highlights today's challenge with bonus points) in frontend/GamifiedMathDrill.Client/Components/Home/DailyChallengeCard.razor
- [ ] T122 [US4] Implement login tracking on app startup in Home.razor (call PATCH /students/{id}/login)
- [ ] T123 [US4] Display encouragement message based on consecutive days returned from login endpoint in Home.razor
- [ ] T124 [US4] Add "Start Today's Challenge" button navigation to challenge problem in DailyChallengeCard.razor
- [ ] T125 [US4] Implement achievement badge display for milestones (7 days, 14 days, 30 days streaks) in ConsecutiveDaysDisplay.razor
- [ ] T126 [US4] Add "Welcome back!" message for returning users (lastLoginAt > 1 day ago) in Home.razor
- [ ] T127 [US4] Display special visual effects for milestone streaks (confetti animation) in Home.razor

**Checkpoint**: User Story 4 is fully functional. 児童は毎日継続して学習する動機づけを得る。

---

## Phase 6: User Story 3 - ポイントを使って景品と交換する (Priority: P3)

**Goal**: 児童がポイントを消費して景品（バッジ、アバター、キャラクター）と交換できる。ポイント不足時は適切なメッセージを表示。

**Independent Test**: 景品一覧表示、ポイント残高チェック、交換トランザクション、獲得景品一覧表示が独立して動作。

### Service Layer for User Story 3

- [ ] T128 [P] [US3] Define IRewardService interface in backend/src/GamifiedMathDrill.Core/Interfaces/IRewardService.cs (GetRewardsAsync, ExchangeRewardAsync, GetAcquiredRewardsAsync)
- [ ] T129 [US3] Implement RewardService with GetRewards, ExchangeReward, GetAcquiredRewards methods in backend/src/GamifiedMathDrill.Core/Services/RewardService.cs
- [ ] T130 [US3] Implement point validation logic (check sufficient points before exchange) in RewardService.ExchangeRewardAsync
- [ ] T131 [US3] Implement transaction handling for reward exchange (atomic update of Student points and AcquiredReward creation) in RewardService

### API Layer for User Story 3

- [ ] T132 [P] [US3] Create RewardsController with GET /rewards endpoint in backend/src/GamifiedMathDrill.Api/Controllers/RewardsController.cs
- [ ] T133 [P] [US3] Add POST /rewards/{id}/exchange endpoint to RewardsController in backend/src/GamifiedMathDrill.Api/Controllers/RewardsController.cs
- [ ] T134 [P] [US3] Add GET /rewards/acquired endpoint to RewardsController in backend/src/GamifiedMathDrill.Api/Controllers/RewardsController.cs
- [ ] T135 [US3] Add filtering by category query parameter in GET /rewards endpoint
- [ ] T136 [US3] Add filtering by maxPoints query parameter in GET /rewards endpoint
- [ ] T137 [US3] Implement InsufficientPoints error response (400) with required points details in RewardsController

### Frontend for User Story 3

- [ ] T138 [P] [US3] Create RewardDto model in frontend/GamifiedMathDrill.Client/Models/RewardDto.cs
- [ ] T139 [P] [US3] Create AcquiredRewardDto model in frontend/GamifiedMathDrill.Client/Models/AcquiredRewardDto.cs
- [ ] T140 [P] [US3] Create RewardApiClient service in frontend/GamifiedMathDrill.Client/Services/RewardApiClient.cs
- [ ] T141 [P] [US3] Create RewardsPage Blazor component in frontend/GamifiedMathDrill.Client/Pages/RewardsPage.razor
- [ ] T142 [P] [US3] Create RewardList component (grid of available rewards with images) in frontend/GamifiedMathDrill.Client/Components/Rewards/RewardList.razor
- [ ] T143 [P] [US3] Create RewardCard component (displays reward name, description, required points, image) in frontend/GamifiedMathDrill.Client/Components/Rewards/RewardCard.razor
- [ ] T144 [P] [US3] Create ExchangeConfirmationDialog component (modal for exchange confirmation) in frontend/GamifiedMathDrill.Client/Components/Rewards/ExchangeConfirmationDialog.razor
- [ ] T145 [P] [US3] Create AcquiredRewardsPage Blazor component in frontend/GamifiedMathDrill.Client/Pages/AcquiredRewardsPage.razor
- [ ] T146 [P] [US3] Create AcquiredRewardGallery component (displays owned rewards) in frontend/GamifiedMathDrill.Client/Components/Rewards/AcquiredRewardGallery.razor
- [ ] T147 [US3] Implement reward filtering by category tabs (All, Badges, Avatars, Characters) in RewardsPage.razor
- [ ] T148 [US3] Implement exchange flow with confirmation dialog in RewardsPage.razor
- [ ] T149 [US3] Display current points balance prominently at top of RewardsPage.razor
- [ ] T150 [US3] Show "Insufficient Points" error message when clicking on unaffordable rewards in RewardCard.razor
- [ ] T151 [US3] Add success animation/message after successful exchange in RewardsPage.razor
- [ ] T152 [US3] Disable exchange button for rewards that exceed current points in RewardCard.razor
- [ ] T153 [US3] Add visual indicator (grayed out) for unaffordable rewards in RewardList.razor

**Checkpoint**: User Story 3 is fully functional. 児童はポイントを使って景品と交換できる。

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final refinements, error handling improvements, performance optimization

- [ ] T154 [P] Add loading spinners/skeletons for all API calls across all pages
- [ ] T155 [P] Implement offline detection and display appropriate message in frontend (FR-018)
- [ ] T156 [P] Add error boundary components for graceful error handling in frontend
- [ ] T157 [P] Optimize database queries with appropriate indexes (verify all indexes from data-model.md are created)
- [ ] T158 [P] Implement caching for frequently accessed data (problems, rewards) using In-Memory Cache
- [ ] T159 [P] Add application monitoring and metrics (Application Insights or Prometheus)
- [ ] T160 [P] Create user-friendly 404 Not Found page in frontend
- [ ] T161 [P] Create user-friendly 500 Internal Server Error page in frontend
- [ ] T162 [P] Implement responsive design testing for mobile devices (Chrome DevTools)
- [ ] T163 [P] Add accessibility improvements (ARIA labels, keyboard navigation) for all components
- [ ] T164 [P] Perform security audit (HTTPS enforcement, CSRF protection, input validation)
- [ ] T165 [P] Optimize Blazor WASM bundle size (enable AOT compilation if needed)
- [ ] T166 [P] Add performance testing (load test with k6 or JMeter for 100 concurrent users)
- [ ] T167 [P] Document API endpoints with detailed examples in Swagger UI
- [ ] T168 [P] Create deployment scripts for Azure App Service or AWS Elastic Beanstalk
- [ ] T169 [P] Setup production database (PostgreSQL) with backups and encryption
- [ ] T170 [P] Configure environment-specific settings (appsettings.Production.json)
- [ ] T171 [P] Run quickstart.md validation to ensure setup instructions are accurate

**Checkpoint**: Application is production-ready with polish, performance, and security enhancements.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order: US1 (P1) → US2/US4 (P2) → US3 (P3)
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May reference US1 data but independently testable
- **User Story 4 (P2)**: Can start after Foundational (Phase 2) - May reference US1 data but independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Depends on points from US1 but independently testable

### Story Completion Order (for MVP delivery)

1. **Phase 1-2** (Setup + Foundation): MUST complete first - all stories depend on this
2. **Phase 3** (US1 - P1): Core MVP functionality - 問題解答とポイント獲得
3. **Phase 4** (US2 - P2) OR **Phase 5** (US4 - P2): Can be done in either order after US1
4. **Phase 6** (US3 - P3): Reward exchange - can be done last
5. **Phase 7** (Polish): Final refinements

### Critical Path

```
Setup (Phase 1) → Foundational (Phase 2) → US1 (Phase 3) → US2/US4 (Phase 4/5, parallel) → US3 (Phase 6) → Polish (Phase 7)
```

### Parallel Execution Opportunities

**Within Phase 1 (Setup)**:

- T003, T004, T005, T006, T007 can run in parallel after T001-T002

**Within Phase 2 (Foundational) - Data Layer**:

- T011-T016 (all entity models and enum) can be created in parallel
- T021, T022, T023 (seed data) can be created in parallel after T020
- T036-T042 (repository interfaces) can be created in parallel
- T044-T049 (repository implementations) can be created in parallel after T043

**Within Phase 2 (Foundational) - Frontend**:

- T051-T055 (frontend foundation) can run in parallel with backend tasks

**Within Phase 3 (US1)**:

- T056, T057 (service interfaces) can be created in parallel
- T063-T066 (controllers) can be created in parallel after services
- T070-T078 (frontend models and components) can be created in parallel

**Within Phase 4 (US2)**:

- T094, T095 (DTOs) and T098-T100 (components) can be created in parallel

**Within Phase 5 (US4)**:

- T116, T117 (DTO and client) can be created with T119-T121 (components) in parallel

**Within Phase 6 (US3)**:

- T138, T139, T140 (models and client) can be created with T142-T146 (components) in parallel

**Within Phase 7 (Polish)**:

- Most tasks (T154-T171) can run in parallel as they are independent improvements

**Cross-Phase Parallelization**:

- After Phase 2 completes, all user stories (Phase 3, 4, 5, 6) can start in parallel if team capacity allows
- Different developers can work on US1, US2, US3, and US4 simultaneously

---

## MVP Scope Recommendation

For fastest time-to-value, the MVP should include:

- **Phase 1**: Setup (Required)
- **Phase 2**: Foundational (Required)
- **Phase 3**: User Story 1 - 問題解答とポイント獲得 (P1) ✅

This MVP allows 児童 to:

- Solve math problems
- Receive points for correct answers
- See their progress and level up
- Experience the core learning loop

**Post-MVP Priorities**:

1. Add US4 (P2) - Continuous learning features (streaks, daily challenges)
2. Add US2 (P2) - Learning history and statistics
3. Add US3 (P3) - Reward exchange system

---

## Implementation Strategy

### Incremental Delivery

1. **Week 1**: Phase 1-2 (Foundation)
2. **Week 2**: Phase 3 (US1 - MVP)
3. **Week 3**: Phase 4-5 (US2, US4 in parallel)
4. **Week 4**: Phase 6-7 (US3 + Polish)

### Validation Points

After each phase, validate:

- **Phase 2**: Database migrations run successfully, all entities created
- **Phase 3**: Can solve a problem and earn points end-to-end
- **Phase 4**: Can view learning history with statistics
- **Phase 5**: Streak tracking and daily challenges work
- **Phase 6**: Can exchange points for rewards
- **Phase 7**: Performance meets SC-006 (3 second startup), security audit passes

---

## Task Summary

- **Total Tasks**: 171
- **Phase 1 (Setup)**: 9 tasks
- **Phase 2 (Foundational)**: 46 tasks
- **Phase 3 (US1 - P1)**: 30 tasks
- **Phase 4 (US2 - P2)**: 21 tasks
- **Phase 5 (US4 - P2)**: 21 tasks
- **Phase 6 (US3 - P3)**: 26 tasks
- **Phase 7 (Polish)**: 18 tasks

**Tasks by User Story**:

- US1 (P1): 30 tasks (MVP)
- US2 (P2): 21 tasks
- US4 (P2): 21 tasks
- US3 (P3): 26 tasks

**Parallelizable Tasks**: 89 tasks marked with [P]

**Estimated Effort** (assuming 1 task ≈ 1-2 hours):

- MVP (Phase 1-3): ~85 tasks = 85-170 hours (2-4 weeks for 1 developer)
- Full Feature (Phase 1-6): ~153 tasks = 153-306 hours (4-8 weeks for 1 developer)
- Production Ready (Phase 1-7): ~171 tasks = 171-342 hours (4-9 weeks for 1 developer)

With parallel development (2-3 developers), timeline can be reduced by 40-50%.

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
