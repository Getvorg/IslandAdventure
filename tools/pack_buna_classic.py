"""Pack native frame geometry with V1 beech material and new modular foliage."""
from pathlib import Path
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

metadata={'crown':[136,112],'branch':[40,40],'trunk':[176,264],'palette':palette}
(root/'tools/art/buna-classic/layout.json').write_text(json.dumps(metadata,indent=2)+'\n')
print('Unchanged Buna II stem packed; separate joint builder supplies connected components')
