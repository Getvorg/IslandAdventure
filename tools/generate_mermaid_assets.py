"""Original six-frame mermaid sprite. Python + Pillow; no runtime generation."""
from pathlib import Path
from PIL import Image, ImageDraw
import math
ROOT=Path(__file__).resolve().parents[1]
INK='#152638'; HAIR='#243349'; HAIR_HI='#405774'; HAIR_LIGHT='#647b94'
SKIN='#c6dcd4'; SKIN_SHADE='#83b3b5'; SKIN_LIGHT='#e6eee0'
TAIL='#227d88'; TAIL_SHADE='#205563'; TAIL_HI='#4fb9ab'; FIN='#79d5b7'
SHELL='#576aa0'; SHELL_HI='#94b7cb'; GOLD='#dac58b'
frames=[]
for frame in range(6):
 im=Image.new('RGBA',(32,32));d=ImageDraw.Draw(im)
 wave=int(round(math.sin(frame*math.tau/6)*2))
 bob=1 if frame in (2,3) else 0
 def poly(points,color): d.polygon([(x,y+bob) for x,y in points],fill=color)
 def line(points,color,width=1):d.line([(x,y+bob) for x,y in points],fill=color,width=width)
 # Long flowing hair behind the human shoulders.
 poly([(19,3),(24,3),(27,6),(25,12),(21,14),(18,18),(12,19+wave),(9,17+wave),(13,16),(12,12),(15,9),(15,6)],INK)
 poly([(19,4),(24,4),(26,7),(23,12),(19,14),(17,17),(12,18+wave),(15,15),(14,12),(17,8)],HAIR)
 line([(20,5),(18,9),(17,14),(13,17+wave)],HAIR_HI,2)
 line([(18,10),(16,14),(17,16)],HAIR_LIGHT)
 # Broad fish tail narrows into an animated forked fin.
 poly([(17,18),(23,18),(24,21),(21,25),(16,27),(10,26+wave),(7,28+wave),(4,27+wave),(7,23+wave),(12,23),(15,21)],INK)
 poly([(18,19),(22,19),(22,22),(19,24),(15,25),(10,24+wave),(9,26+wave),(7,25+wave),(11,24),(15,22)],TAIL)
 poly([(18,20),(22,20),(20,23),(15,24),(11,24+wave),(15,22)],TAIL_HI)
 line([(18,23),(15,25),(11,25+wave)],TAIL_SHADE)
 for x,y in [(18,21),(20,22),(16,23),(14,24)]:
  line([(x,y),(x+1,y)],FIN)
 poly([(8,24+wave),(5,22+wave),(2,21+wave),(3,25+wave),(1,28+wave),(5,28+wave),(9,26+wave)],INK)
 poly([(7,25+wave),(3,23+wave),(4,25+wave),(3,27+wave),(6,27+wave)],FIN)
 line([(4,24+wave),(7,25+wave),(4,27+wave)],TAIL)
 # Far arm, neck and torso. Shell top gives the character a readable outfit.
 poly([(18,12),(21,11),(24,13),(24,17),(22,20),(18,20),(17,16)],INK)
 poly([(19,12),(22,12),(23,14),(22,18),(19,19),(18,16)],SKIN_SHADE)
 poly([(20,11),(22,11),(22,14),(20,15),(19,13)],SKIN)
 poly([(18,14),(21,15),(23,14),(23,17),(21,18),(18,17)],SHELL)
 line([(18,14),(20,15),(21,14)],SHELL_HI)
 line([(22,14),(23,15)],SHELL_HI)
 line([(18,18),(22,18)],GOLD)
 # A reaching arm sways slightly opposite the tail.
 arm=1 if frame in (1,2) else -1 if frame == 5 else 0
 line([(23,13),(25,16),(26+arm,18),(25+arm,20)],INK,3)
 line([(23,14),(24,16),(26+arm,18),(25+arm,19)],SKIN,2)
 # Human face: forward brow/nose and a visible eye.
 poly([(20,5),(24,5),(26,7),(26,9),(27,10),(25,11),(24,13),(21,12),(19,10),(19,7)],INK)
 poly([(21,6),(24,6),(25,8),(25,10),(26,10),(24,11),(23,12),(21,11),(20,9),(20,7)],SKIN)
 line([(23,6),(24,7)],SKIN_LIGHT)
 line([(20,9),(21,11),(23,12)],SKIN_SHADE)
 line([(24,8),(25,8)],INK)
 d.point((24,8+bob),FIN)
 d.point((24,11+bob),HAIR_HI)
 # Fringe, a small pearl clasp and a pointed ear fin.
 poly([(19,4),(24,4),(25,6),(22,6),(20,8),(19,11),(18,8)],HAIR)
 line([(20,5),(23,5)],HAIR_LIGHT)
 poly([(19,8),(17,7),(18,10),(20,10)],TAIL_HI)
 d.point((19,7+bob),GOLD)
 frames.append(im)
sheet=Image.new('RGBA',(64,64*6))
for i,im in enumerate(frames):
 sheet.alpha_composite(im.resize((64,64),Image.Resampling.NEAREST),(0,i*64))
path=ROOT/'Assets/NPCs/Mermaid.png';path.parent.mkdir(parents=True,exist_ok=True);sheet.save(path)
