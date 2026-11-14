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
# A preview of actual native-size sprites, never the large AI source illustration.
magick "$tmp/preview-Tuft.png" "$tmp/preview-SmallBush.png" "$tmp/preview-SmallFern.png" "$tmp/preview-Moss.png" "$tmp/preview-Roots.png" -background none -gravity south +append "$tmp/green.png"
magick "$tmp/preview-FallenLog.png" "$tmp/preview-HollowLog.png" "$tmp/preview-Pebble.png" "$tmp/preview-Rock.png" "$tmp/preview-LargeRock.png" "$tmp/preview-Boulder.png" -background none -gravity south +append "$tmp/props.png"
magick "$tmp/green.png" "$tmp/props.png" -background '#6588ac' -gravity west -append -alpha remove \
 -filter point -resize 200% -depth 8 PNG32:"$root/tools/art/grove/ecosystem-preview.png"
