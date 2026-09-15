# Building and sharing the demo

How to turn the project into something a teacher can play from a link — in the
browser, or as a Windows download — and put both on one itch.io page.

**Recommended:** upload **both** to the same page. The browser version is the
easy one to open; the Windows download is the fallback if the browser version
struggles on someone's machine.

---

## Part 1 — Build

Both builds are one menu item. Each produces a **zip ready to upload**, and the
folder opens on its own when it is done.

Everything goes into `Builds/` at the project root, which is gitignored — builds
never end up in the repo.

### Before any build

1. **Save the scene** (Ctrl+S). Unsaved changes are not in the build.
2. **Check the Console is clear of red errors.** A project that does not compile
   will not build.
3. **Close anything that has files in `Builds/` open** — an Explorer window
   inside it, or the game still running from last time.

### A. The web build (plays in the browser)

1. **`FLAIR → Build → Web (play in browser)`**
2. Wait. **The first web build is slow** — Unity re-imports the project for the
   web platform, which can take 5–15 minutes. Later builds are much faster.
3. When it finishes, Explorer opens on **`Builds/Flair-Web.zip`**.

What the tool sets for you, so you do not have to find these in Player Settings:
- **Compression: Gzip, with Decompression Fallback on.** Without this, itch.io
  shows a loading bar that never finishes. It is the most common reason Unity
  web builds fail there.
- **Default canvas 1280×720.**
- **The vision video is copied in for the build.** Browsers cannot play Unity's
  imported video clips, so the web build loads the MP4 by URL instead. This is
  automatic and cleans up after itself.

> **You cannot test a web build by double-clicking `index.html`.** Browsers block
> Unity's files when they are opened straight from disk, and you get a blank page
> or an error. Test it on itch.io instead — see Part 2, where you can keep the
> page private while you check it.

### B. The Windows build (download and run)

1. **`FLAIR → Build → Windows (.exe download)`**
2. Wait — this one is usually a couple of minutes.
3. Explorer opens on **`Builds/Flair-Windows.zip`**.

**Test it before uploading:** open `Builds/Windows/`, double-click the `.exe`,
and play through once. Unlike the web build, this one you *can* test locally.

---

## Part 2 — Put it on itch.io

### Create the account

1. Go to **itch.io** and **Register** (top right). Free.
2. Confirm your email — itch.io will not let you publish until you do.

### Create the project page

1. Top right, click the arrow next to your name → **Upload new project**.
2. Fill in:
   - **Title:** `FLAIR — Demo`
   - **Project URL:** leave the suggestion, or shorten it
   - **Short description:** one line, e.g. *A noir detective who solves crimes by smell.*
   - **Classification:** Games
   - **Kind of project:** **HTML** ← this is what makes it play in the browser

### Upload the web build

1. Under **Uploads**, click **Upload files** and choose **`Flair-Web.zip`**.
2. When it has uploaded, tick **"This file will be played in the browser"**.
   If you do not tick it, itch.io treats it as a download and nothing plays.

### Upload the Windows build

1. **Upload files** again, choose **`Flair-Windows.zip`**.
2. Next to it, tick the **Windows** platform icon.
3. Do **not** tick "played in the browser" on this one.

### Embed options (how the browser version appears)

In the section about embedding, set:
- **Viewport dimensions:** `1280` × `720`
- **Fullscreen button:** on — the teacher will want it, and on a phone it is
  the difference between playable and cramped
- **Mobile friendly:** **on** — the game shows touch controls on phones
- **Orientation:** **Landscape**
- **Automatically start on page load:** off, so it waits for a click. That
  first click also gives the page permission to take the mouse, which the
  camera needs.

### Details worth filling in

- **Description:** controls and what to do. Something like:
  > **Computer:** WASD to move, mouse to look, **hold F** to smell, **E** for the
  > scent library, Esc to free the mouse. **Click inside the game once** before
  > playing, so it can take the mouse.
  >
  > **Phone:** turn it sideways and tap fullscreen. Left thumb on the joystick to
  > walk, drag anywhere else to look, **hold SNIFF** to smell.
  >
  > Walk into the droggery at the north end of the street. Find the three scents.
- **A screenshot** — pages without one look abandoned.
- Say it is a **work-in-progress demo**, and that the character, animations and
  the 2D vision are placeholders being replaced with our own work.

### Keep it private, and share it with the teacher

At the bottom, under **Visibility & access**:

- **Draft** — only you can see it. Use this while you test.
- **Restricted** — only people with the password can open it. **Use this for
  the teacher.** Set a password, then send him the page link and the password.
- **Public** — anyone. Not needed for a diploma demo.

Click **Save**. Then **View page** and play it yourself before sending the link.

---

## Part 3 — Check it before you send the link

Open the itch.io page **in a private/incognito window**, as if you were the
teacher, and check:

- [ ] The web version **finishes loading**. A bar that stops near the end is
      almost always compression — rebuild with the FLAIR menu, not Unity's
      Build button with different settings.
- [ ] **Click inside the game**, then the mouse turns the camera.
- [ ] Bunk walks, and **holding F at a marker plays the red ink vision** — not
      the grey "NOT DRAWN YET" panel. If you get the panel, the video did not
      reach the build; tell Viki/Claude.
- [ ] The three clues lead to the **end card**.
- [ ] The **Windows download** unzips and runs.

### On a phone — test both kinds

Phones differ far more than computers, so check **one iPhone and one Android**
before sending the link. Open the page link on the phone itself.

- [ ] Turn it **sideways** and tap the **fullscreen** button.
- [ ] A **joystick** appears bottom-left, and **SNIFF**, **JUMP** and **SCENTS**
      buttons on the right. (They do not appear on a computer — that is correct.)
- [ ] **Left thumb on the joystick walks.** Dragging anywhere else turns the
      camera — and walking with one thumb while looking with the other works.
- [ ] Standing at a scent, the prompt says **"Hold SNIFF"**, not "Hold F".
- [ ] **Holding SNIFF** plays the red ink vision, and the controls disappear
      while it plays.
- [ ] **SCENTS** opens the scent library, and tapping it again closes it.
- [ ] It does not **crash or reload** part-way through — see below if it does.

**Phones are the one thing we cannot fully promise.** A browser on a phone gets far
less memory than on a computer, and iPhones are the strictest. If the page reloads
itself or goes blank on a phone, that is memory, and the likeliest cause is the
53 MB placeholder character. Replacing him with our own Bunk inside the size budget
in `assets.md` is the real fix. Until then, point people to a computer.

### Preview the phone controls without a phone

In Unity: **`FLAIR → Build → Preview Touch Controls In Editor`**, then press Play.
The on-screen controls appear, with the mouse acting as one finger. Good for
checking the layout; it cannot test two thumbs at once, so still try a real phone.
Click the menu item again to turn it off.

**Windows will warn about the .exe.** Because the game is not signed, Windows
shows *"Windows protected your PC."* Tell the teacher to click **More info →
Run anyway**. This is normal for student and indie builds.

---

## Updating the demo later

1. Build again with the FLAIR menu.
2. On the itch.io page: **Edit game** → next to the old file, **delete** it →
   upload the new zip → re-tick **played in the browser** (or **Windows**).
3. Save. The link stays the same, so the teacher does not need a new one.

---

## If something goes wrong

| Symptom | Likely cause |
|---|---|
| Loading bar never finishes on itch.io | Compression. Rebuild through `FLAIR → Build → Web`. |
| Blank page when opening `index.html` locally | Expected — web builds only run from a server. Test on itch.io. |
| Mouse will not turn the camera in the browser | Click inside the game once first. |
| Vision shows the grey panel in the browser | The video was not copied into the build. Check the Console during the build for `[WebVisionBuildStep] Copied 1 vision(s)`. |
| Very slow in the browser | The noir grade and lighting are heavier in a browser. Offer the Windows download. |
| "Windows protected your PC" | Unsigned build. More info → Run anyway. |
| No touch controls on a phone | Mobile friendly is off on the itch.io page, or the build is older than the touch controls. |
| Page reloads or goes blank on a phone | Out of memory. See the phone section above. |
| Camera spins while walking on a phone | The look zone and joystick overlapping. Tell Viki/Claude. |
| Controls too small or large on a phone | Sizes are in `TouchControls.cs`, laid out on a 1920×1080 reference. |
