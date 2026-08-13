# Task board

Everything both of us are building, in one place, so neither of us has to guess
what the other is doing.

- Sizes: **S** ≈ half a day · **M** ≈ 1–3 days · **L** ≈ a week or more.
- Step-by-step instructions: **[tasks-viki.md](tasks-viki.md)** · **[tasks-leta.md](tasks-leta.md)**
- Design bible: **[concept.md](concept.md)**

## The two milestones

These are different deliverables and confusing them will wreck the schedule.

### M1 — Demo · **done by 1 September** · hard deadline **15 September**

Proof to the teachers that we can build a game. **Playable and rough. Not
finished, and not pretending to be.** It has to hold together for five minutes
and show the idea working end to end: walk the city, catch a scent, watch a real
2D vision, learn something, reach an ending.

What makes M1 succeed is *coverage*, not polish — one of everything, visibly
working. A rough model under a good grade with a real vision beats a beautiful
street with a placeholder panel.

**We finish on 1 September, not the 15th.** The fortnight between is buffer, and
it is not optional: it absorbs the build that breaks on another machine, the
vision that exports wrong, the week someone is ill. A plan with no slack fails on
the first bad day. If we are done early, the buffer becomes polish.

**That leaves 19 days from 13 August.** It is tight, and it is the reason every
M1 task says "rough".

### M2 — Bachelor project · **done by 30 April** · due **May**

The finished one-level game. Ready to play, looking finished, start to end:
dialogue, the full case, the confrontation, real art, real audio, menus.

Everything in M1 gets **finished** here, not replaced. Nothing built for the demo
should be throwaway.

Same logic: finish end of April, keep May for submission, presentation and the
problems we have not thought of yet.

### M3 — After the bachelor

Further levels and the bigger systems. Not scheduled.

## Who edits what

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

Stage 2 means the spine of M1 already runs. The demo is mostly a matter of
replacing placeholders with rough-but-real content.

## M1 — the 15 September demo

"Rough" below is deliberate and is not an excuse — it is the correct amount of
work for this milestone.

Ordered by due date. Dates are when the task is **finished**, not started.

| Due | # | Task | Size | Owner | M1 scope |
|---|---|---|---|---|---|
| **Sat 16 Aug** | 25 | Test build on another machine | S | **Viki** | Do it first. A demo that fails to launch is not a demo. |
| **Mon 17 Aug** | 17 | 2D vision style guide | S | **Leta** | Full. Cheap, and gates all vision art. |
| **Wed 19 Aug** | 3 | Clue / case data model | M | **Leta** | Full. Everything reads it — build it properly once. **Viki is blocked until this lands.** |
| **Wed 19 Aug** | 1 | Case skeleton | M | **Viki** | **Rough:** the truth + a 3-clue chain. Full Part 5 is M2. |
| **Sun 23 Aug** | 13 | District layout | M | **Leta** | **Rough:** greybox. One street, one interior, one crime scene. |
| **Sun 23 Aug** | 20 | Noir grade | M | **Leta** | **Rough pass.** Highest visual impact per hour on the board. |
| **Tue 26 Aug** | 11 | Bunk model | M | **Leta** | **Rough:** silhouette and scale. Detail is M2. |
| **Tue 26 Aug** | 18a | 2D vision — Viki's half | M | **Viki** | **One vision each**, not the full set. |
| **Tue 26 Aug** | 18b | 2D vision — Leta's half | M | **Leta** | Same. Agree the format before either starts. |
| **Wed 27 Aug** | 4 | Clue gating | S | **Viki** | **Minimal:** 3 clues collected → ending fires. |
| **Fri 28 Aug** | 24 | Level scripting | M | **Viki** | **Rough:** 3 markers placed and wired. |
| **Fri 28 Aug** | 12 | Bunk rig + breath animation | M | **Leta** | **Rough:** idle + the deep breath. Hand Viki the clip length. |
| **Sat 30 Aug** | 19 | Swap placeholder → real vision player | S | **Viki** | Full. The payoff for how Stage 2 was built. |
| **Sat 30 Aug** | 22 | Footsteps + smell cue + ambience | M | **Leta** | **Minimal:** three sounds. Huge effect for the cost. |
| **Sun 31 Aug** | 9 | Vision close / replay button | S | **Viki** | **Minimal:** close is enough. Replay if cheap. |
| **Sun 31 Aug** | 10 | Title card + end card | S | **Viki** | **Minimal:** not a menu system. |
| **Mon 1 Sep** | — | **Demo playable end to end. Build it and run it elsewhere.** | — | both | Everything integrated. |
| ongoing | 26 | Milestone tracking | M | **Viki** | Weekly check against this table. |

**M1 balance — Viki 8 tasks / 13 effort · Leta 7 tasks / 15 effort.**

### The three dates that matter most

- **17–19 Aug.** Leta's tasks 17 and 3 unblock everything else. If these slip,
  the whole schedule slips behind them and no amount of later effort recovers it.
- **28 Aug.** Task 12 must reach Viki with the breath clip length, or task 19 has
  nothing to time the transition against.
- **1 Sep.** Integrated and built. Not "nearly done".

## Week by week

| Week | Viki | Leta |
|---|---|---|
| **13–19 Aug** | Test build · rough case skeleton | Style guide · clue data model |
| **20–26 Aug** | His vision · start level scripting | District greybox · noir grade · Bunk model · her vision |
| **27–31 Aug** | Gating · level scripting · vision player swap · close button · cards | Rig + breath · three sounds |
| **1 Sep** | **Integrate, build, test elsewhere** | **Integrate, build, test elsewhere** |
| **2–15 Sep** | Buffer. Fix what the build breaks; polish if it does not. | Buffer. |

## M2 — the finished level, done by 30 April

Everything from M1, finished, plus the systems and art the demo skipped. Month by
month, so a nine-month runway does not quietly become a two-month panic.

| Due | # | Task | Category | Size | Owner |
|---|---|---|---|---|---|
| **Sep** | — | Demo retrospective — what broke, what to redo properly | — | S | both |
| **Sep** | 1+ | Full Part 5: complete clue chain + red herring | Design | M | **Viki** |
| **Sep** | 2 | Part 4: characters + scent signatures | Design | M | **Viki** |
| **Oct** | 14 | Environment modular kit | 3D art | L | **Leta** |
| **Oct** | 6 | Dialogue system (bubbles + choices) | Systems | L | **Viki** |
| **Nov** | 5 | Client conversation → case start | Systems | M | **Viki** |
| **Nov** | 11+ | Finish Bunk: detailed model + full animation set | 3D art | L | **Leta** |
| **Dec** | 8 | Case file UI (code) | UI | L | **Viki** |
| **Dec** | 15 | Environment materials + textures | Textures | L | **Leta** |
| **Jan** | 7 | Final confrontation | Systems | L | **Viki** |
| **Jan** | 16 | Character textures | Textures | M | **Leta** |
| **Jan** | 31 | UI visual design: case file, menus, HUD | 2D art | M | **Leta** |
| **Feb** | 18+ | Remaining 2D visions | 2D art | L | **split evenly** |
| **Feb** | 10+ | Full menu + results screen | UI | M | **Viki** |
| **Mar** | 27 | Red herrings behaving as red herrings | Systems | M | **Viki** |
| **Mar** | 13+ | Finish district: lighting + set dressing | 3D art | M | **Leta** |
| **Mar** | 21 | Scent visual effect | VFX | M | **Leta** |
| **Mar** | 23 | Audio mixer + full pass | Audio | M | **Leta** |
| **Apr** | — | **Integration, bug fixing, final build, play-testing** | — | L | both |

**Stretch — only if the above is genuinely finished:**

| # | Task | Size | Owner |
|---|---|---|---|
| 28 | Smell stamina / over-smell | M | **Viki** |
| 29 | Wolf-restraint meter | L | **Viki** |
| 30 | Vision replay degradation | M | **Viki** |

These are the concept's signature mechanics, and `concept.md` Part 7 names them
as the first things to cut. A finished level without them beats an unfinished one
with them.

**April is for integration, not building.** Every task above lands in March at the
latest. Anything still being built in April is a task that should have been cut in
February.

**M2 balance is provisional** — Viki carries more systems work than Leta carries
art. Revisit it at the September retrospective, when we actually know how fast we
each work.

## M3 — after the bachelor

Antidote crafting · units economy · save/persistence · third-person and
reflections · library and inventory tabs · clue-authoring editor tools ·
performance pass · further levels.

## Where our work meets

Agree these **before** building either side, or one of us redoes work.

| Interface | Provider | Consumer | Agree first |
|---|---|---|---|
| Clue data shape | Leta (3) | Viki (4, 24, later 8) | Field names, how a clue is authored. **Viki is blocked until this lands.** |
| Deep-breath animation | Leta (12) | Viki (19) | Trigger name + exact length in seconds |
| 2D vision files | Both (18a/18b) | Viki (19) | One export format, resolution and length |
| District scene | Leta (13) | Viki (24) | **Scene ownership** — one of us in `SampleScene.unity` at a time |

## If M1 starts slipping

Cut in this order:

1. **18a/18b down to one vision total** instead of one each
2. **Vision replay button** (9) — close alone is enough
3. **Ambience** (22) — keep footsteps and the smell cue, drop room tone
4. **Bunk model and rig** (11, 12) — fall back to the capsule and cut the breath beat

Cutting 4 costs the most: framing Bunk mid-transition is the moment that sells the
idea. Cut it last.

**Never cut 3, 19 or 25.** The data model blocks everything, the real vision *is*
the demo, and an unbuilt demo cannot be shown.
