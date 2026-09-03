FLAIR — GAMEPLAY SYSTEMS
How everything works. This is the programming spec.

Index: [concept.md](concept.md) | World + map: [world.md](world.md) | Characters + case: [case.md](case.md) | Production: [production.md](production.md)

PART 6 — Gameplay Systems (how everything works)
For each system: a plain description, the decisions to make, and questions. This section becomes your programming spec.
6.1 Core loop
The repeating cycle of play. Draft: Client Conversation (starting point of the invg) → Explore (3D) → Detect a lead (smell/look) → Trigger a Vision (2D) → Gain a clue → Update deductions → repeat → Final confrontation.
Your version of the loop: CORRECT

6.2 3D movement & orientation
Perspective: ▢ First-person ▢ Third-person - First Person - baseline; "Third Person" - to see our character changing in a reflective surface; when the changes begin we can have visual clues on the screen; the transf is triggered by something the
player has to discover, prevent, take care of while simultaniously supporting the invg

Controller type: ▢ CharacterController (simpler, recommended) ▢ Rigidbody physics - upto discussion
Can the player run / crouch / interact? List verbs: run/sprint stamina. jump, crouch (warewolf - diff. physics)
❓ How does the camera work, and what can the player NOT do - camera-eyes

PERSPECTIVE - DECIDED 3 Sep 2026: THIRD PERSON.
Changed on the teacher's feedback after the September showing. We are animators, and in
first person the player never sees a single frame of the animation we make. The camera is
now a classic follow rig: ~3.5m behind and slightly above, orbited with the mouse, and
Bunk turns to face whatever direction is pushed rather than strafing.
What this costs, and it is the point: Bunk needs a real animation set - idle, walk, run,
turn in place, jump, land, the smell gesture, the breath. Previously he needed almost
nothing because nobody saw him.
What it does not change: the camera was never parented to the player (see PlayerCameraRig),
so the vision transition still works. It becomes a push-in rather than a reveal, because
Bunk is already on screen.
Watch: the service alley is 5m wide with a right-angle blind corner. The rig has a spring
arm that pulls in when a wall is in the way, but the alley may need widening.

6.3 The Smell system (👃 "Smell")
How it works: player enters a scent zone → prompt appears → smelling reveals a trail / a vision / info.
How is a scent shown to the player? (visual trail? screen-edge effect? audio?) - visual, sometimes deceptive (all scents will have the same visual design, but discovered only when smelled)
Do scents fade or persist? - fade - the fainter the smell, the blurrier the 2D cutscene/object/vision
❓ How does the player tell a real scent from a red-herring scent? Is that skill, or luck? - luck, everything can be deceptive, it is up to the player to distinguish right from wrong
❓ Can the player "over-smell" and get overwhelmed (ties to wolf-restraint)? - yes, with stamina - can affect other senses (vision, visual cues on the screen)
there will be things that will affect the sense of smell (blocked nose, too many scents) - in these cases the player is forced to use deductive methods to win - or worst case - just lose the level

6.4 The Investigate system (🔍 "Investigate")
Classic look-at/examine for non-scent clues (documents, objects, dialogue info).
What object types are examinable? - depending on the case (objects, dialogue)
❓ Where does examined info go? (a case-file UI? a notebook? Bunk's monologue?) - case library/inventory - the player has different categories in the main library - scent library, mind - visionary libraries with replayable 2D visions; dialogues,
asset/tool/object library (inventory)

6.5 The 2D Vision system (the watch-only cutscenes)
How it works: a trigger (a key clue) plays a stylized 2D animated reconstruction; player control is removed, then returned.
Vision trigger: what causes one? - an examined scent - only true ones; the fake ones lose time, concrecety, they are there to "run down" the smelling stamina
Delivery tech: ☑ Pre-rendered video (Video Player) ▢ In-engine 2D animation (Animator/Timeline)
DECIDED 3 Sep 2026. Both, with a clear division of labour:
- PNG sequences are the MASTERS. Drawn per the style guide, kept in the repo, and the
  thing anyone re-edits. Krita 2048, 32-bit RGBA.
- A rendered video per vision is what the game PLAYS, through Unity's Video Player.
Why not play the PNGs directly: at 12fps a 12-second vision is ~145 files. Five visions
is 700+ image assets through Git LFS, and LFS is already the tightest constraint on this
project. One video file per vision is a fraction of that and Unity streams it.
This settles what task 19 builds: a VisionPlayer subclass driving a VideoPlayer.
How is control removed & restored cleanly? - a pop-up button (one for "play the vision"; one for "replay" (in the library/inventory); one for closing)
❓ How long is a vision? (Short — players lose patience fast when they can't act.) - a few seconds (10-15s)
❓ Can a vision be skipped/replayed? (Strongly recommend skippable on replay.) - replayable - yes, NOT skippable; the more times the player replayes a vision, the more distorted it becomes and brings less/untrue info over time replayed
❓ List every vision the demo needs (see Part 7 content list) - upto discussion

6.6 Dialogue / interrogation
Format: ▢ Speech bubbles (your concept) ▢ Branching choices ▢ Both - hybrid (upto discussion)
Do dialogue choices matter mechanically, or are they flavor? - they matter for the outcome of the invg
❓ How does "believe / don't believe the witness" work, and what does a wrong call cost? - losing time, opportunities, the client requires time of completing the invg

6.7 The wolf-restraint mechanic (NEW — from the book)
Suggested: a composure/tension meter that rises with danger, blood, or over-smelling; maxing it risks an uncontrolled transformation.
▢ Include it in the demo? ▢ Save for later? - partially included
If included: what raises it, what lowers it, what happens at max? - triggers - smelling blood; when opening the inventory, there will be an object that makes Bunk remember his sister and what he did to her (killed her) - a photo of her f.e. -
the more the player stays with an open inventory (the more they replay visions, NOT actively play, etc.), the more this object/photo triggers the transformation and makes the character less controllably playable
❓ Is transformation a fail state, a last-resort tool in the finale, or story-only? - usually a fail state, but in late-game, the player would learn to "tame" this state of the character and use it as a tool for more complex cases and taking 
advantage of the mutant powers - but the player will have to aquire/craft (maybe with the help of the witch) some kind of an antidote that helps the player control the untamed state. If the player runs out of antidote, the state becomes 
uncontrollable again. During investigation, the player will have to collect materials/ingredients for this antidote (some of the ingredients will requre the player to fo "illegal" for a detective things - f.e. to collect blood/hair from a victim - but
this can cost the loss of the GAME - they player starts from scratch - level 1, no matter the progress.

HOW OFTEN BUNK TRANSFORMS - decided
Once. In the whole game. At the final level.
The restraint above is therefore not a system that ships in Level 1 - it is a promise the game
keeps making and refusing to pay out. What builds across the levels is the pressure: the photo of
his sister in the inventory, the blood, the over-smelling, the near-misses. The meter can exist as
narrative texture long before it ever resolves.
Why hold it back: a transformation the player sees once, at the end, after a whole game of Bunk
holding it down, is worth more than a mechanic they use every level. Held back, it is a payoff.
Spent early, it is a cooldown.
Consequences for the build:
- Level 1 has no transformation. Bunk fights the Penitent in human form (see 6.8).
- No wolf meter, no antidote crafting and no tame/untame states are needed for Level 1.
- The final level gets one transformation and one transformed fight, both as 2D visions.
- The antidote, the ingredient gathering and the "start from scratch" penalty above are ideas for
  the space between here and there. None are scheduled.

6.8 Final confrontation
Your concept: face the likely culprit; may be a duel OR helping catch them.
Which is it for the demo? - hybrid; depends on the case. Bunk is strong enough to face a criminal who refuses to face the consequenses of their actions.
Is it skill-based (a fight/QTE), deduction-based (present evidence, catch the lie), or both? - both; the player needs enough evidence to get to the final "battle" of the level but the "battle" itself depends on the culprit.
❓ Can the player fail the finale, and what happens then? - yes. in case of fail in the final "battle" the case is solved but there is no justice for Bunk's client. In every level the solved cases bring some kind of "units" to the player. This units
help him in the next level - more informators; richer inventory etc.

HOW THE FIGHT IS BUILT - decided
There is no 3D combat system. The fight plays as a 2D vision, through the same
VisionPlayer pipeline the clues already use.
Why, and this is not a compromise:
- It serves the pillar. "You investigate in 3D; you understand in 2D." The fight is where the
  case resolves, so 2D is where it belongs.
- Hand-drawn black-white-red violence carries the book's body-horror-and-wit tone far better
  than two low-poly models colliding would.
- It costs one animation instead of a whole system, and the pipeline is already built and tested.
- It scales. The final level's one transformation fight is the same pipeline, and 2D lets it be
  as violent as the book without anyone building a gore system. That fight is the single most
  important image in the whole game, and drawing it beats simulating it.
The fight still has an outcome: one or two timed inputs during the sequence, or a single
decisive choice that only reads correctly if the player learned the Penitent's tell. Win and
lose both stay real, so the endings in 5.6 are unchanged.
Bunk stays physical in the 3D world, cheaply - he shoulders a stuck door, breaks the droggery's
back lock, takes a witness by the collar mid-interrogation. Animations and triggers, not a system.
This keeps the side of him the book is built on without building combat for it.
The bodyguard at the Doctor's door is one scripted 2D beat, or a talk-past. Not a second fight.
6.9 Progression & feedback
How does the player know they're making progress? (case file filling up? clue counter?) - there will be case file in which the player fills up the "data" - something like a puzzle
How are wrong choices communicated without frustrating? - after a few wrong choices the client comes back to bunk and gives hints (could be information, could be a 2D vision that the player has missed)

6.10 Bunk's Mind Inventory — the deduction board (NEW)
Lives in the "mind" category of the case library/inventory (6.4). This is how a case actually gets solved, and it's the mechanism behind the deduction half of the finale (6.8).
How it works: at the start of a case, the client's opening briefing/backstory is logged in a separate case window, not the mind inventory itself. Opening the mind inventory shows a running list of questions Bunk asks himself about the case.
Some questions already carry an answer — authored backstory/experience Bunk already has. These read to the player as leads, not things to solve.
The first question, on every case with no exception, is "What is the client hiding from me?" — always present, never pre-answered.
As the player finds hints while investigating (smell/look/dialogue — 6.3/6.4/6.6), they attach each hint to whichever question they believe it answers. Attaching is free and ongoing through the whole case — nothing is checked question by question.
The board is checked as a whole at final submission: this is the "present evidence" moment of the final confrontation (6.8). Every question matched to its correct hint wins the case. Any wrong pairing triggers 6.8's existing finale-fail outcome (case solved, no justice for the client, fewer units carried forward) — harder than the ongoing, softer wrong-deduction penalty in 5. (payment/patience loss), which covers missteps made earlier in the investigation, not the final submission.
❓ Questions to answer:
Can one hint answer more than one question, or is it strictly one hint → one question?
Must every question have a hint attached before the player can submit, or can they submit with gaps?
Are Bunk's own pre-answered questions visible from the start of the case, or do they unlock as related clues are found?

