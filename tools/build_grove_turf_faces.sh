#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)";trap 'rm -rf "$tmp"' EXIT
# Exact source pixels: 16x10 = 2px blade tips + 4px sod + 4px root transition.
# 20x20 cells allow a 2px outward fringe. Only right-angle transforms are used.
for n in 0 1 2 3 4 5; do
 magick "$root/Assets/Grove/GrassEdge.png" -crop "16x10+$((n*18))+2" +repage \
  -background none -gravity north -extent 20x20 "$tmp/top-$n.png"
 magick "$tmp/top-$n.png" -rotate 90 "$tmp/right-$n.png"
 magick "$tmp/top-$n.png" -flip "$tmp/bottom-$n.png"
 magick "$tmp/top-$n.png" -rotate 270 "$tmp/left-$n.png"
done
rows=()
for face in top right bottom left; do
 files=();for n in 0 1 2 3 4 5; do files+=("$tmp/$face-$n.png");done
 row="$tmp/$face.png";rows+=("$row")
 magick "${files[@]}" +append "$row"
done
magick "${rows[@]}" -append -depth 8 PNG32:"$root/Assets/Grove/GrassFaces.png"
