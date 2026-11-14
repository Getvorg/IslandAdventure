"""Exact color-only remap of brown foliage pixels to the native stem palette."""
from pathlib import Path
from PIL import Image
from build_camphor_assets import WOOD
root=Path(__file__).resolve().parents[1]
for name in ('Camphor_Tops','Camphor_Boughs','Camphor_Branches'):
    path=root/'Assets/Grove'/f'{name}.png'
    image=Image.open(path).convert('RGBA')
    colors=set(image.get_flattened_data())
    mapping={}
    for p in colors:
        r,g,b,a=p
        if a and r>g*1.12 and g>b*1.1:
            # Keep already matched sockets; map original warm generated wood by shade.
            target=p[:3] if p[:3] in WOOD.values() else WOOD[min(WOOD,key=lambda q:sum((v-w)**2 for v,w in zip(q,p[:3])))]
            mapping[p]=(*target,a)
    image.putdata([mapping.get(p,p) for p in image.get_flattened_data()])
    image.save(path)
