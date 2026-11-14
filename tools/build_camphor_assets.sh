#!/usr/bin/env bash
# Pack the newly approved Camphor artwork into native tree frames (ImageMagick 7).
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
camphor_tmp="$(mktemp -d)"; trap 'rm -rf "$camphor_tmp"' EXIT
source="$root/tools/art/grove/concepts/camphor-parts-source.png"
# Hard alpha, a shared foliage palette and logical pixels before the final 2x scale.
for part in crown left right; do
 case "$part" in
  crown) crop=1180x724+0+0; fit=128x84; box=128x88;;
  left) crop=450x400+1180+280; fit=18x16; box=20x20;;
  right) crop=480x400+1692+280; fit=18x16; box=20x20;;
 esac
 magick "$source" -crop "$crop" +repage -channel A -threshold 85% +channel \
  -trim +repage -filter box -resize "$fit" -channel A -threshold 50% +channel \
  +dither -colors 32 -background none -gravity south -extent "$box" \
  -scale 200% -depth 8 PNG32:"$camphor_tmp/$part.png"
done
# The crown's bottom ten pixels join the same 16px native trunk.
magick -size 4x1 xc:'#35281f' xc:'#4d3828' xc:'#654a32' xc:'#7c5d3d' +append "$camphor_tmp/wood-palette.png"
# Use a fresh narrow crop from the approved concept for calm, longitudinal bark.
magick "$root/tools/art/grove/concepts/ancient-camphor.png" -crop 92x360+455+1000 +repage \
 -alpha off -filter box -resize 10x64! +dither -remap "$camphor_tmp/wood-palette.png" \
 -scale 200% "$camphor_tmp/wood.png"
magick -size 176x264 xc:none "$camphor_tmp/atlas.png"
for ((r=0;r<12;r++)); do
 for ((c=0;c<8;c++)); do
  x=$((c*22)); y=$((r*22)); sy=$(((r*17+c*11)%54*2))
  magick "$root/tools/art/grove/layouts/camphor-native-mask.png" -crop "20x20+$x+$y" +repage -alpha extract "$camphor_tmp/mask.png"
  magick "$camphor_tmp/wood.png" -crop "20x20+0+$sy" +repage "$camphor_tmp/mask.png" \
   -alpha off -compose CopyOpacity -composite "$camphor_tmp/frame.png"
  magick "$camphor_tmp/atlas.png" -colorspace sRGB "$camphor_tmp/frame.png" -geometry "+$x+$y" -compose over -composite "$camphor_tmp/atlas.png"
 done
done
magick "$camphor_tmp/atlas.png" -depth 8 PNG32:"$root/Assets/Grove/Camphor.png"
# Pack a single canopy with native trailing gutters.
magick "$camphor_tmp/crown.png" -background none -gravity northwest -extent 258x178 -depth 8 PNG32:"$root/Assets/Grove/Camphor_Tops.png"
# Give both branch sockets a solid 8px wood joint at the renderer's attachment line.
for side in left right; do
 x=32; if [[ "$side" == right ]];then x=0;fi
 magick "$camphor_tmp/wood.png" -crop 8x8+6+24 +repage "$camphor_tmp/socket.png"
 magick "$camphor_tmp/$side.png" "$camphor_tmp/socket.png" -geometry "+$x+20" -compose over -composite \
  -background none -gravity northwest -extent 42x42 "$camphor_tmp/$side.png"
done
magick "$camphor_tmp/left.png" "$camphor_tmp/right.png" +append "$camphor_tmp/branch-row.png"
magick "$camphor_tmp/branch-row.png" "$camphor_tmp/branch-row.png" "$camphor_tmp/branch-row.png" -append \
 -depth 8 PNG32:"$root/Assets/Grove/Camphor_Branches.png"
