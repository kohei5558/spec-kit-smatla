#!/usr/bin/env bash
# 開発用の秘密情報を .NET User Secrets に登録する（リポジトリにはコミットしない）。
# 既に設定済みの値は上書きしない。本番では環境変数（例: Jwt__SecretKey）で渡すこと。
set -euo pipefail

API_PROJECT="$(cd "$(dirname "$0")/.." && pwd)/backend/src/GamifiedMathDrill.Api"

if dotnet user-secrets list --project "$API_PROJECT" 2>/dev/null | grep -q '^Jwt:SecretKey = '; then
  echo "Jwt:SecretKey は設定済みです。スキップします。"
else
  dotnet user-secrets set "Jwt:SecretKey" "$(openssl rand -base64 48)" --project "$API_PROJECT" >/dev/null
  echo "Jwt:SecretKey を生成して登録しました。"
fi
