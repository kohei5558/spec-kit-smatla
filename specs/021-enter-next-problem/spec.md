# Feature Specification: 結果の画面で Enter を押すと次の問題へ

**Feature Branch**: `feature/021-enter-next-problem`
**Created**: 2026年10月28日
**Status**: Implemented
**Input**: 子どもの声「答えたあと『次の問題へ』を押すのが めんどい」

## 決めたこと

- 答えたあとの結果の画面（正解・不正解）で Enter を押すと、「次の問題へ」と同じく次の問題に進む
- 次の問題が出たら、答えの欄にカーソルが入る（今まで通り）。キーボードだけで「入力 → Enter（答える）→ Enter（次へ）」と続けられる
- 答えたときの Enter の押しっぱなし（キーのくり返し）では進まない。ボタンにフォーカスがあるときの Enter でも 1問だけ進む
- 画面に「Enter キーでも つぎへ すすめるよ」と出す
- 漢字の読み（50音ボタン）・書き・ことばの答えは画面のボタンで入れるので、答えるときの Enter は今回は対象外

## Tasks

- [x] T001 E2E: 答えを入れて Enter で答える → 結果の画面にとどまる → Enter で次の問題（読み込みは1回だけ）- e2e/tests/child.spec.ts
- [x] T002 `AnswerFeedback`: 表示したら枠にフォーカスし、Enter で次へ（押しっぱなし・2回目は無視）
