#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
# Four narrow 16x128 vines. Each has seven body frames and one terminal frame.
read -r width height < <(magick identify -format '%w %h\n' "$root/tools/art/grove/vine-source.png")
cw=$((width/4)); vines=()
for n in 0 1 2 3; do
 file="$tmp/vine-$n.png";vines+=("$file")
 magick "$root/tools/art/grove/vine-source.png" -crop "${cw}x${height}+$((n*cw))+0" +repage \
  -channel A -threshold 65% +channel -trim +repage -filter box -resize 6x64! \
  -channel A -threshold 20% +channel +dither -colors 12 \
  -background none -gravity center -extent 8x64 -scale 200% -depth 8 PNG32:"$file"
done
magick "${vines[@]}" +append -depth 8 PNG32:"$root/Assets/Grove/Vine.png"
