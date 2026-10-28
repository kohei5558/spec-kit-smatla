# Implementation Plan: 国語（漢字）

**Spec**: [spec.md](./spec.md)　**中身**: [kanji-batch1.md](./kanji-batch1.md)

## 方針

今の仕組み（計算の種類・答えの形・種類ごとのレベル・問題の同期）に「教科」と「文字の答え」を足す。算数の問題・学習記録は変えない。

## モデル

- `CalculationType` に `KanjiReading = 8`（読み）・`KanjiWriting = 9`（書き）・`KanjiWords = 10`（ことば）を末尾に足す。名前は「計算」のままだが、問題の種類として使う（名前を変えると API・DB・画面の全部に手が入るため）
- `Subject`（`Math` / `Japanese`）と `Subjects.Of(type)`・`Subjects.Types(subject)` を足す。種類から教科が決まる（DB には持たない）
- `AnswerFormat` に `Text = 9`（ひらがなを入力）・`Choice = 10`（4つから選ぶ）を足す
- `Problem` に `CorrectText`（文字の正解。最大20）・`Choices`（選択肢をスペース区切り。最大60）を足す。文字の問題の `CorrectAnswer` は 0
- `LearningRecord` に `StudentText`（子どもの文字の答え。最大20）を足す
- マイグレーション `AddKanjiProblems`（SQLite・PostgreSQL）。`ProblemSynchronizer` は `CorrectText`・`Choices` も合わせる

## 問題を作る

- `KanjiBatch1`（Core/Models）に kanji-batch1.md の中身をそのまま持つ。`KanjiProblemGenerator` が問題にする（`ProblemGenerator` の末尾、文章問題のあと）
- 問題文は `[ ]` で強調する部分を囲む。読み「[駅]まで あるく。」、書き「[えき]の まえで まつ。」、ことば「[えきいん]」。画面は種類ごとの指示（「[ ]の 読みを ひらがなで 書こう」など）を出す
- 選択肢の並びは問題を作るときに乱数でまぜて保存する（乱数の種は固定なので毎回同じ）
- むずかしさ: 30字を並びの順に 3字ずつ むずかしさ 1〜10 に分ける（レベル1 では 6字から始まり、レベルが上がると新しい字が出る）。字が増えたら spec の表（小1〜小3）に合わせて分け直す

## 出題・判定

- `GET /api/problems/next` に `subject`（`Math` / `Japanese`）を足す。種類の指定が無いとき（すべて）は、その教科の種類だけから選ぶ。`subject` も無いときは算数（今までどおり）
- 回答 API に `text` を足す。`CorrectText` がある問題は、前後の空白を除いた `text` が一致すれば正解（数の欄は見ない）
- 問題 API は `choices` を返す（正解は返さない）。回答の結果・学習記録は `correctText`・`studentText` を返す
- 解説は 022 で足した（[022-kanji-explanations](../022-kanji-explanations/explanations.md)）
- デイリーチャレンジは 今までどおり 四則だけ

## 画面

- ホーム: 「さんすう / こくご」を切りかえて、その教科の種類を出す。国語は「漢字の読み・漢字の書き・漢字のことば・こくご ミックス」
- 問題: 問題文の `[ ]` を色で強調。読みは 50音ボタン（`KanaPad`: 50音・゛゜・小さい字・けす）で入力、書き・ことばは 4つの大きいボタンから選ぶ
- 回答の結果・学習記録・学習結果（タブ・集計）・保護者の画面（種類ごとのレベル）に、漢字の種類の名前と文字の答え
