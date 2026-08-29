# 2D Vision Style Guide (Task 17)

Settles how every hand-drawn vision looks, so two people drawing separate
visions (18a/18b) produce frames that read as one game. Palette is fixed by
`concept.md` Part 1 — black, white, blood-red, nothing else.

Test frames live in
[`reference/2D_cutscenes/vision-tests/`](../reference/2D_cutscenes/vision-tests/) —
name new ones descriptively, e.g. `vision-test-frame-01.png`.

---

## Palette

Black, white and red only — no other hue, per Part 1. Fill in the exact values
you're drawing with so both of us pick from the same three swatches.

- Black: `#120417`
- White: `#ffffff`
- Blood-red: `#b60c28`

## Line & brush

- Line weight: `min 10px`
- Brush name / tool: `Ink-3 Gpen`
- Contrast target (how hard the black/white split is): `hard split/ high contrast`

## Red — how much, and what earns it

Part 1: red is crime, urgency, fatality — used sparingly and always
meaningfully. Roughly 5% of pixels per Part 3.5.

- What earns red in a frame: `the object that matters to the plot; danger / violence`
- Rough % of frame it should ever cover: `18-22`

## Format

- Frame rate: `12`
- Resolution: `for stills: Krita export - 4k; Unity downscale - 2048; for animations: Krita export: 2048; Unity import: 1024
- Export format: `PNG, 32-bit RGBA, lossless, store alpha channel (transparency)`

## Test frames

List each test frame and what it's testing.

Rule these two establish: **red could relatively mark a hint** — where exactly it
lands can shift depending on whether the hint is an object or a person (silhouette).
For an object-hint, red could color the object itself, or, in some instances, around the important object. For a person-hint, red can instead mark the space/context around them while the figure stays a flat black silhouette — one variation, not a fixed pairing; the silhouette doesn't have to always sit on a red background.

1. ![wine glass, hard black/white/red split](../reference/2D_cutscenes/vision-tests/vision-test-frame-01.png)
   `vision-test-frame-01.png` — object-hint: hard-edge black/white/red split,
   red fills the hinted object itself (the wine).
2. ![silhouette figure in a red archway](../reference/2D_cutscenes/vision-tests/vision-test-frame-02.png)
   `vision-test-frame-02.png` — person-hint example: flat black silhouette
   figure, red marks the archway/frame around them instead of the figure —
   one variation of how a person-hint can be handled, not the only one.
3. ![red ribbon motif with gradient shading](../reference/2D_cutscenes/vision-tests/vision-test-frame-03.png)
   `vision-test-frame-03.png` — experiment: gradient-shaded red form (scent trail) with a
   white highlight line, unlike frames 1–2's flat fill. Testing whether
   gradients can add depth without breaking the flat noir look.

## Shared with Viki

- [x] Test frames shown to Viki
- [x] Viki agrees the format/palette/line weight before drawing 18a

**Done when:** two people could draw a frame each and they would look like the
same game.
