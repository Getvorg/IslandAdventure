"""Verify the whole Buna drawing and, optionally, loaded-game strip coordinates."""
from pathlib import Path
from collections import deque
import json,sys
from PIL import Image,ImageOps
root=Path(__file__).resolve().parents[1]
image=Image.open(root/'Assets/Grove/Buna.png').convert('RGBA')
layout=json.loads((root/'tools/art/buna/layout.json').read_text())
assert image.size==(layout['width'],layout['height'])==(288,448)
assert layout['anchorX']==136 and layout['tileHeight']==28
for name in ('Buna','BunaStump'):
 im=Image.open(root/f'Assets/Grove/{name}.png').convert('RGBA')
 assert set(im.getchannel('A').get_flattened_data())<={0,255}
 for y in range(0,im.height,2):
  for x in range(0,im.width,2):
   p=im.getpixel((x,y));assert all(im.getpixel((x+dx,y+dy))==p for dx in (0,1) for dy in (0,1)),(name,x,y)
opaque={(x,y) for y in range(448) for x in range(288) if image.getpixel((x,y))[3]}
q=deque([(136,440)]);seen=set()
while q:
 p=q.popleft()
 if p in seen or p not in opaque:continue
 seen.add(p);x,y=p;q.extend(((x-1,y),(x+1,y),(x,y-1),(x,y+1)))
assert seen==opaque,'detached foliage or root fragment in original tree'
# A band offset may change by at most one logical pixel between neighbors.
for drive in (-1,-.5,0,.5,1):
 shifts=[round(2*max(0,min(1,(444-y)/444))**2*drive)*2 for y in range(0,448,16)]
 assert shifts[-1]==0 and max(abs(a-b) for a,b in zip(shifts,shifts[1:]))<=2
if len(sys.argv)>1:
 rows=json.loads(Path(sys.argv[1]).read_text())
 for x in (17,43,164):
  samples=sorted((row for row in rows if row['x']==x),key=lambda row:row['y'])
  assert len(samples)==28
  rebuilt=Image.new('RGBA',image.size)
  for n,row in enumerate(samples):
   assert row['sourceY']==n*16 and row['mirror']==(x==43)
   part=image.crop((0,row['sourceY'],288,row['sourceY']+16))
   rebuilt.paste(ImageOps.mirror(part) if row['mirror'] else part,(0,n*16))
  expected=ImageOps.mirror(image) if x==43 else image
  assert rebuilt.tobytes()==expected.tobytes(),'game strip mapping introduces a seam'
print('Buna connected silhouette, exact 2x2/binary alpha and wind continuity passed'+('; loaded native-strip mapping passed' if len(sys.argv)>1 else ''))
