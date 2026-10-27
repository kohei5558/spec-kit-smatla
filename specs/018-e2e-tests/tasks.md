# Implementation Tasks: 画面の流れの自動テスト（E2E）

**Feature**: 018-e2e-tests
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月27日

## Tasks

- [x] T001 `e2e/`（Playwright Test、API の起動スクリプト、設定）
- [x] T002 setup: 保護者ログイン・端末登録（storageState を保存）- e2e/tests/setup.ts
- [x] T003 [US1] 保護者: ダッシュボード・メニュー、景品の追加・編集（画像つき）、子供アカウントの作成（同じ PIN の注意）- e2e/tests/parent.spec.ts
- [x] T004 [US1] 子供: メニュー、足し算（判定・ひっ算）、各単元を1問ずつ、学習結果、デイリーチャレンジ、交換申請 - e2e/tests/child.spec.ts
- [x] T005 [US1] 保護者: 交換申請の詳細と承認 - e2e/tests/approve.spec.ts
- [x] T006 わざと不具合（メニューのリンク先の誤り）を入れて、失敗することを確かめる
- [x] T007 CI に e2e ジョブを追加（失敗時はレポートを保存）
- [x] T008 README・CLAUDE.md
