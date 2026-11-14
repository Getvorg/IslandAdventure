#!/usr/bin/env bash
# Approved Camphor foliage -> crisp 2x pixels, native fallback and wider gallery boughs.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
camphor_tmp="$(mktemp -d)"; trap 'rm -rf "$camphor_tmp"' EXIT
source="$root/tools/art/grove/concepts/camphor-parts-source.png"
magick "$source" -crop 1180x724+0+0 +repage -channel A -threshold 85% +channel \
 -trim +repage -filter box -resize 128x84 -channel A -threshold 50% +channel \
 +dither -colors 32 -background none -gravity south -extent 128x88 -scale 200% \
 -background none -gravity northwest -extent 270x178 -depth 8 PNG32:"$root/Assets/Grove/Camphor_Tops.png"
for style in 0 1 2; do
 for side in left right; do
  crop=450x400+1180+280; if [[ "$side" == right ]];then crop=480x400+1692+280;fi
  fit=28x18; if [[ "$style" == 1 ]];then fit=30x20;fi
  if [[ "$style" == 2 ]];then fit=26x20;fi
  magick "$source" -crop "$crop" +repage -channel A -threshold 85% +channel \
   -trim +repage -filter box -resize "$fit" -channel A -threshold 50% +channel \
   +dither -colors 32 -background none -gravity south -extent 32x24 -scale 200% \
   -depth 8 PNG32:"$camphor_tmp/$side-$style.png"
  magick "$camphor_tmp/$side-$style.png" -background none -gravity northwest -extent 66x50 "$camphor_tmp/$side-$style.png"
 done
 magick "$camphor_tmp/left-$style.png" "$camphor_tmp/right-$style.png" +append "$camphor_tmp/row-$style.png"
done
magick "$camphor_tmp/row-0.png" "$camphor_tmp/row-1.png" "$camphor_tmp/row-2.png" -append \
 -depth 8 PNG32:"$root/Assets/Grove/Camphor_Boughs.png"
# ModTree's legacy-soil branch API requires 40x40 frames. Retain a dedicated atlas.
for style in 0 1 2; do
 for side in left right; do
  magick "$camphor_tmp/$side-$style.png" -crop 64x48+0+0 +repage -filter box -resize 20x16! \
   -channel A -threshold 50% +channel -background none -gravity south -extent 20x20 \
   -scale 200% "$camphor_tmp/native-$side-$style.png"
  magick "$camphor_tmp/native-$side-$style.png" -background none -gravity northwest -extent 42x42 "$camphor_tmp/native-$side-$style.png"
 done
 magick "$camphor_tmp/native-left-$style.png" "$camphor_tmp/native-right-$style.png" +append "$camphor_tmp/native-row-$style.png"
done
magick "$camphor_tmp/native-row-0.png" "$camphor_tmp/native-row-1.png" "$camphor_tmp/native-row-2.png" -append \
 -depth 8 PNG32:"$root/Assets/Grove/Camphor_Branches.png"
# Share the stem's wood palette across visible crown limbs and branch sockets.
python3 -B "$root/tools/match_camphor_wood.py"
