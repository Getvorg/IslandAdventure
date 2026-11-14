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
# Rebuild the dedicated textured turf rim.
python3 "$root/tools/build_grove_grass_atlas.py"
bash "$root/tools/build_grove_ground_plants.sh"
# Refresh object previews from the new actual atlases, not the old source crops.
for name in Tuft; do
 cols=2; rows=1
 previews=()
 for ((r=0;r<rows;r++)); do
  cells=()
  for ((c=0;c<cols;c++)); do
   file="$tmp/new-$name-$r-$c.png";cells+=("$file")
   magick "$root/Assets/Grove/$name.png" -crop "16x16+$((c*18))+$((r*18))" +repage "$file"
  done
  file="$tmp/new-$name-row-$r.png";previews+=("$file")
  magick "${cells[@]}" +append "$file"
 done
 magick "${previews[@]}" -append "$tmp/preview-$name.png"
done
# New rounded crown. Native foliage frames use a 2px trailing gutter.
magick "$root/tools/art/grove/tree-foliage-source.png" -crop 930x724+0+0 +repage \
 -channel A -threshold 85% +channel -trim +repage -filter box -resize 124x92 \
 -channel A -threshold 45% +channel +dither -colors 40 -background none -gravity south -extent 128x96 \
 -scale 200% -gravity northwest -extent 258x194 -depth 8 PNG32:"$root/Assets/Grove/Tree_Tops.png"
# Prepare foliage first so the bark material can be calibrated to its actual wood colors.
for side in left right; do
 crop=590x724+930+0; pad=2
 if [[ "$side" == right ]]; then crop=652x724+1520+0; pad=3; fi
 magick "$root/tools/art/grove/tree-foliage-source.png" -crop "$crop" +repage \
  -channel A -threshold 85% +channel -trim +repage -filter box -resize 20x16 \
  -channel A -threshold 40% +channel +dither -colors 32 -background none -gravity north \
  -splice "0x$pad" -gravity northwest -extent 20x20 -scale 200% \
  -gravity northwest -extent 42x42 "$tmp/branch-$side.png"
done
magick "$root/tools/art/grove/bark-source.png" -alpha off -filter box -resize 64x64! -scale 200% "$tmp/bark.png"
# Read-only color measurement; ImageMagick applies the calibration to the generated bark.
read -r red_factor green_factor blue_factor < <(python3 - "$tmp/bark.png" "$tmp/branch-left.png" "$tmp/branch-right.png" <<'PYCOLORS'
import subprocess,sys

def brown_mean(paths):
    sums=[0,0,0]; count=0
    for path in paths:
        raw=subprocess.check_output(['magick',path,'-depth','8','RGBA:-'])
        for i in range(0,len(raw),4):
            r,g,b,a=raw[i:i+4]
            if a==255 and r>g*1.12 and g>b*1.1:
                for k,v in enumerate((r,g,b)):sums[k]+=v
                count+=1
    if not count:raise RuntimeError('No opaque wood pixels in reference')
    return [v/count for v in sums]
source=brown_mean([sys.argv[1]]); target=brown_mean(sys.argv[2:])
print(*(f'{t/s:.8f}' for s,t in zip(source,target)))
PYCOLORS
)
magick "$tmp/bark.png" -channel R -evaluate Multiply "$red_factor" \
 -channel G -evaluate Multiply "$green_factor" -channel B -evaluate Multiply "$blue_factor" \
 +channel +dither -colors 32 "$tmp/bark.png"
# Quiet vanilla-like stem; approved foliage and branch art stay separate.
bash "$root/tools/build_grove_trunk.sh"
# Six-pixel connectors touch the native trunk at y=24 / y=30.
# They use the exact same bark pixels as the trunk, rather than a separate color.
for side in left right; do
 x=32; y=22
 if [[ "$side" == right ]]; then x=0; y=28; fi
 magick "$tmp/bark.png" -crop 8x6+0+0 +repage "$tmp/joint.png"
 magick "$tmp/branch-$side.png" "$tmp/joint.png" -geometry "+$x+$y" -compose over -composite "$tmp/branch-$side.png"
done
magick "$tmp/branch-left.png" "$tmp/branch-right.png" +append "$tmp/branch-row.png"
magick "$tmp/branch-row.png" "$tmp/branch-row.png" "$tmp/branch-row.png" -append -depth 8 PNG32:"$root/Assets/Grove/Tree_Branches.png"
# A preview of actual native-size sprites, never the large AI source illustration.
magick "$tmp/preview-Tuft.png" "$tmp/preview-SmallBush.png" "$tmp/preview-SmallFern.png" "$tmp/preview-Moss.png" "$tmp/preview-Roots.png" -background none -gravity south +append "$tmp/green.png"
magick "$tmp/preview-FallenLog.png" "$tmp/preview-HollowLog.png" "$tmp/preview-Pebble.png" "$tmp/preview-Rock.png" "$tmp/preview-LargeRock.png" "$tmp/preview-Boulder.png" -background none -gravity south +append "$tmp/props.png"
magick "$tmp/green.png" "$tmp/props.png" -background '#6588ac' -gravity west -append -alpha remove \
 -filter point -resize 200% -depth 8 PNG32:"$root/tools/art/grove/ecosystem-preview.png"
