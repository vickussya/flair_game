# Asset pipeline and storage

How art gets from Blender and Krita into the game, and how we stay inside our
storage budget — for this level and the levels after it.

Read this before committing anything binary.

---

## The one rule

> **If Unity imports it, it goes in the repo. If only a human opens it, it does not.**

| Human opens it — **keep out of git** | Unity imports it — **commit it** |
|---|---|
| `.blend` Blender scenes | `.fbx` exported models |
| `.kra` Krita paintings | `.png` exported textures and frames |
| `.psd` Photoshop | `.webm` rendered visions |
| Audio project files | `.wav` / `.ogg` sound |
| Render caches, sculpt layers | `.ttf` fonts |

This is not tidiness. A character `.blend` with sculpt layers and packed textures
runs 100–300 MB, and **every save you commit stores another whole copy**. Ten
saves of one file can cost more than the entire finished game.

The exports are what the game runs on, and they are ten to twenty times smaller.

## Where the source files live

**A shared cloud folder, not the repo.** OneDrive or Drive, mirroring this
structure:

```
FLAIR-source/
  characters/   bunk.blend, barbara.blend
  environment/  street-kit.blend, droggery.blend
  visions/      vision-01-hollow-vial.kra, ...
  audio/
```

Cloud storage keeps 30 days of file history, which covers "I broke it yesterday".
It does not give you branches or real version control on sources.

If that starts to hurt, the upgrade is a **second git repo** (`flair-source`).
Be clear about what it buys, though: **GitHub bills LFS per account, not per
repository**, so a second repo shares this one's quota and saves nothing on
storage. What it does buy is a game repo that stays small and fast to clone —
nobody should pull 5GB of sculpts to fix a script — plus real history on the
sources. Worth it when both of us are touching the same source files; not worth
it while we each own a separate character.

Either way, do not solve it by putting sources back in here.

`.gitignore` actively blocks `.blend`, `.kra` and friends, so an absent-minded
`git add -A` cannot cost you a data pack. If you ever genuinely need one tracked,
`git add -f` says you meant it.

## How a sculpted character becomes a game asset

The thing to know before starting Bunk or Barbara: **the sculpt is never the game
asset.**

| Step | Where it lives |
|---|---|
| 1. Sculpt — millions of triangles | `.blend`, source folder. Never enters Unity |
| 2. Retopologise — clean low-poly over the sculpt | `.blend` |
| 3. UV unwrap the low-poly | `.blend` |
| 4. Bake the high-poly detail down — normal, AO | outputs to the repo |
| 5. Texture — base colour, roughness, normal | PNG to the repo |
| 6. Rig — armature and weights | `.blend` |
| 7. Export FBX — low-poly, armature, the takes the game plays | FBX to the repo |

Step 4 is what makes the budgets below achievable rather than a fight. The
sculpt's detail becomes a normal map, so twenty million triangles ship as twenty
thousand and still read as sculpted.

**Triangle budgets:** 15-30k for Bunk, who gets a close-up in the vision push-in.
5-10k for anyone seen at a distance. At those counts an FBX lands inside the size
budget on its own.

**Export the rig as Humanoid.** It keeps Mixamo animation usable as a base for
locomotion even after the placeholder character is gone, which is a real shortcut
on the largest animation task we have.

**On the Mixamo placeholder in Assets/Characters/Bunk/:** it is 53MB because a
Mixamo download bundles the full skeleton and embedded textures at whatever size
they ship. Slimming a placeholder is wasted work, so it is committed as-is --
decided 15 Sep 2026, because the teacher wants an animated 3D character in the
demo early and both of us need the body to work against. It is committed ONCE:
do not commit new versions of it. When our own Bunk exists, that is a new file.

**The Mixamo animations are placeholders too.** `Bunk_Idle`, `Bunk_Walk` and
`Bunk_Sniff` import as Humanoid (enforced by `CharacterImportRules`), so they would
technically retarget onto the Bunk we sculpt. But for the diploma our animations
are what is graded, so they must not ship in the submission. Keep them as timing
and blocking reference while animating, then replace them. See the placeholder
register in tasks.md.

## Export budgets

Targets, not laws — but if something is five times over, find out why before
committing it.

| Asset | Target | Notes |
|---|---|---|
| Character FBX | **5–15 MB** | Bunk's first export was 53 MB. That is the failure case. |
| Prop / kit piece FBX | **< 2 MB** | |
| Texture | **2K max, 1K for most** | Only hero surfaces need 2K. Nothing needs 4K. |
| Vision video | **5–10 MB** | WebM VP8, 12 fps, 1024px |
| Sound effect | **< 1 MB** | OGG, mono for anything not music |

**Getting a character from 50 MB to 10 MB**, in order of payoff:

1. **Do not embed textures in the FBX.** Export them separately as PNG. Embedded
   textures cannot be compressed, swapped or shared between models.
2. **Apply and remove subdivision** before export. Export the cage, not the
   subdivided result.
3. **Decimate.** Bunk gets one close-up, in the vision push-in. He does not need
   film topology.
4. **Export only the collections you need** — no rigs-for-reference, no
   lighting setups, no duplicate meshes hiding on another layer.
5. **Trim the animation.** Export the takes the game plays, not every experiment.

## Committing binaries

**Replace files, never version them in the filename.** `bunk.fbx` edited five
times costs the same as `bunk_v1..v5.fbx` in storage, but the second one also
leaves four dead files in the project forever. Git is the version history.

**Commit at milestones, not at saves.** "Bunk blocked out", "Bunk rigged",
"Bunk textured" — three commits, not thirty. Each intermediate export you commit
is permanent weight in the repo.

**Always commit the `.meta` alongside the asset.** A texture without its `.meta`
gets a new GUID on the other person's machine and materials silently detach.

**Extract, do not embed.** When Unity imports an FBX with embedded materials,
use *Extract Textures* and *Extract Materials*. Extracted PNGs are smaller,
shared between models, and swappable without re-exporting anything.

## The budget

Rough estimate for the finished level:

| | Size |
|---|---|
| 2 characters, models + textures | ~60 MB |
| Environment kit + textures | ~130 MB |
| 5 vision videos | ~40 MB |
| Audio | ~20 MB |
| Reference images | ~30 MB |
| **Current state, roughly** | **~280 MB** |

Storage counts **every version ever committed**, not the current state. Four
meaningful revisions of the big assets over eight months puts the level near
**1 GB** — which is the entire free tier, with nothing left for level 2.

Bandwidth counts too: every clone and every pull that fetches changed binaries.
Two people working daily will move several times the storage figure over a year.

**So: a data pack is the right call, not an emergency measure.** GitHub's £/$5
a month buys 50 GB of storage and 50 GB of bandwidth — comfortably more than
this project and the next few levels will ever need. Doing it now removes a
recurring interruption that has already cost us one afternoon.

Check usage: GitHub → Settings → Billing and licensing → Git LFS Data.

## Planning for the next levels

The pattern holds, and most of the cost is one-off:

- **The environment kit is the reusable investment.** Build it modular and level 2
  is new arrangements of pieces that already exist, not new geometry. This is the
  single biggest saving available to us.
- **Characters are per-level and do not compress.** Every new speaking character
  is another 30 MB and weeks of animation. Reuse the cast wherever the story lets
  you — the book has recurring figures for a reason.
- **Give each level its own folder** — `Assets/Levels/01-hollow-vial/` — and keep
  shared things (`Assets/Kit/`, `Assets/Characters/`) outside them. Then a level
  can be cut or reordered without untangling it from the rest.
- **Revisit this document when the kit exists.** The budgets above are estimates
  made before anything real was built; once the street kit is done we will have
  actual numbers.

## A note on OneDrive, checked and cleared

The project sits under a OneDrive path (`OneDrive/Documenten/git/flair/`) because
Windows redirects Documents into OneDrive by default — nobody chose it.

**Sync is excluded for this folder, so it is not a problem.** Verified 8 Sep 2026:
OneDrive is running for the account, but `Library/` carries none of the cloud
placeholder attributes it would have if it were being synced.

Worth knowing rather than acting on, because it can come back:

- **On a new machine or a fresh Windows install**, the exclusion does not travel
  with you. Check it before opening the project.
- **Leta's clone may not be excluded.** If hers is under Documents and syncing,
  she has the problem this section was written about.

Why it matters when it does happen: OneDrive does not read `.gitignore`, so it
syncs `Library/` — gigabytes of pure cache — and can lock files while Unity is
mid-import, which surfaces as Unity corruption that looks like random bugs.

If it ever needs fixing, clone fresh outside OneDrive rather than moving the
folder: `Library/` is regenerable and a half-synced copy is not worth carrying.
