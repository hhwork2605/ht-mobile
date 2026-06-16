#!/usr/bin/env pwsh
# CHỈ DÙNG DEV: xoá schema + tạo lại + migrate. (Seed chạy khi start app.)
$ErrorActionPreference = "Stop"
$root  = Split-Path $PSScriptRoot -Parent
$infra = Join-Path $root "src/HtMobile.Infrastructure"
$web   = Join-Path $root "src/HtMobile.Web"

$ans = Read-Host "Xoá toàn bộ DB rồi migrate lại? (yes/no)"
if ($ans -ne "yes") { Write-Host "Huỷ."; exit 0 }

dotnet ef database drop -f -p $infra -s $web
dotnet ef database update  -p $infra -s $web
Write-Host "Xong. Chạy scripts\seed.ps1 để nạp dữ liệu mẫu." -ForegroundColor Green
