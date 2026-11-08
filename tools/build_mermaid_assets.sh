#!/usr/bin/env bash
# Convert the generated art sheet into aligned game frames; ImageMagick 7 required.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
mermaid_tmp="$(mktemp -d)"
trap 'rm -rf "$mermaid_tmp"' EXIT
for variant in blue red; do
    column=0
    name=Mermaid
    if [[ "$variant" == red ]]; then column=512; name=Mermaid_Red; fi
    # Remove faint alpha halos and use one palette across every animation frame.
    magick "$root/tools/art/mermaid-source.png" -crop "512x1536+${column}+0" +repage \
        -channel A -threshold 85% +channel +dither -colors 48 "$mermaid_tmp/column.png"
    for frame in 0 1 2 3 4 5; do
        row=$((frame * 256))
        magick "$mermaid_tmp/column.png" -crop "416x256+32+${row}" +repage \
            -filter point -resize 44x44 -background none -gravity center -extent 48x48 \
            -channel A -threshold 50% +channel -scale 200% "$mermaid_tmp/frame-${frame}.png"
    done
    magick "$mermaid_tmp"/frame-*.png -append "$root/Assets/NPCs/${name}.png"
done
