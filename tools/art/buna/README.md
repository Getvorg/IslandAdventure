# Buna / Japanese beech — Fagus crenata

Entirely new original artwork generated for this request; no removed tree art,
vanilla tree textures, old crown or branch layouts were reused as references.
Botanical references describe smooth grey bark, rounded spreading crown and
low branches: NC State Extension (https://plants.ces.ncsu.edu/plants/fagus-crenata/)
and RHS (https://www.rhs.org.uk/plants/21090/fagus-crenata/details).
Forest atmosphere: sacred Japanese woodland inspired by Princess Mononoke.

source.png is the new whole-tree master. build_buna_assets.sh establishes binary
alpha, a common 40-color palette and an exact 2x2 grid at 288x448 game pixels.
layout.json records its 136px root anchor and 28 native stem cells. Stump bark
is sampled from the same new artwork; no unrelated bark material is used.

The image is drawn in 16px horizontal bands by the native stem cells. Branches
and forks therefore remain part of one original drawing instead of being joined
from separate assets. A root-based wind phase shifts bands gently by 2px steps.
Middle gallery sample is mirrored with its own corrected anchor.

Sample positions are the three existing gallery points. The gallery uses native
Tree tile identity for axe removal, grass support and saves. Chopping removes
upper native cells; without the top marker remaining cells show bark only and
keep the original base. Side boughs are visual parts of this first prototype,
not separate axe-target cells. TryGrow checks the whole visual footprint and
skips foreign blocks/liquid. No autonomous tree growth or whole-island placement
is enabled. /grovepreview rebuilds samples without requiring a new world.

Validation: check_buna_art.py verifies connected silhouette, binary alpha and the
2x2 grid. Given a JSON strip export from the loaded-game harness, it reassembles
all 28 bands for each sample, including mirrored art, and compares exact pixels
to the original image. This is a static rendering check; client review remains.
