#!/usr/bin/env bash
# Wrapper EF Core migrations. Dùng: scripts/migrate.sh [add <Name>|update|remove|list]
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
INFRA="$ROOT/src/HtMobile.Infrastructure"
WEB="$ROOT/src/HtMobile.Web"
CMD="${1:-update}"
case "$CMD" in
  add)
    NAME="${2:-}"; [ -z "$NAME" ] && { echo "Cần tên: migrate.sh add <Name>"; exit 1; }
    dotnet ef migrations add "$NAME" -p "$INFRA" -s "$WEB" -o Persistence/Migrations ;;
  update) dotnet ef database update  -p "$INFRA" -s "$WEB" ;;
  remove) dotnet ef migrations remove -p "$INFRA" -s "$WEB" ;;
  list)   dotnet ef migrations list   -p "$INFRA" -s "$WEB" ;;
  *) echo "Usage: migrate.sh [add <Name>|update|remove|list]" ;;
esac
