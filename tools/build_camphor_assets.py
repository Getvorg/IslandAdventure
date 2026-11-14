"""Recolor actual vanilla tree sheets without changing any frame or alpha."""
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
LEAVES = {
 (23,51,28):(15,40,24), (10,74,27):(20,61,30),
 (12,97,35):(29,79,36), (14,106,38):(34,88,39),
 (25,123,60):(40,102,43), (28,138,67):(48,114,46),
 (29,144,69):(52,120,48), (34,168,81):(66,139,54),
 (27,207,90):(87,161,64),
}
PALETTE = WOOD | LEAVES
for source,target,size in [('Tiles_5','Camphor',(176,264)),
                           ('Tree_Tops_0','Camphor_Tops',(246,82)),
                           ('Tree_Branches_0','Camphor_Branches',(84,126))]:
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
    # Check complete original grid, including all three native foliage variants.
    for y in range(0,size[1],2):
        for x in range(0,size[0],2):
            p=result.getpixel((x,y))
            assert all(result.getpixel((x+dx,y+dy))==p for dx in (0,1) for dy in (0,1))
    result.save(TARGET / (target+'.png'))
    print(f'{source}: {size}, native pixels/alpha/frames intact; exact palette transform')
