"""Full native wood frame atlas with a pale beech-timber palette."""
from pathlib import Path
from PIL import Image
root=Path(__file__).resolve().parents[1]
p=root/'tools/art/buna-items/vanilla-wood.png';im=Image.open(p).convert('RGBA')
# Complete native sheet: preserve every form/merge/alpha pixel, not one repeated tile.
colors=sorted({c[:3] for c in im.get_flattened_data() if c[3]},key=sum)
target=[(73,57,41),(111,87,58),(139,111,76),(153,124,87),(178,149,110),(202,174,133),(224,200,161)]
assert im.size==(288,270) and len(colors)==len(target)
mapping=dict(zip(colors,target));result=Image.new('RGBA',im.size)
result.putdata([(*mapping[p[:3]],p[3]) if p[3] else p for p in im.get_flattened_data()])
assert result.getchannel('A').tobytes()==im.getchannel('A').tobytes()
result.save(root/'Assets/Tiles/BunaWoodBlock.png')
