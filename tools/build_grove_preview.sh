#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
read -r w h < <(magick identify -format '%w %h\n' "$root/tools/art/grove/plants-source.png")
for name in grass bush fern crown;do
 x=0;y=0;cw=$((w/2));ch=$((h/2))
 case "$name" in bush) x="$cw";;fern) y="$ch";;crown) x="$cw";y="$ch";ch=$((ch*7/10));;esac
 magick "$root/tools/art/grove/plants-source.png" -crop "${cw}x${ch}+${x}+${y}" +repage -channel A -threshold 85% +channel -trim +repage "$tmp/$name.png"
done
# 2x logical pixels; native tree top uses one 80x80 frame plus 2px padding.
magick "$tmp/crown.png" -filter point -resize 38x38 -background none -gravity south -extent 40x40 -channel A -threshold 50% +channel -scale 200% -gravity northwest -extent 82x82 -depth 8 PNG32:"$root/Assets/Grove/Tree_Tops.png"
magick "$root/tools/art/grove/bark-source.png" -filter point -resize 64x64! +dither -colors 32 -scale 200% "$tmp/bark.png"
magick -size 176x264 tile:"$tmp/bark.png" \( "$root/tools/art/grove/layouts/tree.png" -alpha extract \) -alpha off -compose CopyOpacity -composite -depth 8 PNG32:"$root/Assets/Grove/Tree.png"
# Leaf pattern and native attachment mask for branches. The bark strip joins the trunk.
magick "$tmp/bush.png" -filter point -resize 32x16! -gravity center -crop 8x8+0+0 +repage -alpha off -scale 200% "$tmp/leaf.png"
magick -size 84x126 tile:"$tmp/leaf.png" "$tmp/branch.png"
for y in 0 42 84;do
 magick "$tmp/bark.png" -crop 14x40+0+0 +repage "$tmp/strip.png"
 magick "$tmp/branch.png" "$tmp/strip.png" -geometry "+26+$y" -compose over -composite "$tmp/branch.png"
 magick "$tmp/branch.png" "$tmp/strip.png" -geometry "+42+$y" -compose over -composite "$tmp/branch.png"
done
magick "$tmp/branch.png" \( "$root/tools/art/grove/layouts/branches.png" -alpha extract \) -alpha off -compose CopyOpacity -composite -depth 8 PNG32:"$root/Assets/Grove/Tree_Branches.png"
magick "$tmp/grass.png" -filter point -resize 24x4! -channel A -threshold 50% +channel -scale 200% "$tmp/edge.png"
for n in 0 1 2;do
 magick "$tmp/edge.png" -crop "16x8+$((n*16))+0" +repage -background none -gravity northwest -extent 18x8 "$tmp/edge-$n.png"
 magick "$tmp/edge-$n.png" -background none -gravity south -extent 18x18 "$tmp/tuft-$n.png"
done
magick "$tmp/edge-0.png" "$tmp/edge-1.png" "$tmp/edge-2.png" +append -depth 8 PNG32:"$root/Assets/Grove/GrassEdge.png"
magick "$tmp/tuft-0.png" "$tmp/tuft-1.png" "$tmp/tuft-2.png" +append -depth 8 PNG32:"$root/Assets/Grove/Tuft.png"
for name in bush fern;do
 magick "$tmp/$name.png" -filter point -resize 14x14 -background none -gravity south -extent 16x16 -channel A -threshold 50% +channel -scale 200% "$tmp/object.png"
 for r in 0 1;do
  for c in 0 1;do
   magick "$tmp/object.png" -crop "16x16+$((c*16))+$((r*16))" +repage -background none -gravity northwest -extent 18x18 "$tmp/cell-$r-$c.png"
  done
  magick "$tmp/cell-$r-0.png" "$tmp/cell-$r-1.png" +append "$tmp/row-$r.png"
 done
 magick "$tmp/row-0.png" "$tmp/row-1.png" -append -depth 8 PNG32:"$root/Assets/Grove/$name.png"
done
if [[ -f "$root/tools/art/grove/understory-source.png" ]]; then
 bash "$root/tools/build_grove_understory.sh"
fi
