#!/usr/bin/env pwsh
# Cài đặt môi trường dev lần đầu.
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

Write-Host "==> Khôi phục tool (dotnet-ef)..." -ForegroundColor Cyan
dotnet tool restore

Write-Host "==> Restore NuGet..." -ForegroundColor Cyan
dotnet restore HtMobile.sln

Write-Host "==> npm install (Tailwind/htmx/alpine)..." -ForegroundColor Cyan
Push-Location src/HtMobile.Web
npm install
Pop-Location

Write-Host "==> Khởi động Redis (docker)..." -ForegroundColor Cyan
docker compose up -d

Write-Host ""
Write-Host "Tiếp theo: đặt connection string Supabase qua user-secrets — xem docs/setup.md" -ForegroundColor Yellow
Write-Host "Rồi: scripts\migrate.ps1 update  &&  scripts\dev.ps1" -ForegroundColor Yellow
