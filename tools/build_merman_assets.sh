#!/usr/bin/env bash
# Convert the generated art sheet into aligned game frames; ImageMagick 7 required.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
merman_tmp="$(mktemp -d)"
trap 'rm -rf "$merman_tmp"' EXIT
for variant in blue red; do
    column=0
    name=Merman
    if [[ "$variant" == red ]]; then column=512; name=Merman_Red; fi
    # Remove faint alpha halos and use one palette across every animation frame.
    magick "$root/tools/art/merman-source.png" -crop "512x1536+${column}+0" +repage \
        -channel A -threshold 85% +channel +dither -colors 48 "$merman_tmp/column.png"
    for frame in 0 1 2 3 4 5; do
        row=$((frame * 256))
        magick "$merman_tmp/column.png" -crop "480x256+16+${row}" +repage \
            -filter point -resize 52x52 -background none -gravity center -extent 56x56 \
            -channel A -threshold 50% +channel -scale 200% "$merman_tmp/frame-${frame}.png"
    done
    magick "$merman_tmp"/frame-*.png -append "$root/Assets/NPCs/${name}.png"
done
