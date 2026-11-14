# Sacred grove artwork

## Current grass atlas

The authoritative source is the user-provided converted vanilla texture:
`vanilla/Tiles_2.png` (288x1980, RGBA), plus its original conversion JSON.
The source PNG is kept byte-for-byte. The entire image is retained, including
its white lower regions; no interpretation or trimming of those regions is needed.

Run `python3 tools/build_grove_grass_atlas.py` to build
`Assets/Grove/GrassAtlas.png`. Only these four opaque palette entries change:

| Source RGB | Grove RGB |
| --- | --- |
| 3,32,11 | 12,36,21 |
| 13,101,36 | 31,79,38 |
| 30,150,72 | 60,120,54 |
| 28,216,94 | 92,156,64 |

All other RGBA pixels are identical to the source: dirt, transparency, white
regions, frame positions and padding. There is no cropping, scaling, dithering,
new shape drawing or alpha regeneration. The script asserts these invariants.
`grass-atlas-preview.png` compares the colored top region before/after at 1x;
the full game texture is still 288x1980.

GroveAppearance.DrawEffects changes only TileDrawInfo.drawTexture for unpainted
native Grass tiles in the gallery. Vanilla owns frame selection, merges,
lighting, half blocks and slope clipping. Painted tiles use the native paint
cache, and grass outside the gallery is unchanged. Roots are a separate
PostDraw decoration. The old procedural/rotated-edge turf renderer, textures,
builders and form masks are removed.

## Other foliage and props

`lush-understory-source.png`: approved large shrub/fern.
`foliage-source.png`: small plants, moss and roots.
`props-source.png`: fallen wood and four rock sizes.
`tree-foliage-source.png`: approved crown/branches.
`bark-source.png`: approved leafy-branch connector material.
`camphor-bark-source.png`: quiet stem material for the Ancient Camphor.
`ground-plants-source.png`: current sixteen grass/woodland-flower variations.
`vine-source.png`: four narrow vine varieties.

Run `bash tools/build_grove_preview.sh` with ImageMagick 7 and Python/Pillow.
`objects.tsv` records the remaining crop rectangles, logical fitting boxes,
tile dimensions and variants. The small-plant/vine builders keep their current
0.4.15 source workflow. The unrequested direct-pixel redraw remains reverted.
The separate tall-grass decoration remains removed.

Other generated artwork is fitted to logical pixels, quantized and enlarged
2x, with binary alpha. Native grass uses its supplied original pixel grid.
Each framed decoration tile is 16x16 with a 2px atlas gutter.
`ecosystem-preview.png` assembles actual packed object sprites.

## Trees and contact previews

Tree attachment masks in `layouts/` originate from MIT ExampleMod (license
included there). Trunk-core alpha remains continuous. The approved crown (256x192) and leafy
branch PNGs are unchanged. Branch-only frames inside Tree.png also remain exact
copies of the approved reference in layouts/grove-trunk-reference.png.
The live tree uses compact native root frames; the wide Tree_Roots.png overlay
is removed. Roots.png remains a separate decorative 6x2-tile object.

Sample trees use native GrowEpicTree (20–29 tiles). The native grass identity
supports ordinary tree rules while the regional renderer supplies grove art.
Decoration DrawYOffset is 12 for roots, 6 for logs/stone and 4 for other plants. Gallery refresh resets sample objects
without removing foreign blocks, and keeps its saved Back location.

`python3 tools/render_grove_tree_preview.py` rebuilds tree-frame and ground-contact
previews with actual packed art and selected native grass frames over schematic
soil. These are static contact diagnostics, not graphical-client screenshots.


## Ancient Camphor stem (0.4.22)

The latest user correction requests moderate vanilla-like bark, not a single
flat color and not the former dense diagonal texture. build_grove_trunk.sh
uses four close brown tones (#34271e, #493529, #5b412b, #765438), a quiet
vertical material, soft lateral contour and rare short logical-grid strokes.
The 176x264 atlas keeps the reference alpha and native 22px frame stride.
Twelve branch-only frames are copied verbatim, while crown and leafy-branch
files are kept byte-for-byte. Only the stem/root wood appearance changes.
Every art pixel remains exactly 2x2.

The tree is named Древний камфорник / Ancient Camphor, a sacred-grove tree
inspired by the Japanese woodland atmosphere of Princess Mononoke. Its
approved lush crown/branches, tall native geometry and ordinary wood drops
remain. The localized name is shown by /grovepreview.
