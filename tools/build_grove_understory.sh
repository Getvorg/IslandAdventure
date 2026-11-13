#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
# Large shrub and fern share the new two-panel source; tuft retains its own source.
# Reduce to logical pixels first; every final art pixel is exactly 2x2 game pixels.
for name in bush fern tuft; do
 filter=point; colors=24; edge=50%
 case "$name" in
  bush) crop=1086x724+0+0; size=52x38; canvas=56x40; cols=7; rows=5; filter=box; colors=32; edge=40%;;
  fern) crop=1086x724+1086+0; size=60x46; canvas=64x48; cols=8; rows=6; filter=box; colors=32; edge=35%;;
  tuft) crop=434x724+1738+0; size=7x15!; canvas=8x16; cols=1; rows=2;;
 esac
 source="$root/tools/art/grove/understory-source.png"
 crop_args=(-crop "$crop" +repage)
 if [[ "$name" != tuft ]]; then source="$root/tools/art/grove/lush-understory-source.png"; fi
 # Keep natural proportions; transparent padding does not stretch plant silhouettes.
 magick "$source" "${crop_args[@]}" \
  -channel A -threshold 85% +channel -trim +repage -filter "$filter" -resize "$size" \
  -channel A -threshold "$edge" +channel +dither -colors "$colors" \
  -background none -gravity south -extent "$canvas" -scale 200% "$tmp/object.png"
 for ((r=0;r<rows;r++)); do
  cells=()
  for ((c=0;c<cols;c++)); do
   cell="$tmp/cell-$r-$c.png"
   magick "$tmp/object.png" -crop "16x16+$((c*16))+$((r*16))" +repage \
    -background none -gravity northwest -extent 18x18 "$cell"
   cells+=("$cell")
  done
  magick "${cells[@]}" +append "$tmp/row-$r.png"
 done
 row_files=(); for ((r=0;r<rows;r++)); do row_files+=("$tmp/row-$r.png"); done
 magick "${row_files[@]}" -append "$tmp/atlas.png"
 if [[ "$name" == tuft ]]; then
  magick "$tmp/object.png" -flop "$tmp/mirror.png"
  for r in 0 1; do
   magick "$tmp/mirror.png" -crop "16x16+0+$((r*16))" +repage \
    -background none -gravity northwest -extent 18x18 "$tmp/mirror-$r.png"
  done
  magick "$tmp/mirror-0.png" "$tmp/mirror-1.png" -append "$tmp/mirror-atlas.png"
  magick "$tmp/atlas.png" "$tmp/mirror-atlas.png" "$tmp/atlas.png" +append -depth 8 PNG32:"$root/Assets/Grove/Tuft.png"
 else
  magick "$tmp/atlas.png" -depth 8 PNG32:"$root/Assets/Grove/$name.png"
 fi
done
