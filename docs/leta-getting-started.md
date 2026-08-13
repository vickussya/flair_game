# Leta — getting started

Everything you need to start working on FLAIR, in the order you need it.
Read [concept.md](concept.md) first — it is the design bible and the answer to
"why is it like this?" is almost always in there.

Your branch is **`leatrix_`**.

---

## 1. One-time machine setup

Do all five. Skipping any one of them causes problems that are hard to diagnose
later.

### 1.1 Unity — the exact version

Install **Unity `6000.5.7f1`** through Unity Hub. Not 6000.5.8, not 6000.4.

A different patch version silently rewrites `ProjectSettings/` and asset
serialization the moment you open the project, and those rewrites land in git as
changes you did not make. Everyone must be on the same build.

### 1.2 Git LFS

Install [Git LFS](https://git-lfs.com/), then once per machine:

```sh
git lfs install
```

Art assets (textures, models, audio) are stored through LFS. **Do this before
cloning** — clone first and you get 130-byte pointer files instead of real
images, which looks like corruption.

### 1.3 Clone and get on your branch

```sh
git clone https://github.com/vickussya/flair_game.git
cd flair_game
git switch leatrix_
```

### 1.4 The Unity merge driver

Scenes and prefabs are text files that git cannot merge sensibly on its own.
This points git at Unity's Smart Merge tool. Run inside the repo:

```sh
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/6000.5.7f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p --force --fallback-none %O %B %A %A'
git config merge.unityyamlmerge.recursive binary
```

Check it took:

```sh
git config --get merge.unityyamlmerge.driver
```

Empty output means it did not work. This setting lives in your local clone, so
it does **not** arrive through git — you have to run it yourself.

### 1.5 Open the project

Open the folder in Unity Hub, then load `Assets/Scenes/SampleScene.unity`.
Press Play: you should walk around a grey room, and holding **E** near the small
sphere should trigger the vision sequence.

---

## 2. Rules that are not negotiable

**Never commit to `main` or `dev`.** Work on `leatrix_`, open a pull request into
`dev`. `main` only ever receives finished, tagged releases.

**Never edit `docs/concept.md` on your branch.** It is edited on `dev` only. Two
copies of a 340-line design doc on two branches will drift and then conflict, and
resolving that conflict wrongly loses decisions. If you need a change, tell Viki
or open a tiny PR straight to `dev`.

**Commit `.meta` files with their assets, always.** Every file Unity imports gets
a `.meta` next to it holding its GUID. Commit a texture without its `.meta` and
Unity invents a new GUID on Viki's machine — materials detach from meshes,
components fall off prefabs, and the cause is invisible. If `git status` shows a
`.meta`, it goes in the commit.

**Say in chat before you edit `SampleScene.unity`.** One person in the scene at a
time. It is 2,000 lines and it does not merge cleanly. This matters most for your
district layout work, which lives entirely in that file — see §4.

---

## 3. Daily workflow

```sh
git switch leatrix_
git pull                          # your branch
git merge dev                     # take Viki's latest work

# ... do the work in Unity, save the scene ...

git add -A
git status                        # check .meta files are included
git commit -m "Add cobblestone material for the street kit"
git push
```

When a task is done, open a PR from `leatrix_` into `dev` on GitHub. Viki checks
it out and presses Play. Neither of you is reviewing code — the review is
"does it run and does it look right".

Merge `dev` into your branch **often**, ideally daily. Long-lived branches are
what turn a small scene conflict into an unfixable one.

---

## 4. Your tasks

Sizes: **S** ≈ half a day, **M** ≈ 1–3 days, **L** ≈ a week or more.
Demo deadline is **15 September**.

> **[tasks-leta.md](tasks-leta.md) has the step-by-step for every task below** —
> what it is, how to do it, and how to know it is done. The table here is just
> the running order. **[tasks.md](tasks.md)** shows what Viki is doing.

Do them roughly in this order — the first three unblock other people.

| # | Task | Size | Why this order |
|---|---|---|---|
| 1 | **Clue / case data model** | M | **Blocks Viki.** Decides `concept.md` 8.4 — how a clue is stored so a designer can add one without touching C#. Viki's gating and case-file UI both read whatever you define. Agree the shape with him before building on it. |
| 2 | **2D vision style guide** | S | Blocks all vision art. Black / white / red only. Settle line weight, contrast, frame rate, export format. |
| 3 | **Demo district layout** | M | Blocks the environment kit and Viki's marker placement. Greybox first, shapes and sightlines only, no art. |
| 4 | **Bunk character model** | L | Long lead time. Everything about him downstream waits on this. |
| 5 | **Bunk rig + animations** | L | Idle, walk, and the **deep breath**. The breath is not optional — the vision transition is built around a beat where it plays. See §5. |
| 6 | **Environment modular kit** | L | Street sections plus one interior, built to your §3 layout. |
| 7 | **Environment materials + textures** | L | Your main strength. Wait for the noir grade (#9) to be roughed in or you will texture against the wrong values. |
| 8 | **Character textures** | M | Bunk, to the same style guide. |
| 9 | **Noir grade + post-processing** | M | URP volume: black / white / red. Do a rough pass early — it changes how every texture reads. |
| 10 | **Scent visual effect** | M | The trail or screen-edge cue the player sees when smelling. `concept.md` 6.3: all scents look identical, true or false. |
| 11 | **2D vision animations — your half** | M | Split evenly with Viki by agreement. Coordinate so you are not both drawing the same one. |
| 12 | **Ambience + SFX** | M | Room tone, footsteps, the smell cue. |
| 13 | **Audio mixer setup** | S | Wire the above through a mixer. |

**Total: 13 tasks, 28 effort points** — near-identical to Viki's 14 tasks / 30
points.

Not on your list and not on his: red herrings, the wolf meter, the antidote,
save systems, third-person. Those are post-demo. `concept.md` Part 7 already
names them as the things to cut, so cutting them is following the plan, not
failing it.

---

## 5. Where your work meets Viki's

Four places need an agreement *before* either side builds. Get these wrong and
one of you rebuilds work.

| Interface | You provide | Viki consumes | Agree first |
|---|---|---|---|
| **Clue data** | The data shape (task 1) | Gating, case-file UI, marker wiring | Field names and how a clue is authored. **He is blocked until this exists.** |
| **Deep-breath animation** | An animation clip + its length (task 5) | Plays it during `VisionDirector.breathHoldDuration` | The trigger name and the exact duration in seconds. |
| **2D visions** | Your half of the animations (task 11) | Feeds them to the vision player | One export format and length for both halves, so a single player handles them. |
| **District layout** | The greybox space (task 3) | Places scent markers into it | **Scene ownership.** You both edit `SampleScene.unity`. Hand it over explicitly — finish the layout, push, tell him it is free. |

---

## 6. When something breaks

- **Textures look like 130-byte text files** → LFS was not installed before
  cloning. Run `git lfs install`, then `git lfs pull`.
- **Unity rewrites files you never touched** → wrong Unity version. Check
  `ProjectSettings/ProjectVersion.txt` says `6000.5.7f1`.
- **A scene merge conflict** → do not hand-edit the YAML. Pick one side, redo
  the smaller change in the editor. This is why the one-person-at-a-time rule
  exists.
- **A prefab or material lost its reference for no reason** → a `.meta` file was
  not committed. Find the asset, commit its `.meta`.

Anything else, ask Viki before working around it. A workaround in a Unity
project usually hides the real problem until it is expensive.
