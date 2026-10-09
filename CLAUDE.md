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

./scripts/setup-dev-secrets.sh   # 開発用 JWT キーを User Secrets に登録（初回のみ）
ASPNETCORE_ENVIRONMENT=Development dotnet run --project backend/src/GamifiedMathDrill.Api --urls http://localhost:5242
```

変更後は必ずビルドとテストを通してからコミットする。CI（`.github/workflows/ci.yml`）も同じ内容を実行する。

## 押さえておくべき前提

- 秘密情報（`Jwt:SecretKey`、本番接続文字列など）は appsettings に書かない。開発は User Secrets、本番は環境変数（`Jwt__SecretKey` 等）。
- DB: 開発は SQLite、本番は PostgreSQL。起動時に `DatabaseInitializer` がマイグレーションとシードを実行（テスト環境 `Testing` では実行しない）。デモユーザー（`parent@example.com`）は Development のみ作成。
- モデル変更時は EF マイグレーションを追加する（`dotnet tool restore` の後、`ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add <Name> --project backend/src/GamifiedMathDrill.Infrastructure --startup-project backend/src/GamifiedMathDrill.Api`）。既存マイグレーションは SQLite 用に生成されている。
- `Program.cs` で `SuppressModelStateInvalidFilter = true` のため、DataAnnotations やバインド失敗は自動で 400 にならない。コントローラーで明示的に検証する。
- 結合テストはテストクラス単位で DB を共有する（`IClassFixture`）。他テストのデータが残る前提で、名前は一意にし件数に依存しない書き方をする。
- 子供ログインの JSON キーは `ChildId`（`Api/DTOs/ChildLoginRequest.cs` とフロントの同名クラスで一致させる）。
- 子供の一覧取得と子供ログインには、保護者が登録した端末のトークン（`X-Device-Token` ヘッダー、`DevicesController` / `DeviceService`）が必須。結合テストでは `AuthenticationHelper.UseParentDeviceAsync` で設定する（006）。
- PIN ロックアウトは `IMemoryCache` で 3 回失敗 → 5 分（`ChildAccountService`）。
- MudBlazor は 7 系のまま。FluentAssertions は 6 系のまま（v8 以降は商用有償ライセンス）。
- 子供向け UI の文言はやさしい日本語にする。

## 開発ルール（詳細は WORKFLOW.md）

- main で直接作業しない。`feature/` `fix/` `refactor/` `docs/` `chore/` ブランチで作業し PR を作る。
- コミットメッセージは `feat:` `fix:` `refactor:` `docs:` `test:` `chore:` の接頭辞 + 日本語の説明。
- バグ修正は再現テストを先に書く（constitution IV）。
- 仕様変更や新機能は spec-kit の流れ（specify → plan → tasks → implement）で `specs/` に記録し、実装したら tasks.md のチェックも更新する。
