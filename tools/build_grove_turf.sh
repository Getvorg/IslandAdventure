#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
# Thin sod: 2px tips, 4px opaque green body, 4px irregular root transition.
# Keep the existing atlas contract (body begins at y=4), without thick cushions.
magick "$root/tools/art/grove/turf-source.png" -channel A -threshold 85% +channel \
 -trim +repage -filter point -resize 48x5! -channel A -threshold 50% +channel \
 +dither -colors 12 "$tmp/logical.png"
magick -size 48x1 xc:none "$tmp/blank.png"
magick "$tmp/logical.png" -crop 48x1+0+0 +repage "$tmp/tips.png"
magick "$tmp/logical.png" -crop 48x2+0+1 +repage \
 -background '#3e6c32' -alpha remove -alpha off "$tmp/body.png"
magick "$tmp/logical.png" -crop 48x2+0+3 +repage "$tmp/roots.png"
magick "$tmp/blank.png" "$tmp/tips.png" "$tmp/body.png" "$tmp/roots.png" -append \
 -background none -gravity northwest -extent 48x8 \
 -scale 200% -depth 8 PNG32:"$tmp/strip.png"
frames=()
for n in 0 1 2 3 4 5; do
 file="$tmp/frame-$n.png"; frames+=("$file")
 magick "$tmp/strip.png" -crop "16x16+$((n*16))+0" +repage \
  -background none -gravity northwest -extent 18x16 "$file"
done
magick "${frames[@]}" +append -depth 8 PNG32:"$root/Assets/Grove/GrassEdge.png"
