# Implementation Tasks: まちがえたときの解説

**Feature**: 019-explanations
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月27日

## 設計メモ

- `Core/Services/ExplanationBuilder`: 問題（問題文・種類・答えの形・単位）と子どもの答えから、考え方の文と、よくある まちがいの一言を作る。問題文を読み直して作るので、DB の形は変えない
- 文章問題のひな形の文は `WordProblemTemplates` にまとめ、問題づくり（`WordProblemGenerator`）と読み直しの両方で使う（問題セットは変わらない）
- API: 問題の回答とデイリーチャレンジの回答の結果に `explanation`（文のリスト）と `hint` を足す（不正解のときだけ）
- 画面: 不正解の表示に「💡 かんがえかた」と「🔍」の一言

## Tasks

- [ ] T001 [US1] テスト: 全問題で考え方が作られ、最後の答えが正解と一致する。よくある まちがいの一言は、その答えのときだけ出る - backend/tests/GamifiedMathDrill.Tests.Unit/ExplanationBuilderTests.cs
- [ ] T002 [US1] `WordProblemTemplates`、`ExplanationBuilder`（四則・あまり・時こく・単位・文章問題）
- [ ] T003 [US1] テスト: 不正解のときだけ回答の結果に解説が入る（問題・デイリーチャレンジ）- backend/tests/GamifiedMathDrill.Tests.Integration/ExplanationTests.cs
- [ ] T004 [US1] API に `explanation`・`hint`
- [ ] T005 [US1] 画面（問題・デイリーチャレンジの不正解の表示）、E2E
- [ ] T006 README・CLAUDE.md
