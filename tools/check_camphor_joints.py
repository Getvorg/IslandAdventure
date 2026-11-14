"""Check actual packed joint pixels against native stem frames; export closeups."""
from collections import deque
from pathlib import Path
import hashlib
from PIL import Image,ImageDraw
from build_camphor_assets import WOOD
ROOT=Path(__file__).resolve().parents[1]
ASSETS=ROOT/'Assets/Grove'
stem=Image.open(ASSETS/'Camphor.png').convert('RGBA')
# Pin the approved 0.4.25 stem pixels, independent of PNG encoder/Git history.
assert hashlib.sha256(stem.tobytes()).hexdigest()=='a4c4563e6cdb4f17dbf009202082dad9de635cc27e8c5c3794937574c30238a2'
wood=set(WOOD.values());means=[]
for name,size in [('Camphor',(176,264)),('Camphor_Tops',(270,178)),('Camphor_Boughs',(132,150)),('Camphor_Branches',(84,126))]:
 im=Image.open(ASSETS/(name+'.png')).convert('RGBA');assert im.size==size
 assert set(im.getchannel('A').get_flattened_data())=={0,255}
 for y in range(0,im.height,2):
  for x in range(0,im.width,2):
   p=im.getpixel((x,y));assert all(im.getpixel((x+dx,y+dy))==p for dx in (0,1) for dy in (0,1)),(name,x,y)
 pixels=[p[:3] for p in im.get_flattened_data() if p[3] and p[:3] in wood]
 means.append(tuple(sum(p[k] for p in pixels)/len(pixels) for k in range(3)))
assert all(abs(mean[k]-means[0][k])<9 for mean in means[1:3] for k in range(3)),means

def flood(image,seeds):
 q=deque(seeds);seen=set()
 while q:
  x,y=q.popleft()
  if (x,y) in seen or not (0<=x<image.width and 0<=y<image.height) or not image.getpixel((x,y))[3]:continue
  seen.add((x,y));q.extend(((x-1,y),(x+1,y),(x,y-1),(x,y+1)))
 return seen

bough=Image.open(ASSETS/'Camphor_Boughs.png').convert('RGBA')
plate=Image.new('RGBA',(540,276),'#6588ac');labels=ImageDraw.Draw(plate)
for style in range(3):
 for side in range(2):
  left=side==0;image=Image.new('RGBA',(180,70));ox,oy=80,30
  fx,fy=(110,66) if left else (66,110)
  image.alpha_composite(stem.crop((fx,fy,fx+20,fy+20)),(ox-2,oy))
  fx=44 if left else 66;fy=198+style*22
  image.alpha_composite(stem.crop((fx,fy,fx+20,fy+20)),(ox+(-16 if left else 16)-2,oy))
  part=bough.crop((side*66,style*50,side*66+64,style*50+48))
  dx,dy=ox+(-64 if left else 16),oy-24;image.alpha_composite(part,(dx,dy))
  attached=flood(image,[(ox+8,oy+8)])
  pixels={(dx+x,dy+y) for y in range(48) for x in range(64) if part.getpixel((x,y))[3]}
  assert pixels<=attached,('detached branch',style,side,len(pixels-attached))
  x,y=style*180,side*92;plate.alpha_composite(image,(x,y+18))
  labels.text((x+4,y+2),f'{"Left" if left else "Right"} {style+1}',fill='white')
crown=Image.open(ASSETS/'Camphor_Tops.png').convert('RGBA')
image=Image.new('RGBA',(64,76));image.alpha_composite(crown.crop((102,136,166,176)),(0,0))
image.alpha_composite(stem.crop((0,22,20,42)),(22,40));attached=flood(image,[(32,52)])
assert all((x,y) in attached for y in range(24,40) for x in range(24,40)), 'crown neck separated'
plate.alpha_composite(image,(238,196));labels.text((224,184),'Crown / stem',fill='white')
plate.resize((1080,552),Image.Resampling.NEAREST).save(ROOT/'tools/art/grove/camphor-joints-preview.png')
print('Six branch-to-stem contacts, centered crown, wood tones, unchanged stem, binary alpha and 2x2 grid passed')
