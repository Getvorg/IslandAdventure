#!/usr/bin/env python3
"""Exact palette remap of the user-supplied full vanilla grass atlas.

Do not crop, rescale, reframe, regenerate alpha, or touch any other source colors.
The white lower regions are preserved verbatim, regardless of their runtime use.
"""
from pathlib import Path
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/"tools/art/grove/vanilla/Tiles_2.png"
TARGET=ROOT/"Assets/Grove/GrassAtlas.png"
PALETTE={
    (3,32,11,255):(12,36,21,255),
    (13,101,36,255):(31,79,38,255),
    (30,150,72,255):(60,120,54,255),
    (28,216,94,255):(92,156,64,255),
}


def build():
    source=Image.open(SOURCE).convert("RGBA")
    result=source.copy()
    result.putdata([PALETTE.get(pixel,pixel) for pixel in source.get_flattened_data()])
    result.save(TARGET)
    assert result.size==source.size==(288,1980)
    assert result.getchannel("A").tobytes()==source.getchannel("A").tobytes()
    assert result.crop((0,396,288,1980)).tobytes()==source.crop((0,396,288,1980)).tobytes()
    changed=0
    for before,after in zip(source.get_flattened_data(),result.get_flattened_data()):
        if before in PALETTE:
            assert after==PALETTE[before]
            changed+=1
        else:
            assert before==after
    # Display-only comparison; the complete game texture above is never resized.
    preview=Image.new("RGBA",(600,420),"#596f7e")
    preview.alpha_composite(source.crop((0,0,288,396)),(4,20))
    preview.alpha_composite(result.crop((0,0,288,396)),(308,20))
    draw=ImageDraw.Draw(preview)
    draw.text((4,4),"Original",fill="white")
    draw.text((308,4),"Grove",fill="white")
    preview.save(ROOT/"tools/art/grove/grass-atlas-preview.png")
    print(f"Grass atlas: {source.width}x{source.height}, {changed} pixels remapped; all other RGBA unchanged")


if __name__=="__main__":
    build()
