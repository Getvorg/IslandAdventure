"""Check modular Buna V2 frames/contacts and render both approved variants."""
from pathlib import Path
from collections import deque
import json,hashlib
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[1];assets=ROOT/'Assets/Grove'
v1=Image.open(assets/'Buna.png').convert('RGBA')
assert hashlib.sha256(Image.open(assets/'Buna.png').convert('RGBA').tobytes()).hexdigest()=='d29781087e19cc8bba08179c8cb7df4f0a43605d146d0e9cd225ac37b2951f9d', 'approved V1 pixels changed'
assert hashlib.sha256(Image.open(assets/'BunaStump.png').convert('RGBA').tobytes()).hexdigest()=='a2a2ab633192a8032d09d37c9fbede82cf9cb3f667806a17b39ce72c45835760', 'approved V1 pixels changed'
parts={n:Image.open(assets/(n+'.png')).convert('RGBA') for n in ['BunaClassic','BunaClassic_Tops','BunaClassic_Branches']}
for n,size in [('BunaClassic',(176,264)),('BunaClassic_Tops',(130,114)),('BunaClassic_Branches',(84,126))]:
 im=parts[n];assert im.size==size
 assert set(im.getchannel('A').get_flattened_data())=={0,255}
 for y in range(0,im.height,2):
  for x in range(0,im.width,2):
   p=im.getpixel((x,y));assert all(im.getpixel((x+dx,y+dy))==p for dx in (0,1) for dy in (0,1)),(n,x,y)
layout=Image.open(ROOT/'tools/art/buna-classic/frame-layout.png').convert('RGBA');stem=parts['BunaClassic']
for row in range(12):
 for col in range(8):
  for y in range(20):
   for x in range(20):
    p=layout.getpixel((col*22+x,row*22+y));expected=0 if p[:3]==(247,119,249) else p[3]
    assert stem.getpixel((col*22+x,row*22+y))[3]==expected
for fy in (0,22,44):
 xs=[x for x in range(20) if stem.getpixel((x,fy+8))[3]]
 assert min(xs)==2 and max(xs)==17 and len(xs)==16,'stem is not one block wide'

def connected(im,seed):
 q=deque([seed]);seen=set()
 while q:
  x,y=q.popleft()
  if (x,y) in seen or not (0<=x<im.width and 0<=y<im.height) or not im.getpixel((x,y))[3]:continue
  seen.add((x,y));q.extend(((x-1,y),(x+1,y),(x,y-1),(x,y+1)))
 return seen

for side in range(2):
 im=parts['BunaClassic_Branches'].crop((side*42,0,side*42+40,40))
 pixels={(x,y) for y in range(40) for x in range(40) if im.getpixel((x,y))[3]}
 edge=[p for p in pixels if (p[0]>=38 if side==0 else p[0]<2)]
 assert pixels<=connected(im,edge[0]),'detached bough pixels'
# The short collar joins exactly; the fork above keeps its organic shape.
crown=parts['BunaClassic_Tops']
for y in range(104,112):
 assert all(crown.getpixel((x,y))[3]==255 for x in range(56,72)), 'narrow crown neck'
for fy in (0,22,44):
 assert [crown.getpixel((x,111)) for x in range(56,72)]==[stem.getpixel((x,fy)) for x in range(2,18)],'stem/crown edge palette mismatch'
fixture=json.loads((ROOT/'tools/art/buna-classic/native-fixture.json').read_text());floor=fixture['floor']
height=max(448,max((floor-t['y'])*16+112 for t in fixture['tiles']))+48
baseline=height-24;sheet=Image.new('RGBA',(736,height),'#6588ac')
sheet.alpha_composite(v1,(16,baseline-448))
for root,anchor in [(43,448),(164,640)]:
 tiles=[t for t in fixture['tiles'] if t['root']==root]
 assert sum(t['fx']==22 and t['fy']>=198 for t in tiles)==1,'living crown marker missing'
 tree=Image.new('RGBA',(192,height));center=96
 for t in tiles:
  x=center+(t['x']-root)*16;y=baseline+(t['y']-floor)*16
  tree.alpha_composite(stem.crop((t['fx'],t['fy'],t['fx']+20,t['fy']+20)),(x-2,y))
 for t in tiles:
  fx,fy=t['fx'],t['fy'];x=center+(t['x']-root)*16;y=baseline+(t['y']-floor)*16
  if fy<198:continue
  if fx==22:tree.alpha_composite(parts['BunaClassic_Tops'].crop((0,0,128,112)),(x+8-64,y+16-112))
  elif fx in (44,66):
   style=min(2,(fy-198)//22);sx=0 if fx==44 else 42
   branch=parts['BunaClassic_Branches'].crop((sx,style*42,sx+40,style*42+40))
   tree.alpha_composite(branch,(x-24 if fx==44 else x,y-12))
 opaque={(x,y) for y in range(height) for x in range(192) if tree.getpixel((x,y))[3]}
 joined=connected(tree,(center+8,baseline-8))
 assert len(joined)>=len(opaque)*.995,('detached native crown/bough',root,len(joined),len(opaque))
 sheet.alpha_composite(tree,(anchor-center,0))
draw=ImageDraw.Draw(sheet);draw.rectangle((0,baseline,735,height-1),fill='#6b513a')
draw.text((88,baseline+6),'Buna I',fill='white');draw.text((420,baseline+6),'Buna II',fill='white');draw.text((596,baseline+6),'Buna II tall',fill='white')
sheet.save(ROOT/'tools/art/buna-classic/comparison.png')
# Magnify the actual fork/neck/body contact, without redrawing its pixels.
join=Image.new('RGBA',(64,64),'#6588ac')
join.alpha_composite(parts['BunaClassic_Tops'].crop((32,72,96,112)),(0,0))
join.alpha_composite(stem.crop((2,22,18,42)),(24,40))
join.resize((256,256),Image.Resampling.NEAREST).save(ROOT/'tools/art/buna-classic/neck-closeup.png')

print('V2 native alpha, 16px width, 2x2 grid, branch sockets and actual tree contacts passed; comparison rendered')
