# Feature Specification: 漢字の7回目（小3の のこり20字）

**Feature Branch**: `feature/029-kanji-batch7`
**Created**: 2026年10月30日
**Status**: Implemented
**Input**: 028 の続き。これで 小3の200字が そろう

## 決めたこと

- 中身は [kanji-batch7.md](./kanji-batch7.md)（保護者確認済み）。1〜6回目と同じ形・同じ方針
- 小3までの字で よい ことばが 作れない字（央・究・研・昭・申・丁）は、ことばの問題を 作らない。20字で 54問
- 配当表と 読みは、文部科学省の表で確かめた（[grade-check.md](../020-kanji/grade-check.md) と同じやり方）。申の「シン」は中学校の読みなので「もうす」にした
- むずかしさは 025 の決まりのまま。7回目の20字は むずかしさ10 に入り、むずかしさ 1〜10 が 20字ずつ そろう
- 字が無い むずかしさから下に さがる しくみ（025）は のこす。結合テストは むずかしさ10の 問題を 一時的に 出さないようにして確かめる
- 前の回の問題と学習記録は変えない。7回目は問題セットの末尾に足す

## Tasks

- [x] T001 テスト: 7回分の一覧と問題が同じ中身、むずかしさ 1〜10 が 20字ずつ - backend/tests/GamifiedMathDrill.Tests.Unit/KanjiProblemGeneratorTests.cs、backend/tests/GamifiedMathDrill.Tests.Integration/KanjiProblemTests.cs
- [x] T002 `KanjiBatch7`（一覧からスクリプトで作る）、`KanjiBatches` に足す
- [x] T003 6回目までの問題セットが変わらないこと、6回目までが入った DB で起動して7回目の54問だけが足されること
