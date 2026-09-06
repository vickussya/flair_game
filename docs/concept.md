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

WHERE EVERYTHING LIVES
This document was one long file and got hard to search. It is now split by subject.
Parts 0 and 1 stay here because everything else is judged against them.

[world.md](world.md)
  PART 2   Source material fidelity — what we keep from the book, what we change
  PART 3   World Bible — Unterstadt, the dome, the district
  PART 3.5 Game map — the Bismarkstrasse block, landmarks, HUD layout
           Pictures: /reference/game_map/

[case.md](case.md)
  PART 4   Characters — the cast, what they want, what they hide, their scents
  PART 5   The Case — Level 1, "The Hollow Vial": the truth, the clue chain,
           the red herrings, the solution moment, the endings

[systems.md](systems.md)
  PART 6   Gameplay systems — core loop, movement, smell, investigate, visions,
           dialogue, the wolf, the finale, progression, the mind board

[production.md](production.md)
  PART 7   Content list
  PART 8   Technical design (Unity + C#)
  PART 9   Production plan
  PART 10  Decision gate

[tasks.md](tasks.md)
  Who is building what, by when. The live board — read this one weekly.

[assets.md](assets.md)
  How art gets from Blender and Krita into the game, what belongs in the repo
  and what does not, and the storage budget. Read before committing anything
  binary.

RULE: all of these are edited on the dev branch only, never on a personal branch.
