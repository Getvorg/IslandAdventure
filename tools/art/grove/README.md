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
cache, and grass outside the gallery is unchanged. Decorative roots remain separate objects. The old procedural/rotated-edge turf renderer, textures,
builders and form masks are removed.

## Other foliage and props

`lush-understory-source.png`: approved large shrub/fern.
`foliage-source.png`: small plants, moss and roots.
`props-source.png`: fallen wood and four rock sizes.
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

## Contact preview

The former custom tree was removed in 0.4.23. Version 0.4.24 introduces the
newly approved Ancient Camphor. In 0.4.25 its artwork is based directly on
the complete vanilla tree sheets with an exact palette transform. See
concepts/README.md for sources, native frame packing, tree dimensions and
regional behavior. Run `bash tools/build_camphor_assets.sh` and
`python3 tools/render_camphor_preview.py` to rebuild its assets and join preview.
Roots.png remains the independent decorative root object; fallen logs remain.

Run `python3 tools/render_grove_contact_preview.py` to rebuild the ground-contact
preview from actual packed props and native grass frames over schematic soil.
This is a static diagnostic, not a graphical-client screenshot.
