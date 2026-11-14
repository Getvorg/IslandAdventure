#!/usr/bin/env bash
# Exact palette transform of complete vanilla tree sheets; no generated wood/masks.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
python3 "$root/tools/build_camphor_assets.py"
