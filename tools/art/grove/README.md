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
Low grass uses a narrow blade silhouette; all other silhouettes
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

0.4.11: `vine-source.png` is a generated hanging-vine source using the established
foliage palette. `bash tools/build_grove_vine.sh` produces `Assets/Grove/Vine.png`:
32x256 pixels, sixteen consecutive 32x16 pieces, exact 2x2 pixels and binary alpha.
The runtime keeps native vine tiles, roots, cutting and growth; only their drawing
changes. Leaves extend 8px either side of the native tile. Native Plants/Plants2
rooted in gallery grass use the approved Tuft atlas, including anchored wind sway.
The turf pass now suppresses the vanilla green sprite entirely and draws vanilla
Dirt pixels under the custom exposed-edge material. Upper-slope masks follow the
correct solid sides and all slope masks use Terraria's 2px staircase.

0.4.13 replaces `turf-source.png` with a thin vanilla-proportioned sod strip,
not moss cushions. The packed 108x16 atlas keeps six 16px frames with 2px gutters:
transparent y=0..1, blade tips y=2..3, opaque sod y=4..7, irregular roots
y=8..11, transparent y=12..15. Runtime top sampling is capped at 6px including
the transition, side sampling at 4px, and undersides use only the sparse y=10..11
tips. Sampling/drawing is entirely on a 2x2 grid. The source, atlas builder and
static contact/tree previews were updated together. Static previews use the
actual top sample range with a schematic soil guide, not client screenshots.

0.4.15: `ground-plants-source.png` is a 4x4 source sheet of eight fine grass
silhouettes and eight small woodland-flower variations. The dedicated
`build_grove_ground_plants.sh` packs sixteen 32x32 frames into
`Assets/Grove/GroundPlants.png` (512x32), and also replaces the gallery's
Tuft art while preserving their native object footprints/atlas layouts.
Short grass is fitted to 14x7 logical pixels; taller grass to 12x14, flowers
to 12x12 or 12x13. The runtime anchors all variants to the soil at the same
baseline and deterministically chooses flowers about one quarter of the time.
These remain decorative native Plants/Plants2, not herb/dye resources.

`vine-source.png` now contains four sparse narrow vine designs. The vine builder
packs four 16x128 columns into a 64x128 sheet: seven consecutive body segments
and a terminal segment per variety. A chain keeps its root's variety throughout;
long bodies loop their seven segments, and cutting/growth updates the end frame.
Vines use vanilla 16px width rather than the previous 32px leafy rope. All new
atlases retain binary alpha and exact 2x2 source pixels. `ground-plants-preview.png`
shows the actual packed frames at 4x, and the ecosystem preview is reconstructed
from the updated game atlases. Both are art previews, not client screenshots.
