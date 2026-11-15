#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
buna_items_tmp="$(mktemp -d)";trap 'rm -rf "$buna_items_tmp"' EXIT
source="$root/tools/art/buna-items/source.png"
magick "$root/tools/art/buna-items/nuts-source.png" -channel A -threshold 85% +channel -trim +repage \
 -filter box -resize 16x14 -channel A -threshold 50% +channel +dither -colors 24 \
 -background none -gravity center -extent 16x14 -scale 200% -depth 8 PNG32:"$root/Assets/Items/BunaNut.png"
magick "$source" -crop 600x600+780+80 +repage -channel A -threshold 85% +channel -trim +repage \
 -filter box -resize 16x16 -channel A -threshold 50% +channel +dither -colors 24 \
 -background none -gravity center -extent 16x16 -scale 200% -depth 8 PNG32:"$root/Assets/Items/BunaWood.png"
magick "$source" -crop 720x724+0+0 +repage -channel A -threshold 85% +channel -trim +repage \
 -filter box -resize 8x16 -channel A -threshold 50% +channel +dither -colors 24 \
 -background none -gravity south -extent 8x16 -scale 200% "$buna_items_tmp/sapling.png"
for row in 0 1;do
 magick "$buna_items_tmp/sapling.png" -crop "16x16+0+$((row*16))" +repage \
 -background none -gravity northwest -extent 18x18 "$buna_items_tmp/row-$row.png"
done
magick "$buna_items_tmp/row-0.png" "$buna_items_tmp/row-1.png" -append -depth 8 PNG32:"$root/Assets/Tiles/BunaSapling.png"
for n in 0 1 2;do
 magick "$source" -crop "420x225+1650+$((n*235))" +repage -channel A -threshold 85% +channel -trim +repage \
  -filter box -resize 5x7 -channel A -threshold 50% +channel +dither -colors 12 \
  -background none -gravity center -extent 5x7 -scale 200% "$buna_items_tmp/leaf-$n.png"
done
magick "$buna_items_tmp/leaf-0.png" "$buna_items_tmp/leaf-1.png" "$buna_items_tmp/leaf-2.png" -append -depth 8 PNG32:"$root/Assets/Dusts/BunaLeaf.png"
magick "$root/Assets/Grove/BunaStump.png" -crop 8x8+4+0 +repage -depth 8 PNG32:"$root/Assets/Dusts/BunaBark.png"
python3 -B "$root/tools/build_buna_wood.py"
