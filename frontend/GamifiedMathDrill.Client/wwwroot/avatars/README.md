# Preset Avatar Images

このディレクトリには子供アカウント用のプリセットアバター画像を配置します。

## 必要な画像ファイル（15種類）

1. cat.png - 猫
2. dog.png - 犬
3. panda.png - パンダ
4. lion.png - ライオン
5. rabbit.png - ウサギ
6. bear.png - クマ
7. tiger.png - トラ
8. giraffe.png - キリン
9. elephant.png - ゾウ
10. monkey.png - サル
11. penguin.png - ペンギン
12. koala.png - コアラ
13. frog.png - カエル
14. squirrel.png - リス
15. fox.png - キツネ

## 画像仕様

- **サイズ**: 推奨 256x256px（正方形）
- **フォーマット**: PNG（透過背景推奨）
- **ファイル名**: 英小文字、拡張子.png
- **容量**: 各50KB以下推奨

## 画像準備方法

### Option 1: AIツールで生成

```bash
# 例: Stable Diffusion, DALL-E, Midjourneyなど
# プロンプト例: "cute cartoon cat icon, simple, colorful, for kids, white background, 256x256"
```

### Option 2: フリー素材サイトから取得

- **Flaticon**: https://www.flaticon.com/ (ライセンス確認必須)
- **Icons8**: https://icons8.com/icons/set/animals
- **Noun Project**: https://thenounproject.com/

### Option 3: プレースホルダー（開発用）

開発段階では、以下のプレースホルダーサービスを利用可能：

```html
https://via.placeholder.com/256/FF6B6B/FFFFFF?text=Cat
```

## 実装時の注意

- 実際の画像ファイルを配置するまで、アバター選択UIは動作しません
- 画像はGitにコミットされます（.gitignoreで除外されていません）
- プロダクション環境では、CDNへの移行を検討してください

## 次のステップ

1. 上記の15ファイルを作成/配置
2. `dotnet run` で起動
3. `/api/preset-avatars` で一覧取得を確認
4. フロントエンドのAvatarSelectorコンポーネントでプレビュー
