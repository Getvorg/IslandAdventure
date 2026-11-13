#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
read -r width height < <(magick identify -format '%w %h\n' "$root/tools/art/grove/ground-plants-source.png")
cw=$((width/4)); ch=$((height/4)); frames=()
for ((n=0;n<16;n++)); do
 fit=12x13
 ((n<4)) && fit=14x7
 ((n>=4&&n<8)) && fit=12x14
 ((n>=8&&n<12)) && fit=12x12
 file="$tmp/plant-$n.png";frames+=("$file")
 magick "$root/tools/art/grove/ground-plants-source.png" \
  -crop "${cw}x${ch}+$(((n%4)*cw))+$(((n/4)*ch))" +repage \
  -channel A -threshold 75% +channel -trim +repage -filter box -resize "$fit" \
  -channel A -threshold 35% +channel +dither -colors 16 \
  -background none -gravity south -extent 16x16 -scale 200% -depth 8 PNG32:"$file"
done
magick "${frames[@]}" +append -depth 8 PNG32:"$root/Assets/Grove/GroundPlants.png"
# Update gallery tufts to three distinct short-grass drawings (no mirrored duplicates).
cells=()
for n in 0 1 2; do
 for c in 0 1; do
  file="$tmp/tuft-$n-$c.png";cells+=("$file")
  magick "$tmp/plant-$n.png" -crop "16x16+$((c*16))+16" +repage \
   -background none -gravity northwest -extent 18x18 "$file"
 done
done
magick "${cells[@]}" +append -depth 8 PNG32:"$root/Assets/Grove/Tuft.png"
# Existing tall-grass objects retain their footprint, with a finer blade silhouette.
magick "$tmp/plant-4.png" -filter point -resize 50% -trim +repage -resize 20x22 \
 -background none -gravity south -extent 24x32 -scale 200% "$tmp/tall.png"
rows=()
for r in 0 1 2 3; do
 cells=()
 for c in 0 1 2; do
  file="$tmp/tall-$r-$c.png";cells+=("$file")
  magick "$tmp/tall.png" -crop "16x16+$((c*16))+$((r*16))" +repage \
   -background none -gravity northwest -extent 18x18 "$file"
 done
 row="$tmp/tall-row-$r.png";rows+=("$row")
 magick "${cells[@]}" +append "$row"
done
magick "${rows[@]}" -append -depth 8 PNG32:"$root/Assets/Grove/TallGrass.png"
# Actual game-size frames for visual review, rather than large source illustrations.
magick "${frames[@]:0:8}" -background '#516579' +append "$tmp/grasses.png"
magick "${frames[@]:8:8}" -background '#516579' +append "$tmp/flowers.png"
magick "$tmp/grasses.png" "$tmp/flowers.png" -append -background '#516579' -alpha remove -alpha off -scale 400% \
 "$root/tools/art/grove/ground-plants-preview.png"
