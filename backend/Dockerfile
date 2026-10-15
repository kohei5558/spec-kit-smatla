# API サーバー（Render の無料 Web Service で動かす、012）。ビルドの起点はリポジトリのルート
#   docker build -f backend/Dockerfile -t gamified-math-drill-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json ./
COPY backend/src/ backend/src/
RUN dotnet publish backend/src/GamifiedMathDrill.Api/GamifiedMathDrill.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_CLI_TELEMETRY_OPTOUT=1
USER $APP_UID
# Render は待ち受けるポートを PORT で渡す（ないときは 8080）
CMD ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet GamifiedMathDrill.Api.dll"]
