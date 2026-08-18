# FLAIR (Bunk Romero)

A neo-noir detective game set on Haxan, a planet whose sun is dying into a black
hole. You play Bunk Romero, a werewolf private eye who solves crimes by smell:
you investigate in 3D, and every true lead pulls you into a hand-drawn 2D
"vision" that reconstructs the crime.

Unity 6 (`6000.5.7f1`), URP, C#.

Two milestones: a **rough playable demo by 15 September** as proof of concept for
the teachers, and the **finished one-level game by May** as the bachelor project.
Further levels come after that. See [docs/tasks.md](docs/tasks.md).

## Documentation

The design bible is split by subject. **[docs/concept.md](docs/concept.md) is the
index** — start there; it says where everything lives.

| File | What is in it |
|---|---|
| [docs/concept.md](docs/concept.md) | Pitch and design pillars, plus the index |
| [docs/world.md](docs/world.md) | Source fidelity, the world, the map of the block |
| [docs/case.md](docs/case.md) | The cast, and Level 1: "The Hollow Vial" |
| [docs/systems.md](docs/systems.md) | Gameplay systems — the programming spec |
| [docs/production.md](docs/production.md) | Content list, technical design, decision gate |
| [docs/tasks.md](docs/tasks.md) | The live board: who builds what, by when |

> **Rule: the design docs are only ever edited on `dev`** — directly, or via a
> short-lived PR. Never on a personal branch. Two people editing the same design
> doc on separate branches produces drift and merge conflicts that are tedious to
> untangle and easy to resolve wrongly.

[COLLABORATION.md](COLLABORATION.md) covers the Unity merge driver setup and the
scene/prefab ownership convention.

## Branches

| Branch | Purpose |
|---|---|
| `main` | Releases only. Tagged, tested builds. Never commit directly. |
| `dev` | Integration branch and source of truth for development. |
| `vickussya` | Viki's working branch. Branches off `dev`. |
| `leatrix_` | Leta's working branch. Branches off `dev`. |

**Flow:** personal branch → PR into `dev` → when `dev` is stable and tagged →
merge into `main`.

## Getting started

1. Install Unity **6000.5.7f1** exactly. A different patch version silently
   rewrites `ProjectSettings/` and asset serialization, and those diffs land in
   git.
2. Install [Git LFS](https://git-lfs.com/) before cloning — art assets are
   stored through it. Run `git lfs install` once per machine.
3. Clone, then check out `dev` and branch from there.
4. Set up the UnityYAMLMerge driver — see [COLLABORATION.md](COLLABORATION.md).
5. Open the project in Unity and load `Assets/Scenes/SampleScene.unity`.

## Current state

The live plan is [docs/tasks.md](docs/tasks.md).

- ✅ **Stage 1 — Greybox / prototype.** First-person capsule, walk / look / jump.
- ✅ **Stage 2 — Vertical slice.** Walk to a scent marker, hold to smell, camera
  leaves the eyes to frame Bunk, crosses into a placeholder 2D vision, returns
  control and logs a clue.
- ✅ **The case is written** — [docs/case.md](docs/case.md) Part 5, "The Hollow Vial".
- ⬜ **Phase 1 — the walkable spine**, by 1 September: walk the block, smell three
  clues, watch one real vision, see the clue count fill.

We are building one finished level, done by **30 April**. 1 September is a
progress checkpoint, not a separate deliverable.
