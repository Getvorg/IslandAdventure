#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
# Source illustration has three differently sized panels, not equally sized cells.
# Reduce to logical pixels first; every final art pixel is exactly 2x2 game pixels.
for name in bush fern tuft; do
 filter=point; colors=24; edge=50%
 case "$name" in
  bush) crop=890x724+0+0; size=22x14!; canvas=24x16; cols=3; rows=2;;
  fern) crop=; size=30x22; canvas=32x24; cols=4; rows=3; filter=box; colors=8; edge=25%;;
  tuft) crop=434x724+1738+0; size=7x15!; canvas=8x16; cols=1; rows=2;;
 esac
 source="$root/tools/art/grove/understory-source.png"
 crop_args=(-crop "$crop" +repage)
 if [[ "$name" == fern ]]; then source="$root/tools/art/grove/fern-source.png"; crop_args=(); fi
 # The fern keeps its natural broad aspect ratio: never squeeze it into a tall frame.
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
