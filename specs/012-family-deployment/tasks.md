# Implementation Tasks: 家族で使うための公開（無料サービス）

**Feature**: 012-family-deployment
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月11日

## Phase 1: API（US1・US2）

- [x] T001 [US1][US2] 結合テスト: 招待コード（なし・違う・大文字小文字・正しい）、CORS の設定、ヘルスチェック、画像の DB 保存・ログインなしの取得・削除・1MB の上限 - backend/tests/GamifiedMathDrill.Tests.Integration/DeploymentTests.cs
- [x] T002 [US2] 招待コード（`Registration:InviteCode`、一定時間の比較）- AuthController, RegisterRequest
- [x] T003 [US1] CORS のアドレスを設定 `Cors:AllowedOrigins` に - Program.cs
- [x] T004 [US1] 景品の画像を DB に保存（`RewardImage`、`DatabaseImageStorageService`、`RewardImagesController`）。マイグレーション AddRewardImages（SQLite・PostgreSQL）
- [x] T005 [US1] 転送ヘッダー（Render のプロキシ）と `/healthz`

## Phase 2: 画面（US1・US2）

- [x] T006 [US1] API のアドレスを `wwwroot/appsettings.json` の `ApiBaseAddress` に。画像の URL の解決を `/api/` にも対応
- [x] T007 [US2] 登録画面に招待コードの入力欄
- [x] T008 [US1] API の起動待ち（`ApiWakeGate`、「じゅんびちゅう…」）
- [x] T009 [US1] 登録直後に保護者の画面へ入れずログイン画面に戻っていた不具合を修正（自動ログインを通常のログインと同じ処理にする）

## Phase 3: 公開（US3）

- [x] T010 [US3] `backend/Dockerfile`・`.dockerignore`・`render.yaml`
- [x] T011 [US3] GitHub Actions で画面を Cloudflare Pages に公開（`.github/workflows/deploy-frontend.yml`）
- [x] T012 [US3] 手順書 `docs/DEPLOY.md`、README・CLAUDE.md
- [x] T013 全テスト。Docker のイメージを作り、Render と同じ設定（PORT・PostgreSQL・CORS・招待コード）で起動して確認。公開用にビルドした画面を静的サーバーで配信し、API が止まっている状態から「じゅんびちゅう」→ 起動 → 招待コードで登録 → 保護者の画面、までブラウザで確認。コンテナを作り直しても画像が残ること
