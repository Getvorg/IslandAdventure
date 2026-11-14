#!/usr/bin/env python3
"""Render decoration ground contact from actual packed sprites."""
from pathlib import Path
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "tools/art/grove"
ASSETS = ROOT / "Assets/Grove"


def render_ground_contact():
    """Render actual atlas pieces against the turf baseline and a soil guide."""
    names = {"Roots", "FallenLog", "HollowLog", "Pebble", "Rock", "LargeRock", "Boulder"}
    objects = []
    for line in (ART / "objects.tsv").read_text().splitlines():
        if not line or line.startswith("#"):
            continue
        name, _, _, _, columns, rows, _ = line.split()
        if name in names:
            objects.append((name, int(columns), int(rows)))
    width = sum(columns * 16 + 16 for _, columns, _ in objects) + 16
    baseline = max(rows * 16 for _, _, rows in objects) + 16
    args = ["magick", "-size", f"{width}x{baseline+24}", "xc:#6588ac",
            "-fill", "#6b513a", "-draw", f"rectangle 0,{baseline+4} {width-1},{baseline+23}"]
    with tempfile.TemporaryDirectory() as folder:
        def add(name, crop, x, y):
            path = Path(folder) / f"piece-{len(args)}.png"
            subprocess.run(["magick", str(ASSETS / f"{name}.png"),
                            "-crop", crop, "+repage", str(path)], check=True)
            args.extend([str(path), "-geometry", f"{x:+d}{y:+d}", "-composite"])

        for x in range(0, width, 16):
            add("GrassAtlas", f"16x16+{18+((x//16)%3)*18}+0", x, baseline)
        x = 16
        for name, columns, rows in objects:
            offset = 12 if name == "Roots" else 6
            for row in range(rows):
                for column in range(columns):
                    add(name, f"16x16+{column*18}+{row*18}",
                        x+column*16, baseline-rows*16+offset+row*16)
            x += columns * 16 + 16
        args.append(str(ART / "ground-contact-preview.png"))
        subprocess.run(args, check=True)


if __name__ == "__main__":
    render_ground_contact()
