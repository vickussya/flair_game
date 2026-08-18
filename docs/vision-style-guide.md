# 2D Vision Style Guide (Task 17)

Settles how every hand-drawn vision looks, so two people drawing separate
visions (18a/18b) produce frames that read as one game. Palette is fixed by
`concept.md` Part 1 — black, white, blood-red, nothing else.

Test frames live in [`reference/2D_cutscenes/`](../reference/2D_cutscenes/) —
name new ones descriptively, e.g. `vision-test-frame-01.png`.

---

## Palette

Black, white and red only — no other hue, per Part 1. Fill in the exact values
you're drawing with so both of us pick from the same three swatches.

- Black: `#120417`
- White: `#ffffff`
- Blood-red: `#b60c28`

## Line & brush

- Line weight: `5px`
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

1. `____________` — `____________`
2. `____________` — `____________`
3. `____________` — `____________`

## Shared with Viki

- [ ] Test frames shown to Viki
- [ ] Viki agrees the format/palette/line weight before drawing 18a

**Done when:** two people could draw a frame each and they would look like the
same game.
