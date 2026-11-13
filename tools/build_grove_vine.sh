#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
# Sixteen 32x16 segments: leaves can extend beyond the native 16px-wide vine.
# Keep the established 2x2 grid; long vines use successive pieces of one stem.
magick "$root/tools/art/grove/vine-source.png" -channel A -threshold 70% +channel \
 -trim +repage -filter point -resize 16x128! +dither -colors 20 \
 -channel A -threshold 50% +channel -scale 200% -depth 8 PNG32:"$tmp/vine.png"
cp "$tmp/vine.png" "$root/Assets/Grove/Vine.png"
