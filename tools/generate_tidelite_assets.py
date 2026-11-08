"""Deterministic, original 2x pixel sprites. Run with Python + Pillow from any cwd."""
from pathlib import Path
import random
from PIL import Image, ImageDraw
ROOT = Path(__file__).resolve().parents[1] / 'Assets'
INK='#172c37'; DARK='#263d49'; ROCK='#415663'; MID='#596c76'; PALE='#829196'
DEEP='#166775'; TEAL='#299b9b'; SEA='#65d4bd'; GLINT='#c2f6d0'; GOLD='#b89662'

def save(im, path):
    target=ROOT/path
    target.parent.mkdir(parents=True,exist_ok=True)
    im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST).save(target)

def canvas(w,h):
    im=Image.new('RGBA',(w,h));return im,ImageDraw.Draw(im)

im,d=canvas(16,16)
d.polygon([(1,9),(3,4),(7,2),(10,3),(14,7),(14,12),(10,14),(4,13)],fill=INK)
d.polygon([(2,9),(4,5),(7,3),(10,4),(13,8),(12,12),(8,13),(4,12)],fill=ROCK)
d.polygon([(4,5),(7,3),(9,5),(6,7),(3,8)],fill=MID)
d.line([(5,11),(6,8),(9,7),(10,4)],fill=DEEP,width=3)
d.line([(5,11),(6,8),(9,7),(10,4)],fill=TEAL,width=2)
d.line([(6,10),(7,8),(9,8),(11,5)],fill=SEA,width=1)
d.point((8,8),GLINT);d.line([(10,11),(12,9)],fill=SEA)
d.polygon([(0,13),(2,11),(4,12),(5,15),(1,15)],fill=DARK);d.point((2,13),SEA)
save(im,Path('Items/TideliteOre.png'))

im,d=canvas(16,12)
d.polygon([(1,6),(4,2),(12,2),(15,5),(13,10),(3,10)],fill=INK)
d.polygon([(2,6),(5,3),(11,3),(14,5),(11,7),(4,7)],fill=SEA)
d.polygon([(3,7),(11,8),(13,6),(12,9),(4,9)],fill=DEEP)
d.line([(5,3),(11,3),(13,5)],fill=GLINT);d.line([(4,7),(11,7)],fill=TEAL)
d.line([(8,4),(10,4)],fill=TEAL)
save(im,Path('Items/TideliteBar.png'))

for name,size,darker in [('TideliteBrick',16,False),('TideliteBrickWall',14,True)]:
 im,d=canvas(size,size)
 d.rectangle((1,2,size-2,size-2),fill=INK)
 for y in range(3,size-2,5):
  d.rectangle((2,y,size-3,min(y+3,size-3)),fill=DARK if darker else ROCK)
  d.line([(2,y),(size-3,y)],fill=ROCK if darker else MID)
  split=5 if y%2 else 9
  d.line([(split,y),(split,y+3)],fill=INK)
  d.line([(3,y+2),(size-4,y+2)],fill=DEEP if darker else TEAL)
 save(im,Path('Items/'+name+'.png'))

for name,size in [('Sword',20),('Pickaxe',16),('Axe',18),('Hammer',18)]:
 im,d=canvas(size,size)
 # Handle leans from the bottom-left into the upper-right head.
 d.line([(3,size-4),(size-6,5)],fill=INK,width=4)
 d.line([(3,size-4),(size-6,5)],fill='#76563d',width=2)
 d.line([(4,size-5),(size-6,5)],fill=GOLD)
 d.rectangle((1,size-4,4,size-2),fill=INK);d.point((3,size-3),GOLD)
 if name=='Sword':
  d.polygon([(6,12),(14,2),(18,0),(18,4),(10,14)],fill=INK)
  d.polygon([(7,12),(15,3),(17,2),(16,5),(10,12)],fill=TEAL)
  d.line([(8,11),(16,3)],fill=SEA);d.line([(10,9),(17,2)],fill=GLINT)
  d.line([(5,10),(11,16)],fill=INK,width=3);d.line([(5,10),(11,16)],fill=GOLD)
 elif name=='Pickaxe':
  d.polygon([(2,4),(5,1),(10,1),(14,4),(15,9),(12,6),(8,4),(4,5)],fill=INK)
  d.line([(3,4),(6,2),(10,2),(13,4),(14,7)],fill=SEA,width=2)
  d.line([(4,3),(9,2),(12,3)],fill=GLINT)
 elif name=='Axe':
  d.polygon([(9,1),(15,1),(17,4),(16,9),(12,11),(9,7),(7,5)],fill=INK)
  d.polygon([(10,2),(14,2),(16,4),(15,8),(12,9),(10,6),(9,5)],fill=TEAL)
  d.line([(14,2),(16,4),(15,8),(12,9)],fill=GLINT,width=1)
  d.line([(10,3),(13,4),(12,7)],fill=SEA)
 else:
  d.polygon([(6,2),(12,0),(17,5),(12,10),(5,5)],fill=INK)
  d.polygon([(7,3),(12,1),(16,5),(12,8),(6,5)],fill=TEAL)
  d.line([(8,3),(12,2),(15,5)],fill=GLINT)
  d.line([(12,7),(15,5)],fill=DEEP,width=2)
 save(im,Path('Items/Tidelite'+name+'.png'))

# Native vanilla tile framing: 16x16 source pixels + 2px padding, 16x15 frames.
# Every frame is separately authored from a deterministic variation seed.
for ore in [True,False]:
 sheet=Image.new('RGBA',(144,135))
 for row in range(15):
  for col in range(16):
   im,d=canvas(8,8);rng=random.Random(721+row*97+col*37)
   d.rectangle((0,0,7,7),fill=ROCK)
   if ore:
    for _ in range(15):
     x,y=rng.randrange(8),rng.randrange(8);d.point((x,y),rng.choice([DARK,ROCK,MID]))
    x=rng.randrange(2,5)
    vein=[(x-1,7),(x,5),(x-1,3),(x+1,1),(x+2,0)]
    d.line(vein,fill=DEEP,width=3);d.line(vein,fill=TEAL,width=2)
    d.line([(x,6),(x+1,5),(x,3),(x+2,1)],fill=SEA)
    d.point((x+1,4),GLINT)
   else:
    for y in [0,4]:
     d.line([(0,y),(7,y)],fill=DARK)
     d.line([(0,y+1),(7,y+1)],fill=MID)
     split=2 if y==0 else 6;d.line([(split,y),(split,y+3)],fill=DARK)
    d.line([(0,3),(7,3)],fill=DEEP)
    d.line([(1,3),(5,3)],fill=TEAL)
   sheet.alpha_composite(im,(col*9,row*9))
 save(sheet,Path('Tiles/Tidelite'+('Ore' if ore else 'Brick')+'.png'))

im,d=canvas(9,9)
d.polygon([(0,4),(2,1),(6,1),(7,3),(7,7),(0,7)],fill=INK)
d.polygon([(1,4),(3,2),(6,2),(6,4)],fill=SEA)
d.rectangle((1,5,6,6),fill=TEAL);d.line([(3,2),(6,2)],fill=GLINT)
save(im,Path('Tiles/TideliteBar.png'))

# Native wall framing: 32px frames + 4px padding, 13 columns x 5 rows.
sheet=Image.new('RGBA',(234,90))
for row in range(5):
 for col in range(13):
  im,d=canvas(16,16);d.rectangle((0,0,15,15),fill=DARK)
  for y in range(0,16,4):
   d.line([(0,y),(15,y)],fill=INK)
   d.line([(0,y+1),(15,y+1)],fill=ROCK)
   for x in range((y//4%2)*4,16,8):d.line([(x,y),(x,y+3)],fill=INK)
   d.line([(1,y+3),(6,y+3)],fill=DEEP)
  sheet.alpha_composite(im,(col*18,row*18))
save(sheet,Path('Walls/TideliteWall.png'))

