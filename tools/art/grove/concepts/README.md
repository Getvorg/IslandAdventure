# Ancient Camphor — new design (0.4.24)

The user approved ancient-camphor.png as the new whole-tree design after the
previous tree was removed. camphor-parts-source.png contains a newly generated
canopy and left/right boughs, using that concept and ecosystem-preview.png
(existing grove plants) as references. Neither source image is loaded in game.

## Current implementation (0.4.25)

After in-game review the user explicitly requested actual vanilla textures as
the basis. The earlier concept and generated parts are archived references only;
neither is used by the builder. build_camphor_assets.py now transforms the exact
palettes of vanilla/Tiles_5.png, Tree_Tops_0.png and Tree_Branches_0.png.
No crop, resize, quantization, custom material or alpha reconstruction is used.
All native wood shading, root/branch joints and three foliage variants remain.

Camphor.png is 176x264; Camphor_Tops.png is 246x82 (three 80x80 frames at
82px stride); Camphor_Branches.png is 84x126 (40px frames at 42px stride).
The builder verifies dimensions, identical alpha, unchanged non-palette pixels
and an exact 2x2 grid. Native compact roots need no separate root overlay.

Native tree IDs, epic-tree height, wood drops and chopping remain. Regional
CamphorAppearance applies only to unpainted native trees rooted on gallery
grass. Native painting handles painted trees; other areas retain vanilla art.
Legacy GroveGrass soils have an AncientCamphor ModTree registration until
material migration restores native grass. No new autonomous growth is enabled.

/grovepreview refreshes three sample trees and props on every use. Automatic
layout upgrade 5 also adds trees to worlds saved with the tree-free version.
Occupied sample space is skipped. Back locations and other blocks survive.

camphor-preview-tiles.json is a native frame fixture exported by the loaded
mod test harness. Run python3 tools/render_camphor_preview.py for an actual-size
packed-art join diagnostic, not a graphical-client screenshot. Whole-island
rollout remains separate from this gallery prototype.
