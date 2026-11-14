#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
tmp="$(mktemp -d)";trap 'rm -rf "$tmp"' EXIT
reference="$root/tools/art/grove/layouts/grove-trunk-reference.png"
# Quiet vanilla-like vertical bark, four close browns, exact 2x2 art pixels.
magick xc:'#34271e' xc:'#493529' xc:'#5b412b' xc:'#765438' +append "$tmp/palette.png"
magick "$root/tools/art/grove/camphor-bark-source.png" -alpha off \
 -filter box -resize 10x64! +dither -remap "$tmp/palette.png" "$tmp/logical-wood.png"
# Rare short grain marks on the final logical grid: one quiet stroke per
# native frame variation, rather than noise or diagonal cross-hatching.
magick "$tmp/logical-wood.png" -fill '#5b412b' -draw 'rectangle 3,0 6,63' \
 -fill '#34271e' \
 -draw 'line 4,5 4,8 line 6,22 6,25 line 3,36 3,39 line 5,52 5,55' \
 -fill '#765438' -draw 'point 5,6 point 5,23 point 4,37 point 6,53' \
 -scale 200% "$tmp/wood.png"
cp "$reference" "$tmp/trunk.png"
for ((row=0;row<12;row++)); do
 for ((col=0;col<8;col++)); do
  # Keep all branch-only frames exactly as approved, including bare twigs.
  if (( (col==3 && row<3) || (col==4 && row>=3 && row<6) || ((col==2 || col==3) && row>=9) )); then continue; fi
  magick "$reference" -crop "20x20+$((col*22))+$((row*22))" +repage \
   -alpha extract "$tmp/mask.png"
  magick "$tmp/wood.png" -crop "20x20+0+$((((row*17+col*11)%54)*2))" +repage \
   "$tmp/mask.png" -alpha off -compose CopyOpacity -composite "$tmp/frame.png"
  # Outline only the lateral silhouette, avoiding horizontal seams per tile.
  magick "$tmp/mask.png" -filter point -resize 50% "$tmp/logical-mask.png"
  magick "$tmp/logical-mask.png" -virtual-pixel black \
   -morphology Erode '3x1:1,1,1' "$tmp/inside.png"
  magick "$tmp/logical-mask.png" "$tmp/inside.png" -fx 'max(0,u-v)' -scale 200% "$tmp/edge.png"
  magick -size 20x20 xc:'#34271e' "$tmp/edge.png" -alpha off \
   -compose CopyOpacity -composite "$tmp/outline.png"
  magick "$tmp/frame.png" "$tmp/outline.png" -compose over -composite "$tmp/frame.png"
  magick "$tmp/trunk.png" "$tmp/frame.png" -geometry "+$((col*22))+$((row*22))" \
   -compose Copy -composite "$tmp/trunk.png"
 done
done
magick "$tmp/trunk.png" -depth 8 PNG32:"$root/Assets/Grove/Tree.png"
