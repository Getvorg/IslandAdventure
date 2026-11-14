#!/usr/bin/env python3
"""Assemble actual runtime face samples over schematic soil for visual review."""
import json
from pathlib import Path
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/"tools/art/grove"
cases=json.loads((ART/"turf-face-cases.json").read_text())
atlas=Image.open(ROOT/"Assets/Grove/GrassFaces.png").convert("RGBA")
corners=Image.open(ROOT/"Assets/Grove/GrassCorners.png").convert("RGBA")
image=Image.new("RGBA",(384,240),"#596f7e")
draw=ImageDraw.Draw(image)
for index,case in enumerate(cases):
    origin_x=(index%3)*128+56
    origin_y=(index//3)*80+38
    draw.text(((index%3)*128+4,(index//3)*80+4),case["name"],fill="#eef0e7")
    for cell in case["soil"]:
        x,y=origin_x+cell["x"],origin_y+cell["y"]
        draw.rectangle((x,y,x+1,y+1),fill="#99734e")
    for cell in case["samples"]:
        source=atlas.crop((cell["sx"],cell["sy"],cell["sx"]+2,cell["sy"]+2))
        image.alpha_composite(source,(origin_x+cell["x"],origin_y+cell["y"]))
    for cell in case.get("corners",[]):
        source=corners.crop((cell["sx"],cell["sy"],cell["sx"]+20,cell["sy"]+20))
        image.alpha_composite(source,(origin_x+cell["x"],origin_y+cell["y"]))
# Display zoom only. Atlas pixels are copied unchanged at their runtime positions.
image.resize((1152,720),Image.Resampling.NEAREST).save(ART/"turf-forms-preview.png")
