"""Recolor the complete vanilla stem/root sheet without changing frame or alpha."""
from pathlib import Path
from PIL import Image
ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'tools/art/grove/vanilla'
TARGET = ROOT / 'Assets/Grove'
# Keep all six native wood shades, including their contour and relative contrast.
WOOD = {
 (58,48,42):(40,32,26), (86,62,44):(60,44,31),
 (120,85,60):(82,60,41), (151,107,75):(102,76,50),
 (169,125,93):(122,92,63), (191,143,111):(143,110,77),
}
PALETTE = WOOD
def build():
    for source,target,size in [('Tiles_5','Camphor',(176,264))]:
        original = Image.open(SOURCE / (source+'.png')).convert('RGBA')
        assert original.size == size
        pixels = list(original.get_flattened_data())
        mapped = [(*PALETTE.get(p[:3],p[:3]),p[3]) if p[3] else p for p in pixels]
        result = Image.new('RGBA',size)
        result.putdata(mapped)
        assert result.getchannel('A').tobytes() == original.getchannel('A').tobytes()
        for before,after in zip(pixels,mapped):
            assert before[3]==after[3]
            if before[3]==0 or before[:3] not in PALETTE: assert before==after
        # Preserve the native stem/root pixel grid.
        for y in range(0,size[1],2):
            for x in range(0,size[0],2):
                p=result.getpixel((x,y))
                assert all(result.getpixel((x+dx,y+dy))==p for dx in (0,1) for dy in (0,1))
        result.save(TARGET / (target+'.png'))
        print(f'{source}: {size}, native pixels/alpha/frames intact; exact palette transform')

if __name__ == "__main__":
    build()
