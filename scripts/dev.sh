#!/usr/bin/env bash
# Chạy Web + Tailwind watch song song.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
WEB="$ROOT/src/HtMobile.Web"
( cd "$WEB" && npm run dev:css ) &
CSS_PID=$!
trap 'kill $CSS_PID 2>/dev/null || true' EXIT
cd "$WEB" && dotnet watch run
