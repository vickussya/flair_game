FLAIR — PRODUCTION
Content list, technical design, build order and the decision gate.

Index: [concept.md](concept.md) | World + map: [world.md](world.md) | Characters + case: [case.md](case.md) | Systems: [systems.md](systems.md)
Live task board: [tasks.md](tasks.md)

PART 7 — Content List for the Demo (what must actually exist)
The "how much are we really building" reality check. Fill in real counts.
Scenes needed:
▢ Main menu / title
▢ The level itself
▢ end / results screen
3D environment assets: street sections, interiors, key props: interactive watch; something like a folder/bag - the thing that the character opens when the inventory button is clicked
Characters (models + animations): ____ total. List: __________________
2D visions: ____ total. List each: __________________________________
Dialogue: ____ conversations, roughly ____ lines total.
UI screens/elements: case file? clue prompts? dialogue box? menu? List: upto discussion
Audio: ambience, SFX (smell cue, footsteps), music, voice (▢ none ▢ some ▢ full)? - some
❓ Questions to answer:
What is the shortest version of this level that still proves the concept? (Build that first.) - client -> case starts -> Bunk finds a hint -> Bunk chatches the criminal
Which assets can be placeholder (greybox / stand-in) until the end? - upto discussion
What can you cut if you run out of time, without breaking the demo? - antidote; uncontrollable transformations; false hints

PART 8 — Technical Design (Unity + C#)
Only fill this once Parts 0–7 are mostly answered. This translates design → build.
8.1 Engine setup
Unity version: __________ (you're installing 6.x — pick one and lock it for the whole team)
Render pipeline: ▢ URP (recommended) ▢ Built-in ▢ HDRP
Input: ▢ new Input System package (recommended) ▢ old Input Manager
Packages you'll need: Input System, ▢ Timeline, ▢ Video Player (for visions), ▢ Cinemachine (camera), ______
❓ Is everyone on the same version? (Mismatched versions corrupt shared projects.)
8.2 Version control (do this on DAY ONE)
▢ Git + a host (GitHub/GitLab) with Git LFS for large art/binary files
▢ A .gitignore for Unity (Unity provides one)
❓ Who sets up the repo, and how do teammates pull it?
8.3 Scene & object architecture
List your scenes and what loads them: __________________
Key persistent objects (managers): e.g. GameManager, CaseManager, VisionManager, DialogueManager, AudioManager — which do you need? __________________
8.4 How data is represented
A clue, a case, a line of dialogue — decide the data shape. For narrative games, ScriptableObjects are the standard beginner-friendly way to store clues/cases as editable assets (no code changes to add a clue).
☑ Clues as ScriptableObjects ▢ Hard-coded ▢ External file (JSON/CSV)
DECIDED (task 3, 19 Aug 2026). Each clue is a ScriptableObject asset -
Assets/Scripts/ClueData.cs, one asset per clue in Assets/Clues/.
❓ How does a designer add or edit a clue without touching C#?
Create > Flair > Clue makes a new asset; fill it in the Inspector, then drop it
on a ScentMarker's Clue field. No code changes, no recompile. ScentMarker holds
no clue fields of its own any more, it only points at one of these.
Fields: clueId, displayName, description, isTrueScent, leadsTo, visionDuration,
visionId. Names agreed with Viki 28 Aug 2026 - renaming them means rewriting his
gating and case-file code, so treat them as fixed.
8.5 Systems → scripts map
Design system (Part 6)
Script(s) / components
Notes
Movement & camera


Smell detection


Investigate/examine


Vision playback


Dialogue


Case/clue tracking


Wolf-restraint meter


Finale


8.6 Build & platform
Target platform: ▢ Windows ▢ Mac ▢ WebGL (itch.io) ▢ other
❓ Have you done a test build early (not just at the end)?

PART 9 — Production Plan (in what order to build)
A suggested milestone ladder — reorder to taste. Don't start a milestone until the previous one works.
Greybox / prototype — a player capsule moving in an empty room; prove movement + camera. ▢
One clue, one vision — walk to a marker, press Smell, a placeholder vision plays, control returns, a clue is logged. This is your vertical slice: the whole loop in miniature. ▢
Full case logic — all clues, red herrings, the deduction/gating, wrong-path handling. ▢
Content pass — real environment art, real vision animations, dialogue, audio. ▢
Finale — the confrontation. ▢
Polish — menus, UI, transitions, skippable visions, bug fixing. ▢
Ship the demo — build, test on another machine, release/share. ▢
❓ Questions to answer:
What is your vertical slice exactly, and by when? (Step 2 is the single most important milestone.)
What's your biggest technical unknown, and can you spike a tiny test of it this week?
What are the top 3 risks to finishing? __________________

PART 10 — DECISION GATE ✅ (answer before real production)
Don't write production C# until most of these are ✅. Copy this list somewhere visible.
[ ] Pitch and the one primary pillar are locked
[ ] Rights/permission status of using Flair is clear
[ ] Adapting a real case or original case — decided
[ ] The case truth is fully written (who/how/why + timeline)
[ ] The clue chain is mapped and solvable by logic
[ ] Minimum clues to unlock finale — a number is chosen
[ ] What a wrong choice costs — defined
[ ] Perspective (1st/3rd) and controller type — chosen
[ ] Vision delivery (video vs in-engine) — chosen
[ ] Wolf-restraint mechanic — in or out of the demo
[ ] Finale is deduction / skill / both — decided
[ ] Full content list exists with real counts
[ ] Unity version + render pipeline + input system — locked for whole team
[ ] Git repo with LFS — set up
[ ] Clue/case data representation — chosen
[ ] Vertical slice defined with a date
[ ] "Done" for the demo — written as one testable sentence

