# Sacred grove artwork

Sources: `lush-understory-source.png` (approved large shrub/fern),
`foliage-source.png` (grass, small plants, moss and roots), `props-source.png`
(fallen wood and four rock sizes), `tree-source.png` (bark), `tree-foliage-source.png` (rounded crown/branches).
Bark uses the right panel of tree-source. New foliage crops are recorded in the builder.
Generated panel boundaries are recorded explicitly to avoid clipping props.

Run `bash tools/build_grove_preview.sh` from the repository with ImageMagick 7.
`objects.tsv` records crop rectangles, logical fitting boxes, tile dimensions
and variant counts. Each game tile is 16x16 with a 2px atlas gutter. Builders
reduce artwork to logical resolution first, quantize colors and alpha, and
then upscale 2x: native art pixels are exactly 2x2, with no partial alpha.
Both low and tall grass use a narrow blade silhouette; all other silhouettes
keep their natural aspect ratio. `ecosystem-preview.png` reconstructs the
actual small sprites and enlarges them 2x, rather than showing source art.

Tree atlas starts from the MIT ExampleMod attachment masks in `layouts/`, newly
rendered bark/leaf RGB, a single 256x192 canopy frame (2px trailing padding)
and native 40x40 branch frames with independent leafy sprites. Trunk-core alpha
is repaired to be continuous; side-only root/branch shapes stay separate. `Tree_Roots.png` is unframed; `Roots.png` is
the same drawing split into an ordinary 6x2-tile decorative object.

Live trees grow on custom turf, drop vanilla wood, and render in normal world
lighting. Only the turf rim uses moss pixels; its dirt base uses vanilla Dirt.
Large shrub and fern source/art were retained from 0.4.3.

`tree-frame-preview.png` is a zero-wind native-frame reconstruction from the
exported `tree-preview-tiles.json` fixture. Run
`python3 tools/render_grove_tree_preview.py` to check complete-tree joins,
rather than comparing source illustrations to in-game sprites.
