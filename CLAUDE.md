# CLAUDE.md

小学生（主な対象は小3・8〜9歳）向けのゲーミフィケーション型学習ドリル（算数と、国語の漢字）。spec-kit による仕様駆動開発。

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

画面の流れの自動テスト（E2E、018）は `e2e/` で `npm ci && npx playwright test`（本物の API と画面をまっさらな SQLite で起動する。開発中のサーバーは止めておく）。画面を変えたら E2E も通す。

変更後は必ずビルドとテストを通してからコミットする。CI（`.github/workflows/ci.yml`）も同じ内容と E2E を実行する。

## 押さえておくべき前提

- 秘密情報（`Jwt:SecretKey`、本番接続文字列など）は appsettings に書かない。開発は User Secrets、本番は環境変数（`Jwt__SecretKey` 等）。
- DB: 開発は SQLite、本番は PostgreSQL。起動時に `DatabaseInitializer` がマイグレーションとシードを実行（テスト環境 `Testing` では実行しない）。デモユーザー（`parent@example.com`）は Development のみ作成。
- DB の種類は設定 `DatabaseProvider`（`Sqlite` / `PostgreSQL`、未指定なら開発は SQLite・それ以外は PostgreSQL）。マイグレーションは DB ごとに `backend/src/GamifiedMathDrill.Migrations.Sqlite` と `GamifiedMathDrill.Migrations.PostgreSQL` に分かれている。
- モデル変更時は**両方**にマイグレーションを追加する（`dotnet tool restore` の後）:
  - `ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add <Name> --project backend/src/GamifiedMathDrill.Migrations.Sqlite --startup-project backend/src/GamifiedMathDrill.Api --output-dir Migrations`
  - `ASPNETCORE_ENVIRONMENT=Production DatabaseProvider=PostgreSQL Jwt__SecretKey=design-time-only-key-design-time-only-key ConnectionStrings__DefaultConnection="Host=localhost;Database=design" dotnet ef migrations add <Name> --project backend/src/GamifiedMathDrill.Migrations.PostgreSQL --startup-project backend/src/GamifiedMathDrill.Api --output-dir Migrations`
  - 反映漏れは CI の `has-pending-model-changes` で検出される
- `Program.cs` で `SuppressModelStateInvalidFilter = true` のため、DataAnnotations やバインド失敗は自動で 400 にならない。コントローラーで明示的に検証する。
- E2E（`e2e/tests`）: 順番は setup（保護者ログイン・端末登録）→ parent / child → approve。ログインの回数制限（15分で5回）があるので保護者のログインは setup の1回だけ（`storageState`）。問題の正解は API が返さないので E2E 用の DB から読む（`correctAnswer`）。画面の例外と API の 500 番台はどのテストでも失敗にする。
- 結合テストはテストクラス単位で DB を共有する（`IClassFixture`）。他テストのデータが残る前提で、名前は一意にし件数に依存しない書き方をする。
- 子供ログインの JSON キーは `ChildId`（`Api/DTOs/ChildLoginRequest.cs` とフロントの同名クラスで一致させる）。
- 子供の一覧取得と子供ログインには、保護者が登録した端末のトークン（`X-Device-Token` ヘッダー、`DevicesController` / `DeviceService`）が必須。結合テストでは `AuthenticationHelper.UseParentDeviceAsync` で設定する（006）。
- 全APIは既定でログイン必須（`Program.cs` の FallbackPolicy）。未ログインで使うAPIだけ `[AllowAnonymous]` を付ける。
- 学習者ID（studentId）を受け取るAPIは `this.CanAccessStudentAsync(IStudentAccessService, studentId)` で確認し、不可なら 404 を返す（子供は自分、保護者は自分の子供のみ）。子供と学習者の対応は子供ユーザーの `StudentId` で判定し、名前では探さない。
- 景品は家庭ごと（`Reward.ParentId`）。一覧・詳細・更新・削除・交換・交換申請は `this.GetFamilyParentIdAsync` で家庭を確認し、他家庭は 404。初期景品（`Core/Models/StarterRewards.cs`）は保護者の登録時に家庭ごとにコピーされる（007）。
- 問題は `Core/Services/ProblemGenerator.cs` が難易度表（specs/008）に従って作る（乱数の種固定）。起動のたびに `ProblemSynchronizer` が DB の問題を最新のセットに合わせる（足りない問題を追加、セットにない問題は削除せず `Problem.IsActive = false` にして出題しない、011）。出題・チャレンジの問題選びは `IsActive` の問題だけ。問題の作り方を変えると、公開済みの DB でも次の起動で入れ替わる。レベルと連続正解数は**計算の種類ごと**（`StudentLevel`、014）。四則とあまりのあるわり算は学年で始まり（`GradeStartLevel`）、それ以外の種類（新しい単元）はレベル1から。行が無い種類はレベル1として扱う（`StudentLevelRules`）。出題の難易度は問題の種類のレベルで決め、回答はその種類の連続正解数・レベルだけを変える。連続正解数はレベルアップ用で、正答率には学習記録を使う。「◯日連続」は1問以上解いた日（日本時間）を学習記録から `StudyStreak` で計算する（`Student.ConsecutiveDays` は使わない。ログインでは増えない）。
- あまりのあるわり算（`CalculationType.DivisionWithRemainder`、009）は `Problem.CorrectRemainder` / `LearningRecord.StudentRemainder` を持ち、商とあまりの両方が合うときだけ正解。新しい計算の種類は `ProblemGenerator` の種類リストの**末尾**に足す（既存の問題セットを変えないため）。デイリーチャレンジ（答えは1つ）には出さない。
- 時こくと時間（`CalculationType.Time`、013）は `TimeProblemGenerator` が作る（時刻は 1時〜11時台で12時をまたがない）。答えの形は `Problem.AnswerFormat`（数・あまり・時と分・時間と分・分・分と秒・秒）で、2つの欄がある形は2つ目の答えを `CorrectRemainder` / `StudentRemainder`（回答 API の `remainder`）に持つ。判定は種類ではなく「`CorrectRemainder` があるか」で行う。時計の絵の時刻は `Problem.ClockTimes`（"3:40"）。画面の答えの表示は `Models/AnswerText`。最近出た問題を除くと出せる問題が無いとき（時こくの難易度1は3問）は、最近の問題でも出す。
- 長さ・重さ・かさ（`CalculationType.Measurement`、015）は `MeasurementProblemGenerator` が作る。答えの形は `AnswerFormat.Units` で、欄の単位を `Problem.AnswerUnits`（"mm"、2つなら "km m"）に持つ（API は `answerUnits` のリスト）。単位の組は 10倍・100倍（cm-mm、m-cm、L-dL）と 1000倍（km-m、kg-g、t-kg、L-mL）。dL は大人になってもほぼ使わないため、dL-mL は出さず、L-dL はなおし方（難易度1〜3）だけ。
- 文章問題（`CalculationType.WordProblem`、017）は `WordProblemGenerator` が、保護者が確認したひな形（`specs/017-word-problems/templates.md`）に数を入れて作る。**ひな形の文章を変える・足すときは、先に templates.md を保護者に確認してもらう**（テストもひな形の文章をそのまま持っている）。答えは `AnswerFormat.Units`（"本"）か、あまりのあるわり算は `AnswerFormat.UnitsRemainder`（"人 まい"、画面は「[ ]人 あまり [ ]まい」）。
- まちがえたときの解説（019）は `ExplanationBuilder`（Core/Services、種類ごとに partial）が問題文を読み直して作る（DB には持たない）。文は保護者が確認した `specs/019-explanations/explanations.md` の形で、**文を変える・足すときは先に保護者に確認する**。回答の結果（問題・デイリーチャレンジ）の `explanation`・`hint` に入る（不正解のときだけ）。問題の作り方（問題文の形）を変えたら解説も合わせる（`ExplanationBuilderTests` が全問題で確かめる）。文章問題のひな形の文は `WordProblemTemplates` にまとめてある。
- 国語（漢字、020）: 問題の種類は `CalculationType.KanjiReading`（読み）・`KanjiWriting`（書き）・`KanjiWords`（ことば）。教科は `Subjects.Of(type)` で決まる（`Subject` は DB に持たない）。答えは文字で、`Problem.CorrectText`（正解）・`Problem.Choices`（選択肢、スペース区切り）・`LearningRecord.StudentText`。答えの形は `AnswerFormat.Text`（50音ボタンで ひらがな）と `Choice`（4つから選ぶ）。`CorrectText` がある問題は文字で判定する（数の欄は見ない）。中身は保護者が確認した回ごとの一覧（`specs/020-kanji/kanji-batch1.md`・`specs/023-kanji-batch2/kanji-batch2.md`・`specs/024-kanji-batch3/kanji-batch3.md`・`specs/026-kanji-batch4/kanji-batch4.md`・`specs/027-kanji-batch5/kanji-batch5.md`・`specs/028-kanji-batch6/kanji-batch6.md`）だけで、`Core/Models/KanjiBatch1.cs`〜`KanjiBatch6.cs` に同じ中身を持ち、`KanjiBatches.All` に回の順で並べる（新しい回は末尾に足す）。むずかしさは、すべての回を入れた順に通して 20字ずつ 1〜10 につめる（025。小3の200字で 20字 × 10。前に入れた字は動かない）。配当表と 読みが小学校の範囲かは、文部科学省の表で確かめる（`specs/020-kanji/grade-check.md`）。字がまだ無い むずかしさの レベルでは、問題がある いちばん上の むずかしさから出す（`ProblemService.GetNextProblemAsync`）。**中身を変える・足すときは、先に一覧を保護者に確認してもらう**（`KanjiProblemGeneratorTests` が一覧のファイルと比べる）。出題の「すべて」は教科ごと（API の `subject`、無ければ算数）。画面では「こくご ミックス」を `"Japanese"` として持つ（`CalculationTypeNames.JapaneseMix`）。漢字の解説（022）は `ExplanationBuilder.Kanji` が `KanjiBatches` の中身と、確認済みの型（`specs/022-kanji-explanations/explanations.md`）で作る（`Build` に文字の答え `text` を渡す）。
- デイリーチャレンジ（010）は学習者ごと・日本時間の日ごとに1つ（`DailyChallenge.StudentId`）。その日に初めて開いたときに、四則から1つ選んだ種類のレベルの最大難易度から作る。回答は子供本人だけ・1日1回（2回目は 409、`AnsweredAt` を同時実行の確認に使う）。連続正解数・レベルは変えない。時刻は `TimeProvider` で取得し、「今日」は `Core/Services/JapanTime`（日本時間、DB の日時は UTC）で決める。
- 公開（012、手順は `docs/DEPLOY.md`）: 画面は Cloudflare Pages、API は Render（`backend/Dockerfile`・`render.yaml`）、DB は Neon。画面が使う API のアドレスは `wwwroot/appsettings.json` の `ApiBaseAddress`（デプロイ時に書き換え。Blazor はビルド時にあった設定ファイルしか読まないので `appsettings.Production.json` は足さない）。API の CORS は `Cors:AllowedOrigins`、招待コードは `Registration:InviteCode`（設定があるときだけ登録に必要）。保護者がアップロードした景品の画像は DB（`RewardImage`、`/api/reward-images/{id}`、ログイン不要・1MB まで）に保存し、画面では `ImageUrls.Resolve` で API のアドレスを付ける。無料の API は止まるため、画面は `ApiWakeGate` が `/healthz` で起動を待つ。接続元 IP は `RemoteIpAddress` だけを使い、`X-Forwarded-For` を自分で読まない（偽装で回数制限を逃れられるため）。プロキシの後ろでは `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`（`render.yaml`）で ASP.NET Core に反映させる。
- PIN ロックアウトは `IMemoryCache` で 3 回失敗 → 5 分（`ChildAccountService`）。
- MudBlazor は 7 系のまま。FluentAssertions は 6 系のまま（v8 以降は商用有償ライセンス）。
- 子供向け UI の文言はやさしい日本語にする。問題の難易度は子供の「レベル」と混ざらないよう画面では「むずかしさ」と表示する。

## 開発ルール（詳細は WORKFLOW.md）

- main で直接作業しない。`feature/` `fix/` `refactor/` `docs/` `chore/` ブランチで作業し PR を作る。
  - ブランチは機能・修正1つごとに、中身が分かる名前で `main` から作る（例: `fix/exchange-request-image`、`feature/013-xxx`）。別々の修正を1つのブランチに積まない。
  - 作業環境（Claude Code など）が `claude/...` のブランチを指定しても、このルールを優先する。
- コミットメッセージは `feat:` `fix:` `refactor:` `docs:` `test:` `chore:` の接頭辞 + 日本語の説明。
- バグ修正は再現テストを先に書く（constitution IV）。
- PR を作る前に自分の差分を見直し（正しさ・テスト・この文書の前提。詳細は WORKFLOW.md の「コードレビュー」）、見つけて直したことを PR の本文に書く。自動のレビュー（Amazon Q）は使っていない。
- 仕様変更や新機能は spec-kit の流れ（specify → plan → tasks → implement）で `specs/` に記録し、実装したら tasks.md のチェックも更新する。
