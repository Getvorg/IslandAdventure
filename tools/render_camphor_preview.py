#!/usr/bin/env python3
"""Assemble an exported native tree with ImageMagick; do not repaint its art.

Uses TileDrawing's zero-wind 20px trunk, 64px bough and custom 256px crown frames.
This is a static join diagnostic, not a graphical-client screenshot.
"""
import json
from pathlib import Path
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "tools/art/grove"
ASSETS = ROOT / "Assets/Grove"


def render():
    sample = json.loads((ART / "concepts/camphor-preview-tiles.json").read_text())
    tiles = sample["tiles"]
    trunk = sample["trunkX"]
    floor = sample["floor"]
    crown_width, crown_height = 256, 176
    top = min(t["y"] * 16 + 16 - crown_height for t in tiles
              if t["fx"] == 22 and t["fy"] >= 198)
    origin_x = trunk * 16 + 8 - 200
    origin_y = top - 20
    baseline = floor * 16 - origin_y
    # Soil is a schematic contact plane; actual turf and tree pieces are native sprites.
    args = ["magick", "-size", f"400x{baseline + 24}", "xc:#6588ac",
            "-fill", "#6b513a", "-draw", f"rectangle 0,{baseline+4} 399,{baseline+23}"]
    with tempfile.TemporaryDirectory() as folder:
        def add(name, crop, x, y):
            path = Path(folder) / f"piece-{len(args)}.png"
            subprocess.run(["magick", str(ASSETS / f"{name}.png"),
                            "-crop", crop, "+repage", str(path)], check=True)
            args.extend([str(path), "-geometry",
                         f"{x-origin_x:+d}{y-origin_y:+d}", "-composite"])

        for x in range(origin_x // 16, (origin_x + 400) // 16 + 1):
            add("GrassAtlas", f"16x16+{18+(x%3)*18}+0", x*16, floor*16)
        for tile in tiles:
            add("Camphor", f"20x20+{tile['fx']}+{tile['fy']}",
                tile["x"] * 16 - 2, tile["y"] * 16)
        for tile in tiles:
            if tile["fy"] < 198:
                continue
            x, y = tile["x"] * 16, tile["y"] * 16
            if tile["fx"] == 22:
                add("Camphor_Tops", f"{crown_width}x{crown_height}+0+0",
                    x + 8 - crown_width // 2, y + 16 - crown_height)
            elif tile["fx"] == 44:
                add("Camphor_Boughs", f"64x48+0+{min(2,max(0,(tile['fy']-198)//22))*50}", x - 48, y - 24)
            elif tile["fx"] == 66:
                add("Camphor_Boughs", f"64x48+66+{min(2,max(0,(tile['fy']-198)//22))*50}", x, y - 24)
        args.append(str(ART / "camphor-preview.png"))
        subprocess.run(args, check=True)


if __name__ == "__main__":
    render()
