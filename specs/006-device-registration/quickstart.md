# Quickstart: 子供用端末の登録（006）の動作確認

開発環境で、端末登録から子供ログイン・解除までを確認する手順。

## 起動

```bash
./scripts/setup-dev-secrets.sh   # 初回のみ
rm -f backend/src/GamifiedMathDrill.Api/gamifiedmathdrill.db*   # デモユーザーを作り直す場合
ASPNETCORE_ENVIRONMENT=Development dotnet run --project backend/src/GamifiedMathDrill.Api --urls http://localhost:5242
dotnet run --project frontend/GamifiedMathDrill.Client --urls http://localhost:5071
```

> 2026-10-09 以前に作成した開発用 DB では、デモの子供の PIN が平文で保存されておりログインできない。DB を削除して作り直すこと。

## 手順

1. ブラウザで http://localhost:5071/child-login を開く
   → 「この端末はまだ子供用に登録されていません。」と表示され、子供の名前は表示されない
2. 「保護者のログインはこちら」から `parent@example.com` / `Parent123!` でログインし、`/parent/children` を開く
3. 「この端末を子供用に登録する」→ 端末名「リビングのタブレット」→「登録する」
   → 「この端末は子供用に登録済みです」と表示され、一覧に「この端末」バッジ付きで表示される
4. `/child-login` を開く → 花子・次郎のカードだけが表示される
5. 花子を選び PIN `1234` でログイン → ホーム画面に移動する
6. 保護者で再ログインし `/parent/children` の端末一覧で「解除」→「解除する」
7. `/child-login` を開く → 手順1と同じ案内に戻る

## 確認結果（2026-10-09）

Playwright（Chromium）で手順1〜7を自動実行し、すべて期待どおりに動作した。ページの JavaScript エラーなし。
