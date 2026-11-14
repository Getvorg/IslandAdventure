#!/usr/bin/env python3
"""Compose four opaque slope shapes from existing edge pixels, no new palette."""
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
faces=Image.open(ROOT/"Assets/Grove/GrassFaces.png").convert("RGBA")
atlas=Image.new("RGBA",(120,80))
for variant in range(6):
    for shape,side_rows in enumerate(((2,3),(1,2),(0,3),(0,1))):
        result=Image.new("RGBA",(20,20))
        for row in side_rows:
            result.alpha_composite(faces.crop((variant*20,row*20,variant*20+20,row*20+20)))
        for y in range(0,16,2):
            for x in range(0,16,2):
                boundary=(x if shape in (0,3) else 14-x)
                lower=shape<2
                inside=y>=boundary if lower else y<=boundary
                if not inside:
                    result.paste((0,0,0,0),(x+2,y+2,x+4,y+4))
                    continue
                depth=y-boundary if lower else boundary-y
                # Move intact 2px columns of the top/bottom source along the
                # diagonal's staircase. No 45-degree filtering or blur.
                source_y=2+depth if lower else 2+14-depth
                row=0 if lower else 2
                if 0<=source_y<=18:
                    fragment=faces.crop((variant*20+x+2,row*20+source_y,
                                         variant*20+x+4,row*20+source_y+2))
                    result.alpha_composite(fragment,(x+2,y+2))
                # A tiny enclosed centre must be turf too. Close transparent
                # holes with the existing last opaque source cell, not a new fill color.
                if result.getpixel((x+2,y+2))[3]==0:
                    sample_y=4 if lower else 14
                    result.paste(faces.crop((variant*20+x+2,row*20+sample_y,
                                            variant*20+x+4,row*20+sample_y+2)),(x+2,y+2))
        # Runtime cardinal faces draw their own outward tips. This texture only
        # fills the exact triangle, so it cannot bleed into a neighboring tile.
        clipped=Image.new("RGBA",(20,20))
        clipped.paste(result.crop((2,2,18,18)),(2,2))
        atlas.paste(clipped,(variant*20,shape*20))
atlas.save(ROOT/"Assets/Grove/GrassCorners.png")
