#!/usr/bin/env python3
"""Author foliage directly in final 2x2 pixels. No tracing, shrinking or smoothing."""
import argparse
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ASSETS, ART = ROOT / "Assets/Grove", ROOT / "tools/art/grove"
COLORS = dict(d="#203f2d", s="#335b35", g="#52803e", l="#7eaa4d",
              h="#adc26c", w="#eee9d4", c="#b8c9bd", y="#d4b968",
              v="#b0a0cc", u="#726da1", b="#a7ced5", a="#6c9da9",
              p="#d4a9b8", r="#9d738e")


class Pixels:
    def __init__(self, width=16, height=16):
        self.image = Image.new("RGBA", (width*2, height*2))
        self.draw = ImageDraw.Draw(self.image)

    def dot(self, x, y, color):
        if 0 <= x < self.image.width//2 and 0 <= y < self.image.height//2:
            self.draw.rectangle((x*2,y*2,x*2+1,y*2+1),fill=COLORS[color])

    def line(self, points, color):
        for (x,y),(end_x,end_y) in zip(points,points[1:]):
            dx,dy = abs(end_x-x),-abs(end_y-y)
            sx,sy = 1 if x < end_x else -1,1 if y < end_y else -1
            error = dx+dy
            while True:
                self.dot(x,y,color)
                if (x,y)==(end_x,end_y):
                    break
                twice=2*error
                if twice>=dy:
                    error+=dy
                    x+=sx
                if twice<=dx:
                    error+=dx
                    y+=sy

    def blade(self, points, shade="g"):
        self.line(points,shade)
        self.dot(*points[-1],"l")
        if len(points)>2:
            self.dot(*points[-2],"l")

    def leaves(self,x,y,side):
        self.dot(x+side,y,"g")
        self.dot(x+side*2,y-1,"l")
        self.dot(x+side,y-1,"s")

    def flower(self,x,y,light,shadow,bell=False):
        if bell:
            self.dot(x,y-1,shadow)
            for dx in (-1,0,1):
                self.dot(x+dx,y,light)
            self.dot(x-1,y+1,shadow)
            self.dot(x+1,y+1,shadow)
        else:
            for dx,dy in ((0,-1),(-1,0),(1,0),(0,1)):
                self.dot(x+dx,y+dy,light if dy<=0 else shadow)
            self.dot(x,y,"y")


# Hand-placed blade paths: root first, tip last. Transparent gaps are deliberate.
GRASSES = [
 [[(6,15),(5,12),(3,10)],[(7,15),(7,11),(6,8)],[(8,15),(9,12),(11,10)],[(9,15),(11,14),(13,13)]],
 [[(5,15),(4,13),(2,12)],[(6,15),(6,12),(5,10)],[(8,15),(9,12),(10,11)],[(9,15),(11,13),(13,12)]],
 [[(6,15),(4,12),(2,11)],[(7,15),(7,12),(8,9)],[(8,15),(10,12),(12,11)]],
 [[(6,15),(7,11),(9,7)],[(7,15),(9,12),(12,10)],[(9,15),(11,13),(14,12)],[(5,15),(5,13),(4,11)]],
 [[(6,15),(5,10),(3,7)],[(7,15),(7,8),(6,3)],[(8,15),(10,10),(12,6)],[(9,15),(12,13),(14,11)]],
 [[(6,15),(6,9),(4,5)],[(7,15),(8,8),(10,4)],[(8,15),(11,10),(13,8)],[(5,15),(3,12),(2,10)]],
 [[(6,15),(4,11),(1,9)],[(7,15),(6,8),(7,4)],[(8,15),(9,9),(11,5)],[(9,15),(12,11),(14,10)]],
 [[(5,15),(7,10),(10,7)],[(7,15),(9,8),(13,4)],[(9,15),(12,10),(15,9)],[(6,15),(6,12),(5,10)]],
]


def grass(index):
    canvas=Pixels()
    for n,path in enumerate(GRASSES[index]):
        canvas.blade(path,"g" if n%2 else "s")
    for x,color in ((6,"d"),(7,"s"),(8,"g"),(9,"s")):
        canvas.dot(x,15,color)
    return canvas.image


def flowers(index):
    canvas=Pixels()
    heads=[[(6,7),(10,10)],[(5,9),(9,6)],[(6,6),(10,9)],[(5,8),(10,6)],
           [(6,5),(9,9),(5,11)],[(5,7),(10,8)],[(7,5),(10,10)],[(5,9),(9,6)]][index]
    for n,(x,y) in enumerate(heads):
        canvas.line([(7+n%2,15),(7+n%2,12),(x,y+1)],"s" if n else "g")
    canvas.leaves(7,13,-1)
    canvas.leaves(8,12,1)
    canvas.blade([(6,15),(4,13),(3,12)],"s")
    canvas.blade([(9,15),(11,14),(12,12)],"g")
    palette=[("w","c"),("w","y"),("v","u"),("b","a"),
             ("w","c"),("w","y"),("b","a"),("p","r")][index]
    for x,y in heads:
        canvas.flower(x,y,*palette,bell=index in (1,4))
    canvas.dot(7,15,"d")
    canvas.dot(8,15,"s")
    return canvas.image


def object_atlas(image,cols,rows):
    atlas=Image.new("RGBA",(cols*18,rows*18))
    for r in range(rows):
        for c in range(cols):
            atlas.paste(image.crop((c*16,r*16,c*16+16,r*16+16)),(c*18,r*18))
    return atlas


def build_plants():
    frames=[grass(n) for n in range(8)]+[flowers(n) for n in range(8)]
    atlas=Image.new("RGBA",(512,32))
    for n,frame in enumerate(frames):
        atlas.paste(frame,(n*32,0))
    atlas.save(ASSETS/"GroundPlants.png")
    tufts=Image.new("RGBA",(108,18))
    for n in range(3):
        tufts.paste(object_atlas(frames[n].crop((0,16,32,32)),2,1),(n*36,0))
    tufts.save(ASSETS/"Tuft.png")
    tall=Pixels(24,32)
    for n,path in enumerate([
        [(8,31),(6,24),(3,20)],[(9,31),(9,21),(7,14)],
        [(10,31),(12,22),(14,12)],[(11,31),(15,25),(19,21)],
        [(15,31),(14,24),(12,19)],[(16,31),(17,22),(19,16)],
        [(17,31),(20,26),(22,24)],
    ]):
        tall.blade(path,"s" if n%3==0 else "g")
    object_atlas(tall.image,3,4).save(ASSETS/"TallGrass.png")
    preview=Image.new("RGBA",(256,64),"#596f7e")
    for n,frame in enumerate(frames):
        preview.alpha_composite(frame,((n%8)*32,(n//8)*32))
    # Display-only zoom: the game atlases themselves have never been resized.
    preview.resize((1024,256),Image.Resampling.NEAREST).save(ART/"ground-plants-preview.png")


def vine(variant):
    canvas=Pixels(8,64)
    for y in range(64):
        canvas.dot(3,y,"g" if (y+variant)%5==0 else "s")
    for frame in range(7):
        for y,side in ((frame*8+1+(frame+variant)%2,-1),(frame*8+5,1)):
            if variant==0:
                canvas.dot(3+side,y,"g")
                canvas.dot(3+side*2,y-1,"l")
                canvas.dot(3+side*2,y,"g")
            elif variant==1:
                if frame%3==1 and side==1:
                    continue
                canvas.dot(3+side,y,"g")
                canvas.dot(3+side*2,y,"l")
                canvas.dot(3+side*2,y+1,"s")
            elif variant==2:
                canvas.dot(3+side,y,"g")
                canvas.dot(3+side*2,y+1,"l")
                canvas.dot(3+side*2,y+2,"g")
            else:
                canvas.dot(3+side,y,"g")
                canvas.dot(3+side*2,y-1,"l")
                canvas.dot(3+side*2,y+1,"g")
    # A proper last segment, joining every body frame at the same stem pixel.
    canvas.dot(2,58,"l")
    canvas.dot(4,60,"g")
    canvas.dot(2,61,"g")
    canvas.dot(3,62,"g")
    canvas.dot(3,63,"l")
    return canvas.image


def build_vines():
    atlas=Image.new("RGBA",(64,128))
    for n in range(4):
        atlas.paste(vine(n),(n*16,0))
    atlas.save(ASSETS/"Vine.png")
    preview=Image.new("RGBA",atlas.size,"#596f7e")
    preview.alpha_composite(atlas)
    preview.resize((256,512),Image.Resampling.NEAREST).save(ART/"vines-preview.png")


if __name__=="__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("part",choices=("plants","vines","all"),default="all",nargs="?")
    args=parser.parse_args()
    if args.part in ("plants","all"):
        build_plants()
    if args.part in ("vines","all"):
        build_vines()
