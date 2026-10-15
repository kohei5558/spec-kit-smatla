# 開発用の秘密情報を .NET User Secrets に登録する（Windows PowerShell / PowerShell 7 用）。
# 既に設定済みの値は上書きしない。本番では環境変数（例: Jwt__SecretKey）で渡すこと。
$ErrorActionPreference = "Stop"

$apiProject = Join-Path $PSScriptRoot "..\backend\src\GamifiedMathDrill.Api"

$existing = dotnet user-secrets list --project $apiProject 2>$null
if ($existing -match "^Jwt:SecretKey = ") {
    Write-Host "Jwt:SecretKey は設定済みです。スキップします。"
}
else {
    $bytes = New-Object byte[] 48
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    $key = [Convert]::ToBase64String($bytes)
    dotnet user-secrets set "Jwt:SecretKey" $key --project $apiProject | Out-Null
    Write-Host "Jwt:SecretKey を生成して登録しました。"
}
