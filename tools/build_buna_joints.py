"""Derive V2 connected components from the accepted whole Buna and native joints."""
from pathlib import Path
from collections import deque
import json
from PIL import Image,ImageFilter
root=Path(__file__).resolve().parents[1];assets=root/'Assets/Grove';art=root/'tools/art/buna-classic'
stem=Image.open(assets/'BunaClassic.png').convert('RGBA')
whole=Image.open(assets/'Buna.png').convert('RGBA')
# The accepted stem is authoritative: a common contour and cross-section.
profile=[stem.getpixel((x,0)) for x in range(2,18,2)]
colors=sorted({p[:3] for p in stem.get_flattened_data() if p[3]},key=sum)

def iswood(p):
 r,g,b,a=p
 return a and max(r,g,b)-min(r,g,b)<65 and r>=g*.85 and g<=r*1.15

def runs(xs):
 result=[]
 for x in xs:
  if not result or x!=result[-1][-1]+1:result.append([x])
  else:result[-1].append(x)
 return result

def shade(mask):
 out=Image.new('RGBA',mask.size)
 for y in range(mask.height):
  for run in runs([x for x in range(mask.width) if mask.getpixel((x,y))]):
   for n,x in enumerate(run):
    # Narrow limbs keep a light core; wide limbs inherit the stem profile.
    if len(run)<4:color=colors[min(len(colors)-1,2 if n==0 else 1)]
    else:color=profile[round(n/(len(run)-1)*7)][:3]
    out.putpixel((x,y),(*color,255))
 return out

# Master crown: the already accepted whole drawing, not an independently drawn Y.
source=whole.crop((0,0,288,320)).resize((64,56),Image.Resampling.NEAREST)
wood=Image.new('L',(68,56));leaves=Image.new('RGBA',(68,56))
for y in range(56):
 for x in range(64):
  p=source.getpixel((x,y))
  if iswood(p):wood.putpixel((x+4,y),255)
  elif p[3]:leaves.putpixel((x+4,y),p)
# V1's root axis becomes x34 logical after left padding. Widen its own mask
# gently only near the bottom, into the native stem's eight logical pixels.
for y in range(50,56):
 near=[x for x in range(25,43) if wood.getpixel((x,y))]
 for x in near:wood.putpixel((x,y),0)
 width=6 if y<52 else 8
 for x in range(34-width//2,34+width//2):wood.putpixel((x,y),255)
# Complete wood hidden by foliage before extracting components. Paths remain
# within the accepted whole silhouette; the leaf layer hides these inner arms.
allowed={(x,y) for y in range(56) for x in range(68) if wood.getpixel((x,y)) or leaves.getpixel((x,y))[3]}
def flood(points,seeds):
 q=deque(seeds);seen=set()
 while q:
  p=q.popleft()
  if p in seen or p not in points:continue
  seen.add(p);x,y=p;q.extend(((x-1,y),(x+1,y),(x,y-1),(x,y+1)))
 return seen
while True:
 pixels={(x,y) for y in range(56) for x in range(68) if wood.getpixel((x,y))}
 joined=flood(pixels,[(34,55)]);missing=pixels-joined
 if not missing:break
 q=deque(joined);previous={p:None for p in joined};found=None
 while q and found is None:
  x,y=q.popleft()
  for p in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
   if p not in allowed or p in previous:continue
   previous[p]=(x,y)
   if p in missing:found=p;break
   q.append(p)
 if found is None:
  # Tiny isolated color flecks have no anatomical connection in the source.
  for x,y in missing:wood.putpixel((x,y),0)
  break
 while previous[found] is not None:
  wood.putpixel(found,255);found=previous[found]
assert flood({(x,y) for y in range(56) for x in range(68) if wood.getpixel((x,y))},[(34,55)])=={(x,y) for y in range(56) for x in range(68) if wood.getpixel((x,y))}, 'broken bare crown'
crownwood=shade(wood);crownwood.paste(Image.frombytes('RGBA',(8,1),bytes(c for p in profile for c in p)),(30,55))
crown=crownwood.copy();crown.alpha_composite(leaves)
# The socket is part of this common master before splitting into game sprites.
crownwood=crownwood.resize((136,112),Image.Resampling.NEAREST)
crown=crown.resize((136,112),Image.Resampling.NEAREST)
out=Image.new('RGBA',(138,114));out.paste(crown,(0,0));out.save(assets/'BunaClassic_Tops.png')

# Small foliage cut from the SAME accepted whole image, with wood excluded.
leaf=whole.crop((0,0,288,304)).copy()
leaf.putdata([p if not iswood(p) else (0,0,0,0) for p in leaf.get_flattened_data()])
leaf=leaf.crop(leaf.getbbox())
layout=Image.open(art/'joint-layout.png').convert('RGBA')
branches=Image.new('RGBA',(84,126));bare=Image.new('RGBA',(84,126))

def components(points):
 points=set(points);result=[]
 while points:
  q=deque([points.pop()]);part=set(q)
  while q:
   x,y=q.popleft()
   for p in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
    if p in points:points.remove(p);part.add(p);q.append(p)
  result.append(part)
 return result

for style in range(3):
 for side in range(2):
  logical=layout.crop((side*42,style*42,side*42+40,style*42+40)).resize((20,20),Image.Resampling.NEAREST)
  core=Image.new('L',(20,20));core.putdata([255 if p==(255,255,255,255) else 0 for p in logical.get_flattened_data()])
  near=core.filter(ImageFilter.MaxFilter(3))
  arm=Image.new('RGBA',(20,20))
  for y in range(20):
   for x in range(20):
    p=logical.getpixel((x,y))
    if core.getpixel((x,y)):arm.putpixel((x,y),(*colors[-2],255))
    elif p==(0,0,0,255) and near.getpixel((x,y)):arm.putpixel((x,y),(*colors[0],255))
  arm=arm.resize((40,40),Image.Resampling.NEAREST)
  # Include the original native child arm from the unchanged trunk atlas.
  # Overlapping pixels therefore match the stem's own actual frame exactly.
  child=stem.crop(((44 if side==0 else 66),198+style*22,(64 if side==0 else 86),218+style*22))
  arm.alpha_composite(child,(22 if side==0 else -2,12))
  arm_pixels={(x,y) for y in range(40) for x in range(40) if arm.getpixel((x,y))[3]}
  arm_roots=[p for p in arm_pixels if (p[0]>=38 if side==0 else p[0]<2)]
  assert flood(arm_pixels,arm_roots)==arm_pixels,('broken bare arm',style,side)
  bare.paste(arm,(side*42,style*42))
  result=arm.copy()
  spots=components((x,y) for y in range(20) for x in range(20) if logical.getpixel((x,y))==(204,204,204,255))
  for spot in spots:
   x0=min(x for x,y in spot);x1=max(x for x,y in spot);y0=min(y for x,y in spot);y1=max(y for x,y in spot)
   w=max(8,min(14,x1-x0+5));h=max(6,min(11,y1-y0+2))
   tile=leaf.resize((w,h),Image.Resampling.BOX)
   tile.putalpha(tile.getchannel('A').point(lambda a:255 if a>=128 else 0))
   # Preserve the shared 2px grid when positioning the leaf masses.
   result.alpha_composite(tile.resize((w*2,h*2),Image.Resampling.NEAREST),((x0+x1+1-w)//2*2,(y0+y1+1-h)//2*2))
  # Crop this bough out of the shared node AFTER composing its native child:
  # the interface pixels are exactly the same ones the stem renderer owns.
  result.alpha_composite(child,(22 if side==0 else -2,12))
  for y in range(40):
   for x in range(36 if side==0 else 0,40 if side==0 else 4):
    if arm.getpixel((x,y))[3]:result.putpixel((x,y),arm.getpixel((x,y)))
  branches.paste(result,(side*42,style*42))
branches.save(assets/'BunaClassic_Branches.png')
# Wood-first diagnostic: the crown and all six arms meet full stem sections.
plate=Image.new('RGBA',(616,288),'#6588ac')
plate.alpha_composite(crownwood,(24,16))
for row in range(4):plate.alpha_composite(stem.crop((2,(row%3)*22,18,(row%3)*22+16)),(84,128+row*16))
for style in range(3):
 for side in range(2):
  node=Image.new('RGBA',(112,104));px=48
  parent=stem.crop((110 if side==0 else 66,66+style*22,130 if side==0 else 86,86+style*22))
  node.alpha_composite(stem.crop((0,0,20,20)),(px-2,24))
  node.alpha_composite(parent,(px-2,40))
  node.alpha_composite(stem.crop((0,22,20,42)),(px-2,56))
  node.alpha_composite(bare.crop((side*42,style*42,side*42+40,style*42+40)),(px-40 if side==0 else px+16,28))
  plate.alpha_composite(node,(224+style*128,side*128+8))
plate.resize((1232,576),Image.Resampling.NEAREST).save(art/'wood-joints.png')
metadata=json.loads((art/'layout.json').read_text());metadata['crown']=[136,112];metadata['crownAnchor']=[68,112];metadata['branchStyles']=['native0','native1','native2'];(art/'layout.json').write_text(json.dumps(metadata,indent=2)+'\n')
print('Wood-first Buna II: shared stem nodes, three directional arms, crown cut from accepted whole tree')
