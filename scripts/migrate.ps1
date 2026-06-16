#!/usr/bin/env pwsh
# Wrapper EF Core migrations — luôn dùng đúng -p (Infrastructure) / -s (Web).
# Dùng:  scripts\migrate.ps1 add <Name> | update | remove | list
param(
    [Parameter(Position = 0)][string]$Cmd = "update",
    [Parameter(Position = 1)][string]$Name = ""
)
$ErrorActionPreference = "Stop"
$root  = Split-Path $PSScriptRoot -Parent
$infra = Join-Path $root "src/HtMobile.Infrastructure"
$web   = Join-Path $root "src/HtMobile.Web"

switch ($Cmd) {
    "add" {
        if (-not $Name) { Write-Error "Cần tên: scripts\migrate.ps1 add <Name>"; exit 1 }
        dotnet ef migrations add $Name -p $infra -s $web -o Persistence/Migrations
    }
    "update" { dotnet ef database update      -p $infra -s $web }
    "remove" { dotnet ef migrations remove     -p $infra -s $web }
    "list"   { dotnet ef migrations list       -p $infra -s $web }
    default  { Write-Host "Usage: migrate.ps1 [add <Name>|update|remove|list]" }
}
