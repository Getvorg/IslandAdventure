# Ancient Camphor — new design (0.4.24)

The user approved ancient-camphor.png as the new whole-tree design after the
previous tree was removed. camphor-parts-source.png contains a newly generated
canopy and left/right boughs, using that concept and ecosystem-preview.png
(existing grove plants) as references. Neither source image is loaded in game.

Run bash tools/build_camphor_assets.sh to pack Camphor.png (176x264 native
20px frames, 22px stride), Camphor_Tops.png (256x176 plus trailing gutter),
and Camphor_Branches.png (40x40, 42px stride, two columns/three native styles).
Only attachment alpha is inherited from ExampleMod's native layout, not the
removed tree's art. Bark is newly sampled from the concept into four warm
browns. Hard alpha and 2x2 pixels are applied before packing. There is no wide
root overlay: native bottom frames supply compact roots at soil level.

Native tree IDs, epic-tree height, wood drops and chopping remain. Regional
CamphorAppearance applies only to unpainted native trees rooted on gallery
grass. Native painting handles painted trees; other areas retain vanilla art.
Legacy GroveGrass soils have an AncientCamphor ModTree registration until
material migration restores native grass. No new autonomous growth is enabled.

/grovepreview refreshes three sample trees and props on every use. Automatic
layout upgrade 4 also adds trees to worlds saved with the tree-free version.
Occupied sample space is skipped. Back locations and other blocks survive.

camphor-preview-tiles.json is a native frame fixture exported by the loaded
mod test harness. Run python3 tools/render_camphor_preview.py for an actual-size
packed-art join diagnostic, not a graphical-client screenshot. Whole-island
rollout remains separate from this gallery prototype.
