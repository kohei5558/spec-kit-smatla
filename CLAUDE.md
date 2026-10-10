# CLAUDE.md

小学生（主な対象は小3・8〜9歳）向けのゲーミフィケーション型算数ドリル。spec-kit による仕様駆動開発。

## 構成

- `backend/` ASP.NET Core 10 Web API（`GamifiedMathDrill.sln`）
  - `src/GamifiedMathDrill.Api` コントローラー・DTO・ミドルウェア・`Program.cs`
  - `src/GamifiedMathDrill.Core` モデル・インターフェース・ドメインサービス
  - `src/GamifiedMathDrill.Infrastructure` EF Core（`ApplicationDbContext`、マイグレーション、シード）・Identity・サービス実装
  - `tests/GamifiedMathDrill.Tests.Unit` / `tests/GamifiedMathDrill.Tests.Integration`（xUnit、`WebApplicationFactory` + InMemory DB）
- `frontend/GamifiedMathDrill.Client` Blazor WebAssembly + MudBlazor 7（ソリューション外の単独プロジェクト）
- `specs/###-feature/` 機能ごとの spec.md / plan.md / tasks.md
- `.specify/memory/constitution.md` プロジェクト原則（児童の安全・プライバシーは交渉不可）

## コマンド

```bash
dotnet build backend/GamifiedMathDrill.sln
dotnet build frontend/GamifiedMathDrill.Client
dotnet test backend/GamifiedMathDrill.sln
dotnet test backend/tests/GamifiedMathDrill.Tests.Integration --filter "FullyQualifiedName~ChildLoginTests"

./scripts/setup-dev-secrets.sh   # 開発用 JWT キーを User Secrets に登録（初回のみ。Windows は scripts/setup-dev-secrets.ps1）
ASPNETCORE_ENVIRONMENT=Development dotnet run --project backend/src/GamifiedMathDrill.Api --urls http://localhost:5242
```

変更後は必ずビルドとテストを通してからコミットする。CI（`.github/workflows/ci.yml`）も同じ内容を実行する。

## 押さえておくべき前提

- 秘密情報（`Jwt:SecretKey`、本番接続文字列など）は appsettings に書かない。開発は User Secrets、本番は環境変数（`Jwt__SecretKey` 等）。
- DB: 開発は SQLite、本番は PostgreSQL。起動時に `DatabaseInitializer` がマイグレーションとシードを実行（テスト環境 `Testing` では実行しない）。デモユーザー（`parent@example.com`）は Development のみ作成。
- DB の種類は設定 `DatabaseProvider`（`Sqlite` / `PostgreSQL`、未指定なら開発は SQLite・それ以外は PostgreSQL）。マイグレーションは DB ごとに `backend/src/GamifiedMathDrill.Migrations.Sqlite` と `GamifiedMathDrill.Migrations.PostgreSQL` に分かれている。
- モデル変更時は**両方**にマイグレーションを追加する（`dotnet tool restore` の後）:
  - `ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add <Name> --project backend/src/GamifiedMathDrill.Migrations.Sqlite --startup-project backend/src/GamifiedMathDrill.Api --output-dir Migrations`
  - `ASPNETCORE_ENVIRONMENT=Production DatabaseProvider=PostgreSQL Jwt__SecretKey=design-time-only-key-design-time-only-key ConnectionStrings__DefaultConnection="Host=localhost;Database=design" dotnet ef migrations add <Name> --project backend/src/GamifiedMathDrill.Migrations.PostgreSQL --startup-project backend/src/GamifiedMathDrill.Api --output-dir Migrations`
  - 反映漏れは CI の `has-pending-model-changes` で検出される
- `Program.cs` で `SuppressModelStateInvalidFilter = true` のため、DataAnnotations やバインド失敗は自動で 400 にならない。コントローラーで明示的に検証する。
- 結合テストはテストクラス単位で DB を共有する（`IClassFixture`）。他テストのデータが残る前提で、名前は一意にし件数に依存しない書き方をする。
- 子供ログインの JSON キーは `ChildId`（`Api/DTOs/ChildLoginRequest.cs` とフロントの同名クラスで一致させる）。
- 子供の一覧取得と子供ログインには、保護者が登録した端末のトークン（`X-Device-Token` ヘッダー、`DevicesController` / `DeviceService`）が必須。結合テストでは `AuthenticationHelper.UseParentDeviceAsync` で設定する（006）。
- 全APIは既定でログイン必須（`Program.cs` の FallbackPolicy）。未ログインで使うAPIだけ `[AllowAnonymous]` を付ける。
- 学習者ID（studentId）を受け取るAPIは `this.CanAccessStudentAsync(IStudentAccessService, studentId)` で確認し、不可なら 404 を返す（子供は自分、保護者は自分の子供のみ）。子供と学習者の対応は子供ユーザーの `StudentId` で判定し、名前では探さない。
- 景品は家庭ごと（`Reward.ParentId`）。一覧・詳細・更新・削除・交換・交換申請は `this.GetFamilyParentIdAsync` で家庭を確認し、他家庭は 404。初期景品（`Core/Models/StarterRewards.cs`）は保護者の登録時に家庭ごとにコピーされる（007）。
- 問題は `Core/Services/ProblemGenerator.cs` が難易度表（specs/008）に従って作る（乱数の種固定）。起動のたびに `ProblemSynchronizer` が DB の問題を最新のセットに合わせる（足りない問題を追加、セットにない問題は削除せず `Problem.IsActive = false` にして出題しない、011）。出題・チャレンジの問題選びは `IsActive` の問題だけ。問題の作り方を変えると、公開済みの DB でも次の起動で入れ替わる。子供の始めるレベルは学年で決まる（`Core/Models/GradeStartLevel.cs`）。`Student.CorrectAnswers` はレベルアップ用の**連続正解数**で、正答率には学習記録を使う。
- あまりのあるわり算（`CalculationType.DivisionWithRemainder`、009）は `Problem.CorrectRemainder` / `LearningRecord.StudentRemainder` を持ち、商とあまりの両方が合うときだけ正解。新しい計算の種類は `ProblemGenerator` の種類リストの**末尾**に足す（既存の問題セットを変えないため）。デイリーチャレンジ（答えは1つ）には出さない。
- デイリーチャレンジ（010）は学習者ごと・日本時間の日ごとに1つ（`DailyChallenge.StudentId`）。その日に初めて開いたときに今のレベルの最大難易度から作る。回答は子供本人だけ・1日1回（2回目は 409、`AnsweredAt` を同時実行の確認に使う）。連続正解数・レベルは変えない。時刻は `TimeProvider` で取得し、「今日」は `Core/Services/JapanTime`（日本時間、DB の日時は UTC）で決める。
- PIN ロックアウトは `IMemoryCache` で 3 回失敗 → 5 分（`ChildAccountService`）。
- MudBlazor は 7 系のまま。FluentAssertions は 6 系のまま（v8 以降は商用有償ライセンス）。
- 子供向け UI の文言はやさしい日本語にする。問題の難易度は子供の「レベル」と混ざらないよう画面では「むずかしさ」と表示する。

## 開発ルール（詳細は WORKFLOW.md）

- main で直接作業しない。`feature/` `fix/` `refactor/` `docs/` `chore/` ブランチで作業し PR を作る。
- コミットメッセージは `feat:` `fix:` `refactor:` `docs:` `test:` `chore:` の接頭辞 + 日本語の説明。
- バグ修正は再現テストを先に書く（constitution IV）。
- 仕様変更や新機能は spec-kit の流れ（specify → plan → tasks → implement）で `specs/` に記録し、実装したら tasks.md のチェックも更新する。
