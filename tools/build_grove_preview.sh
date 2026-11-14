#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
bash "$root/tools/build_grove_understory.sh"
bash "$root/tools/build_grove_ecosystem.sh"
bash "$root/tools/build_grove_vine.sh"
bash "$root/tools/build_buna_assets.sh"
