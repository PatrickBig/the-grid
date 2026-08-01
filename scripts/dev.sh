#!/usr/bin/env bash
# Dev workflow helper for The Grid. Intentionally a small, fixed set of subcommands so the
# whole script can be allowlisted once in Claude Code settings instead of prompting per
# docker-compose/dotnet invocation. Run from anywhere; paths are resolved relative to this file.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOURCE_DIR="$(cd "$SCRIPT_DIR/../source" && pwd)"
COMPOSE_FILES=(-f docker-compose.yml -f docker-compose.override.yml)
HOST="${GRID_HOST:-https://localhost:20443}"
ADMIN_PASSWORD="${DEFAULT_ADMIN_PASSWORD:-TheGrid123!}"

cd "$SOURCE_DIR"

usage() {
  cat <<'EOF'
Usage: scripts/dev.sh <command> [args]

  up                          Build and start the local dev stack (postgres, redis, server, adminer)
  down                        Stop the local dev stack
  status                      Show docker-compose ps for the stack
  setup                       Re-run the one-shot /setup container (migrations + seed admin/group)
  reset-db                    Wipe the postgres volume and bring the stack back up fresh
  seed-test-org [name] [slug] Log in as the seeded admin and create a test organization via the API
                               (defaults: "Test Organization" / "test-org")
  remigrate                   Delete and regenerate a single fresh Initial EF migration for both
                               providers (Postgres + Sqlite). Does NOT touch the running DB or
                               containers — follow with 'reset-db' to apply against a clean database.
EOF
}

cmd="${1:-help}"
[ $# -gt 0 ] && shift || true

case "$cmd" in
  up)
    docker-compose "${COMPOSE_FILES[@]}" up -d --build
    ;;

  down)
    docker-compose "${COMPOSE_FILES[@]}" down
    ;;

  status)
    docker-compose "${COMPOSE_FILES[@]}" ps
    ;;

  setup)
    docker-compose "${COMPOSE_FILES[@]}" run --rm thegrid.setup
    ;;

  reset-db)
    echo "Wiping the postgres volume and restarting the stack fresh..."
    docker-compose "${COMPOSE_FILES[@]}" down -v
    docker-compose "${COMPOSE_FILES[@]}" up -d --build
    ;;

  seed-test-org)
    org_name="${1:-Test Organization}"
    org_slug="${2:-test-org}"

    resp=$(curl -sk -X POST "$HOST/api/v1/account/login" \
      -H "Content-Type: application/json" \
      -d "{\"email\":\"admin\",\"password\":\"$ADMIN_PASSWORD\"}")
    token=$(echo "$resp" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

    if [ -z "$token" ]; then
      echo "Failed to log in as admin. Response: $resp" >&2
      exit 1
    fi

    curl -sk -X POST "$HOST/api/v1/organizations" \
      -H "Authorization: Bearer $token" \
      -H "Content-Type: application/json" \
      -d "{\"name\":\"$org_name\",\"slug\":\"$org_slug\"}" \
      -w "\nHTTP %{http_code}\n"
    ;;

  remigrate)
    echo "Regenerating a single fresh Initial migration for Postgres and Sqlite..."
    echo "(schema is still unstable pre-v1 — see CLAUDE.md: no incremental migrations yet)"

    rm -f TheGrid.Postgres/Migrations/*.cs
    dotnet ef migrations add Initial \
      --project TheGrid.Postgres/TheGrid.Postgres.csproj \
      --startup-project TheGrid.Server/TheGrid.Server.csproj \
      -- postgresql "Host=postgres;Username=postgres;Password=thegrid123;Database=thegrid"

    rm -f TheGrid.Sqlite/Migrations/*.cs
    SystemOptions__DatabaseProvider=Sqlite dotnet ef migrations add Initial \
      --project TheGrid.Sqlite/TheGrid.Sqlite.csproj \
      --startup-project TheGrid.Server/TheGrid.Server.csproj \
      -- sqlite "Data Source=thegrid.db"

    echo ""
    echo "Done. Review the new migration files, then run 'scripts/dev.sh reset-db' to apply them"
    echo "against a clean local database."
    ;;

  help|-h|--help)
    usage
    ;;

  *)
    echo "Unknown command: $cmd" >&2
    echo "" >&2
    usage >&2
    exit 1
    ;;
esac
