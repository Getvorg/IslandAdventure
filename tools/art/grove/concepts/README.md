# Ancient Camphor — new design (0.4.24)

The user approved ancient-camphor.png as the new whole-tree design after the
previous tree was removed. camphor-parts-source.png contains a newly generated
canopy and left/right boughs, using that concept and ecosystem-preview.png
(existing grove plants) as references. Neither source image is loaded in game.

## Current implementation (0.4.26)

User clarification: keep the actual vanilla-based trunk and roots, but use
original Camphor crown and branches. Camphor.png is byte-identical to 0.4.25:
complete Tiles_5.png with the same exact six-shade wood palette transform.
The stem builder does not crop, resize, quantize or reconstruct native alpha.

build_camphor_foliage.sh packs the approved generated Camphor parts separately:
- Camphor_Tops.png: 256x176 crown plus native trailing gutter (258x178).
- Camphor_Boughs.png: 64x48 gallery branches, 66x50 stride, two sides/three styles.
- Camphor_Branches.png: separate 40x40 fallback required by the ModTree API.

match_camphor_wood.py matches brown pixels to the stem's six wood shades;
foliage stays green. Hard alpha and 2x2 pixels are established before packing.
Solid 8px sockets match the renderer's pivot and avoid thin branch attachments.
The original concept and parts source remain; vanilla foliage sheets are now
references only. No generated material is applied to the trunk or roots.

Native tree IDs, epic-tree height, wood drops and chopping remain. Regional
CamphorAppearance applies only to unpainted native trees rooted on gallery
grass. Native painting handles painted trees; other areas retain vanilla art.
Legacy GroveGrass soils have an AncientCamphor ModTree registration until
material migration restores native grass. No new autonomous growth is enabled.

/grovepreview refreshes three sample trees and props on every use. Automatic
layout upgrade 6 also adds trees to worlds saved with the tree-free version.
Occupied sample space is skipped. Back locations and other blocks survive.

camphor-preview-tiles.json is a native frame fixture exported by the loaded
mod test harness. Run python3 tools/render_camphor_preview.py for an actual-size
packed-art join diagnostic, not a graphical-client screenshot. Whole-island
rollout remains separate from this gallery prototype.
