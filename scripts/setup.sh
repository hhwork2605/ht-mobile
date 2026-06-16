#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
echo "==> dotnet tool restore (dotnet-ef)"; dotnet tool restore
echo "==> dotnet restore";                  dotnet restore HtMobile.sln
echo "==> npm install";                      ( cd src/HtMobile.Web && npm install )
echo "==> docker compose up -d (redis)";     docker compose up -d
echo "Tiếp: đặt user-secrets (docs/setup.md) -> scripts/migrate.sh update -> scripts/dev.sh"
