FLAIR (Bunk Romero) - Preproduction Plan & Design Bible
A fill-in template. Based on Emil Minchev's Нюх (Flair), Bunk Romero #1. Target: a single-level 3D detective demo with 2D "vision" cutscenes, built in Unity + C#.


PART 0 — Project Frame
Working title: Flair
What it is: Flair is a neo-noir detective game set on Haxan, a planet whose sun is slowly dying into a black hole, where the last of humanity and its mutants share the domed megalopolis of Unterstadt. 
You play Bunk Romero - a werewolf private eye who keeps the beast on a leash and cracks cases with an inhumanly sharp nose. 
You explore the city's streets in 3D, sniff out leads the police can't, and every clue pulls you into a hand-drawn 2D "vision" that pieces the crime together, all rendered in stark black, white, and blood-red. 
This first case is a self-contained demo: a single mystery, one confrontation, and proof that the fiercest monsters wear human faces. Based on Emil Minchev's novel Нюх (Flair).


Team & roles:
Design / writing: Viki, Leta
Programming (C#): Viki, Leta, Claude
2D art / visions: Viki, Leta
3D art / environment: Viki, Leta
Textures: Leta
3D environment: Viki
assests: Viki, Leta
Characters: Bunk (demo)
Audio: -
Producer / keeps the plan moving: Viki
goal of this demo - Bachelor project
Who is it for - Natfa
What is your hard deadline or time budget - Demo - 15th Sep; final ~ May
What does "done" look like for this one level? - playable game with finished look. Only one level.

PART 1 — Design Pillars
Pillars are 3–4 short phrases that every later decision must serve. If a feature doesn't serve a pillar, cut it. This is your anti-scope-creep weapon.
Fill in 3–4 pillars.
"A detective who solves crimes by smell." → Set in the original story of the book. 

"Noir mood: black / white / red, gore with wit. Apocalytpic style" → Noir is the main style detective stories work with. The mix between noir and apocalyptic is
helps recreate the mood of the world built in the books. This mix represents the classic interesting detective storyline
blended with the destructive nature of our world. This constrast brings the idea of the unknown future whilst giving the comfort
of the well-known retro mystery mood. The gamer should feel both comforted and challenged/provoked. Red represents crime, fatallity,
danger and urgency. Black represents mystery, the unknown and uncertain. White represents justice, hope and uncovering the truth.

"You investigate in 3D; you understand in 2D." → 2D cutscenes add more diversity to the design; support the noir mood and makes the game
more enjoyable and multi-dimensional. The 2D scenes cover the reqiurement for animation. They will be replayable and will help the player to better understand the case and see the case through the detective's eyes (nose). This will help with the immersion into the story's world.


If you had to cut the game to one pillar, which survives?
	- "A detective who solves crimes by smell." 

Is this demo more about atmosphere, puzzle/deduction, or story? 
- Puzzle/deduction (mystery)

PART 2 — Source Material Fidelity (from the book)
Decide, on purpose, what you take from Flair and what you invent. This prevents both "we contradicted the book" and "we're just doing generic noir."
Canon elements to keep:
▢ Planet Haxan, with the megalopolis Unterstadt under a protective dome
▢ The dying sun slowly swallowed by an invisible black hole (it's the star, not the planet melting) - important for the story, not the gameplay itself - it can be mentioned as part of the lore
▢ Bunk (Vincent) Romero, werewolf detective, restrains the beast inside him
▢ Investigates via supernatural smell + classic deduction
▢ Rogues' gallery: corrupt politicians, gangsters, femmes fatales, witches, pain-worshipping fanatics - helpful for gameart and envirnoment design
▢ Tone: body-horror gore + humor + literary in-jokes - helpful for recreating the book world and character
▢ Background serial-killer thread (the book's connective tissue) as a hook for future levels - not in the final bachelor project

What we are deliberately inventing / changing for the game: 
- hints - true and deceptional - the deceptional ones lead to other cases/confusion that slow down the main investigation. This will decide wether the player wins/loses.
- level credits - money/points/units given in order for the player to use to win the level. Some of the units could be gathered during different situations in the levels and 
being used in later-game.
- additional help - informators/tools


Are you adapting a real case from the book (recommended: the opening case, "A Vial of Wind" — the witch's apothecary robbery) or writing an original case in the same world? - a real case, but modified - the game must be interesting for people who have read the book and not predictable.

How much can you change before a Flair reader feels it's "not Flair"? Where's your line? - change the hints, add red-herrings, motive mods, change culprits in some cases.

Do you have rights / permission from the author or publisher, or is this a non-commercial fan/learning project? - we are planning to contact the author after the first level of the game is finished.

PART 3 — World Bible (the fiction)
Write these so any team member can answer "what is this place like?" the same way.
Setting — Unterstadt under the dome:
Look & feel in one paragraph: retro, noir-style, apocalyptic, dangerous, mysterious, urgent
What district is the demo set in? - Bismarkstrasse (where theh droggery is)
What's on the streets? (signage, tech level, weather-under-a-dome, crowds - a city, the dome, semi-destroyed surroundings and futuristic elements

The dying sun / black hole — does it affect gameplay or is it backdrop? - backdrop


Mutants & prejudice — how is it shown in this level? (a slur, a refused entry, graffiti, a wary NPC?) - hybrid - storytelling and hints along the game; slurs


What are 3 concrete sensory details of Unterstadt a player will notice in 5 minutes? - visuals + sound, danger
What does the dome mean day-to-day? (artificial light? recycled air? a visible seam in the sky?) - artificial light (like the world itself) - UNDERGROUND	
Is magic/tech soft or hard? (Can the player rely on consistent rules, or is it vibes?) - hybrid

PART 3.5 — GAME MAP (visual reference)
The demo level, described so it can be greyboxed, drawn, or handed to an image generator.
Anything marked [inferred] is not fixed by the rest of this document - change it freely.
Pictures of everything below live in /reference/game_map/ (see /reference/README.md).
Where an image and this section disagree, this section wins - it is the copy both of us edit.

ONE-LINE SUMMARY
Bismarkstrasse - a claustrophobic, rain-slick street canyon deep inside the buried domed
megalopolis of Unterstadt, in black, white and blood-red.

VIEW & PERSPECTIVE
In game: first person ("camera-eyes", see 6.2). The isometric view below exists only as a
design reference, not as a camera the player ever gets. [inferred]
Play area: a north-south rectangle, roughly 2:1. A street canyon, not an open field - but a
walkable one. The first greybox was built to the letter of this section, at 3:1 with 8m streets,
an 18m-deep plaza and a 3m alley, and it walked as claustrophobic in the bad sense. Streets are
now 12m, the plaza 30x24, the alley 5m. [widths set by walking it, 28 Aug 2026]
The south end is no longer pinched: the office doorway recess is the full width of the street,
which reads better than opening out of a narrow slot. The north end still is.
Exact dimensions live in Assets/Editor/DistrictGreybox.cs, which builds the greybox from a
coordinate table. Change the numbers there and re-run FLAIR > Greybox to reshape the level.
The block is a slot - buildings rise 5-6 storeys and are capped by the dome ceiling hanging
low overhead. The map has a roof and you can see it. [ceiling framing inferred]

WORLD LAYOUT (south to north)
- SOUTH END - Bunk's office, the start point. Narrow soot-blackened doorway under a dead neon
  sign, one lit second-floor window, three steps down to street level. [inferred]
- LOWER STREET - cracked asphalt, rusted tram rails ending in rubble, burnt-out chassis shoved
  against the kerb as barricades, shuttered steel storefronts, torn bills, anti-mutant graffiti
  slurs in red. [street furniture inferred; graffiti canon]
- CENTRE - the Plaza. The street widens into the map's hub and main chokepoint. A dead fountain
  with a headless robed statue in a dry cracked basin. Four routes meet here. [inferred]
- WEST - the Service Alley. 5m wide, bins and crates, fire escapes above, a sharp dogleg making
  a blind corner - the two legs meet at a true right angle, so you cannot see round it from
  either side. Connects the plaza to the droggery's back door. The escape route and the tightest
  chokepoint. [was "two people wide"; widened 28 Aug 2026 - at 3m the player capsule could not
  get past the crates]
- EAST - the Blocked Arch. A grand tiled transit archway sealed by collapse: rubble, twisted
  rebar, one swinging warning lamp. The map's dead end - it ends the world without a fence. [inferred]
- NORTH END - The Droggery. Barbara's apothecary, the crime scene and the objective. Widest and
  most ornate facade on the block, raised on a stone stoop. Front window shattered OUTWARD, glass
  on the pavement rather than inside. Inside: floor-to-ceiling apothecary bottles, hanging dried
  herbs, long counter, overturned stool, wall cabinet standing open and emptied. Rear door ajar
  onto the alley. [robbed apothecary canon; staging inferred]
Connectivity: a loop with one spur. Office - Street - Plaza - Droggery front, with the Alley as a
parallel back route from Plaza to Droggery rear, and the Arch as a decorative dead end. The loop
lets the player circle the crime scene. [inferred]
The three scent markers sit at: the droggery interior by the emptied cabinet, the alley dogleg,
and the plaza fountain. [three clues canon; placement inferred]

ART STYLE & MOOD
Style: stylised low-poly 3D with hand-painted texture detail. Chunky readable geometry, heavy
grain, visible ink texture. A graphic-novel panel in three dimensions, not photoreal. [inferred]
Palette - three values only, and this is load-bearing:
  Black - mystery, the unknown. Crushed shadow with no detail recovery. Most of the frame.
  White - justice, truth, hope. Hard highlights, lamp glow, wet reflections, fog.
  Blood-red - crime, urgency, fatality. Used sparingly and always meaningfully: graffiti, the
  apothecary sign, scent traces, blood, warning lamps. Roughly 5% of pixels.
No other hue anywhere. Not desaturated colour - absent colour.
Lighting: entirely artificial, there is no sun down here. Hard pools of white from caged wall
lamps and hanging bulbs, long throws of pure shadow between. Deep chiaroscuro, light shafts made
visible by dust and steam, wet ground doubling every source.
Time of day: none. Perpetual sunless underground.
Atmosphere: retro-noir crossed with slow apocalypse. Damp, enclosed, recycled air visible as haze.
Beautiful and unwell. The street should feel like it is holding its breath.

KEY LANDMARKS & TERRAIN
- The dome ceiling - vast ribbed concrete-and-glass lid ~26m up with a visible structural seam,
  condensation dripping, faint red hazard lights along the ribs. It replaces sky entirely.
  [was ~40m. That contradicted "capped by the dome ceiling hanging low overhead" above: with
  5-6 storey buildings, 40m leaves a 22m void over the rooftops and the street stops feeling
  lidded. 26m sits 8m clear of the roofs. If we would rather have the void, the greybox tool
  has a Ceiling flag and a DomeY constant - decide before task 20's lighting, since the ceiling
  is what the artificial light bounces off. 28 Aug 2026]
- The droggery facade - carved stone surround, hanging iron bracket sign with a witch's mark in
  red, bottles in the intact half of the window, shattered glass across the stoop.
- The dead fountain - headless robed statue, dry cracked basin. The plaza's anchor.
- The blocked arch - tiled transit mouth choked with rubble and rebar, single swinging lamp.
- Fire escapes and catwalks - black iron zig-zags up the west facades, one raised walkway crossing
  the alley and casting a hard bar of shadow. [inferred]
- Semi-collapsed building on the east row, sheared open, floors visible in cross-section with
  furniture still in place.
- Steam vents and cable runs - pipework bolted along walls venting at ankle height, heavy cable
  bundles swagged overhead. Futuristic elements grafted crudely onto old masonry.
- Signage - Germanic shopfront lettering, a street plate reading BISMARKSTRASSE, torn bills.
  Monochrome with occasional red. [typography inferred]
- Graffiti - anti-mutant slurs in aggressive red scrawl, layered and partly scrubbed out.
- Elevation - mostly flat, with three steps up to the droggery stoop, three down to Bunk's door,
  and the raised catwalk over the alley. Standing water and puddles are the only "water".
- Scent traces - thin drifting ribbons of red vapour, low to the ground, identical everywhere.
  [all scents look alike is canon per 6.3; vapour form inferred]

UI & HUD OVERLAY
Style: flat high-contrast graphic UI with light case-file skeuomorphism. Thin white rules, black
panels at ~85% opacity, typewriter/stencil lettering, occasional paper grain. No gloss, no bevels,
no glass. Red only for alerts and scent. [style inferred; the elements themselves come from Part 6]
The HUD borders the map and never covers its centre. This is a game about looking.
- Bottom-centre - interaction prompt. One white line on a black slab: "HOLD E - SMELL", scent name
  beneath in smaller type. Only visible in range.
- Bottom-left - scent stamina bar. Horizontal segmented, white filling, draining to red. Nose glyph.
- Above it - composure / wolf meter. Thinner, red outline, empty at rest, fills upward. [visual inferred]
- Top-left - case file tab. Folded-corner dossier icon, "CLUES 1/3", three slots that fill like a puzzle.
- Top-right - units counter. Plain numeral with a coin-stamp glyph.
- Right edge - organiser bracelet. Strap-and-screen icon, pulses red when Maddie makes contact. [visual inferred]
- Bottom-right - the watch prop, rendered as a small face showing case time elapsed. [placement inferred]
- Top-right inset - minimap. Small square, black field, white line-work street plan, red dot for the
  player, hollow red rings for unexamined scents. [fully inferred - no minimap elsewhere in this doc]
- Library / inventory overlay - full black panel, four vertical tabs: SCENTS / VISIONS / DIALOGUE /
  OBJECTS. Grid of card thumbnails, unfound entries as empty outlined slots.
- Vision panel controls - three flat buttons beneath the vision frame: PLAY / REPLAY / CLOSE.
  No skip button (6.5).
- Objective marker - thin red diamond outline over the droggery, no fill, fading with distance.

IMAGE-PROMPT VERSION (paste into an image generator)
Isometric cutaway diorama of a narrow underground city street block, high-angle three-quarter view,
long north-south canyon of five-storey buildings sliced open like a dollhouse, rendered strictly in
black, white and blood-red with no other colour. A vast ribbed concrete-and-glass dome ceiling hangs
low overhead with a visible structural seam, dripping condensation, faint red hazard lamps along its
ribs, no sky and no sun. At the far north end an ornate apothecary shopfront with a shattered
outward-blown window, glass on the stoop, shelves of bottles and hanging dried herbs inside, an iron
bracket sign with a red witch's mark. At the near south end a soot-blackened doorway with one lit
second-floor window and a dead neon sign. Between them a small plaza with a dry cracked fountain and
a headless robed statue, rusted tram rails sunk in wet asphalt, burnt-out car chassis as barricades,
shuttered steel storefronts plastered with torn bills and aggressive red graffiti scrawl. A tight
service alley with a dogleg bend runs behind the west row, cluttered with bins and crates, black iron
fire escapes zig-zagging above, a raised catwalk casting a hard shadow bar. To the east a grand tiled
transit archway sealed by rubble and twisted rebar under a single swinging warning lamp, and a
sheared-open half-collapsed building showing its floors in cross-section. Steam vents at ankle height,
heavy cable bundles swagged overhead, Germanic shopfront lettering, a street plate reading
BISMARKSTRASSE. Hard pools of white lamplight with pure black shadow between, deep chiaroscuro,
visible dust haze and light shafts, wet reflective ground doubling every light. Thin ribbons of red
vapour drifting low near three points of interest. Stylised low-poly geometry with hand-painted ink
texture and heavy film grain, graphic-novel noir, apocalyptic and damp and enclosed. Framed by a flat
high-contrast detective case-file interface: thin white rules on black panels, typewriter lettering, a
segmented stamina bar bottom-left, a folded-corner dossier icon reading CLUES 1/3 top-left, a small
black-and-white line-work minimap inset top-right, a hollow red diamond marker hovering over the
apothecary, centre of frame left clear.

NEGATIVE PROMPTS
colour, colorful, blue, green, orange, teal, yellow, sepia, pastel / sunlight, daylight, blue sky,
clouds, open sky, outdoors / photorealistic, hyperrealistic, photograph / cheerful, clean, tidy,
pristine, new / modern city, skyscrapers, glass towers, contemporary cars / crowds, many people,
busy street / fantasy medieval, castle, forest, nature, grass, trees / anime, chibi, cartoon mascot /
text overlays, watermark, signature, captions / cluttered HUD over centre of image, glossy UI,
bevelled buttons, glass morphism / top-down flat 2D floorplan, blueprint / lens flare, bokeh,
depth-of-field blur

❓ Questions to answer:
Does the layout above match what you two actually picture? Settle it before the greybox (task 13).
Is Bunk's office the start point, or does the demo open at the droggery with the client?

PART 4 — Characters
For each: name, role, what they want, what they're hiding, and their scent signature (this is a smell-detective game — everyone should have one).
Bunk (Vincent) Romero — the player
Personality / voice: Bold, clever, physically strong, black sense of humor, attractive. Voice - deep, manly.
What restrains the wolf? (willpower / pills / ritual?) - willpower (sometimes pills too)
What triggers the wolf? (blood? threat? a specific scent?) - rabies, instinct of self-preservation (he transforms when hie life is in danger); memories of what he did ti his sister (killed her)
The Client (in the book, a beautiful woman / witch who was robbed)
Name / species: Barbara (witch)
What they want: to find the robber and get back the expensive medicine to cure her client | What they're hiding: __________________
Scent signature: fake eye, unlimited power, lust
The Informant
Name / role: Maddie (Bunk's daughter) | How they contact Bunk: through the orginiser bracelet
The Culprit / final suspect
Name / species: __________________
Motive: __________________ | Method: __________________ | Mistake that exposes them: __________________
Scent signature (their "tell"): __________________
Other NPCs / witnesses (list 2–4):
__________________ | __________________ | __________________
❓ Questions to answer:
Does every speaking character have a distinct scent the player could learn?
Who lies, and does their lie leave a clue? (Theme: a lie can reveal more than the truth, because it hides a motive.)
How many characters can you realistically voice / animate for a demo? (Fewer = better.)

PART 5 — The Case (design this BEFORE any tech)
A mystery must be solvable backwards first. Design the truth, then hide it.
5.1 — The truth (players never see this directly):
What actually happened, start to finish: _______________________________
Who did it, how, why: _______________________________
Timeline of the crime: _______________________________
5.2 — The logline the player starts with:
"___________ hires Bunk to investigate ___________."
5.3 — Clue chain (each clue should point to the next; smell-clues marked 👃):
#
Clue
Where found
👃 or 🔍
Leads to
1




2




3




4




5




5.4 — Red herrings / "filler" (your concept's пълнеж/заблуда — dead-end scents):
__________________ (why it looks relevant / why it isn't)


5.5 — The solution moment — what makes the player sure who did it?


❓ Questions to answer:
Can the case be solved by logic alone, or does it need a lucky guess? (It should be logic.) - logic
What's the minimum number of clues required to unlock the finale? (Your open question — decide it.) - 3
What happens on a wrong deduction? (Your concept says the investigation stalls / client re-meets — define exactly.) - Client lowers payment, client loses patience; user gains less units needed to finish the level and go to the next one.
Is there one solution or branching? (For a demo: one solution, a few wrong paths, is plenty.) - a few

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
Delivery tech: ▢ Pre-rendered video (Video Player) ▢ In-engine 2D animation (Animator/Timeline) - upto discussion
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

6.8 Final confrontation
Your concept: face the likely culprit; may be a duel OR helping catch them.
Which is it for the demo? - hybrid; depends on the case. Bunk is strong enough to face a criminal who refuses to face the consequenses of their actions.
Is it skill-based (a fight/QTE), deduction-based (present evidence, catch the lie), or both? - both; the player needs enough evidence to get to the final "battle" of the level but the "battle" itself depends on the culprit.
❓ Can the player fail the finale, and what happens then? - yes. in case of fail in the final "battle" the case is solved but there is no justice for Bunk's client. In every level the solved cases bring some kind of "units" to the player. This units
help him in the next level - more informators; richer inventory etc.
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

Tip: keep this file in your repo next to your concept doc. Revisit Part 1 (pillars) whenever you're tempted to add a feature — if it doesn't serve a pillar, it doesn't go in the demo.