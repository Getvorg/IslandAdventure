"""Match native wood tones by shade distribution and graft exact native joints."""
from collections import Counter, deque
from pathlib import Path
from PIL import Image
from build_camphor_assets import WOOD
root=Path(__file__).resolve().parents[1]
assets=root/'Assets/Grove'
stem=Image.open(assets/'Camphor.png').convert('RGBA')
native=Image.open(root/'tools/art/grove/vanilla/Tree_Branches_0.png').convert('RGBA')
wood=set(WOOD.values())
counts=Counter(p[:3] for p in stem.get_flattened_data() if p[3] and p[:3] in wood)
ordered=sorted(counts,key=sum)
thresholds=[];total=sum(counts.values());cumulative=0
for color in ordered:
    cumulative+=counts[color];thresholds.append(cumulative/total)
def brown(p):
    r,g,b,a=p
    return a and r>g*1.12 and g>b*1.1

def match(image):
    original=list(image.get_flattened_data())
    counts=Counter(p for p in original if brown(p))
    mapping={};cumulative=0;total=sum(counts.values())
    for color in sorted(counts,key=lambda p:sum(p[:3])):
        quantile=(cumulative+counts[color]/2)/total
        rank=next(i for i,t in enumerate(thresholds) if quantile<=t)
        mapping[color]=(*ordered[rank],color[3]);cumulative+=counts[color]
    image.putdata([mapping.get(p,p) for p in original])
    return image

def joint(style,left):
    part=native.crop((0 if left else 42,style*42,40 if left else 82,style*42+40))
    part.putdata([(*WOOD[p[:3]],p[3]) if p[3] and p[:3] in WOOD else (0,0,0,0) for p in part.get_flattened_data()])
    return part

for name in ('Camphor_Tops','Camphor_Boughs','Camphor_Branches'):
    path=assets/f'{name}.png';image=match(Image.open(path).convert('RGBA'))
    if name=='Camphor_Tops':
        # The source's neck centers at x134, not half of its former 256px canvas.
        # Widening the transparent canvas keeps every leaf and centers the neck.
        # Trim only brown neck overhang, then insert a complete native stem strip.
        for y in range(166,176):
            for x in range(image.width):
                if not 126<=x<142 and brown(image.getpixel((x,y))):image.putpixel((x,y),(0,0,0,0))
        strip=stem.crop((2,22,18,42))
        image.alpha_composite(strip,(126,156))
    else:
        large=name=='Camphor_Boughs'
        stride_x,stride_y,w=(66,50,64) if large else (42,42,40)
        for style in range(3):
            for side in range(2):
                left=side==0;origin_x=side*stride_x;origin_y=style*stride_y
                if style==2 and left:
                    # This native arm rises above the generated limb's socket.
                    # Move the existing art to it, rather than painting a bridge.
                    h=48 if large else 40;up=2 if large else 8
                    part=image.crop((origin_x,origin_y+up,origin_x+w,origin_y+h))
                    image.paste((0,0,0,0),(origin_x,origin_y,origin_x+w,origin_y+h))
                    image.alpha_composite(part,(origin_x,origin_y))
                # Replace the attachment wood with the original native taper,
                # rather than a solid rectangular plug. Foliage is untouched.
                zone=8
                for y in range(stride_y-2):
                    for x in range(w-zone if left else 0,w if left else zone):
                        p=(origin_x+x,origin_y+y)
                        if brown(image.getpixel(p)):image.putpixel(p,(0,0,0,0))
                image.alpha_composite(joint(style,left),(origin_x+(24 if left and large else 0),origin_y+(12 if large else 0)))
    if name!='Camphor_Tops':
        # A branch must be one continuous component from its native root.
        h=48 if large else 40
        for style in range(3):
            for side in range(2):
                ox,oy=side*stride_x,style*stride_y
                opaque={(x,y) for y in range(h) for x in range(w) if image.getpixel((ox+x,oy+y))[3]}
                queue=deque(p for p in opaque if p[0]>=w-2) if side==0 else deque(p for p in opaque if p[0]<2)
                connected=set()
                while queue:
                    p=queue.popleft()
                    if p in connected or p not in opaque:continue
                    connected.add(p);x,y=p;queue.extend(((x-1,y),(x+1,y),(x,y-1),(x,y+1)))
                assert len(connected)>=len(opaque)*.95,(name,style,side,'disconnected main limb')
                for x,y in opaque-connected:image.putpixel((ox+x,oy+y),(0,0,0,0))
    image.save(path)
