# Implementation Tasks: 子供用端末の登録

**Feature**: 006-device-registration
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月8日

## Format: `[ID] [P?] [Story] Description - file path`

- **[P]**: 他タスクと並行実施可能（別ファイル・依存なし）
- **[Story]**: US1 = 端末登録、US2 = 未登録端末での非公開、US3 = 端末一覧・解除

## 設計メモ

- 端末トークンは `RandomNumberGenerator` で32バイト生成し Base64Url で返す。DB には SHA-256 ハッシュ（16進）を保存する
- クライアントは端末トークンを `X-Device-Token` ヘッダーで送る。フロントは LocalStorage の `deviceToken` / `deviceId` に保存する
- API（保護者認証）: `POST /api/devices`（登録）、`GET /api/devices`（一覧）、`DELETE /api/devices/{id}`（解除）
- API（端末トークン）: `GET /api/devices/current/children`（その家庭の有効な子供一覧）
- 子供ログイン `POST /api/auth/child/login` に `X-Device-Token` を必須化。トークン不正・他家庭の子供は 401（PIN 失敗回数に数えない）
- `GET /api/child-accounts/public` と `IChildAccountService.ListAllActiveAsync` は削除

---

## Phase 1: Foundational（全ストーリーの前提）

- [x] T001 RegisteredDevice エンティティ作成（Id, ParentId, Name, TokenHash, CreatedAt, LastUsedAt） - backend/src/GamifiedMathDrill.Core/Models/RegisteredDevice.cs
- [x] T002 ApplicationDbContext に DbSet と設定追加（TokenHash 一意インデックス、ParentId インデックス、保護者削除で連鎖削除、Name 最大30文字） - backend/src/GamifiedMathDrill.Infrastructure/Data/ApplicationDbContext.cs
- [x] T003 EF マイグレーション AddRegisteredDevices 追加 - backend/src/GamifiedMathDrill.Infrastructure/Migrations/
- [x] T004 [P] DTO 作成（RegisterDeviceRequest, RegisterDeviceResponse(トークン含む), RegisteredDeviceDto(トークン含まず)） - backend/src/GamifiedMathDrill.Core/DTOs/RegisteredDeviceDtos.cs
- [x] T005 IDeviceService 定義（RegisterAsync, ListAsync, RevokeAsync, ResolveParentIdAsync(token)） - backend/src/GamifiedMathDrill.Core/Interfaces/IDeviceService.cs
- [x] T006 DeviceService 実装（トークン生成・ハッシュ化、10台上限、所有者チェック、最終利用日時の間引き更新） - backend/src/GamifiedMathDrill.Infrastructure/Services/DeviceService.cs
- [x] T007 DI 登録 - backend/src/GamifiedMathDrill.Api/Program.cs
- [x] T008 結合テスト用ヘルパーに端末登録・トークン付きリクエストを追加 - backend/tests/GamifiedMathDrill.Tests.Integration/Helpers/AuthenticationHelper.cs

## Phase 2: User Story 1 - 保護者が端末を子供用に登録する (P1) 🎯 MVP

### Tests（先に書く）

- [ ] T009 [P] [US1] 端末登録の結合テスト（登録成功・トークン返却、未認証401、子供ロールで403、端末名の検証、11台目で400） - backend/tests/GamifiedMathDrill.Tests.Integration/DeviceRegistrationTests.cs
- [ ] T010 [P] [US1] 登録端末で自家庭の有効な子供だけ取得できるテスト（停止中は除外、他家庭は含まれない） - backend/tests/GamifiedMathDrill.Tests.Integration/DeviceRegistrationTests.cs
- [ ] T011 [P] [US1] 登録端末のトークンで子供ログインできるテスト - backend/tests/GamifiedMathDrill.Tests.Integration/ChildLoginTests.cs

### Implementation

- [ ] T012 [US1] DevicesController 作成（POST /api/devices、GET /api/devices/current/children） - backend/src/GamifiedMathDrill.Api/Controllers/DevicesController.cs
- [ ] T013 [US1] ChildAuthController.LoginAsync に端末トークン検証と家庭一致チェックを追加 - backend/src/GamifiedMathDrill.Api/Controllers/ChildAuthController.cs
- [ ] T014 [US1] 既存の子供ログイン系テストを端末トークン付きに更新 - backend/tests/GamifiedMathDrill.Tests.Integration/ChildLoginTests.cs, ChildAccountSecurityTests.cs, ChildAccountTests.cs
- [ ] T015 [P] [US1] DeviceApiClient 作成（登録・一覧・解除・子供一覧、`X-Device-Token` 付与、LocalStorage 保存） - frontend/GamifiedMathDrill.Client/Services/DeviceApiClient.cs
- [ ] T016 [US1] AuthService.ChildLoginAsync で `X-Device-Token` を送る - frontend/GamifiedMathDrill.Client/Services/AuthService.cs
- [ ] T017 [US1] 子供アカウント管理画面に「この端末を子供用に登録する」ボタンと端末名入力ダイアログ、「登録済み」表示を追加 - frontend/GamifiedMathDrill.Client/Pages/ChildAccountManagement.razor
- [ ] T018 [US1] 子供ログイン画面を端末トークンで子供一覧を取得する形に変更 - frontend/GamifiedMathDrill.Client/Pages/ChildLogin.razor

## Phase 3: User Story 2 - 未登録の端末では子供の情報が見えない (P1)

### Tests（先に書く）

- [ ] T019 [P] [US2] トークンなし・不正トークンで子供一覧が401のテスト - backend/tests/GamifiedMathDrill.Tests.Integration/DeviceRegistrationTests.cs
- [ ] T020 [P] [US2] トークンなし・他家庭トークンで子供ログインが401、PIN失敗回数が増えないテスト - backend/tests/GamifiedMathDrill.Tests.Integration/ChildAccountSecurityTests.cs
- [ ] T021 [P] [US2] `GET /api/child-accounts/public` が404のテスト - backend/tests/GamifiedMathDrill.Tests.Integration/DeviceRegistrationTests.cs

### Implementation

- [ ] T022 [US2] `GET /api/child-accounts/public` と ListAllActiveAsync を削除 - backend/src/GamifiedMathDrill.Api/Controllers/ChildAccountController.cs, backend/src/GamifiedMathDrill.Core/Interfaces/IChildAccountService.cs, backend/src/GamifiedMathDrill.Infrastructure/Services/ChildAccountService.cs
- [ ] T023 [US2] フロントの GetPublicChildAccountsAsync を削除 - frontend/GamifiedMathDrill.Client/Services/ChildAccountApiClient.cs
- [ ] T024 [US2] 子供ログイン画面に未登録端末の案内（やさしい日本語、保護者ログインへのリンク）と、401時のトークン削除を追加 - frontend/GamifiedMathDrill.Client/Pages/ChildLogin.razor

## Phase 4: User Story 3 - 登録済み端末の確認・解除 (P2)

### Tests（先に書く）

- [ ] T025 [P] [US3] 端末一覧（自分の端末のみ、トークンを含まない）のテスト - backend/tests/GamifiedMathDrill.Tests.Integration/DeviceRegistrationTests.cs
- [ ] T026 [P] [US3] 解除後にトークンが無効になるテスト、他の保護者の端末は404で解除されないテスト - backend/tests/GamifiedMathDrill.Tests.Integration/DeviceRegistrationTests.cs

### Implementation

- [ ] T027 [US3] DevicesController に GET /api/devices、DELETE /api/devices/{id} を追加 - backend/src/GamifiedMathDrill.Api/Controllers/DevicesController.cs
- [ ] T028 [US3] 子供アカウント管理画面に登録端末一覧（端末名・登録日・最終利用日・「この端末」表示）と解除ボタン・確認ダイアログを追加 - frontend/GamifiedMathDrill.Client/Pages/ChildAccountManagement.razor

## Phase 5: 仕上げ

- [ ] T029 全テスト実行、フロントエンドのビルド確認 - backend/, frontend/
- [ ] T030 開発環境で手動確認（登録 → 子供ログイン → 解除 → 未登録案内） - specs/006-device-registration/quickstart.md に手順を記載
- [ ] T031 CLAUDE.md と backend README の子供ログインの説明を更新 - CLAUDE.md, backend/README.md

## 依存関係

- Phase 1 → Phase 2 → Phase 3 / Phase 4（Phase 3 と 4 は並行可）→ Phase 5
- T013（子供ログインの端末必須化）と T014（既存テスト更新）は同時に入れる（片方だけだと既存テストが失敗する）
