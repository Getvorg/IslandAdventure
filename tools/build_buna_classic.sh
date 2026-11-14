#!/usr/bin/env bash
# V2: independent native trunk atlas, crown and directional branch sprites.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
buna2_tmp="$(mktemp -d)";trap 'rm -rf "$buna2_tmp"' EXIT
source="$root/tools/art/buna-classic/foliage-source.png"
magick "$source" -crop 1000x768+0+0 +repage -channel A -threshold 85% +channel \
 -trim +repage -filter box -resize 64x56! -channel A -threshold 50% +channel +dither -colors 32 \
 -scale 200% -depth 8 PNG32:"$root/Assets/Grove/BunaClassic_Tops.png"
for side in left right;do
 crop=420x420+1000+200;if [[ "$side" == right ]];then crop=468x420+1500+200;fi
 magick "$source" -crop "$crop" +repage -channel A -threshold 85% +channel \
  -trim +repage -filter box -resize 18x16 -channel A -threshold 50% +channel +dither -colors 24 \
  -background none -gravity south -extent 20x20 -scale 200% -depth 8 PNG32:"$buna2_tmp/$side.png"
done
# The accepted V1 supplies the new beech's own bark material, never old tree art.
magick "$root/Assets/Grove/BunaStump.png" -alpha off -filter box -resize 10x32! +dither -colors 5 \
 -scale 200% -depth 8 PNG32:"$buna2_tmp/bark.png"
python3 -B "$root/tools/pack_buna_classic.py" "$buna2_tmp"
