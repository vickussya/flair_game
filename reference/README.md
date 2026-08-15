# Reference

Visual reference for the look of the game. Mood boards, style targets and
generated concepts — **not** production assets.

Nothing here is imported by Unity. That is the point of keeping it at the repo
root: anything under `Assets/` gets a `.meta` file and is pulled into the asset
database, which reference images have no business being in.

The written counterpart is [docs/concept.md](../docs/concept.md) — Part 1 for the
palette, Part 3 for the world, **Part 3.5 for the map**.

## Folders

### `game_map/`
The Bismarkstrasse block: the isometric street view and the case-file floorplan.

These were generated from the Part 3.5 description, so the two match — the dome
ribs and red hazard lamps, the dead fountain with its headless statue, the
droggery's outward-shattered window and red witch-mark sign, the blocked tiled
arch, the sunken tram rails, and the HUD showing `CLUES 1/3` with the nose-glyph
stamina bar.

Treat these as the layout target for the greybox (task 13). Where an image and
Part 3.5 disagree, the doc wins — it is the thing both of us edit.

### `environment/`
Street, architecture and atmosphere targets: the domed ceiling, the wet
chiaroscuro lighting, decayed masonry with futuristic elements grafted on.
Feeds the district layout, the modular kit and the material pass (tasks 13, 14, 15).

### `Bunk_character/`
Look and silhouette for Bunk. Feeds the model and character textures (tasks 11,
16). Part 4 has the written character: bold, clever, physically strong, black
sense of humour, deep voice.

### `2D_cutscenes/`
Style targets for the hand-drawn visions. Feeds the style guide (task 17), which
in turn gates every vision either of us draws.

## Rules

**Three colours only — black, white, blood-red.** Part 1 is explicit about what
each means: black is mystery, white is truth, red is crime and urgency. An image
in here that leans on other hues is a mood reference, not a style target, and
should be labelled as such.

**These files go through Git LFS.** `.png` and `.jpg` are covered by
`.gitattributes`, so they do not bloat the git history — but **every version
counts against the quota**. Re-saving a 5 MB image ten times costs 50 MB, not 5.
Replace files rather than accumulating `_v2`, `_final`, `_final2`.

GitHub's free tier is 1 GiB of LFS storage and 1 GiB of bandwidth a month. This
folder is currently ~19 MB.

**Keep video out.** It is the worst value-per-megabyte here. Link it rather than
committing it.

**Rename what you add.** Prefer `droggery-interior-01.png` over
`c56e0a5e-eed5-44d1-b48c-f43e8996d7c0.jpg`. Descriptive names make these
referenceable from the docs; hashes and download names do not.
