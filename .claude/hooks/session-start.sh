#!/bin/bash
# Claude Code クラウドセッション開始時に .NET SDK と依存パッケージを用意する
set -euo pipefail

if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

cd "$CLAUDE_PROJECT_DIR"

# .NET 10 SDK（Ubuntu公式リポジトリ版。builds.dotnet.microsoft.com はプロキシで遮断されるため apt を使う）
if ! dotnet --list-sdks 2>/dev/null | grep -q '^10\.'; then
  apt-get update -qq
  DEBIAN_FRONTEND=noninteractive apt-get install -y -qq dotnet-sdk-10.0
fi

if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
  echo 'export DOTNET_CLI_TELEMETRY_OPTOUT=1' >> "$CLAUDE_ENV_FILE"
  echo 'export DOTNET_NOLOGO=1' >> "$CLAUDE_ENV_FILE"
fi
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

dotnet restore backend/GamifiedMathDrill.sln
dotnet restore frontend/GamifiedMathDrill.Client

# 開発用 JWT キー（User Secrets、冪等）
./scripts/setup-dev-secrets.sh
