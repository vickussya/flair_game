# Task board

Everything both of us are building, in one place, so neither of us has to guess
what the other is doing.

- Sizes: **S** ≈ half a day · **M** ≈ 1–3 days · **L** ≈ a week or more.
- Step-by-step instructions: **[tasks-viki.md](tasks-viki.md)** · **[tasks-leta.md](tasks-leta.md)**
- Design bible: **[concept.md](concept.md)** — the case is Part 5, "The Hollow Vial".

## What we are building

**One finished level, done by 30 April**, submitted in May. Not a rough demo that
gets replaced later — the real thing, built once, to final quality.

**1 September is a progress checkpoint, not a deliverable.** We show the teachers
what exists. It still has to *run* — "playable proof" is what was promised, and a
thing that only opens in the editor does not count. But it does not have to be
finished, and nothing is built twice to make that date.

That is the change from the old plan: there is no longer a rough version and a
finished version. There is one build, and it gets further along.

## Who edits what

| File | Edited by | How |
|---|---|---|
| `tasks.md` (this file) | **Viki only** (he owns production tracking) | on `dev` |
| `tasks-viki.md` | Viki | on `vickussya`, then PR to `dev` |
| `tasks-leta.md` | Leta | on `leatrix_`, then PR to `dev` |
| `concept.md` | either | **on `dev` only**, never on a personal branch |

## Done

| | What |
|---|---|
| ✅ | **Stage 1** — greybox: capsule walks, looks, jumps |
| ✅ | **Stage 2** — vertical slice: marker → smell → vision → clue logged |
| ✅ | **25** — test build, runs on another machine |
| ✅ | **1** — Part 5 written: "The Hollow Vial" |

## Phase 1 — the walkable spine · by **1 September**

Goal for the checkpoint: **walk the block, smell three clues, watch one real
vision, see the clue count fill.** That is a game, not a slideshow.

| Due | # | Task | Size | Owner |
|---|---|---|---|---|
| **19 Aug** | 3 | Clue / case data model — **blocks Viki** | M | **Leta** |
| **21 Aug** | 17 | 2D vision style guide | S | **Leta** |
| **24 Aug** | 13 | Block greybox: street, plaza, alley, droggery, church, Doctor's, office | M | **Leta** |
| **24 Aug** | 4 | Clue gating — 3 clues unlock the finale | M | **Viki** |
| **27 Aug** | 20 | Noir grade, first pass | M | **Leta** |
| **27 Aug** | 24 | Level scripting — place and wire clues 1, 2, 3 | M | **Viki** |
| **30 Aug** | 18a/18b | One 2D vision, drawn together | M | **both** |
| **31 Aug** | 19 | Swap placeholder → real vision player | S | **Viki** |
| **1 Sep** | — | **Checkpoint: build it and run it elsewhere** | — | both |

## Phase 2 — systems · **September → December**

The case only works if these exist. This is the heaviest phase and it is almost
all Viki, which is the main scheduling risk on the board.

| Due | # | Task | Size | Owner |
|---|---|---|---|---|
| Sep | 33 | Hologram scene — four customers, scent matching *(new)* | M | **Viki** |
| Sep | 39 | Hologram visual style — semi-transparent, static wipe *(new)* | S | **Leta** |
| Oct | 6 | Dialogue system (bubbles + choices) | L | **Viki** |
| Oct | 34 | Mind inventory / deduction board, 6.10 *(new)* | L | **Viki** |
| Oct | 41 | Interiors: droggery, church, Doctor's, office *(new)* | L | **Leta** |
| Nov | 2 | Dialogue script — every line the cast speaks | M | **Viki** |
| Nov | 5 | Client conversation → case start | M | **Viki** |
| Nov | 11 | Bunk model | L | **Leta** |
| Dec | 35 | Midnight clock, advancing per action *(new)* | S | **Viki** |
| Dec | 36 | Units economy + level results *(new)* | M | **Viki** |
| Dec | 8 | Case file UI | L | **Viki** |
| Dec | 12 | Bunk rig + animations | L | **Leta** |

## Phase 3 — the finale, content and art · **January → March**

| Due | # | Task | Size | Owner |
|---|---|---|---|---|
| Jan | 32 | Fight sequence — 2D vision playback, timed inputs, win/lose branch *(new)* | M | **Viki** |
| Jan | 37 | The Penitent: model, rig, mask-tear reveal *(new)* | L | **Leta** |
| Jan | 14 | Environment modular kit | L | **Leta** |
| Feb | 7 | Final confrontation — accusation, then the fight | L | **Viki** |
| Feb | 40 | Draw the fight vision — Bunk vs the Penitent, the mask-tear *(new)* | M | **Leta** |
| Feb | 42 | Physical verbs — door shoulder, lock break, collar grab: animations *(new)* | S | **Leta** |
| Feb | 43 | Physical verbs — triggers and hookup *(new)* | S | **Viki** |
| Feb | 15 | Environment materials + textures | L | **Leta** |
| Feb | 27 | Red herrings behaving as red herrings — Borlow, Rook | M | **Viki** |
| Mar | 18+ | Remaining 2D visions | L | **split evenly** |
| Mar | 9 | Vision play / replay / close controls | M | **Viki** |
| Mar | 10 | Menu + results screen | M | **Viki** |
| Mar | 38 | Barbara: model and rig *(new)* | M | **Leta** |
| Mar | 16 | Character textures | M | **Leta** |
| Mar | 31 | UI visual design: case file, board, menus | M | **Leta** |
| Mar | 21 | Scent visual effect | M | **Leta** |
| Mar | 22 | Ambience + SFX | M | **Leta** |
| Mar | 23 | Audio mixer + full pass | S | **Leta** |

## Phase 4 — integration · **April**

| # | Task | Size | Owner |
|---|---|---|---|
| — | Bug fixing, balance, play-testing on other people | L | both |
| — | Final build | S | **Viki** |
| 26 | Milestone tracking | — | **Viki**, throughout |

**April is for integration, not building.** Everything above lands in March.
Anything still under construction in April should have been cut in January.

## Balance

| | Tasks | Effort (S=1, M=2, L=3) |
|---|---|---|
| **Viki** | 19 | **39** |
| **Leta** | 20 | **42** |

Viki owns systems, narrative and production. Leta owns the art pipeline and the
clue data model. Task 18 is split evenly by agreement.

## Where our work meets

| Interface | Provider | Consumer | Agree first |
|---|---|---|---|
| Clue data shape | Leta (3) | Viki (4, 24, 8, 34) | Field names, how a clue is authored. **Viki is blocked until this lands.** |
| Deep-breath animation | Leta (12) | Viki (19) | Trigger name + exact length in seconds |
| Hologram look | Leta (39) | Viki (33) | How a hologram reads and how the static wipe plays |
| 2D vision files | Both (18) | Viki (19) | One export format, resolution and length |
| The fight vision | Leta (40) | Viki (32) | Where the timed inputs fall in the animation, and how the win and lose branches differ |
| The block | Leta (13, 41) | Viki (24) | **Scene ownership** — one of us in the scene at a time |

## Risks

**There is no 3D combat system, by decision** (see systems.md 6.8). The fight is a
2D vision played through the pipeline that already works. That removed the
biggest risk on this board — a whole new system neither of us knows how to build —
without removing the fight, which is part of who Bunk is and which level 2's
transformation needs to exist.

**The deduction board (34) is now the largest unknown.** It is the piece with no
precedent, it is what the case resolves through, and it is on the never-cut list.
If it looks shaky in October, say so then rather than in February.

**Phase 2 is Viki-heavy.** Six systems tasks in four months against Leta's art,
which parallelises better. Watch it in October.

**The 1 September checkpoint is 12 days away** and depends on task 3 landing now.

## If we run out of time

Cut in this order. Nothing here breaks the level.

1. Scent visual effect (21)
2. Menus, down to a title card (10)
3. Vision replay controls (9) — keep close
4. Physical verbs (42, 43) — flavour, not structure
5. The fight's timed inputs (part of 32) — the fight vision still plays, it just
   resolves on evidence rather than reflexes

**Never cut 3, 4, 24, 34 or the final build.** The data model, the gating, the
clue placement and the deduction board are the game.

## Not in this level

Werewolf transformation and the wolf-restraint meter (6.7). By decision the game
contains exactly **one** transformation, at the **final level** — so no meter, no
antidote and no tame/untame states are needed here or in level 2. Level 1's
finale is a human-form fight.

Also out: smell stamina, vision replay degradation, save systems, the library
tabs, editor tooling, further levels.
