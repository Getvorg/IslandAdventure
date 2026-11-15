#!/usr/bin/env bash
# Build the unchanged stem, then extract connected wood/leaf component masters.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
buna2_tmp="$(mktemp -d)";trap 'rm -rf "$buna2_tmp"' EXIT
magick "$root/Assets/Grove/BunaStump.png" -alpha off -filter box -resize 10x32! +dither -colors 5 \
 -scale 200% -depth 8 PNG32:"$buna2_tmp/bark.png"
python3 -B "$root/tools/pack_buna_classic.py" "$buna2_tmp"
python3 -B "$root/tools/build_buna_joints.py"
