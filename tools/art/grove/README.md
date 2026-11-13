# Sacred grove artwork

Sources: `lush-understory-source.png` (approved large shrub/fern),
`foliage-source.png` (grass, small plants, moss and roots), `props-source.png`
(fallen wood and four rock sizes), `bark-source.png` (new shared bark), `turf-source.png` (continuous ground rim), `tree-foliage-source.png` (rounded crown/branches).
Bark RGB is calibrated to the opaque brown pixels of the branch sprites;
the same calibrated material fills trunk and six-pixel branch connectors.
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
lighting. The turf rim has a separate continuous source: six 16x16 frames use a 4px fringe, 8px fully opaque textured body and 4px
ragged lower edge. Source rows 4..11 cover world y=0..7, hiding the vanilla
dirt outline. Top/bottom tips retain alpha. It no longer slices separate moss cushions. The dirt
base remains vanilla Dirt.
Large shrub and fern source/art were retained from 0.4.3.

`tree-frame-preview.png` is a zero-wind native-frame reconstruction from the
exported `tree-preview-tiles.json` fixture. Run
`python3 tools/render_grove_tree_preview.py` to check complete-tree joins,
rather than comparing source illustrations to in-game sprites.

Sample trees again use native GrowEpicTree (20–29 tiles). Every plain
/grovepreview call rebuilds the sample, resetting old paint/coatings on its
own decorations/trees while preserving the saved Back position and foreign blocks.
RootSink=14 embeds live roots; TileObjectData.DrawYOffset is 12 for exposed
roots, 6 for wood/stone props, 4 for other plants. The static renderer now
includes the actual turf rim and a schematic soil guide.
`ground-contact-preview.png` assembles the actual rock/log/root atlas cells
with these native draw offsets to catch floating props before client review.

`bash tools/build_grove_turf.sh` rebuilds only the turf. Its 48x8 logical
source reduction retains more cluster detail than the old 24x4 strip.
Six frame gutters remain transparent, but are never sampled by the renderer.
The body is checked for complete alpha coverage and no near-black opaque pixels.
