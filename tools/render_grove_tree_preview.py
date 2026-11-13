#!/usr/bin/env python3
"""Assemble an exported native tree with ImageMagick; do not repaint its art.

Uses TileDrawing's zero-wind 20px trunk, 40px branch and ModTree crown frames.
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
    sample = json.loads((ART / "tree-preview-tiles.json").read_text())
    tiles = sample["tiles"]
    trunk = sample["trunkX"]
    floor = sample["floor"]
    crown_width, crown_height = 256, 192
    top = min(t["y"] * 16 + 16 - crown_height for t in tiles
              if t["fx"] == 22 and t["fy"] >= 198)
    origin_x = trunk * 16 + 8 - 200
    origin_y = top - 20
    baseline = floor * 16 - origin_y
    args = ["magick", "-size", f"400x{baseline + 24}", "xc:#6588ac"]
    with tempfile.TemporaryDirectory() as folder:
        def add(name, crop, x, y):
            path = Path(folder) / f"piece-{len(args)}.png"
            subprocess.run(["magick", str(ASSETS / f"{name}.png"),
                            "-crop", crop, "+repage", str(path)], check=True)
            args.extend([str(path), "-geometry",
                         f"{x-origin_x:+d}{y-origin_y:+d}", "-composite"])

        for tile in tiles:
            add("Tree", f"20x20+{tile['fx']}+{tile['fy']}",
                tile["x"] * 16 - 2, tile["y"] * 16)
        for tile in tiles:
            if tile["fy"] < 198:
                continue
            x, y = tile["x"] * 16, tile["y"] * 16
            if tile["fx"] == 22:
                add("Tree_Tops", f"{crown_width}x{crown_height}+0+0",
                    x + 8 - crown_width // 2, y + 16 - crown_height)
            elif tile["fx"] == 44:
                add("Tree_Branches", "40x40+0+0", x - 24, y - 12)
            elif tile["fx"] == 66:
                add("Tree_Branches", "40x40+42+0", x, y - 12)
        add("Tree_Roots", "96x32+0+0", trunk * 16 + 8 - 48,
            floor * 16 - 32 + 6)
        args.append(str(ART / "tree-frame-preview.png"))
        subprocess.run(args, check=True)


if __name__ == "__main__":
    render()
