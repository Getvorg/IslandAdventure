#!/usr/bin/env bash
# New whole-tree artwork -> exact game pixels; no previous tree assets are used.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
buna_tmp="$(mktemp -d)";trap 'rm -rf "$buna_tmp"' EXIT
magick "$root/tools/art/buna/source.png" -channel A -threshold 85% +channel -trim +repage \
 -filter box -resize 144x224! -channel A -threshold 50% +channel +dither -colors 40 \
 -scale 200% -depth 8 PNG32:"$root/Assets/Grove/Buna.png"
python3 - "$root" <<'PY'
from pathlib import Path
from PIL import Image
import json,sys
root=Path(sys.argv[1]);image=Image.open(root/'Assets/Grove/Buna.png').convert('RGBA')
y=384;xs=[x for x in range(image.width) if image.getpixel((x,y))[3]]
anchor=round(((min(xs)+max(xs)+1)/2)/2)*2
# Small bark sample for the exposed stump after native chopping, never foliage.
image.crop((anchor-8,336,anchor+8,400)).save(root/'Assets/Grove/BunaStump.png')
metadata={'width':288,'height':448,'anchorX':anchor,'tileHeight':28,'stumpWidth':16,'stumpHeight':64}
(root/'tools/art/buna/layout.json').write_text(json.dumps(metadata,indent=2)+'\n')
for name in ('Buna','BunaStump'):
 im=Image.open(root/f'Assets/Grove/{name}.png').convert('RGBA')
 assert set(im.getchannel('A').get_flattened_data())=={0,255} if name=='Buna' else set(im.getchannel('A').get_flattened_data())<={0,255}
 for y in range(0,im.height,2):
  for x in range(0,im.width,2):
   p=im.getpixel((x,y));assert all(im.getpixel((x+dx,y+dy))==p for dx in (0,1) for dy in (0,1))
print('Buna: original whole sprite 288x448, anchor',anchor,'binary alpha and 2x2 grid passed')
PY
