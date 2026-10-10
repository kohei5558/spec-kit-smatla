# 計算ドリル（Gamified Math Drill）

小学生（主な対象は小3）向けの、ポイントとごほうびで続けられる算数ドリルです。
子供は問題を解いてポイントをため、保護者が用意した景品と交換できます。保護者は子供のアカウント・景品・交換申請を管理し、学習状況を確認できます。

## プロジェクト構成

```
spec-kit-smatla/
├── backend/                          … API サーバー（ASP.NET Core 10）
│   ├── GamifiedMathDrill.sln
│   ├── src/
│   │   ├── GamifiedMathDrill.Api/                   … コントローラー・起動設定（Program.cs）
│   │   ├── GamifiedMathDrill.Core/                  … モデル・ドメインサービス
│   │   ├── GamifiedMathDrill.Infrastructure/        … DB（EF Core）・認証・初期データ
│   │   ├── GamifiedMathDrill.Migrations.Sqlite/     … DB マイグレーション（開発用 SQLite）
│   │   └── GamifiedMathDrill.Migrations.PostgreSQL/ … DB マイグレーション（本番用 PostgreSQL）
│   └── tests/
│       ├── GamifiedMathDrill.Tests.Unit/            … 単体テスト
│       └── GamifiedMathDrill.Tests.Integration/     … 結合テスト（API を通したテスト）
├── frontend/
│   └── GamifiedMathDrill.Client/     … 画面（Blazor WebAssembly + MudBlazor）
├── specs/                            … 機能ごとの仕様書・タスク（spec-kit）
├── scripts/                          … 開発用スクリプト
├── .specify/memory/constitution.md   … プロジェクトの原則
├── CLAUDE.md                         … 開発時の前提・注意点（AI 向け）
└── WORKFLOW.md                       … ブランチ・PR のルール
```

アプリはバックエンド（API）とフロントエンド（画面）の **2つを同時に起動** して使います。

| | URL | 役割 |
|---|---|---|
| バックエンド | http://localhost:5242 | API（Swagger: http://localhost:5242/swagger） |
| フロントエンド | http://localhost:5071 | ブラウザで開く画面 |

開発環境では DB に SQLite（ファイル `backend/src/GamifiedMathDrill.Api/gamifiedmathdrill.db`）を使います。初回起動時に自動で作られます。

## 実行手順

### 1. 必要なもの

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

インストール後、`dotnet --version` が `10.` で始まることを確認してください。

### 2. 初回だけ行う準備

リポジトリを取得し、開発用の秘密鍵（ログイン用トークンの署名キー）を登録します。鍵はリポジトリには保存されず、この PC の中だけに保存されます。

**Mac（ターミナル）**

```bash
git clone https://github.com/kohei5558/spec-kit-smatla.git
cd spec-kit-smatla
./scripts/setup-dev-secrets.sh
```

**Windows（PowerShell）**

```powershell
git clone https://github.com/kohei5558/spec-kit-smatla.git
cd spec-kit-smatla
powershell -ExecutionPolicy Bypass -File scripts\setup-dev-secrets.ps1
```

「Jwt:SecretKey を生成して登録しました。」と表示されれば完了です。

### 3. 起動

ターミナル（PowerShell）を **2つ** 開き、どちらもリポジトリのフォルダ（`spec-kit-smatla`）で実行します。コマンドは Mac・Windows 共通です。

**1つ目: バックエンド**

```bash
dotnet run --project backend/src/GamifiedMathDrill.Api --launch-profile http
```

`Now listening on: http://localhost:5242` と表示されたら起動完了です（ブラウザで Swagger が開くことがあります）。

**2つ目: フロントエンド**

```bash
dotnet run --project frontend/GamifiedMathDrill.Client --launch-profile http
```

起動したらブラウザで http://localhost:5071 を開きます。

止めるときは、それぞれのターミナルで `Ctrl + C` を押します。

### 4. 使ってみる

開発環境では、初回起動時にデモ用のアカウントが作られます。

| 種類 | ログイン情報 |
|---|---|
| 保護者 | メール `parent@example.com` / パスワード `Parent123!` |
| 子供 | 花子（3年生・PIN `1234`）、次郎（1年生・PIN `5678`） |

1. http://localhost:5071/login を開いて **保護者ログイン** し、ダッシュボードの「子供アカウント管理」ボタンを押す
2. 画面下の「子供用の端末」で **「この端末を子供用に登録する」** を押し、端末名（例: リビングのタブレット）を付けて登録する
   - 子供ログイン画面は、この登録をした端末（ブラウザ）でしか使えません
3. ログアウトして **子供ログイン画面**（http://localhost:5071/child-login）を開き、花子を選んで PIN `1234` でログインする
4. ホーム画面で計算の種類を選んで問題を解く。たまったポイントで「景品一覧」から交換を申請できる
   - 問題の難しさは学年で始まるレベルが変わる（1年→レベル1、2年→3、3年→5、4年以上→7）。連続で正解するとレベルが上がる
   - 「あまりのあるわり算」は、こたえとあまりの2つを入れて答える
   - ホームの「今日のチャレンジ」は1日1回だけ挑戦でき、正解すると30ポイントもらえる（問題はその子のレベルに合わせて出る）
5. 保護者でログインし直すと、ダッシュボードで学習状況、「交換申請一覧」で申請の承認・却下ができる

### 5. 開発用 DB を作り直す

データを初期状態に戻したいときは、バックエンドを止めてから DB ファイルを削除し、もう一度起動します。**学習記録・アカウント・ポイントなどもすべて消えます。**

DB の構造の変更（マイグレーション）と問題の追加・入れ替えは、起動時に自動で反映されるため、作り直す必要はありません。

**Mac**

```bash
rm backend/src/GamifiedMathDrill.Api/gamifiedmathdrill.db*
```

**Windows（PowerShell）**

```powershell
Remove-Item backend\src\GamifiedMathDrill.Api\gamifiedmathdrill.db*
```

## テスト

```bash
dotnet test backend/GamifiedMathDrill.sln
```

GitHub に push すると、CI（`.github/workflows/ci.yml`）でビルド・テスト・マイグレーションの確認が自動で実行されます。

## 本番環境（PostgreSQL）で動かす場合

本番では DB に PostgreSQL を使います。次の環境変数を設定してバックエンドを起動してください。デモ用アカウントは作られません。DB の表はアプリの起動時に自動で作られます。

| 環境変数 | 内容 |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | PostgreSQL の接続文字列（例: `Host=...;Database=...;Username=...;Password=...`） |
| `Jwt__SecretKey` | ログイン用トークンの署名キー（32文字以上のランダムな文字列） |

## 関連資料

- [backend/README.md](backend/README.md) … API・デバッグ方法など、バックエンドの詳細
- [frontend/GamifiedMathDrill.Client/README.md](frontend/GamifiedMathDrill.Client/README.md) … 画面側の詳細
- [specs/](specs/) … 機能ごとの仕様書とタスク（001〜011）
- [CLAUDE.md](CLAUDE.md) … 開発時の前提・注意点（マイグレーションの追加手順など）
- [WORKFLOW.md](WORKFLOW.md) … ブランチ・PR のルール
