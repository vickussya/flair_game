# Viki — task instructions

Your 14 tasks, with what each one is, how to do it, and how to know it is done.
Overview of both our workloads: **[tasks.md](tasks.md)**.

Branch: **`vickussya`**. Tick your own boxes here; nobody else edits this file.

## Which of these are due when

Two separate deliverables — see [tasks.md](tasks.md).

**M1 — the rough demo. Finish by 1 September**, hard deadline the 15th. Playable
proof for the teachers, not a finished thing. Rough is the correct amount of work
here, not a compromise. The fortnight after 1 Sep is buffer, not working time.

| Due | Task | M1 scope |
|---|---|---|
| **Sat 16 Aug** | 25 Test build | - Done. The build works.
| **Wed 19 Aug** | 1 Case skeleton | Rough: truth + 3-clue chain only |
| **Tue 26 Aug** | 18a Your vision | One vision, not the set |
| **Wed 27 Aug** | 4 Clue gating | Minimal: 3 clues → ending |
| **Fri 28 Aug** | 24 Level scripting | Rough: 3 markers wired |
| **Sat 30 Aug** | 19 Real vision player | Full |
| **Sun 31 Aug** | 9 Vision close button | Close is enough |
| **Sun 31 Aug** | 10 Title + end card | Not a menu system |
| **Mon 1 Sep** | Demo integrated and built | — |
| ongoing | 26 Milestone tracking | Weekly check |

You are blocked on Leta twice: task 3 (due 19 Aug) gates your 4 and 24, and task
12 (due 28 Aug) gates your 19. If either slips, say so early rather than waiting.

**M2 — the bachelor project, May.** The finished level. Everything above gets
completed rather than replaced, plus: **2**, **6**, **5**, **7**, **8**, the full
Part 5, full menus, and the stretch mechanics (red herrings, smell stamina, wolf
meter, replay degradation).

The task descriptions below are written to the **finished** M2 standard. For M1,
do the rough version the board specifies and come back.

**Start with 25.** A build that fails on someone else's machine is the one
failure mode that turns the demo into nothing, and it costs half a day to rule
out now instead of on 14 September.

---

## ⬜ 1. Write Part 5: the case (L)

**Blocks tasks 4, 5, 7 and 24. Nothing real can be built until this exists — do it first.**

Fill Part 5 of [concept.md](concept.md). A mystery has to be solvable backwards:
write the truth, then hide it.

1. **5.1 — the truth.** What actually happened, who did it, how, why, and the
   timeline. The player never sees this directly, but every clue is carved out of it.
2. **5.2 — the logline.** "___ hires Bunk to investigate ___." One sentence.
3. **5.3 — the clue chain.** 4–5 clues. For each: what it is, where it is found,
   whether it is a smell 👃 or a look 🔍, and **which clue it points to next**.
   Each clue must narrow the suspect list — if removing a clue changes nothing,
   it is decoration.
4. **5.4 — one red herring.** What makes it look relevant, and what proves it is not.
5. **5.5 — the solution moment.** The specific fact that makes the player *sure*.
6. Answer the open question: **how many clues unlock the finale?** Pick a number.
   With 5 clues, 3 is a reasonable gate.

**Done when:** you can hand the doc to someone who has not read the book and they
can solve it on logic alone, with no guessing.

**Note:** edit `concept.md` **on `dev`**, not on your branch.

---

## ⬜ 2. Write Part 4: characters + scent signatures (M)

**Blocks task 6.** Fill Part 4 for: Bunk, the client, the informant, the culprit,
and 2–3 witnesses.

1. For each: name, species, what they want, what they are hiding.
2. **A scent signature for every speaking character.** This is a smell-detective
   game; a character with no smell cannot be part of a smell puzzle.
3. Decide who lies, and whether the lie leaves a clue. Part 4 already frames the
   theme: a lie hides a motive, so it reveals one.
4. Keep the cast small. Every character is a model, a voice and animations.

**Done when:** every speaking character has a distinct scent the player could
learn and later recognise.

---

## ⬜ 4. Case gating (M)

**Needs tasks 1 and 3.** The rule that decides when the player may confront the culprit.

1. Wait for Leta's clue data model (task 3) and build against it.
2. Extend `Assets/Scripts/ClueLog.cs` — it already tracks collected clues and
   raises `ClueLogged`. Add the gate: are enough of the *required* clues held?
3. Expose the threshold in the Inspector; do not hard-code the number.
4. Raise an event when the gate opens, so the UI and finale can react without
   polling.
5. Handle the wrong-path case from 6.9: after N wrong choices the client returns
   with a hint.

**Done when:** collecting the required clues fires an event once, and collecting
fewer never does.

---

## ⬜ 5. Client conversation → case start (M)

**Needs tasks 1 and 6.** The opening beat: the client hires Bunk, and the case begins.

1. Build it as the first user of the dialogue system rather than a special case.
2. On the conversation ending, start the case: enable the scent markers for
   clue 1, log the logline into the case file.
3. Trigger it on level start, so the demo opens with it.

**Done when:** starting the demo puts you in the conversation, and finishing it
leaves you free to explore with the first lead recorded.

---

## ⬜ 6. Dialogue system (L)

**Needs task 2. Blocks 5 and 7.** Your largest systems task — start it early.

1. Data first: a conversation is a list of lines (speaker, text) plus optional
   choices, each choice pointing to the next node. Match the pattern Leta sets in
   task 3 so there is one way to author content, not two.
2. Runtime: a component that walks the nodes, shows a line, waits for input,
   branches on choice.
3. UI: speech bubbles per 6.6, plus a choice list. Reuse the `Canvas` and the
   `VisionHud` pattern already in the scene.
4. Lock player control during dialogue the same way `VisionDirector` does —
   `PlayerController.SetControlEnabled(false)`. Do not invent a second mechanism.
5. Wire choices to consequences: 6.6 says believing or doubting a witness costs
   time and opportunities.

**Done when:** a two-branch conversation plays start to finish, control returns
cleanly, and the chosen branch is recorded.

---

## ⬜ 7. Final confrontation (L)

**Needs tasks 1, 4 and 6.** Per 6.8 it is a hybrid: enough evidence gets you
there, then the confrontation itself resolves.

1. Gate entry on task 4's "enough clues" event.
2. Deduction phase: present evidence, catch the lie. Reuse the dialogue system.
3. Resolution: keep the demo version simple. A correct accusation resolves it.
4. Failure per 6.8: the case is solved but the client gets no justice. The demo
   must handle both endings.
5. Route both into the results screen (task 10).

**Done when:** both a win and a loss can be reached and each lands on a
distinguishable ending.

---

## ⬜ 8. Case file UI (L)

**Needs task 3.** Per 6.9 the player watches the case file fill up like a puzzle.

1. Subscribe to `ClueLog.ClueLogged` — the event already exists.
2. Show collected clues; show unfound ones as blanks so progress is legible.
3. Open and close it on a key without disturbing the vision flow; check
   `VisionDirector.IsPlaying` before opening.
4. Freeze player control while it is open, same mechanism as everything else.
5. Keep the layout plain — 6.7 eventually hangs the wolf-restraint trigger off
   time spent in here, so leave room for that later.

**Done when:** finding a clue visibly fills a slot, and reopening the file shows
everything found so far.

---

## ⬜ 9. Vision play / replay / close controls (M)

Per 6.5 the vision needs on-screen buttons: play, replay, close.

1. Extend `Assets/Scripts/VisionHud.cs` — it already owns the prompt, the fade
   and the vision panel.
2. Add the three buttons. Replay re-runs the current vision; close returns control.
3. **Visions are replayable but not skippable** (6.5). Do not add a skip button.
4. Leave a hook for replay degradation — each replay is meant to get more
   distorted and less truthful — but do not build it; it is post-demo.
5. Buttons mean a visible cursor: unlock it while the panel is up, relock after.

**Done when:** you can replay a vision from its panel and close it back into
first person with control restored.

---

## ⬜ 10. Main menu + results screen (M)

Part 7 lists three scenes: menu, level, results.

1. New scene `MainMenu`: title, Play, Quit.
2. New scene `Results`: outcome from task 7, clues found, and a return to menu.
3. Add all three to Build Settings in order.
4. Load between them by scene name; keep a single place that does the loading.

**Done when:** menu → level → results → menu runs as a loop without a manual
scene change.

---

## ⬜ 18a. 2D vision animations — your half (M)

**Needs task 17 (Leta's style guide).** Task 18 is split evenly between us — a
project requirement, not a preference.

1. Wait for the style guide, then agree with Leta **who draws which vision**.
2. Agree one export format, resolution and length for both halves. Two formats
   means task 19 needs two code paths.
3. Draw and export your half.
4. 10–15 seconds each per 6.5. Players lose patience when they cannot act.

**Done when:** your half is exported in the agreed format and plays in Unity.

---

## ⬜ 19. Swap placeholder → real vision player (S)

**Needs tasks 12, 18a and 18b.** The payoff for how Stage 2 was built.

1. Write a new class extending `Assets/Scripts/VisionPlayer.cs` — a
   `VideoVisionPlayer` (Video Player component) or `TimelineVisionPlayer`,
   whichever the export format from 18 calls for. This settles 6.5.
2. Implement the three members: `Begin`, `IsFinished`, `End`.
3. On `GameSystems`, replace `PlaceholderVisionPlayer` with it and drag the new
   component into `VisionDirector`'s Vision Player field.
4. Hook Leta's deep-breath animation into `VisionDirector.breathHoldDuration` and
   set that duration to the clip's real length.

**Done when:** a real 2D vision plays through the existing transition and
`VisionDirector` was not modified to make it work.

---

## ⬜ 24. Level scripting: markers and chain (M)

**Needs tasks 1, 3 and 13.** Turning the written case into a playable level.

1. Take the district scene from Leta (task 13). **Confirm she is out of
   `SampleScene.unity` before you start** — one person in the scene at a time.
2. Place a `ScentMarker` per clue from your chain. The component already carries
   clue id, display name, radius, vision length and `IsTrueScent`.
3. Set `IsTrueScent = false` on the red herring. Nothing reads that flag yet, so
   note in the doc that the behaviour is still to build.
4. Wire the chain so each clue leads to the next per 5.3.
5. Walk the level start to finish and confirm it is solvable by logic alone.

**Done when:** the case can be played through from client conversation to finale
without editor intervention.

---

## ⬜ 25. Early test build + pipeline (S)

**Do this in week one, not at the end.** Part 8.6 asks whether you have built
early — the honest answer today is no.

1. Pick the target platform. Windows standalone is the safe default; WebGL/itch
   is friendlier to share but has real constraints.
2. Build now, at Stage 2. A build that breaks is much cheaper to fix now.
3. **Run it on a machine that is not yours.** Missing scenes and editor-only
   assumptions only show up there.
4. Repeat at every milestone.

**Done when:** a build runs on another machine and the Stage 2 loop works in it.

---

## ⬜ 26. Milestone tracking / triage (M)

**Ongoing, not a one-off.** You own production (`concept.md` names you producer).

1. Keep [tasks.md](tasks.md) current — you are its only editor.
2. Weekly, check remaining effort against remaining days. 33 days for ~58 effort
   points across two people is tight.
3. When something slips, cut in the order in `tasks.md`. Cutting early and on
   purpose beats running out of time.
4. Watch the blocking chain: tasks 1 and 3 gate most of the work. If either
   stalls, everything behind it stalls silently.
5. Keep Part 10's decision gate honest — tick items only when actually decided.

**Done when:** the demo ships on 15 September, or the scope was cut early enough
that it did.
