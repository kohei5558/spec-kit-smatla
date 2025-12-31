<!--
Sync Impact Report:
Version: 0.0.0 → 1.0.0 (MAJOR - Initial Constitution)
Modified Principles: N/A (initial creation)
Added Sections:
  - Core Principles (5 principles: User-Centric Design, Data Privacy & Child Safety, Specification-First Development, Test-Driven Quality, Continuous Learning & Adaptation)
  - Technology Stack (ASP.NET Core, C#, online-only architecture, single-user model)
  - Development Workflow (branching, review, documentation standards)
  - Governance (amendment process, compliance verification)
Removed Sections: None
Templates Requiring Updates:
  ✅ plan-template.md - Already includes "Constitution Check" section, no changes needed
  ✅ spec-template.md - Already enforces spec-first principles (no implementation details, testable requirements), no changes needed
  ✅ tasks-template.md - Already organized by user stories for independent testing, no changes needed
  ✅ checklist-template.md - No changes needed (general quality checklist)
  ✅ agent-file-template.md - No changes needed (agent guidance template)
Follow-up TODOs: None - all placeholders filled, templates validated
-->

# Gamified Math Drill App Constitution

## Core Principles

### I. User-Centric Design (NON-NEGOTIABLE)

すべての機能は小学 3 年生の児童とその保護者のニーズを最優先に設計しなければならない。

**Rules:**

- UI/UX は対象年齢（8-9 歳）に適した視覚的でわかりやすいデザインであること
- 操作は直感的で、説明なしで理解できること
- 保護者が児童の学習状況を容易に確認できること
- アクセシビリティを考慮し、多様な学習スタイルに対応すること

**Rationale:** 教育アプリの成功は、児童が楽しみながら継続的に使用できるかどうかにかかっている。複雑さや混乱は学習意欲を低下させる。

---

### II. Data Privacy & Child Safety (NON-NEGOTIABLE)

児童のプライバシーと安全は最優先事項であり、すべてのデータ取り扱いは厳格な基準に従う。

**Rules:**

- 児童の個人情報は最小限のみ収集すること
- データは暗号化して保存し、セキュアな通信プロトコルを使用すること
- 第三者へのデータ共有は保護者の明示的な同意がない限り禁止
- 児童に適切でないコンテンツや広告は一切表示しない

**Rationale:** 児童向けアプリは特別な責任を伴う。法的コンプライアンスと倫理的配慮は交渉の余地がない。

---

### III. Specification-First Development

すべての機能は実装前に詳細な仕様書を作成し、ステークホルダーの承認を得なければならない。

**Rules:**

- 仕様書は実装の詳細を含まず、ユーザー価値とビジネスニーズに焦点を当てること
- すべての要件はテスト可能で測定可能であること
- Edge case と受入基準を明確に定義すること
- 仕様の変更は文書化し、影響を評価すること

**Rationale:** 明確な仕様は手戻りを防ぎ、開発効率を高める。教育関係者、保護者、開発者間の共通理解を確立する。

---

### IV. Test-Driven Quality

品質は事後対応ではなく、設計段階から組み込まれなければならない。

**Rules:**

- すべての機能要件に対応するテストケースを定義すること
- 自動テストによる継続的な品質検証を実施すること
- バグ修正前に再現テストを作成すること
- パフォーマンス基準（例：起動 3 秒以内）を満たすこと

**Rationale:** 児童の学習体験を妨げるバグやパフォーマンス問題は許容できない。早期の品質確保がコストを削減する。

---

### V. Continuous Learning & Adaptation

アプリは児童の学習パターンに適応し、最適な学習体験を提供し続けなければならない。

**Rules:**

- 児童の進捗データを分析し、適切な難易度調整を行うこと
- フィードバックループを設計し、継続的な改善を実施すること
- 学習効果を測定可能な指標（正答率向上、継続率など）で評価すること
- 教育的価値のある新機能を優先的に検討すること

**Rationale:** 効果的な教育ツールは静的ではなく、児童の成長に合わせて進化する。データに基づく意思決定が学習成果を最大化する。

---

## Technology Stack

**Mandatory Constraints:**

- **Backend Framework**: ASP.NET Core
- **Programming Language**: C#
- **Architecture Style**: オンライン必須アーキテクチャ（インターネット接続が常に必要）
- **User Model**: シングルユーザー専用（1 デバイス=1 児童）

**Guidelines:**

- RESTful API 設計原則に従うこと
- データベース選択はスケーラビリティとパフォーマンスを考慮すること
- フロントエンドは児童にとって使いやすいレスポンシブデザインを採用すること
- 依存関係は最小限に保ち、セキュリティパッチを適用可能にすること

**Rationale:** 技術スタックの統一により、保守性と開発効率が向上する。ASP.NET Core は堅牢でスケーラブルなエンタープライズアプリケーション構築に適している。

---

## Development Workflow

### Branching Strategy

- **Feature branches**: `###-feature-name` 形式（例：001-gamified-math-drill）
- **Specification**: 各機能は `specs/###-feature-name/spec.md` に文書化
- **Quality Gates**: チェックリストによる仕様の完全性検証

### Review Process

- 仕様書は実装前にレビューと承認を必須とする
- すべての[NEEDS CLARIFICATION]マーカーは実装前に解決すること
- User Stories は独立してテスト可能であることを確認すること

### Documentation Standards

- 仕様書は Markdown 形式で `.specify/` ディレクトリに保存
- テンプレートに従い、必須セクションをすべて埋めること
- 技術的な実装の詳細は仕様書に含めない

---

## Governance

この憲法はすべての開発活動の基盤となり、他のプラクティスや判断に優先する。

**Amendment Process:**

1. 変更提案を文書化し、影響範囲を明確化
2. バージョン番号をセマンティックバージョニングに従い更新：
   - MAJOR: 後方互換性のない原則の削除・再定義
   - MINOR: 新原則の追加または既存原則の重要な拡張
   - PATCH: 明確化、文言修正、誤字修正
3. 関連テンプレートとの整合性を検証
4. 影響レポートをコメントとして憲法ファイルの先頭に追加

**Compliance Verification:**

- すべての仕様レビューで憲法への準拠を確認
- 憲法違反は明確に指摘し、修正を要求
- 例外が必要な場合は、正当な理由を文書化し承認を得る

**Constitution Supersedes:**

- 個人の好みや習慣
- 未文書化のローカルルール
- 「いつもこうやってきた」という慣習

**Version**: 1.0.0 | **Ratified**: 2025-12-30 | **Last Amended**: 2025-12-30
