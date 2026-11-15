# Buna II — shared wood/leaf component masters (0.4.33)

The user keeps Buna I and the accepted 16px Buna II stem, but requests coherent
branches/crown built from that stem rather than adjusted independent pictures.
Buna.png, BunaStump.png, BunaClassic.png and the V1 source are unchanged.

Current pipeline: build_buna_classic.sh -> pack_buna_classic.py (same stem) ->
build_buna_joints.py. The accepted whole Buna I supplies the crown silhouette,
visible wood mask and leaf layer. Hidden inner wood is connected through the
existing leaf silhouette before extraction. All wood uses the accepted stem's
own cross-section/contour palette. Leaves are overlaid only after wood is whole.
No generated replacement trunk/fork is pasted onto the image.

frame-layout.png and joint-layout.png provide native geometry only, from the
MIT-licensed tModLoader ExampleMod (license included):
https://raw.githubusercontent.com/tModLoader/tModLoader/stable/ExampleMod/Content/Tiles/Plants/ExampleTree.png
https://raw.githubusercontent.com/tModLoader/tModLoader/stable/ExampleMod/Content/Tiles/Plants/ExampleTree_Branches.png
Pink guides and grey leaf placeholders are not copied as game art. Three
native forms per side are retained, never a repeated branch image. Actual child
arm pixels from the unchanged stem are composed into the same node before the
40px branch is extracted. All overlapping interface pixels are RGBA-identical.
Old independent foliage-source.png/crown-source.png are no longer used/retained.

Outputs: stem 176x264; crown 136x112 with native trailing gutter (138x114);
branches 40x40, 42px stride, 2 columns/3 distinct native styles. Crown axis x68.
The short crown collar and native child arm remain fixed during tip sway.
Legacy ModTree foliage settings retain branch frame 0/1/2 and use crown frame0.

Gallery layout9 is unchanged: V1 left (17), normal V2 center (43), tall V2 right
(164). Native growth, branch chopping, wood, saving and outside trees remain.
Run check_buna_classic.py for source signatures, native alpha, grid/width,
six arm forms, exact child-frame overlap, crown seam and actual native fixture
contacts. It renders comparison.png and neck-closeup.png. wood-joints.png shows
the connected wood before leaf composition. Static review is complete; review
in the graphical game client remains pending.
