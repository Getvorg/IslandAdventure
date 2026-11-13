#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
# 48x8 logical pixels retain leaf clusters that the old 24x4 reduction lost.
# The four middle rows hide the vanilla dirt outline; tips may be transparent.
magick "$root/tools/art/grove/turf-source.png" -channel A -threshold 85% +channel \
 -trim +repage -filter point -resize 48x8! -channel A -threshold 50% +channel \
 +dither -colors 20 "$tmp/logical.png"
magick "$tmp/logical.png" -crop 48x2+0+0 +repage "$tmp/tips.png"
magick "$tmp/logical.png" -crop 48x4+0+2 +repage \
 -background '#386c3c' -alpha remove -alpha off "$tmp/body.png"
magick "$tmp/logical.png" -crop 48x2+0+6 +repage "$tmp/roots.png"
magick "$tmp/tips.png" "$tmp/body.png" "$tmp/roots.png" -append \
 -scale 200% -depth 8 PNG32:"$tmp/strip.png"
frames=()
for n in 0 1 2 3 4 5; do
 file="$tmp/frame-$n.png"; frames+=("$file")
 magick "$tmp/strip.png" -crop "16x16+$((n*16))+0" +repage \
  -background none -gravity northwest -extent 18x16 "$file"
done
magick "${frames[@]}" +append -depth 8 PNG32:"$root/Assets/Grove/GrassEdge.png"
