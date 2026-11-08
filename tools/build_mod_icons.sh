#!/usr/bin/env bash
# Original generated artwork -> native tModLoader and Workshop icon sizes.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
icon_tmp="$(mktemp -d)"
trap 'rm -rf "$icon_tmp"' EXIT
width="$(magick identify -format '%w' "$root/tools/art/mod-icon-source.png")"
height="$(magick identify -format '%h' "$root/tools/art/mod-icon-source.png")"
center_x=$((width / 2)); center_y=$((height / 2)); top_y=$((height * 95 / 1000))
# Keep the medallion interior solid navy; only its exterior is transparent.
magick -size "${width}x${height}" xc:none -fill '#07182f' \
    -draw "circle ${center_x},${center_y} ${center_x},${top_y}" \
    "$root/tools/art/mod-icon-source.png" -compose over -composite \
    -channel A -threshold 85% +channel +dither -colors 48 "$icon_tmp/source.png"
magick "$icon_tmp/source.png" -filter point -resize 40x40 -channel A -threshold 50% +channel -scale 200% -depth 8 PNG32:"$root/icon.png"
magick "$icon_tmp/source.png" -filter point -resize 15x15 -channel A -threshold 50% +channel -scale 200% -depth 8 PNG32:"$root/icon_small.png"
magick "$root/icon.png" -filter point -resize 480x480 -depth 8 PNG32:"$root/icon_workshop.png"
