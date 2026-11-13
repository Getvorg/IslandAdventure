# Sacred grove artwork

Sources: `lush-understory-source.png` (approved large shrub/fern),
`foliage-source.png` (grass, small plants, moss and roots), `props-source.png`
(fallen wood and four rock sizes), `tree-source.png` (canopy/bark).
The canopy occupies the left 1448x724 region; bark is the right panel.
Generated panel boundaries are recorded explicitly to avoid clipping props.

Run `bash tools/build_grove_preview.sh` from the repository with ImageMagick 7.
`objects.tsv` records crop rectangles, logical fitting boxes, tile dimensions
and variant counts. Each game tile is 16x16 with a 2px atlas gutter. Builders
reduce artwork to logical resolution first, quantize colors and alpha, and
then upscale 2x: native art pixels are exactly 2x2, with no partial alpha.
Both low and tall grass use a narrow blade silhouette; all other silhouettes
keep their natural aspect ratio. `ecosystem-preview.png` reconstructs the
actual small sprites and enlarges them 2x, rather than showing source art.

Tree atlas uses the MIT ExampleMod attachment masks in `layouts/`, newly
rendered bark/leaf RGB, a single 192x128 canopy frame (2px trailing padding)
and native 40x40 branch frames. `Tree_Roots.png` is unframed; `Roots.png` is
the same drawing split into an ordinary 6x2-tile decorative object.

Live trees grow on custom turf, drop vanilla wood, and render in normal world
lighting. Only the turf rim uses moss pixels; its dirt base uses vanilla Dirt.
Large shrub and fern source/art were retained from 0.4.3.
