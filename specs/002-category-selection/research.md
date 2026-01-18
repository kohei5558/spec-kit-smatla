# Research: 問題カテゴリ選択機能

**Feature**: 002-category-selection | **Date**: 2026-01-18
**Purpose**: Phase 0 - 技術的な不明点を解決し、実装の方向性を確立する

## Research Topics

### 1. ブラウザストレージによるカテゴリ状態の永続化

**Question**: ページ再読み込み後もカテゴリ選択状態を保持するため、localStorage と sessionStorage のどちらが適切か？

**Research**:

- **localStorage**: ブラウザを閉じても永続化されるが、仕様では「次回起動時は再度選択」が前提（Assumptions より）
- **sessionStorage**: ブラウザタブを閉じるまで保持される。ページ再読み込みには耐える

**Decision**: **sessionStorage を使用**

**Rationale**:

- 仕様の「カテゴリ選択はセッション単位で管理され、永続化（保存）は不要（次回起動時は再度選択）」に合致
- ページ再読み込み後も同じセッション内であれば保持される（Clarification 3 の要件を満たす）
- タブを閉じたら状態がクリアされるため、複数タブで異なるカテゴリを選択できる
- セキュリティ的にも、セッション終了時に自動的にクリアされる

**Implementation Note**:

- Key: `category-selection`
- Value: JSON 形式（例: `{"type":"Addition"}` または `{"type":null}` for「すべて」）

**Alternatives Considered**:

- localStorage: 永続化が強すぎる。次回起動時に前回の選択が残ってしまう
- URL パラメータのみ: ページ再読み込みでパラメータが消える可能性がある

---

### 2. 「すべて（ミックス）」の内部表現

**Question**: 「すべて（ミックス）」選択時、API に渡すパラメータ値は何が適切か？

**Research**:
既存の `GetNextProblemAsync` メソッドシグネチャを確認：

```csharp
Task<Problem?> GetNextProblemAsync(int studentId, List<int>? excludeRecentIds = null);
```

カテゴリフィルタ追加後の想定シグネチャ：

```csharp
Task<Problem?> GetNextProblemAsync(
    int studentId,
    CalculationType? category = null,  // オプショナル
    List<int>? excludeRecentIds = null);
```

**Decision**: **`CalculationType? category = null` を使用し、null は「すべて」を意味する**

**Rationale**:

- null は「フィルタなし」を意味する自然な表現
- 既存の動作（ランダム出題）と後方互換性がある
- API 呼び出し側でカテゴリ未指定の場合も null で統一できる
- Enum に新しい値（例: `All = 4`）を追加する必要がない

**Implementation Note**:

- フロントエンド: 「すべて」選択時は category パラメータを省略（undefined）
- バックエンド: category が null の場合、CalculationType によるフィルタリングをスキップ

**Alternatives Considered**:

- `CalculationType.All = 4` を追加: Enum の意味が曖昧になる（「全カテゴリ」は計算種類ではない）
- 特別な文字列値（"ALL"）: 型安全性が失われる

---

### 3. カテゴリ別学習履歴の集計方法

**Question**: 既存の LearningRecord テーブルには Problem への参照があり、Problem.CalculationType でカテゴリがわかる。集計時のクエリパフォーマンスは問題ないか？

**Research**:
既存のインデックス確認（ApplicationDbContext.cs より）:

```csharp
entity.HasIndex(e => e.CalculationType);
```

既存の LearningRecordsController に CalculationType フィルタが既にある：

```csharp
[FromQuery] CalculationType? calculationType = null
```

**Decision**: **既存のインデックスとフィルタ機能を活用。追加のインデックス不要**

**Rationale**:

- CalculationType にインデックスが既に存在
- LearningRecord → Problem の JOIN は必須だが、インデックスによりパフォーマンス最適化済み
- 小学 3 年生向けアプリなので学習履歴レコード数は数千〜数万程度（スケール面で問題なし）
- 既存の集計ロジックを流用できる

**Implementation Note**:

- ResultsPage.razor でカテゴリ別に集計結果を表示（4 つのカテゴリ + 全体）
- API 呼び出しは calculationType パラメータ付きで 4 回 + フィルタなしで 1 回（合計 5 回）
- または単一のエンドポイントでカテゴリ別集計を一括取得する新規 API 追加を検討

**Alternatives Considered**:

- LearningRecord に CalculationType を非正規化: データ重複、整合性リスク
- 集計テーブルを別途作成: 過剰な最適化、メンテナンスコスト増

---

### 4. カテゴリ選択 UI のデザインパターン

**Question**: MudBlazor で小学 3 年生にわかりやすいカテゴリ選択 UI を実装する最適なコンポーネントは？

**Research**:
MudBlazor の選択肢:

- **MudChip**: タグのような見た目、選択状態の表現が可能
- **MudButton**: 標準的なボタン、色やアイコンでカスタマイズ可能
- **MudCard**: カード形式、広い面積で視覚的に目立つ
- **MudRadioGroup**: ラジオボタン、排他選択を明示

**Decision**: **MudCard + MudButton の組み合わせを使用**

**Rationale**:

- MudCard でカテゴリごとに独立したカードを作成
- 各カードにアイコン（足し算: ➕、引き算: ➖、掛け算: ✖️、割り算: ➗）とカテゴリ名を大きく表示
- 選択状態を色（Primary）と枠線で視覚的に強調
- 小学 3 年生でも直感的に理解できる
- MudBlazor のレスポンシブグリッド（MudGrid）で横並び配置

**Implementation Note**:

```razor
<MudGrid>
  <MudItem xs="12" sm="6" md="3">
    <MudCard Class="category-card @(selectedCategory == Addition ? "selected" : "")">
      <MudCardContent>
        <MudIcon Icon="@Icons.Material.Filled.Add" Size="Size.Large" />
        <MudText Typo="Typo.h5">たしざん</MudText>
      </MudCardContent>
    </MudCard>
  </MudItem>
  <!-- 他のカテゴリも同様 -->
</MudGrid>
```

**Alternatives Considered**:

- MudChip: 小さすぎて視認性が低い
- MudRadioGroup: 見た目が地味で子供向けではない

---

### 5. カテゴリ未選択時のバリデーション実装場所

**Question**: カテゴリ未選択で問題画面に進もうとした場合、バリデーションはフロントエンド、バックエンド、どちらで行うべきか？

**Research**:

- **フロントエンドのみ**: ナビゲーション前にチェック、UI の即座のフィードバック
- **バックエンドのみ**: API レベルでの検証、セキュリティ的には堅牢
- **両方**: 最も安全だが実装コスト増

**Decision**: **フロントエンドで主バリデーション、バックエンドは既存の動作を維持**

**Rationale**:

- フロントエンドで問題画面への遷移を防止（NavigationManager を使用）
- ユーザーに即座にメッセージを表示（MudAlert コンポーネント）
- バックエンド API は category=null を許容（既存の「ランダム出題」と同じ動作）
- セキュリティリスクは低い（カテゴリ選択はビジネスロジックではなく UX 改善）

**Implementation Note**:

- Home.razor で「問題を解く」ボタンクリック時にカテゴリ選択をチェック
- 未選択の場合は MudSnackbar または MudAlert で「カテゴリを選んでね！」メッセージ表示
- バックエンド API は変更なし（category=null は有効な値として扱う）

**Alternatives Considered**:

- バックエンドで 400 Bad Request を返す: 不要なネットワーク往復、UX が悪い
- 両方で実装: 過剰な防御、メンテナンスコスト増

---

## Summary of Decisions

| トピック         | 決定                 | 理由                               |
| ---------------- | -------------------- | ---------------------------------- |
| 状態の永続化     | sessionStorage       | セッション単位管理、仕様に合致     |
| 「すべて」の表現 | `category = null`    | 後方互換性、自然な表現             |
| 学習履歴集計     | 既存インデックス活用 | パフォーマンス十分、追加コスト不要 |
| UI デザイン      | MudCard + Icon       | 児童向けの視認性、直感的           |
| バリデーション   | フロントエンド主体   | 即座のフィードバック、UX 優先      |

## Next Steps (Phase 1)

1. **data-model.md**: カテゴリ選択状態のデータ構造を定義
2. **contracts/**: API 契約を定義（GetNextProblemAsync の拡張）
3. **quickstart.md**: 開発者向けのクイックスタートガイド作成
4. **agent context update**: Copilot 用の技術コンテキスト更新
