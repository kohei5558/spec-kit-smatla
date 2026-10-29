# Feature Specification: 漢字の むずかしさの分け方の見直し

**Feature Branch**: `feature/025-kanji-difficulty`
**Created**: 2026年10月29日
**Status**: Implemented
**Input**: 漢字が90字になり、「回ごとに3字ずつ むずかしさ1〜10」ではレベル1から出る字が増え続けるため（[proposal.md](./proposal.md)）

## 決めたこと（案A）

- すべての回を入れた順に通して、20字ずつ むずかしさ 1〜10 につめる。小3の200字で ちょうど 20字 × 10
- 新しい回は空いている上の むずかしさに入る。前に入れた字は二度と動かない
- 今回だけ、いまの90字の むずかしさが入れかわる（古い問題は しまわれ、新しい むずかしさの問題が足される。学習記録は消えない）
- 字がまだ入っていない むずかしさの レベルでは、問題がある いちばん上の むずかしさから出す（算数は全部の むずかしさに問題があるので変わらない）
- 小1・小2 の字を入れるときに、分け方を あらためて決める

## Tasks

- [x] T001 テスト: 90字が むずかしさ1〜4 に20字ずつ・5 に10字、一覧の順に入る - backend/tests/GamifiedMathDrill.Tests.Unit/KanjiProblemGeneratorTests.cs
- [x] T002 テスト: 漢字のレベル10でも、むずかしさ5 の漢字が出る - backend/tests/GamifiedMathDrill.Tests.Integration/KanjiProblemTests.cs
- [x] T003 `KanjiProblemGenerator`（通し番号で 20字ずつ）、`ProblemService`（下の むずかしさへ さがす）
- [x] T004 生成結果を前と比べて、変わるのは漢字の むずかしさだけ（242問）。3回目までが入った DB で起動し、242問が入れかわって 学習記録は変わらない
