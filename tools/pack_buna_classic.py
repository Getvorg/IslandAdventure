"""Pack native frame geometry with V1 beech material and new modular foliage."""
from pathlib import Path
from collections import Counter
import sys,json
from PIL import Image
root=Path(__file__).resolve().parents[1];assets=root/'Assets/Grove';tmp=Path(sys.argv[1])
layout=Image.open(root/'tools/art/buna-classic/frame-layout.png').convert('RGBA')
bark=Image.open(tmp/'bark.png').convert('RGBA')
palette=sorted({p[:3] for p in bark.get_flattened_data()},key=sum)
# A common edge band joins any random native body variant without horizontal seams.
calm=[bark.getpixel((x,32)) for x in range(20)]
atlas=Image.new('RGBA',(176,264))
for row in range(12):
 for col in range(8):
  shape=layout.crop((col*22,row*22,col*22+20,row*22+20))
  mask=shape.getchannel('A')
  mask.putdata([0 if p[:3]==(247,119,249) else p[3] for p in shape.get_flattened_data()])
  sy=((row*7+col*11)%22)*2
  material=bark.crop((0,sy,20,sy+20))
  for y in (0,1,18,19):
   for x in range(20):material.putpixel((x,y),calm[x])
  pixels=list(material.get_flattened_data())
  pixels=[(*palette[0],p[3]) if q==(0,0,0,255) else p for p,q in zip(pixels,shape.get_flattened_data())]
  material.putdata(pixels)
  material.putalpha(mask);atlas.paste(material,(col*22,row*22))
atlas.save(assets/'BunaClassic.png')

def grey(p):
 r,g,b,a=p
 return a and max(r,g,b)-min(r,g,b)<65 and r>=g*.85 and g<=r*1.15

def match(image):
 colors=Counter(p for p in image.get_flattened_data() if grey(p));total=sum(colors.values());cumulative=0;mapping={}
 for p in sorted(colors,key=lambda p:sum(p[:3])):
  rank=min(len(palette)-1,int((cumulative+colors[p]/2)/total*len(palette)))
  mapping[p]=(*palette[rank],p[3]);cumulative+=colors[p]
 image.putdata([mapping.get(p,p) for p in image.get_flattened_data()]);return image

crown=match(Image.open(assets/'BunaClassic_Tops.png').convert('RGBA'))
# Shape only the last part of the new organic neck, using its own wood pixels.
# No rectangular trunk texture is pasted through the fork.
logical=crown.resize((64,56),Image.Resampling.NEAREST)
for y in range(46,56):
 xs=[x for x in range(20,44) if logical.getpixel((x,y))[3] and logical.getpixel((x,y))[:3] in palette]
 runs=[]
 for x in xs:
  if not runs or x!=runs[-1][-1]+1:runs.append([x])
  else:runs[-1].append(x)
 run=min(runs,key=lambda r:abs((r[0]+r[-1]+1)/2-32))
 start,end=run[0],run[-1]+1
 width=max(end-start,6) if y<52 else 8
 left=32-width//2
 wood=logical.crop((start,y,end,y+1)).resize((width,1),Image.Resampling.NEAREST)
 for x in run:logical.putpixel((x,y),(0,0,0,0))
 logical.paste(wood,(left,y))
crown=logical.resize((128,112),Image.Resampling.NEAREST)
# Only the final logical row matches the stem edge exactly.
crown.paste(atlas.crop((2,0,18,2)),(56,110))
out=Image.new('RGBA',(130,114));out.paste(crown,(0,0));out.save(assets/'BunaClassic_Tops.png')
branches=Image.new('RGBA',(84,126))
for side in range(2):
 image=match(Image.open(tmp/('left.png' if side==0 else 'right.png')).convert('RGBA'))
 # Locate the generated wood at the attachment edge, then align it to the
 # vanilla left/right root pivots (40,24) and (0,30), on the 2px grid.
 points=[(x,y) for y in range(40) for x in range(40) if image.getpixel((x,y))[3] and image.getpixel((x,y))[:3] in palette]
 edge=max(x for x,y in points) if side==0 else min(x for x,y in points)
 ys=[y for x,y in points if abs(x-edge)<=1]
 center=round((min(ys)+max(ys)+1)/4)*2
 dx=(39-edge) if side==0 else -edge;dx=round(dx/2)*2
 dy=(24 if side==0 else 30)-center
 aligned=Image.new('RGBA',(40,40));aligned.alpha_composite(image,(dx,dy))
 # Pack the aligned directional bough into all three native style slots.
 for style in range(3):branches.paste(aligned,(side*42,style*42))
branches.save(assets/'BunaClassic_Branches.png')
metadata={'crown':[128,112],'branch':[40,40],'trunk':[176,264],'palette':palette}
(root/'tools/art/buna-classic/layout.json').write_text(json.dumps(metadata,indent=2)+'\n')
print('Buna V2 packed: 16px native stem, 128x112 crown, separate 40x40 branches')
