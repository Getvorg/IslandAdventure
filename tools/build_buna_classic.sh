#!/usr/bin/env bash
# Build the unchanged stem, then extract connected wood/leaf component masters.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
buna2_tmp="$(mktemp -d)";trap 'rm -rf "$buna2_tmp"' EXIT
magick "$root/Assets/Grove/BunaStump.png" -alpha off -filter box -resize 10x32! +dither -colors 5 \
 -scale 200% -depth 8 PNG32:"$buna2_tmp/bark.png"
python3 -B "$root/tools/pack_buna_classic.py" "$buna2_tmp"
magick "$root/tools/art/buna-classic/dense-leaves-source.png" -channel A -threshold 85% +channel -trim +repage \
 -filter box -resize 64x52! -channel A -threshold 50% +channel +dither -colors 24 \
 -scale 200% -depth 8 PNG32:"$buna2_tmp/crown-leaves.png"
python3 -B "$root/tools/build_buna_joints.py" "$buna2_tmp/crown-leaves.png"
