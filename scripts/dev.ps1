#!/usr/bin/env pwsh
# Chạy Web + Tailwind watch song song.
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$web  = Join-Path $root "src/HtMobile.Web"

Write-Host "==> Tailwind --watch (nền)..." -ForegroundColor Cyan
$css = Start-Process -PassThru -NoNewWindow -WorkingDirectory $web -FilePath "npm" -ArgumentList "run", "dev:css"

try {
    Write-Host "==> dotnet watch run..." -ForegroundColor Cyan
    Push-Location $web
    dotnet watch run
}
finally {
    Pop-Location
    if ($css -and -not $css.HasExited) { Stop-Process -Id $css.Id -Force }
}
