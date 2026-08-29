using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Flair.EditorTools
{
    /// <summary>
    /// Task 20: the noir grade. Black, white and blood-red per concept.md
    /// Part 1, lit entirely artificially per Part 3 -- there is no sun down
    /// here. A rough pass, done before any real art exists, because the grade
    /// changes what every texture reads as and task 15 cannot start against
    /// the wrong values.
    ///
    /// Three separate jobs, all run by one menu item:
    ///
    /// 1. THE GRADE. Overrides on the Global Volume's profile.
    /// 2. THE ENVIRONMENT. Kills the sun and the sky, drops ambient to almost
    ///    nothing, and turns on haze. Under a dome there is no sky to light
    ///    anything, so all fill light has to be deliberate.
    /// 3. THE PRACTICALS. Actual lights in the district -- caged wall lamps
    ///    throwing hard pools of white, with red only where the fiction earns
    ///    it. Without these the level is simply black.
    ///
    /// On why the palette works with a plain desaturation: every surface in
    /// the greybox is neutral grey, so no geometry can carry hue at all. The
    /// only colour that can reach the frame comes from the lights. Make the
    /// practicals white and the accents red, and you get black/white/red for
    /// free -- no per-hue trickery needed. That stops being true once textures
    /// arrive, and the saturation value here will need revisiting then.
    /// </summary>
    public static class NoirGrade
    {
        private const string LightRootName = "District Lighting";
        private const string MaterialFolder = "Assets/Materials";

        private static Material fixtureMat, bulbMat, redBulbMat;
        private static int fixturePieces;

        // Palette. Part 1: red is crime and urgency, white is truth.
        private static readonly Color Lamp = new Color(1f, 1f, 1f);
        // Pushed nearer pure red than a real bulb would be. The grade drains
        // 58% of the saturation out of everything, so the colour has to start
        // over-saturated to arrive on screen looking like red at all.
        private static readonly Color Red = new Color(1f, 0.02f, 0.035f);

        // How hard to push saturation back into the red band, on the curve's
        // scale where 0.5 is neutral and 1.0 is double. Raise it if the
        // accents still read pink; drop it if they start to glow radioactive.
        private const float RedSat = 0.88f;

        // URP attenuates punctual lights by inverse square, so intensity is
        // NOT "how bright the pool looks" -- it is brightness at one metre.
        // A wall lamp 6m above the pavement delivers intensity/36 by the time
        // it lands. Roughly: intensity = wanted_brightness * distance^2.
        //
        // That is why these numbers look enormous next to a default light's 1.
        // Range only sets where the light is culled; it does not scale it.
        //
        // The second factor is albedo. The greybox materials are deliberately
        // dark -- the ground is 16% -- so it reflects a sixth of what lands on
        // it. Full sum for a wall lamp: 200 / 7.2m^2 = 3.9 irradiance, times
        // 0.16 albedo = 0.62 linear, which ACES rolls to a soft mid-grey.
        // Re-derive these once real textures exist: their albedo will be
        // different, and these are tuned against 16% ground, not asphalt.
        //
        // Cone shape carries as much of the look as intensity. Wide outer
        // angles with a very small inner angle give light that falls off over
        // metres instead of stopping at a hard rim -- pools that bleed into
        // each other rather than spotlights on a stage.
        //
        // Part 3.5 wants roughly 5% of pixels red. Accents stay few and small.
        private const float LampIntensity = 200f;  // ~7m throw onto 16% ground
        private const float LampRange = 21f;
        private const float AccentIntensity = 45f; // lights its own fitting, ~1.5m

        // The red hazard lights on the dome ribs, 24m up and running the whole
        // length of the street. Point lights, so there is no cone to widen --
        // dispersion here is range, and range is the only thing that decides
        // whether these read as separate lamps or as one continuous red wash
        // overhead. At 24m spacing a 30m range means neighbours now overlap.
        private const float DomeHazardIntensity = 48f;
        private const float DomeHazardRange = 30f;

        [MenuItem("FLAIR/Noir/Apply Noir Look")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[FLAIR] Leave play mode before applying the noir look.");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Apply noir look");
            int group = Undo.GetCurrentGroup();

            bool graded = GradeVolumeProfile();
            SetEnvironment();
            PlacePracticals();

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log(
                "[FLAIR] Noir look applied" + (graded ? "." : " -- but NO volume profile was found, see warning above.") +
                $"\nBuilt {fixturePieces} fixture pieces. If that number is 0, the " +
                "lamps have no visible hardware and something failed silently.\n" +
                "The sun is off and the sky is gone: every lit surface is lit by " +
                "a lamp you can point at. Walk it in play mode, not the Scene view " +
                "-- the Scene view has its own lighting and will lie to you.\n" +
                "Tune LampIntensity / AccentIntensity in NoirGrade.cs and re-run.");
        }

        [MenuItem("FLAIR/Noir/Remove Practical Lights")]
        public static void RemoveLights()
        {
            var existing = GameObject.Find(LightRootName);
            if (existing == null)
            {
                Debug.Log("[FLAIR] No District Lighting object in the scene.");
                return;
            }

            Undo.DestroyObjectImmediate(existing);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        // ---------------------------------------------------------------
        // 1. The grade
        // ---------------------------------------------------------------

        /// <summary>
        /// Writes the overrides onto whichever profile the scene's Global
        /// Volume actually points at, rather than a hardcoded path -- if
        /// someone swaps the profile, this follows it.
        /// </summary>
        private static bool GradeVolumeProfile()
        {
            var volume = FindGlobalVolume();
            if (volume == null || volume.sharedProfile == null)
            {
                Debug.LogWarning("[FLAIR] No Global Volume with a profile in the scene. " +
                                 "The lighting is set up but nothing is graded.");
                return false;
            }

            var profile = volume.sharedProfile;
            Undo.RecordObject(profile, "Noir grade");

            // Clear out any null entries left behind by an earlier run that
            // added components without attaching them to the asset. Left in
            // place they stay dead forever, because TryGet cannot see them and
            // Add would only pile another one on top.
            int dead = profile.components.RemoveAll(c => c == null);
            if (dead > 0)
            {
                Debug.Log($"[FLAIR] Removed {dead} dead override slot(s) from the profile.");
            }

            // ACES crushes the blacks and rolls the highlights hard, which is
            // most of the look before any other override touches it.
            var tone = GetOrAdd<Tonemapping>(profile);
            tone.mode.overrideState = true;
            tone.mode.value = TonemappingMode.ACES;

            // Drain the colour and push the contrast. Not fully to -100:
            // the red accents have to survive, and they are the only hue in
            // the frame anyway.
            var colour = GetOrAdd<ColorAdjustments>(profile);
            Override(colour.postExposure, 0.75f);
            Override(colour.contrast, 25f);
            Override(colour.saturation, -58f);

            // That -58 drains the red accents along with everything else, and
            // ACES desaturates bright colour toward white on top of it, which
            // together land the accents as pink. Rather than lift the global
            // saturation -- which would un-noir the whole frame -- put the
            // saturation back into the red band alone.
            var curves = GetOrAdd<ColorCurves>(profile);
            curves.hueVsSat.overrideState = true;
            curves.hueVsSat.value = RedBoostCurve();

            // "Crushed shadow with no detail recovery. Most of the frame."
            var smh = GetOrAdd<ShadowsMidtonesHighlights>(profile);
            smh.shadows.overrideState = true;
            smh.shadows.value = new Vector4(0.9f, 0.9f, 0.95f, -0.08f);
            smh.highlights.overrideState = true;
            smh.highlights.value = new Vector4(1f, 1f, 1f, 0.1f);

            // Wet ground doubling every source, lamp glow, light shafts.
            var bloom = GetOrAdd<Bloom>(profile);
            Override(bloom.threshold, 0.85f);
            Override(bloom.intensity, 0.9f);
            Override(bloom.scatter, 0.72f);

            // Part 3.5: heavy grain, visible ink texture.
            var grain = GetOrAdd<FilmGrain>(profile);
            grain.type.overrideState = true;
            grain.type.value = FilmGrainLookup.Medium6;
            Override(grain.intensity, 0.45f);
            Override(grain.response, 0.75f);

            // Pulls the eye to the centre of frame. Part 3.5: the HUD borders
            // the map and never covers its centre -- this is a game about looking.
            var vig = GetOrAdd<Vignette>(profile);
            vig.color.overrideState = true;
            vig.color.value = Color.black;
            Override(vig.intensity, 0.45f);
            Override(vig.smoothness, 0.45f);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            WarnIfLdrGrading();
            return true;
        }

        private static Volume FindGlobalVolume()
        {
            foreach (var v in Object.FindObjectsByType<Volume>())
            {
                if (v.isGlobal) return v;
            }
            return null;
        }

        /// <summary>
        /// ACES in LDR grading mode bands badly in the near-blacks, which is
        /// exactly where this look lives. Worth flagging rather than changing:
        /// it is a project-wide setting and Viki renders through it too.
        /// </summary>
        private static void WarnIfLdrGrading()
        {
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null && urp.colorGradingMode == ColorGradingMode.LowDynamicRange)
            {
                Debug.LogWarning(
                    "[FLAIR] The active URP asset grades in Low Dynamic Range. This look " +
                    "lives in the near-blacks, which is where LDR grading bands worst. " +
                    "Consider setting Grading Mode to High Dynamic Range on PC_RPAsset -- " +
                    "but it is a shared setting, so agree it with Viki first.");
            }
        }

        // ---------------------------------------------------------------
        // 2. The environment
        // ---------------------------------------------------------------

        private static void SetEnvironment()
        {
            // Part 3: artificial light, no sun. The scene ships with a
            // directional light, which is the single biggest thing making the
            // greybox read as daytime outdoors.
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name != "Directional Light" || !go.activeSelf) continue;

                Undo.RecordObject(go, "Turn off the sun");
                go.SetActive(false);
                Debug.Log("[FLAIR] Directional Light disabled -- Part 3 says there is no sun " +
                          "down here. Re-enable it if you need to see the greybox flat.");
            }

            // No sky under a dome. Skybox ambient would otherwise keep filling
            // the scene with daylight even with the sun switched off.
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.13f, 0.134f, 0.15f);
            RenderSettings.reflectionIntensity = 0.15f;

            // Recycled air, visible as haze. Also does the heavy lifting on
            // depth: without it the far end of the street reads as flat black.
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.055f, 0.058f, 0.07f);
            RenderSettings.fogDensity = 0.012f;

            // With no skybox the camera would clear to whatever it likes.
            var cam = Camera.main;
            if (cam != null)
            {
                Undo.RecordObject(cam, "Black background");
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
            }
        }

        // ---------------------------------------------------------------
        // 3. The practicals
        // ---------------------------------------------------------------

        /// <summary>
        /// Hard pools of white from caged wall lamps, long throws of pure
        /// shadow between them (Part 3.5). Positions come from the greybox's
        /// own layout constants, so moving a street moves its lamps.
        /// </summary>
        private static void PlacePracticals()
        {
            RemoveExistingLights();
            LoadFixtureMaterials();
            fixturePieces = 0;

            var rootGo = new GameObject(LightRootName);
            Undo.RegisterCreatedObjectUndo(rootGo, "Place practical lights");
            var root = rootGo.transform;

            StreetLamps(Group(root, "Street"));
            Plaza(Group(root, "Plaza"));
            Alley(Group(root, "Alley"));
            Droggery(Group(root, "Droggery"));
            Accents(Group(root, "Red Accents"));
        }

        /// <summary>
        /// Alternating sides so the street reads as a rhythm of light and
        /// dark rather than an evenly lit corridor. 14m apart leaves real
        /// black between pools at a 17m range.
        /// </summary>
        private static void StreetLamps(Transform g)
        {
            const float spacing = 14f;
            bool east = true;

            for (float z = 4f; z < DistrictGreybox.UpperZ1; z += spacing)
            {
                // The plaza lights itself; skip its span.
                if (z > DistrictGreybox.StreetZ1 - 2f && z < DistrictGreybox.PlazaZ1) continue;

                float wallX = east ? DistrictGreybox.StreetW : -DistrictGreybox.StreetW;
                float x = east ? wallX - 0.5f : wallX + 0.5f;

                // Only the east-side lamps cast, to stay inside the shadow
                // atlas. Every lamp still lights; half of them also throw.
                WallLamp(g, $"StreetLamp_{z:0}", new Vector3(x, 5.5f, z), wallX, east);
                east = !east;
            }
        }

        private static void Plaza(Transform g)
        {
            const float pw = DistrictGreybox.PlazaW;

            // Shifted clear of the greybox's own Lamp_W / Lamp_E prop boxes,
            // which sit at z 44-45 west and z 50-51 east. Two fixtures in one
            // spot would z-fight.
            WallLamp(g, "PlazaLamp_W1", new Vector3(-pw + 0.5f, 5.5f, 47f), -pw, true);
            WallLamp(g, "PlazaLamp_W2", new Vector3(-pw + 0.5f, 5.5f, 57f), -pw);
            WallLamp(g, "PlazaLamp_E1", new Vector3(pw - 0.5f, 5.5f, 43f), pw);
            WallLamp(g, "PlazaLamp_E2", new Vector3(pw - 0.5f, 5.5f, 57f), pw);

            // The plaza's anchor should be the brightest thing in it -- it is
            // where the player is meant to look, and one of the three clues
            // sits at its foot. Lit from two directions.

            // Hung high over the fountain, throwing straight down. This is the
            // one that carries it: a hard top light on a headless statue casts
            // a long shadow across the basin, which is the whole point of a
            // dead fountain as a landmark.
            var top = MakeLight(g, "FountainTopLight", new Vector3(0f, 9f, 50f),
                                Quaternion.Euler(90f, 0f, 0f), LightType.Spot);
            top.color = Lamp;
            top.intensity = 85f;
            top.range = 18f;
            top.spotAngle = 88f;
            top.innerSpotAngle = 10f;
            top.shadows = LightShadows.Soft;

            // Slung on a wire spanning the plaza, the way a square gets lit
            // when there is nothing overhead to fix a lamp to.
            var tf = Group(g, "FountainTopLight_Fixture");
            Box(tf, "Wire", new Vector3(0f, 9.5f, 50f), new Vector3(pw * 2f, 0.07f, 0.07f), fixtureMat);
            Box(tf, "Flex", new Vector3(0f, 9.28f, 50f), new Vector3(0.07f, 0.44f, 0.07f), fixtureMat);
            Box(tf, "Shade", new Vector3(0f, 9.3f, 50f), new Vector3(0.95f, 0.18f, 0.95f), fixtureMat);
            Box(tf, "Bulb", new Vector3(0f, 9f, 50f), new Vector3(0.44f, 0.46f, 0.44f), bulbMat);

            // A weaker up-light from the basin rim, to keep the statue from
            // going solid black underneath. No shadows -- the top light owns
            // those, and the shadow atlas has a budget.
            var up = MakeLight(g, "FountainUplight", new Vector3(0f, 0.6f, 50f),
                               Quaternion.Euler(-90f, 0f, 0f), LightType.Spot);
            up.color = Lamp;
            up.intensity = 11f;
            up.range = 12f;
            up.spotAngle = 70f;
            up.innerSpotAngle = 8f;
            up.shadows = LightShadows.None;

            var uf = Group(g, "FountainUplight_Fixture");
            Box(uf, "Lens", new Vector3(0f, 0.55f, 50f), new Vector3(0.4f, 0.12f, 0.4f), bulbMat);
        }

        /// <summary>
        /// Dimmer and further apart than the street. Part 3.5 calls the alley
        /// the tightest chokepoint, and the blind dogleg only works if you
        /// cannot see what is round it.
        /// </summary>
        private static void Alley(Transform g)
        {
            // Strung on wires across the alley rather than bracketed, which is
            // both cheaper to read and more the right kind of squalid.
            Hang(g, "AlleyLamp_A1", new Vector3(-20.5f, 5f, 62f), 72f, 14f,
                 DistrictGreybox.LegAX0, DistrictGreybox.LegAX1);
            Hang(g, "AlleyLamp_A2", new Vector3(-20.5f, 5f, 70f), 58f, 13f,
                 DistrictGreybox.LegAX0, DistrictGreybox.LegAX1);
            // The dogleg corner falls in the 12m gap between A2 (z 70) and B1
            // (z 82), which left it unreadable rather than atmospheric -- dark
            // enough that you cannot tell there is a turn there at all.
            //
            // A lamp ON the corner rather than a brighter A2 or B1: hung at
            // the junction it lights the floor you are standing on and very
            // little of either leg, so you still cannot see round the corner
            // from down the alley. Brightening the neighbours would have lit
            // the legs instead and cost the blindness that makes it work.
            //
            // Dimmer and shorter-ranged than the rest of the alley on purpose.
            Hang(g, "AlleyLamp_Corner", new Vector3(-23f, 5f, 74.5f), 46f, 11f,
                 DistrictGreybox.LegBX0, DistrictGreybox.LegAX1);

            Hang(g, "AlleyLamp_B1", new Vector3(-25.5f, 5f, 82f), 72f, 14f,
                 DistrictGreybox.LegBX0, DistrictGreybox.LegBX1);
            // Where leg B opens east into the back yard (z 96). B1 sits at
            // z 82 with a 14m range, so it dies exactly at the turn, and the
            // yard lamp is 9m east of it near the rear door -- the corner
            // itself had nothing reaching it at all.
            //
            // Set just short of the opening so it lights the approach and
            // spills into the west end of the yard, which was the other dark
            // patch. This is the way in to the crime scene, so it wants to be
            // findable rather than atmospheric.
            Hang(g, "AlleyLamp_YardTurn", new Vector3(-25.5f, 5f, 95f), 56f, 12f,
                 DistrictGreybox.LegBX0, DistrictGreybox.LegBX1);

            Hang(g, "AlleyLamp_Yard", new Vector3(-16f, 5f, 99f), 85f, 16f,
                 DistrictGreybox.LegBX0, -10f);
        }

        private static void Droggery(Transform g)
        {
            // Inside the shop. The crime scene should be the one place in the
            // level that is properly lit -- the player has to read it.
            // Ceiling-mounted: the shop has a real ceiling at 6m to hang from.
            //
            // Kept down the centre line and at least 3m off every wall. The
            // room is only 5.2m tall, so a 135-degree cone up there washes the
            // walls at point-blank range and clips them to white -- inverse
            // square is brutal at a metre. Narrower cones, aimed at floor.
            Hang(g, "DroggeryInterior_1", new Vector3(0f, 5.2f, 95f), 110f, 14f, 0f, 0f, 6f, 95f);
            Hang(g, "DroggeryInterior_2", new Vector3(0f, 5.2f, 101f), 110f, 14f, 0f, 0f, 6f, 95f);

            // Spill onto the stoop through the shattered window. This sits
            // OUTSIDE the shop: indoors it was 1.5m from the front wall and
            // pointing straight at it, which blew the whole wall out.
            var spill = MakeLight(g, "WindowSpill", new Vector3(5.5f, 2.6f, 89f),
                                  Quaternion.Euler(38f, 180f, 0f), LightType.Spot);
            spill.color = Lamp;
            spill.intensity = 40f;
            spill.range = 9f;
            spill.spotAngle = 85f;
            spill.innerSpotAngle = 10f;
            spill.shadows = LightShadows.None;

            WallLamp(g, "StoopLamp", new Vector3(-9.5f, 5.5f, 91f), -10f);
        }

        /// <summary>
        /// The only colour in the level. Part 3.5 puts red on the apothecary
        /// sign, warning lamps and hazard lights, and asks for roughly 5% of
        /// pixels -- so these are few, small and short-ranged.
        /// </summary>
        private static void Accents(Transform g)
        {
            // The dead neon over Bunk's door. Dead, but not quite dead.
            Point(g, "OfficeNeon", new Vector3(0f, 4.8f, DistrictGreybox.OfficeZ1 + 0.5f), 40f, 9f);

            // The swinging warning lamp at the blocked arch.
            Point(g, "ArchWarning", new Vector3(18f, 5.8f, 50f), AccentIntensity, 9f);

            // The witch's mark on the droggery's bracket sign.
            Point(g, "WitchMark", new Vector3(9f, 4.6f, 93.5f), 40f, 9f);

            // Faint hazard lights along the dome ribs. High, dim and wide --
            // they are there to make the ceiling readable, not to light anything.
            for (float z = DistrictGreybox.GroundZ0 + 6f; z < DistrictGreybox.GroundZ1; z += 24f)
            {
                Point(g, $"DomeHazard_{z:0}", new Vector3(0f, DistrictGreybox.DomeY - 2f, z),
                      DomeHazardIntensity, DomeHazardRange);
            }
        }

        // ---------------------------------------------------------------
        // Light helpers
        // ---------------------------------------------------------------

        /// <summary>
        /// A caged lamp on a wall, throwing down and across the street.
        ///
        /// Shadows are opt-in because URP packs every shadow-casting light into
        /// one 2048 atlas: ask for eleven and it silently halves them all, so
        /// you trade a few crisp shadows for a lot of mushy ones. Chiaroscuro
        /// wants the crisp ones.
        /// </summary>
        private static void WallLamp(Transform g, string name, Vector3 pos, float wallX,
                                     bool shadows = false)
        {
            bool east = wallX > pos.x;

            var l = MakeLight(g, name, pos,
                              Quaternion.Euler(52f, east ? -90f : 90f, 0f), LightType.Spot);
            l.color = Lamp;
            l.intensity = LampIntensity;
            l.range = LampRange;
            l.spotAngle = 122f;
            l.innerSpotAngle = 10f;
            l.shadows = shadows ? LightShadows.Soft : LightShadows.None;

            // A bracket back to the wall, a hood, and a bulb the light appears
            // to come out of.
            var f = Group(l.transform.parent, name + "_Fixture");
            Box(f, "Bracket", new Vector3((pos.x + wallX) * 0.5f, pos.y + 0.42f, pos.z),
                new Vector3(Mathf.Abs(wallX - pos.x), 0.1f, 0.1f), fixtureMat);
            Box(f, "Hood", new Vector3(pos.x, pos.y + 0.32f, pos.z),
                new Vector3(0.66f, 0.14f, 0.66f), fixtureMat);
            Box(f, "Bulb", pos, new Vector3(0.4f, 0.42f, 0.4f), bulbMat);
        }

        /// <summary>
        /// A bulb hanging straight down. Pass wireX0 != wireX1 to string it on
        /// a wire across the alley; pass a ceilingY to drop it from a ceiling
        /// instead. No shadows -- see WallLamp.
        /// </summary>
        private static void Hang(Transform g, string name, Vector3 pos, float intensity,
                                 float range, float wireX0 = 0f, float wireX1 = 0f,
                                 float ceilingY = 0f, float cone = 135f)
        {
            var l = MakeLight(g, name, pos, Quaternion.Euler(90f, 0f, 0f), LightType.Spot);
            l.color = Lamp;
            l.intensity = intensity;
            l.range = range;
            l.spotAngle = cone;
            l.innerSpotAngle = 10f;
            l.shadows = LightShadows.None;

            var f = Group(l.transform.parent, name + "_Fixture");
            float top = ceilingY > 0f ? ceilingY : pos.y + 1.4f;

            if (!Mathf.Approximately(wireX0, wireX1))
            {
                Box(f, "Wire", new Vector3((wireX0 + wireX1) * 0.5f, top, pos.z),
                    new Vector3(Mathf.Abs(wireX1 - wireX0), 0.06f, 0.06f), fixtureMat);
            }

            Box(f, "Flex", new Vector3(pos.x, (pos.y + 0.3f + top) * 0.5f, pos.z),
                new Vector3(0.06f, top - pos.y - 0.3f, 0.06f), fixtureMat);
            Box(f, "Shade", new Vector3(pos.x, pos.y + 0.26f, pos.z),
                new Vector3(0.62f, 0.16f, 0.62f), fixtureMat);
            Box(f, "Bulb", pos, new Vector3(0.32f, 0.34f, 0.32f), bulbMat);
        }

        private static void Point(Transform g, string name, Vector3 pos, float intensity, float range)
        {
            var l = MakeLight(g, name, pos, Quaternion.identity, LightType.Point);
            l.color = Red;
            l.intensity = intensity;
            l.range = range;
            l.shadows = LightShadows.None;

            var f = Group(l.transform.parent, name + "_Fixture");
            Box(f, "Lens", pos, new Vector3(0.34f, 0.34f, 0.34f), redBulbMat);
        }

        /// <summary>
        /// Fixture geometry never casts shadows. A lamp housing wrapped round
        /// its own light source would shadow the light out entirely, and the
        /// shadow atlas has better things to spend slots on.
        /// </summary>
        private static void Box(Transform parent, string name, Vector3 centre,
                                Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = centre;
            go.transform.localScale = size;

            var r = go.GetComponent<MeshRenderer>();
            if (r != null)
            {
                r.sharedMaterial = mat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // Nothing should collide with a lamp six metres overhead.
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            fixturePieces++;
        }

        private static Light MakeLight(Transform parent, string name, Vector3 pos,
                                       Quaternion rot, LightType type)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetLocalPositionAndRotation(pos, rot);

            var l = go.AddComponent<Light>();
            l.type = type;
            // Realtime: nothing here is lightmapped yet, and baking a greybox
            // that gets regenerated is wasted time.
            l.lightmapBakeType = LightmapBakeType.Realtime;
            l.renderMode = LightRenderMode.Auto;
            return l;
        }

        private static Transform Group(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void RemoveExistingLights()
        {
            var existing = GameObject.Find(LightRootName);
            if (existing != null) Undo.DestroyObjectImmediate(existing);
        }

        // ---------------------------------------------------------------
        // Volume plumbing
        // ---------------------------------------------------------------

        /// <summary>
        /// Three materials for the fixtures: dark metal for the housings, and
        /// two emissive ones for the bulbs and lenses.
        ///
        /// The emissive colours are deliberately above 1.0. Bloom's threshold
        /// is 0.85, so a bulb at exactly white would sit right on the edge and
        /// only sometimes glow; pushing it well over means the source blooms
        /// reliably, which is what sells it as the thing emitting the light.
        /// </summary>
        private static void LoadFixtureMaterials()
        {
            fixtureMat = FixtureMaterial("Greybox_Fixture", new Color(0.09f, 0.09f, 0.1f), Color.black);
            bulbMat = FixtureMaterial("Greybox_Bulb", Color.white, Color.white * 4f);
            redBulbMat = FixtureMaterial("Greybox_BulbRed", new Color(0.4f, 0.02f, 0.04f), Red * 6f);
        }

        /// <summary>
        /// Creates the material if missing, and re-applies its settings either
        /// way. Returning an existing asset untouched would mean changing an
        /// emission value here did nothing until you deleted the .mat by hand.
        /// </summary>
        private static Material FixtureMaterial(string name, Color albedo, Color emission)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool isNew = mat == null;

            if (isNew)
            {
                if (!AssetDatabase.IsValidFolder(MaterialFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Materials");
                }

                var pipeline = GraphicsSettings.currentRenderPipeline;
                var shader = pipeline != null ? pipeline.defaultShader : Shader.Find("Standard");
                mat = new Material(shader) { name = name };
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", albedo);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", albedo);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);

            if (emission.maxColorComponent > 0f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
                // Nothing here is lightmapped, so keep emission out of GI.
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }

            if (isNew) AssetDatabase.CreateAsset(mat, path);
            else EditorUtility.SetDirty(mat);

            return mat;
        }

        /// <summary>
        /// VolumeProfile.Add puts the component in the profile's list but does
        /// NOT make it a child of the .asset on disk. The instance lives only
        /// in memory: it works for the rest of the editor session, then
        /// serialises as a null list entry and the override is silently gone
        /// after the next domain reload. Adding it through the inspector does
        /// this second step for you; from script it has to be done by hand.
        /// </summary>
        private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet<T>(out var component)) return component;

            component = profile.Add<T>(true);
            component.name = typeof(T).Name;
            component.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }

        /// <summary>
        /// Hue along x (0-1) against a saturation multiplier on y, where 0.5
        /// leaves a hue alone and 1.0 doubles it. Red sits at BOTH ends of the
        /// hue axis, so the boost has to be keyed at 0 and at 1 and fall back
        /// to neutral across the middle -- key only one end and half the reds
        /// in the frame stay pink.
        /// </summary>
        private static TextureCurve RedBoostCurve()
        {
            var curve = new AnimationCurve(
                new Keyframe(0f, RedSat),
                new Keyframe(0.09f, 0.5f),
                new Keyframe(0.9f, 0.5f),
                new Keyframe(1f, RedSat));

            // Loops, so the curve wraps continuously round the hue wheel.
            return new TextureCurve(curve, 0.5f, true, new Vector2(0f, 1f));
        }

        private static void Override(FloatParameter p, float value)
        {
            p.overrideState = true;
            p.value = value;
        }
    }
}
