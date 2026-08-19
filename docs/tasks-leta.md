# Leta — task instructions

Your 13 tasks, with what each one is, how to do it, and how to know it is done.
Overview of both our workloads: **[tasks.md](tasks.md)**. Machine setup and the
branch rules: **[leta-getting-started.md](leta-getting-started.md)**.

Branch: **`leatrix_`**. Tick your own boxes here; nobody else edits this file.

## Which of these are due when

Two separate deliverables — see [tasks.md](tasks.md).

**M1 — the rough demo. Finish by 1 September**, hard deadline the 15th. Playable
proof for the teachers, not a finished thing. **Rough is the correct amount of
work here, not a compromise** — a rough model under a good grade beats a
beautiful one with no grade. The fortnight after 1 Sep is buffer, not working time.

| Due | Task | M1 scope |
|---|---|---|
| **Mon 17 Aug** | 17 Style guide | Full — it gates all vision art |
| **Wed 19 Aug** | 3 Clue data model | Full — **Viki is blocked until this lands** |
| **Sun 23 Aug** | 13 District layout | Greybox only, no art |
| **Sun 23 Aug** | 20 Noir grade | Rough pass |
| **Tue 26 Aug** | 11 Bunk model | Rough: silhouette + scale |
| **Tue 26 Aug** | 18b Your vision | One vision, not the set |
| **Fri 28 Aug** | 12 Rig + breath | Rough: idle + breath. **Send Viki the clip length.** |
| **Sat 30 Aug** | 22 Sound | Three sounds: footsteps, smell cue, ambience |
| **Mon 1 Sep** | Demo integrated and built | — |

**Your first two tasks are the whole schedule's bottleneck.** Task 17 gates every
piece of vision art either of you draws, and task 3 blocks Viki from starting his
systems work at all. They are not the interesting tasks, and they are the ones
that must not slip.

**M2 — the bachelor project, May.** The finished level. Everything above gets
completed rather than replaced, plus: **14**, **15**, **16**, **21**, **23** and
the remaining visions.

The task descriptions below are written to the **finished** M2 standard. For M1,
do the rough version the board specifies and come back to it.

**Start with 3 and 17.** Neither is the interesting work, but task 3 blocks Viki
completely and task 17 gates every piece of vision art either of us draws.

---

## ⬜ 3. Clue / case data model (M)

**Do this first. Viki is blocked on it** — his gating, case file UI and level
scripting all read whatever shape you define.

This settles the open question in `concept.md` 8.4: how is a clue stored so a
designer can add one **without editing C#**?

1. Read 8.4. The three options are ScriptableObjects, hard-coded, or an external
   file (JSON/CSV). **ScriptableObjects are the recommended answer** — each clue
   becomes an asset you create from the Create menu and edit in the Inspector.
2. Look at `Assets/Scripts/ScentMarker.cs`. It currently holds the clue inline as
   plain fields: `clueId`, `displayName`, `isTrueScent`, `visionDuration`. That
   was deliberate — it kept 8.4 open. Your job is to lift those fields out into a
   proper data asset that a marker points at instead.
3. Decide the fields a clue needs: id, display name, what it is, where it points
   next (the chain from Part 5), true or red herring, which vision plays.
4. Build it, then make one real clue asset as a worked example.
5. **Agree the field names with Viki before you go far.** Renaming later means he
   rewrites code against it.
6. Write your decision into `concept.md` 8.4 — on `dev`, not your branch.

**Done when:** you can add a second clue entirely in the Unity Inspector, with no
code changes, and a `ScentMarker` can be pointed at it.

---

## ✅ 17. 2D vision style guide (S)

**Blocks tasks 16, 18a, 18b and 20.** Small but it gates a lot — do it early.

`concept.md` Part 1 is strict about the palette: black, white and red only. Red
is crime and urgency, black is the unknown, white is truth.

1. Draw 2–3 test frames in the intended style.
2. Pin down: line weight, contrast, how much red and what earns it, frame rate,
   resolution, export format.
3. Write it up with the test frames as reference.
4. Share it with Viki — he draws half the visions (18a) and both halves must match.

**Done when:** two people could draw a frame each and they would look like the
same game.

---

## ⬜ 13. Demo district layout (M)

**Blocks tasks 14 and 24.** Greybox only — shapes, no art.

1. Decide the district with Viki. `concept.md` Part 3 leaves it open.
2. Block it out with primitives in `SampleScene.unity`, the way the Stage 1 room
   is built. Streets, one interior, the crime scene.
3. Design for the loop in 6.1: room to explore, sightlines that pull you toward
   leads, somewhere the visions make sense.
4. Walk it in play mode. Distances feel wrong until you do.
5. **Scene ownership:** you and Viki both edit `SampleScene.unity`. Finish, push,
   and tell him explicitly it is free before he starts task 24.

**Done when:** you can walk the whole district and it feels like a place, with no
art in it at all.

---

## ⬜ 11. Bunk character model (L)

**Blocks tasks 12 and 16.** Long lead time — start early even though nothing is
blocked on it today.

1. Read Part 4 for who he is. Werewolf private eye who keeps the beast leashed —
   he should read as a man with something held down.
2. Model at a sensible poly budget. He is the only character with a close-up, in
   the vision transition, where the camera frames him from about 2 m.
3. Keep silhouette readable in near-black. The noir grade removes most of your
   colour information.
4. Build the topology for animation — he has to breathe deeply on camera.
5. Import to Unity, scale so he matches the 2 m player capsule.

**Done when:** he stands in the greybox scene at the right scale and reads
clearly under the noir grade.

---

## ⬜ 20. Noir grade + post-processing (M)

**Needs task 17. Blocks task 15.** Do a rough pass early — it changes how every
texture reads, and texturing against the wrong values wastes days.

1. The scene already has a `Global Volume`. Add your overrides there.
2. Build toward black / white / red per Part 1. Colour Adjustments to drain
   saturation, then bring red back deliberately.
3. Add grain, vignette and bloom to taste. Apocalyptic, retro, under an
   artificial dome — Part 3 says artificial light, no sun.
4. Check it against the greybox before any real art exists.
5. Lock a rough version and tell Viki, then refine later.

**Done when:** the greybox room reads as noir with no art in it.

---

## ⬜ 12. Bunk rig + animations (L)

**Needs task 11. Blocks task 19.** The deep breath is the one that matters most.

1. Rig him. Humanoid so Unity's retargeting is available.
2. Animate idle and walk.
3. **Animate the deep breath.** This is not optional decoration — the whole
   vision transition in `VisionDirector` is built around a beat where it plays.
   The camera swings off his eyes to frame him, then waits. Right now it waits on
   an empty pause.
4. **Tell Viki the trigger name and the exact clip length in seconds.** He sets
   `VisionDirector.breathHoldDuration` to match. Too short and it cuts off; too
   long and the player is staring at nothing.
5. Import, set up the Animator, test in the scene.

**Done when:** triggering the breath in play mode plays cleanly at a length Viki
can use.

---

## ⬜ 14. Environment modular kit (L)

**Needs task 13.** The real geometry replacing your greybox.

1. Build modular pieces that snap on a grid — wall, floor, doorway, street
   section — not one-off meshes. You are dressing a district with two people and
   a month.
2. Follow the greybox proportions from task 13. If a piece does not fit, fix the
   greybox first and tell Viki, because his markers sit in that space.
3. Part 3 calls for signage, futuristic elements, semi-destroyed surroundings,
   and a visible dome.
4. Replace greybox pieces incrementally so the level stays playable throughout.
5. Watch the scene-ownership rule again if you are editing `SampleScene.unity`.

**Done when:** the district is built from real geometry and still walks the same
as the greybox.

---

## ⬜ 18b. 2D vision animations — your half (M)

**Needs task 17.** Split evenly with Viki — a project requirement, not a preference.

1. Agree with Viki **who draws which vision** so you do not both draw the same one.
2. Agree one export format, resolution and length for both halves. Two formats
   means his task 19 needs two code paths.
3. Draw and export your half.
4. 10–15 seconds each per 6.5.
5. Remember these are Bunk's reconstruction of the crime, not a cutscene — the
   player is meant to read information out of them.

**Done when:** your half is exported in the agreed format and plays in Unity.

---

## ⬜ 15. Environment materials + textures (L)

**Needs tasks 14 and 20.** Your main strength, and the largest art task.

1. Do this **after** the noir grade is roughed in. Under that grade most colour
   information disappears; texture that reads beautifully unlit can turn into
   grey mud.
2. Build a small shared material set for the kit rather than per-mesh materials —
   it keeps the look consistent and the build cheap.
3. Value and texture detail carry the whole look here, since hue mostly does not
   survive.
4. Check every material in play mode under the real grade, never in the material
   preview.
5. Textures go through Git LFS automatically — just commit the `.meta` files too.

**Done when:** the district is fully textured and reads clearly in motion under
the noir grade.

---

## ⬜ 16. Character textures (M)

**Needs tasks 11 and 17.** Bunk, in the same language as the visions.

1. Follow the style guide from task 17 so the 3D Bunk and the 2D Bunk are
   recognisably one character.
2. He gets the game's only close-up. Detail matters more here than anywhere.
3. Leave room for the transformation later — post-demo, but do not paint yourself
   into a corner.

**Done when:** he holds up at close range in the vision transition.

---

## ⬜ 21. Scent visual effect (M)

**Needs task 20.** How a smell looks when Bunk catches it.

1. Read 6.3 carefully. **Every scent looks identical** whether true or a red
   herring — telling them apart is the player's job, not the effect's.
2. Pick the form: a trail through the world, or a screen-edge effect. A trail is
   more readable; the screen-edge is more subjective.
3. Build it as a particle system or shader that the existing `SmellInteractor`
   can turn on when a marker is in range.
4. It must survive the noir grade. Red is the obvious choice, but red is also
   crime — check it does not read as blood.
5. **First to cut if time runs short.** The loop works without it.

**Done when:** approaching a marker shows the effect, and true and false scents
look the same.

---

## ⬜ 22. Ambience + SFX (M)

**Blocks task 23.** Part 7 says "some" audio, not full.

1. Room tone for the district — Part 3 is an underground domed city with
   recycled air and artificial light. It should sound enclosed.
2. Footsteps. The single biggest improvement to how movement feels, and cheap.
3. **A smell cue** — the sound of Bunk catching a scent. This carries the core
   mechanic; give it more attention than the rest.
4. A few interaction sounds: vision start, vision end, clue logged.
5. Keep it small. A short loop and a handful of one-shots is enough for a demo.

**Done when:** walking the district with your eyes closed still tells you where
you are and when something happened.

---

## ⬜ 23. Audio mixer + integration (S)

**Needs task 22.** Wiring, not creative work.

1. Create an Audio Mixer with groups: Ambience, SFX, UI. Music later if it exists.
2. Route every source through a group. Nothing plays un-routed.
3. Set sensible levels so the smell cue cuts through the ambience.
4. Add `AudioSource` components where they belong — footsteps on the player,
   ambience on a scene object, UI sounds on the HUD.

**Done when:** one mixer controls everything and nothing is louder than the
mechanic it supports.
