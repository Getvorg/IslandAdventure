#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
while read -r name source crop fit cols rows variants; do
 [[ -z "$name" || "$name" == \#* ]] && continue
 magick "$root/tools/art/grove/$source.png" -crop "$crop" +repage \
  -channel A -threshold 85% +channel -trim +repage -filter box -resize "$fit" \
  -channel A -threshold 40% +channel +dither -colors 32 \
  -background none -gravity south -extent "$((cols*8))x$((rows*8))" \
  -scale 200% -depth 8 PNG32:"$tmp/native.png"
 [[ "$name" == Roots ]] && cp "$tmp/native.png" "$root/Assets/Grove/Tree_Roots.png"
 [[ "$name" == Moss ]] && cp "$tmp/native.png" "$tmp/moss.png"
 for ((r=0;r<rows;r++)); do
  cells=()
  for ((c=0;c<cols;c++)); do
   file="$tmp/cell-$r-$c.png"; cells+=("$file")
   magick "$tmp/native.png" -crop "16x16+$((c*16))+$((r*16))" +repage \
    -background none -gravity northwest -extent 18x18 "$file"
  done
  magick "${cells[@]}" +append "$tmp/row-$r.png"
 done
 row_files=(); for ((r=0;r<rows;r++)); do row_files+=("$tmp/row-$r.png"); done
 magick "${row_files[@]}" -append "$tmp/atlas.png"
 if [[ "$variants" == 3 ]]; then
  # Native style stride includes the 2px tile gutters; mirror complete cells.
  magick "$tmp/atlas.png" -crop 16x16+18+0 +repage -flop -background none -gravity northwest -extent 18x18 "$tmp/mirror-a.png"
  magick "$tmp/atlas.png" -crop 16x16+0+0 +repage -flop -background none -gravity northwest -extent 18x18 "$tmp/mirror-b.png"
  magick "$tmp/mirror-a.png" "$tmp/mirror-b.png" +append "$tmp/mirror.png"
  magick "$tmp/atlas.png" "$tmp/mirror.png" "$tmp/atlas.png" +append -depth 8 PNG32:"$root/Assets/Grove/$name.png"
 else
  magick "$tmp/atlas.png" -depth 8 PNG32:"$root/Assets/Grove/$name.png"
 fi
 # Reconstructed, exact game-size object for comparing the whole art set.
 cp "$tmp/native.png" "$tmp/preview-$name.png"
done < "$root/tools/art/grove/objects.tsv"
# Moss becomes the turf rim too; the installed vanilla dirt underneath stays unchanged.
for n in 0 1 2; do
 magick "$tmp/moss.png" -crop "16x8+$((n*16))+8" +repage -background none -gravity northwest -extent 18x8 "$tmp/edge-$n.png"
done
magick "$tmp/edge-0.png" "$tmp/edge-1.png" "$tmp/edge-2.png" +append -depth 8 PNG32:"$root/Assets/Grove/GrassEdge.png"
# New canopy and bark use Terraria's native tree tile and branch attachment masks.
magick "$root/tools/art/grove/tree-source.png" -crop 1448x724+0+0 +repage \
 -channel A -threshold 85% +channel -trim +repage -filter box -resize 92x60 \
 -channel A -threshold 45% +channel +dither -colors 40 -background none -gravity south -extent 96x64 \
 -scale 200% -gravity northwest -extent 194x130 -depth 8 PNG32:"$root/Assets/Grove/Tree_Tops.png"
magick "$root/tools/art/grove/tree-source.png" -crop 704x704+1458+10 +repage \
 -alpha off -filter box -resize 64x64! +dither -colors 32 -scale 200% "$tmp/bark.png"
magick -size 176x264 tile:"$tmp/bark.png" \( "$root/tools/art/grove/layouts/tree.png" -alpha extract \) \
 -alpha off -compose CopyOpacity -composite -depth 8 PNG32:"$root/Assets/Grove/Tree.png"
magick "$root/Assets/Grove/Tree_Tops.png" -crop 80x40+50+56 +repage -filter box -resize 8x8! -alpha off -scale 200% "$tmp/leaf.png"
magick -size 84x126 tile:"$tmp/leaf.png" "$tmp/branches.png"
for y in 0 42 84; do
 magick "$tmp/bark.png" -crop 14x40+0+0 +repage "$tmp/strip.png"
 for x in 26 42; do
  magick "$tmp/branches.png" "$tmp/strip.png" -geometry "+$x+$y" -compose over -composite "$tmp/branches.png"
 done
done
magick "$tmp/branches.png" \( "$root/tools/art/grove/layouts/branches.png" -alpha extract \) \
 -alpha off -compose CopyOpacity -composite -depth 8 PNG32:"$root/Assets/Grove/Tree_Branches.png"
# A preview of actual native-size sprites, never the large AI source illustration.
magick "$tmp/preview-Tuft.png" "$tmp/preview-TallGrass.png" "$tmp/preview-SmallBush.png" "$tmp/preview-SmallFern.png" "$tmp/preview-Moss.png" "$tmp/preview-Roots.png" -background none -gravity south +append "$tmp/green.png"
magick "$tmp/preview-FallenLog.png" "$tmp/preview-HollowLog.png" "$tmp/preview-Pebble.png" "$tmp/preview-Rock.png" "$tmp/preview-LargeRock.png" "$tmp/preview-Boulder.png" -background none -gravity south +append "$tmp/props.png"
magick "$tmp/green.png" "$tmp/props.png" -background '#6588ac' -gravity west -append -alpha remove \
 -filter point -resize 200% -depth 8 PNG32:"$root/tools/art/grove/ecosystem-preview.png"
