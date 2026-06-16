#!/usr/bin/env pwsh
# Seed dữ liệu mẫu. App tự apply migration + seed khi chạy ở Development
# (xem DbInitializer trong Infrastructure/Persistence/Seed). Script này build & chạy 1 lần.
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:SEED_ON_STARTUP = "true"
Write-Host "==> Chạy app để apply migration + seed (Ctrl+C sau khi thấy 'Seed completed')..." -ForegroundColor Cyan
dotnet run --project (Join-Path $root "src/HtMobile.Web")
