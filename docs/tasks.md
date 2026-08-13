# Task board

Everything both of us are building for the demo, in one place, so neither of us
has to guess what the other is doing.

- **Demo deadline: 15 September.**
- Sizes: **S** ≈ half a day · **M** ≈ 1–3 days · **L** ≈ a week or more.
- Step-by-step instructions: **[tasks-viki.md](tasks-viki.md)** · **[tasks-leta.md](tasks-leta.md)**
- Design bible: **[concept.md](concept.md)**

## Who edits what

To stop us fighting over the same file:

| File | Edited by | How |
|---|---|---|
| `tasks.md` (this file) | **Viki only** (he owns production tracking) | on `dev` |
| `tasks-viki.md` | Viki | on `vickussya`, then PR to `dev` |
| `tasks-leta.md` | Leta | on `leatrix_`, then PR to `dev` |
| `concept.md` | either | **on `dev` only**, never on a personal branch |

Tick your own boxes in your own file. Nobody edits the other person's file.

## Done already

| Stage | What | Status |
|---|---|---|
| 1 | Greybox: capsule walks, looks, jumps | ✅ |
| 2 | Vertical slice: marker → smell → vision → clue logged | ✅ |
| 3 | Full case logic | ⬜ blocked on task 1 |

## The board

| # | Task | Category | Size | Owner | Blocked by | Blocks |
|---|---|---|---|---|---|---|
| 1 | Write Part 5: case truth + clue chain | Design | L | **Viki** | — | 4, 5, 7, 24 |
| 2 | Write Part 4: characters + scent signatures | Design | M | **Viki** | — | 6 |
| 3 | Clue / case data model (settles 8.4) | Systems | M | **Leta** | — | 4, 8, 24 |
| 4 | Case gating: N clues unlock the finale | Systems | M | **Viki** | 1, 3 | 7 |
| 5 | Client conversation → case start | Systems | M | **Viki** | 1, 6 | — |
| 6 | Dialogue system (bubbles + choices) | Systems | L | **Viki** | 2 | 5, 7 |
| 7 | Final confrontation | Systems | L | **Viki** | 1, 4, 6 | — |
| 8 | Case file UI | UI | L | **Viki** | 3 | — |
| 9 | Vision play / replay / close controls | UI | M | **Viki** | — | — |
| 10 | Main menu + results screen | UI | M | **Viki** | — | — |
| 11 | Bunk character model | 3D art | L | **Leta** | — | 12, 16 |
| 12 | Bunk rig + animations (incl. deep breath) | Animation | L | **Leta** | 11 | 19 |
| 13 | Demo district layout (greybox) | Level | M | **Leta** | — | 14, 24 |
| 14 | Environment modular kit | 3D art | L | **Leta** | 13 | 15 |
| 15 | Environment materials + textures | Textures | L | **Leta** | 14, 20 | — |
| 16 | Character textures | Textures | M | **Leta** | 11, 17 | — |
| 17 | 2D vision style guide | 2D art | S | **Leta** | — | 16, 18a, 18b, 20 |
| 18a | 2D vision animations — Viki's half | 2D art | M | **Viki** | 17 | 19 |
| 18b | 2D vision animations — Leta's half | 2D art | M | **Leta** | 17 | 19 |
| 19 | Swap placeholder → real vision player | Systems | S | **Viki** | 12, 18a, 18b | — |
| 20 | Noir grade + post-processing | Rendering | M | **Leta** | 17 | 15 |
| 21 | Scent visual effect | VFX | M | **Leta** | 20 | — |
| 22 | Ambience + SFX | Audio | M | **Leta** | — | 23 |
| 23 | Audio mixer + integration | Audio | S | **Leta** | 22 | — |
| 24 | Level scripting: place markers, wire chain | Level | M | **Viki** | 1, 3, 13 | — |
| 25 | Early test build + build pipeline | Tooling | S | **Viki** | — | — |
| 26 | Milestone tracking / triage | Production | M | **Viki** | — | — |

## Balance

| | Tasks | Effort (S=1, M=2, L=3) |
|---|---|---|
| **Viki** (`vickussya`) | 14 | **30** |
| **Leta** (`leatrix_`) | 13 | **28** |

Viki owns systems, narrative and production. Leta owns the art pipeline — model,
rig, texture, look, VFX, audio — plus the clue data model. Task 18 is split
evenly by agreement.

## Where our work meets

Agree these **before** building either side, or one of us redoes work.

| Interface | Provider | Consumer | Agree first |
|---|---|---|---|
| Clue data shape | Leta (3) | Viki (4, 8, 24) | Field names, how a clue is authored. **Viki is blocked until this lands.** |
| Deep-breath animation | Leta (12) | Viki (19) | Trigger name + exact length in seconds |
| 2D vision files | Both (18a/18b) | Viki (19) | One export format, resolution and length for both halves |
| District scene | Leta (13) | Viki (24) | **Scene ownership** — one of us in `SampleScene.unity` at a time |

## What we cut if we run out of time

In this order. `concept.md` Part 7 already names these as expendable, so cutting
them is following the plan, not failing it.

1. Scent visual effect (21)
2. Menus, down to a title card (10)
3. Character textures, down to flat colours (16)

**Never cut 1 or 3** — everything downstream depends on them.

Already deferred past the demo: red herrings, smell stamina, the wolf-restraint
meter, the antidote, vision replay degradation, third-person and reflections,
the units economy, save systems, the library/inventory tabs, editor tooling.
