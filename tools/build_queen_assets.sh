#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
queen_tmp="$(mktemp -d)"; trap 'rm -rf "$queen_tmp"' EXIT
# Normalize an atlas while keeping the same crop and anchor for every frame.
frames() {
    local source="$1" cols="$2" rows="$3" logical_w="$4" logical_h="$5" inset="$6" target="$7"
    local width height x0 x1 y0 y1 frame=0
    width="$(magick identify -format '%w' "$source")";height="$(magick identify -format '%h' "$source")"
    magick "$source" -channel A -threshold 85% +channel +dither -colors 96 "$queen_tmp/source.png"
    for ((row=0; row<rows; row++)); do
        for ((col=0; col<cols; col++)); do
            x0=$((col * width / cols));x1=$(((col+1) * width / cols))
            y0=$((row * height / rows));y1=$(((row+1) * height / rows))
            magick "$queen_tmp/source.png" -crop "$((x1-x0))x$((y1-y0))+${x0}+${y0}" +repage \
                -filter point -resize "$((logical_w-inset))x$((logical_h-inset))" \
                -background none -gravity center -extent "${logical_w}x${logical_h}" \
                -channel A -threshold 50% +channel -scale 200% -depth 8 \
                PNG32:"$queen_tmp/frame-$(printf '%02d' "$frame").png"
            frame=$((frame+1))
        done
    done
    magick "$queen_tmp"/frame-*.png -append -depth 8 PNG32:"$target"
    rm "$queen_tmp"/frame-*.png
}
frames "$root/tools/art/queen-source.png" 3 4 112 112 10 "$root/Assets/NPCs/Bosses/MermaidQueen.png"
frames "$root/tools/art/moray-source.png" 2 3 72 40 6 "$root/Assets/NPCs/RoyalMoray.png"
# Boss portrait from the first frame, with a transparent 2x pixel grid.
magick "$root/Assets/NPCs/Bosses/MermaidQueen.png" -crop 96x96+106+0 +repage \
    -filter point -resize 18x18 -background none -gravity center -extent 20x20 \
    -scale 200% -depth 8 PNG32:"$root/Assets/NPCs/Bosses/MermaidQueen_Head_Boss.png"
width="$(magick identify -format '%w' "$root/tools/art/royal-effects-source.png")"
height="$(magick identify -format '%h' "$root/tools/art/royal-effects-source.png")"
half=$((width/2))
for part in wave conch; do
    x=0;size=64;target="$root/Assets/Projectiles/RoyalWave.png"
    if [[ "$part" == conch ]];then x="$half";size=24;target="$root/Assets/Items/RoyalConch.png";fi
    magick "$root/tools/art/royal-effects-source.png" -crop "${half}x${height}+${x}+0" +repage \
        -channel A -threshold 85% +channel +dither -colors 64 \
        -filter point -resize "$((size-4))x$((size-4))" -background none -gravity center -extent "${size}x${size}" \
        -channel A -threshold 50% +channel -scale 200% -depth 8 PNG32:"$target"
done
