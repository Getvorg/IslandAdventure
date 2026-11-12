#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
read -r width height < <(magick identify -format '%w %h\n' "$root/tools/art/dark-forest-materials.png")
for name in Soil Stone Bark Moss; do
 x=0;y=0
 case "$name" in Stone) x=$((width/2));; Bark) y=$((height/2));; Moss) x=$((width/2));y=$((height/2));; esac
 magick "$root/tools/art/dark-forest-materials.png" -crop "$((width/2))x$((height/2))+${x}+${y}" +repage -filter point -resize 64x64! +dither -colors 32 -scale 200% -depth 8 PNG32:"$tmp/$name.png"
done
# Standard 16px tile framing, 18px stride. Only layout ALPHA comes from ExampleMod.
for name in Soil Stone Wood Grass; do
 material="$name";[[ "$name" == Wood ]] && material=Bark;[[ "$name" == Grass ]] && material=Soil
 for ((r=0;r<15;r++)); do
  row=()
  for ((c=0;c<16;c++)); do
   patch="$tmp/frame-$r-$c.png"
   magick "$tmp/$material.png" -crop "16x16+$(((c*14)%112))+$(((r*10)%112))" +repage "$tmp/patch.png"
   if [[ "$name" == Grass ]];then
    magick "$tmp/Moss.png" -crop "16x6+$(((c*14)%112))+$(((r*10)%112))" +repage "$tmp/leaf.png"
    magick "$tmp/patch.png" "$tmp/leaf.png" -gravity north -compose over -composite "$tmp/patch.png"
   fi
   magick "$tmp/patch.png" -background none -gravity northwest -extent 18x18 -depth 8 PNG32:"$patch"
   row+=("$patch")
  done
  magick "${row[@]}" +append "$tmp/row-$r.png"
 done
 rows=();for ((r=0;r<15;r++));do rows+=("$tmp/row-$r.png");done
 magick "${rows[@]}" -append "$tmp/sheet.png"
 magick "$tmp/sheet.png" \( "$root/tools/art/layouts/block.png" -alpha extract \) -alpha off -compose CopyOpacity -composite -depth 8 PNG32:"$root/Assets/Tiles/DarkForest/$name.png"
 [[ "$name" == Grass ]] && continue
 magick "$root/Assets/Tiles/DarkForest/$name.png" -crop 16x16+18+18 +repage -depth 8 PNG32:"$root/Assets/Items/Dark$name.png"
done
magick -size 176x264 tile:"$tmp/Bark.png" \( "$root/tools/art/layouts/tree.png" -alpha extract \) -alpha off -compose CopyOpacity -composite -depth 8 PNG32:"$root/Assets/Trees/DarkTree.png"
for material in Soil Stone; do
 magick -size 468x180 tile:"$tmp/$material.png" -brightness-contrast -12x0 \( "$root/tools/art/layouts/wall.png" -alpha extract \) -alpha off -compose CopyOpacity -composite -depth 8 PNG32:"$root/Assets/Walls/DarkForest/$material.png"
done
read -r width height < <(magick identify -format '%w %h\n' "$root/tools/art/dark-forest-foliage.png")
for ((c=0;c<3;c++));do
 for part in top branch;do
  y=0;size=40;[[ "$part" == branch ]] && y=$((height/2)) && size=20
  magick "$root/tools/art/dark-forest-foliage.png" -crop "$((width/3))x$((height/2))+$((c*width/3))+$y" +repage -channel A -threshold 85% +channel -trim +repage -filter point -resize "$((size-2))x$((size-2))" -background none -gravity center -extent "${size}x${size}" -channel A -threshold 50% +channel -scale 200% -background none -gravity northwest -extent "$((size*2+2))x$((size*2+2))" -depth 8 PNG32:"$tmp/$part-$c.png"
 done
done
magick "$tmp/top-0.png" "$tmp/top-1.png" "$tmp/top-2.png" +append -depth 8 PNG32:"$root/Assets/Trees/DarkTree_Tops.png"
for ((c=0;c<3;c++));do
 magick "$tmp/branch-$c.png" \( "$tmp/branch-$c.png" -flop \) +append "$tmp/branches-$c.png"
 magick "$tmp/branch-$c.png" -filter point -resize 7x7 -gravity south -background none -extent 8x8 -scale 200% -gravity northwest -extent 18x18 "$tmp/plant-$c.png"
done
magick "$tmp/branches-0.png" "$tmp/branches-1.png" "$tmp/branches-2.png" -append -depth 8 PNG32:"$root/Assets/Trees/DarkTree_Branches.png"
magick "$tmp/plant-0.png" "$tmp/plant-1.png" "$tmp/plant-2.png" +append -depth 8 PNG32:"$root/Assets/Tiles/DarkForest/Plant.png"
magick "$root/Assets/Tiles/DarkForest/Plant.png" -rotate 90 -gravity northwest -background none -extent 54x54 -depth 8 PNG32:"$root/Assets/Tiles/DarkForest/Vine.png"
