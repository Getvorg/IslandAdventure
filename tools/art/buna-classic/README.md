# Buna II — modular Japanese beech

The user keeps Buna I and asks for a second version with a one-tile stem and
separate crown/bough textures. No V1 image or original source is overwritten.
New foliage-source.png was generated with approved Assets/Grove/Buna.png as
the style/botanical reference.

Native frame layout only: tModLoader ExampleMod ExampleTree.png:
https://raw.githubusercontent.com/tModLoader/tModLoader/stable/ExampleMod/Content/Tiles/Plants/ExampleTree.png
MIT license included. Pink layout markers are excluded from alpha. No deleted
tree image was restored or used. Visible beech bark comes from approved Buna I,
fitted to a calm 16px stem with shared edge bands and a simple contour. New
crown/bough artwork shares its grey palette and native 40/24 and 0/30 pivots.

Run bash tools/build_buna_classic.sh to generate:
- BunaClassic.png: 176x264, 20px frames/22px stride, native roots and joints.
- BunaClassic_Tops.png: 128x112 crown with a 2px trailing gutter.
- BunaClassic_Branches.png: 40x40 boughs, two columns/three rows, 42px stride.

Gallery layout 9 shows V1 left (17), normal V2 center (43), tall V2 right (164).
Native GrowTree/GrowEpicTree supply random height, roots, branches, axe behavior,
wood drops and saves. Regional art is restricted to V2 roots; V1 and trees
elsewhere keep their own renderers. ModTree registration supports legacy grove
soil until migration. No whole-island rollout or autonomous growth is added.

check_buna_classic.py validates the first variant's approved pixel signatures,
all native frame alpha, 16px stem width, 2x2 grid and attached leaf/wood sockets.
native-fixture.json is exported by the loaded-game validation harness. The
checker renders those actual native cells into comparison.png alongside V1,
and verifies attachment of each full crown/bough to the stem. This is a static
zero-wind comparison; graphical-client review remains pending.

Native generation may choose a broken top; initial V2 samples normalize their
cap to the living-crown marker. Chopped stumps still keep native broken tops.

0.4.31: the 16px stem profile extends through the full 24px crown neck, removing
an 8px bottleneck. Bottom edge RGBA matches every native straight-body style.
The final 8px collar is drawn without rotation to retain contact during wind.
The checker exports neck-closeup.png from actual packed pixels. V1, V2 stem
and branch PNGs remain unchanged.
