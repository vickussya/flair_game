using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Flair.EditorTools
{
    /// <summary>
    /// Builds the Bismarkstrasse demo district as a greybox, from concept.md
    /// Part 3.5. Shapes and sightlines only -- no art, per task 13's M1 scope.
    ///
    /// This is a generator rather than hand-placed geometry for two reasons:
    /// SampleScene.unity is one-person-at-a-time and does not merge, so the
    /// layout lives in a file that DOES merge; and walking a greybox always
    /// says the distances are wrong, so regenerating from tweaked numbers beats
    /// dragging 200 cubes around.
    ///
    /// Everything is driven by the coordinate table below. Change a number,
    /// run the menu item again, walk it again.
    ///
    /// Axes: +x is east, +z is north, y is up. The street runs south to north,
    /// office at the south end, droggery at the north.
    ///
    /// Connectivity is Part 3.5's: a loop with one spur. Office - Street -
    /// Plaza - Droggery front, the alley as a parallel back route from the
    /// plaza to the droggery rear, and the arch as a decorative dead end. A
    /// crossroads and a second north-south street were tried and taken back
    /// out; the loop is the shape the level wants.
    ///
    /// DEVIATES FROM Part 3.5 on width only. The first pass was built to the
    /// letter -- 8m streets, a 20x18 plaza, a 3m alley -- and walked as
    /// claustrophobic in the bad sense, so everything is wider here.
    /// </summary>
    public static class DistrictGreybox
    {
        private const string RootName = "District";
        private const string MaterialFolder = "Assets/Materials";

        // ---------------------------------------------------------------
        // The layout, in metres.
        //
        //           rear yard  ..  droggery         z 92..102
        //             |            stoop            z 86..90
        //        leg B |           upper street     z 62..86
        //      dogleg -+                            z 72..77
        //        leg A |                            z 59..72
        //       mouth  +-------->  PLAZA            z 38..62  <-> blocked arch
        //                          lower street     z -1..38
        //                          office           z -8..-1
        // ---------------------------------------------------------------

        internal const float StreetW = 6f;      // main street runs x -6..6
        internal const float PlazaW = 15f;      // plaza widens to x -15..15
        internal const float MassH = 18f;       // buildings: 5-6 storeys at ~3m
        // Set false to take the lid off entirely and leave open space above the
        // rooftops. The level stays sealed either way -- the buildings do that,
        // not the ceiling.
        //
        // static readonly rather than const on purpose: a const here makes the
        // branch below it a compile-time constant, and the compiler warns about
        // unreachable code in whichever state you are not using.
        private static readonly bool Ceiling = true;

        // Part 3.5 says the dome is "~40m up" AND that the buildings are
        // "capped by the dome ceiling hanging low overhead". With 18m buildings
        // those cannot both hold -- 40m leaves a 22m void above the rooftops
        // and the street stops feeling lidded. Going with "capped": 26m puts it
        // 8m over the roofs, close enough to read as a ceiling. Set it back to
        // 40 here if you want the void.
        internal const float DomeY = 26f;

        // South-to-north landmarks, as z coordinates.
        internal const float OfficeZ0 = -9f;    // back of the office recess
        internal const float OfficeZ1 = -1f;    // office doorway meets the street
        internal const float StreetZ1 = 38f;    // lower street ends at the plaza
        internal const float PlazaZ1 = 62f;     // plaza ends, upper street begins
        internal const float UpperZ1 = 90f;     // upper street ends at the stoop
        internal const float StoopZ1 = 94f;     // stoop ends, droggery front wall
        internal const float DrogZ1 = 106f;     // back of the droggery

        // Building rows sit 12m apart either side of the main street.
        internal const float RowW = -18f, RowE = 18f;

        // The service alley, 5m wide: west off the plaza, north up leg A, a
        // sharp dogleg west, then leg B to the droggery's back yard. Legs A and
        // B do not overlap in x at all -- a true right-angle corner, which is
        // what makes it properly blind.
        internal const float MouthZ0 = 54f, MouthZ1 = 59f;
        internal const float LegAX0 = -23f, LegAX1 = -18f;
        internal const float DogZ0 = 72f, DogZ1 = 77f;
        internal const float LegBX0 = -28f, LegBX1 = -23f;
        internal const float YardZ0 = 96f, YardZ1 = 102f;

        // Ground / dome extents, wide enough to cover the alley and the arch.
        internal const float GroundX0 = -33f, GroundX1 = 29f;
        internal const float GroundZ0 = -9f, GroundZ1 = 111f;

        private static Transform root;
        private static Material groundMat, massMat, propMat;

        [MenuItem("FLAIR/Greybox/Build Bismarkstrasse District")]
        public static void Build()
        {
            // Building in play mode would throw the whole thing away on exit.
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[FLAIR] Leave play mode before building the district.");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Build Bismarkstrasse greybox");
            int group = Undo.GetCurrentGroup();

            RemoveExisting();
            LoadMaterials();

            var rootGo = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(rootGo, "Build district");
            root = rootGo.transform;

            BuildShell();
            BuildOffice();
            BuildLowerStreet();
            BuildPlaza();
            BuildBlockedArch();
            BuildUpperStreet();
            BuildServiceAlley();
            BuildDroggery();
            BuildScentAnchors();

            RetireStage1Room();
            MovePlayerToOfficeDoor();
            KeepExistingMarkerReachable();

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Selection.activeGameObject = rootGo;

            Debug.Log(
                "[FLAIR] Bismarkstrasse greybox built.\n" +
                "Walk it (play mode) before trusting any of the distances.\n" +
                "Three empty anchors named ScentAnchor_* mark where the clues go " +
                "-- Viki's task 24 drops markers on those.\n" +
                "Save the scene, push, then tell Viki the scene is free.");
        }

        [MenuItem("FLAIR/Greybox/Remove District")]
        public static void Remove()
        {
            if (!RemoveExisting())
            {
                Debug.Log("[FLAIR] No District object in the scene.");
                return;
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[FLAIR] District removed. The Stage 1 room is still disabled -- " +
                      "re-enable it by hand if you want it back.");
        }

        // ---------------------------------------------------------------
        // Sections
        // ---------------------------------------------------------------

        /// <summary>Ground, and the dome lid that replaces sky entirely.</summary>
        private static void BuildShell()
        {
            var g = Group("Shell");

            // Two ground slabs, not one. The office doorway sits below street
            // level, and a single slab cannot be dug into -- so the street
            // surface stops at the doorway and a lower slab carries on south.
            // Their top faces are at different heights, which is the point:
            // anything coplanar with the ground z-fights with it.
            Block("Ground", g, GroundX0, GroundX1, OfficeZ1, GroundZ1, -1f, 0f, groundMat);
            Block("Ground_OfficeWell", g, GroundX0, GroundX1, GroundZ0, OfficeZ1,
                  -1.45f, -0.45f, groundMat);

            // The map has a roof and you can see it (Part 3.5). Set Ceiling to
            // false for open sky above the rooftops instead -- the level is
            // sealed by the buildings either way, so nothing else depends on it.
            if (!Ceiling) return;

            Block("DomeCeiling", g, GroundX0, GroundX1, GroundZ0, GroundZ1, DomeY, DomeY + 2f);

            // Ribs, so the ceiling reads as structure rather than a flat lid.
            // Inset and sunk into the slab so no face of theirs lands in the
            // same plane as one of its faces.
            for (float z = GroundZ0 + 6f; z < GroundZ1; z += 12f)
            {
                Block("DomeRib", g, GroundX0 + 0.5f, GroundX1 - 0.5f, z, z + 1.2f,
                      DomeY - 1.2f, DomeY + 0.5f);
            }
        }

        /// <summary>
        /// South end: a doorway under a dead neon sign, three steps down to
        /// street level. The start point.
        ///
        /// The recess runs the full width of the street, so the south end does
        /// not narrow. Part 3.5 asks for a block "pinched at both ends" and a
        /// half-width recess delivers that, but it was tried and walked worse:
        /// the street reads better opening straight out of the doorway.
        /// </summary>
        private static void BuildOffice()
        {
            var g = Group("South - Bunks Office");

            // Sit exactly on the well floor: overlapping it instead would put
            // two same-facing surfaces in one plane at the foot of the wall.
            Block("OfficeBlock_W", g, RowW, -StreetW, OfficeZ0, OfficeZ1, -0.45f, MassH);
            Block("OfficeBlock_E", g, StreetW, RowE, OfficeZ0, OfficeZ1, -0.45f, MassH);
            Block("OfficeBlock_Back", g, -StreetW, StreetW, OfficeZ0, OfficeZ0 + 4f, -0.45f, MassH);
            Block("OfficeLintel", g, -StreetW, StreetW, OfficeZ0 + 4f, OfficeZ1, 4f, MassH);

            // Two treads bridging street level (0) down to the well floor
            // (-0.45): three 0.15 rises in all, well inside the
            // CharacterController's 0.3 step offset. Each tread sits ON the
            // well floor and stops short of the surface above it, so no two
            // faces end up coplanar.
            for (int i = 0; i < 2; i++)
            {
                float top = -0.15f * (i + 1);
                float z = OfficeZ1 - (i + 1);
                Block($"OfficeStep_{i + 1}", g, -5f, 5f, z, z + 1f, -0.45f, top, propMat);
            }

            // Both stand proud of the facade rather than flush with it. Flush
            // means their front faces share a plane with the wall's, and the
            // two flicker against each other -- which is what you see from the
            // spawn point, since this is the first thing you look at.
            Block("DeadNeonSign", g, -2.4f, 2.4f, OfficeZ1 - 0.15f, OfficeZ1 + 0.25f, 4.2f, 5.4f, propMat);
            Block("LitWindow", g, -1.6f, 1.6f, OfficeZ1 - 0.15f, OfficeZ1 + 0.15f, 6.5f, 8.5f, propMat);
        }

        /// <summary>
        /// Lower street: cracked asphalt, tram rails ending in rubble,
        /// burnt-out chassis shoved against the kerb. One unbroken run from the
        /// office to the plaza.
        /// </summary>
        private static void BuildLowerStreet()
        {
            var g = Group("Lower Street");

            Block("Row_W", g, RowW, -StreetW, OfficeZ1, StreetZ1, 0f, MassH);
            Block("Row_E", g, StreetW, RowE, OfficeZ1, StreetZ1, 0f, MassH);

            // Solid fill out to the map edge behind both rows.
            Block("Fill_W", g, GroundX0, RowW, GroundZ0, StreetZ1, 0f, MassH);
            Block("Fill_E", g, RowE, GroundX1, GroundZ0, StreetZ1, 0f, MassH);

            Block("Kerb_W", g, -StreetW, -StreetW + 0.4f, OfficeZ1, StreetZ1, 0f, 0.25f, propMat);
            Block("Kerb_E", g, StreetW - 0.4f, StreetW, OfficeZ1, StreetZ1, 0f, 0.25f, propMat);

            // Rusted tram rails, ending in rubble rather than going anywhere.
            Block("TramRail_W", g, -2.4f, -2.1f, 2f, 24f, 0f, 0.15f, propMat);
            Block("TramRail_E", g, 2.1f, 2.4f, 2f, 24f, 0f, 0.15f, propMat);
            Rubble("RailRubble", g, new Vector3(0f, 0f, 25f), 4, 2.4f);

            // Barricades against the kerbs. They break the sightline north so
            // the plaza is not visible from the office door (6.1 wants exploring).
            Block("Chassis_W", g, -5.6f, -2.8f, 10f, 14.5f, 0f, 1.5f, propMat);
            Block("Chassis_E", g, 2.8f, 5.6f, 30f, 34.5f, 0f, 1.4f, propMat);

            // Pipework vented at ankle height, cable bundles swagged overhead.
            Block("Pipes_W", g, -StreetW, -StreetW + 0.5f, 4f, 22f, 0.4f, 0.9f, propMat);
            Block("Cables_Overhead", g, -StreetW, StreetW, 18f, 18.6f, 7f, 7.4f, propMat);
            Block("StreetPlate", g, -StreetW, -StreetW + 0.4f, 30f, 31.6f, 3.4f, 4.6f, propMat);
        }

        /// <summary>
        /// Centre: the plaza, the hub and main chokepoint. Four routes meet --
        /// street south, upper street north, the alley mouth west, the blocked
        /// arch east.
        /// </summary>
        private static void BuildPlaza()
        {
            var g = Group("Centre - Plaza");

            // West side, split by the alley mouth.
            Block("PlazaWall_W_S", g, GroundX0, -PlazaW, StreetZ1, MouthZ0, 0f, MassH);
            Block("PlazaWall_W_N", g, LegAX1, -PlazaW, MouthZ1, PlazaZ1, 0f, MassH);

            // The dead fountain: headless robed statue in a dry cracked basin.
            Cylinder("FountainBasin", g, new Vector3(0f, 0.4f, 50f), 5f, 0.8f, propMat);
            Cylinder("FountainPlinth", g, new Vector3(0f, 1.0f, 50f), 1.4f, 0.4f, propMat);
            Cylinder("HeadlessStatue", g, new Vector3(0f, 2.4f, 50f), 1f, 2.8f, propMat);

            // Caged wall lamps -- the only light down here is artificial.
            Block("Lamp_W", g, -PlazaW, -PlazaW + 0.5f, 44f, 45f, 5f, 5.8f, propMat);
            Block("Lamp_E", g, PlazaW - 0.5f, PlazaW, 56f, 57f, 5f, 5.8f, propMat);
        }

        /// <summary>
        /// East: the Blocked Arch. Ends the world without a fence -- a grand
        /// transit mouth choked with rubble.
        /// </summary>
        private static void BuildBlockedArch()
        {
            var g = Group("East - Blocked Arch");

            Block("ArchWall_S", g, PlazaW, GroundX1, StreetZ1, 46f, 0f, MassH);
            Block("ArchWall_N", g, PlazaW, GroundX1, 54f, PlazaZ1, 0f, MassH);

            // The mouth: open below 7m, solid mass above. Stops where the
            // collapse plug starts rather than overlapping it.
            Block("ArchLintel", g, PlazaW, 24f, 46f, 54f, 7f, MassH);

            // A solid plug does the sealing. Loose rubble at ankle height is not
            // a wall, and the map edge is a few metres past the far side.
            Block("ArchCollapse", g, 24f, GroundX1, 46f, 54f, 0f, MassH);
            Rubble("ArchRubble", g, new Vector3(22.5f, 0f, 50f), 7, 4f);
            Block("WarningLamp", g, 18f, 18.6f, 49.6f, 50.4f, 5.4f, 6.2f, propMat);
        }

        /// <summary>
        /// Upper street, with the semi-collapsed building on the east row --
        /// sheared open, floors visible in cross-section.
        /// </summary>
        private static void BuildUpperStreet()
        {
            var g = Group("Upper Street");

            // The sliver between the plaza's north-west corner and leg A is
            // PlazaWall_W_N's job -- a second block over the same space would
            // be a duplicate, and two identical boxes flicker on every face.
            Block("Row_W", g, RowW, -StreetW, PlazaZ1, UpperZ1, 0f, MassH);

            Block("Row_E_S", g, StreetW, GroundX1, PlazaZ1, 70f, 0f, MassH);
            Block("Row_E_N", g, StreetW, GroundX1, 80f, UpperZ1, 0f, MassH);

            // Only the back half of the collapsed block still stands. The front
            // half is open to the street, floors hanging in cross-section.
            Block("Collapsed_Back", g, 11f, GroundX1, 70f, 80f, 0f, MassH);
            for (int floor = 1; floor <= 5; floor++)
            {
                float y = floor * 3f;
                Block($"Collapsed_Floor_{floor}", g, StreetW, 11f, 70f, 80f, y, y + 0.4f, propMat);
            }
            Rubble("CollapseSpill", g, new Vector3(8f, 0f, 75f), 5, 3.5f);

            Block("Kerb_W", g, -StreetW, -StreetW + 0.4f, PlazaZ1, UpperZ1, 0f, 0.25f, propMat);
        }

        /// <summary>
        /// West: the Service Alley. The parallel back route from the plaza to
        /// the droggery's rear door -- the spur that makes the map a loop, so
        /// the player can circle the crime scene. A sharp dogleg partway up
        /// makes a blind corner: legs A and B share only a corner and do not
        /// overlap in x at all, so there is no sightline round it either way.
        /// </summary>
        private static void BuildServiceAlley()
        {
            var g = Group("West - Service Alley");

            // Outer wall, west of the whole run.
            Block("AlleyOuter_A", g, GroundX0, LegAX0, MouthZ0, DogZ0, 0f, MassH);
            Block("AlleyOuter_B", g, GroundX0, LegBX0, DogZ0, YardZ0, 0f, MassH);

            // The blind corner: coming north up leg A you hit this and must
            // jog west before you can carry on.
            Block("DoglegMass", g, LegBX1, RowW, DogZ1, YardZ0, 0f, MassH);

            // Between the stoop and the alley.
            Block("StoopFlank_W", g, RowW, -10f, UpperZ1, YardZ0, 0f, MassH);

            // Back yard walls.
            Block("YardWall_W", g, GroundX0, LegBX0, YardZ0, YardZ1, 0f, MassH);
            Block("YardWall_N", g, GroundX0, -10f, YardZ1, GroundZ1 - 1f, 0f, MassH);

            // Bins and crates. They hug the walls on purpose: the player capsule
            // is 1.16m across including skin width, so anything that leaves less
            // than ~1.7m stops being clutter and starts being a wall.
            Block("Bin_1", g, LegAX0, LegAX0 + 1.1f, 62f, 63.2f, 0f, 1.3f, propMat);
            Block("Crate_1", g, LegAX0, LegAX0 + 1.1f, 67f, 68.4f, 0f, 1.1f, propMat);
            Block("Crate_2", g, LegAX0 + 0.1f, LegAX0 + 1f, 67.7f, 68.9f, 1.1f, 2f, propMat);
            Block("Bin_2", g, LegBX0, LegBX0 + 1.1f, 82f, 83.2f, 0f, 1.3f, propMat);
            Block("Crate_3", g, LegBX0, LegBX0 + 1.1f, 88f, 89.4f, 0f, 1.2f, propMat);

            // Fire escapes, and the raised catwalk that casts a hard bar of
            // shadow across the street once task 20's lighting lands.
            Block("Catwalk", g, LegAX0, LegAX1, 64f, 66f, 5.5f, 5.8f, propMat);
            Block("FireEscape_A", g, LegAX1 - 0.6f, LegAX1, 60f, 64f, 3f, 3.3f, propMat);
            Block("FireEscape_B", g, LegAX1 - 0.6f, LegAX1, 66f, 70f, 6f, 6.3f, propMat);
        }

        /// <summary>
        /// North end: the droggery. The crime scene and the objective. Front
        /// window shattered OUTWARD, glass on the pavement. Rear door ajar onto
        /// the back yard.
        /// </summary>
        private static void BuildDroggery()
        {
            var g = Group("North - The Droggery");

            // Caps the east side of the block. Without it the stoop opens onto
            // bare ground and the player walks off the map.
            Block("NorthCap_E", g, 10f, GroundX1, UpperZ1, GroundZ1 - 1f, 0f, MassH);

            // Three steps UP to the stoop -- it is raised on a stone plinth.
            for (int i = 0; i < 3; i++)
            {
                float y = 0.15f * (i + 1);
                float z = UpperZ1 + i;
                Block($"Stoop_Step_{i + 1}", g, -6f, 6f, z, z + 1f, 0f, y, propMat);
            }
            Block("Stoop", g, -10f, 10f, UpperZ1 + 3f, StoopZ1, 0f, 0.45f, propMat);

            // Front wall: door gap x -2..2, shattered window gap x 3..8.
            const float wallY = 6f;
            Block("Front_Pier_W", g, -10f, -2f, StoopZ1, StoopZ1 + 1f, 0f, wallY);
            Block("Front_Pier_Mid", g, 2f, 3f, StoopZ1, StoopZ1 + 1f, 0f, 4.2f);
            Block("Front_Pier_E", g, 8f, 10f, StoopZ1, StoopZ1 + 1f, 0f, wallY);
            Block("Front_WindowSill", g, 3f, 8f, StoopZ1, StoopZ1 + 1f, 0f, 1f);
            Block("Front_Lintel", g, -2f, 8f, StoopZ1, StoopZ1 + 1f, 4.2f, wallY);

            // Glass on the pavement rather than inside -- it broke outward.
            Rubble("ShatteredGlass", g, new Vector3(5.5f, 0.45f, StoopZ1 - 1.5f), 6, 3.2f, 0.25f);

            // Hanging iron bracket sign with the witch's mark.
            Block("BracketArm", g, 7.8f, 9.6f, StoopZ1 - 0.2f, StoopZ1 + 0.1f, 5.2f, 5.5f, propMat);
            Block("WitchMarkSign", g, 8.6f, 9.6f, StoopZ1 - 0.3f, StoopZ1, 3.9f, 5.2f, propMat);

            // Shell. Rear door gap on the west wall, 3m wide, onto the yard.
            // Everything here is written relative to StoopZ1 / DrogZ1 so moving
            // the droggery does not leave the furniture behind in the street.
            Block("Wall_W_S", g, -10f, -9f, StoopZ1 + 1f, StoopZ1 + 3f, 0f, wallY);
            Block("Wall_W_N", g, -10f, -9f, StoopZ1 + 6f, DrogZ1, 0f, wallY);
            Block("Wall_E", g, 9f, 10f, StoopZ1 + 1f, DrogZ1, 0f, wallY);
            // Fits between the side walls rather than sharing their outer
            // faces -- the west one is visible from the back yard.
            Block("Wall_N", g, -9f, 9f, DrogZ1 - 1f, DrogZ1, 0f, wallY);
            Block("Ceiling", g, -10f, 10f, StoopZ1, DrogZ1, wallY, wallY + 0.5f);

            // The building above the shop: widest and most ornate on the block.
            Block("UpperFacade", g, -10f, 10f, StoopZ1, DrogZ1, wallY + 0.5f, MassH);

            // Interior. Floor-to-ceiling bottles, long counter, overturned
            // stool, wall cabinet standing open and emptied.
            Block("Shelves_E", g, 8f, 9f, StoopZ1 + 2f, DrogZ1 - 1f, 0f, wallY, propMat);
            Block("Shelves_N", g, -9f, 8f, DrogZ1 - 2f, DrogZ1 - 1f, 0f, wallY, propMat);
            Block("Counter", g, 3f, 4.4f, StoopZ1 + 2f, StoopZ1 + 9f, 0f, 1.1f, propMat);
            Cylinder("OverturnedStool", g, new Vector3(1.4f, 0.22f, StoopZ1 + 5f), 0.45f, 0.44f, propMat);
            Block("EmptiedCabinet", g, -9f, -8.2f, DrogZ1 - 5f, DrogZ1 - 2f, 0.8f, 3.4f, propMat);
            Block("HangingHerbs", g, -6f, 1f, StoopZ1 + 3f, StoopZ1 + 7f, 5.2f, wallY, propMat);
        }

        /// <summary>
        /// Empty transforms at the three clue positions Part 3.5 fixes: the
        /// droggery's emptied cabinet, the alley dogleg, the plaza fountain.
        /// Viki's task 24 hangs ScentMarkers on these, so marker placement does
        /// not need a second opinion about where the fiction puts each clue.
        /// </summary>
        private static void BuildScentAnchors()
        {
            var g = Group("Scent Anchors (for task 24)");

            Anchor("ScentAnchor_Droggery_Cabinet", g, new Vector3(-7f, 1.2f, DrogZ1 - 3f));
            Anchor("ScentAnchor_Alley_Dogleg", g, new Vector3(-23f, 1.2f, 74.5f));
            Anchor("ScentAnchor_Plaza_Fountain", g, new Vector3(0f, 1.2f, 43f));
        }

        // ---------------------------------------------------------------
        // Scene housekeeping
        // ---------------------------------------------------------------

        /// <summary>
        /// The Stage 1 test room is disabled rather than deleted -- it is the
        /// proof the movement slice works, and nothing is gained by losing it.
        /// </summary>
        private static void RetireStage1Room()
        {
            string[] stage1 =
            {
                "Ground", "Wall_N", "Wall_S", "Wall_E", "Wall_W",
                "Cube", "Cube (1)", "Cube (2)", "Cube (3)"
            };

            // Root objects only. The district builds its own "Ground", and a
            // scene-wide Find would disable that instead, dropping the player
            // through the world.
            int disabled = 0;
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (System.Array.IndexOf(stage1, go.name) < 0) continue;
                if (!go.activeSelf) continue;

                Undo.RecordObject(go, "Disable Stage 1 room");
                go.SetActive(false);
                disabled++;
            }

            if (disabled > 0)
            {
                Debug.Log($"[FLAIR] Disabled {disabled} Stage 1 room objects. " +
                          "They are still in the scene if you want them back.");
            }
        }

        private static void MovePlayerToOfficeDoor()
        {
            var player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogWarning("[FLAIR] No 'Player' in the scene -- move it to " +
                                 "(0, 1.1, 2) by hand to start at the office door.");
                return;
            }

            Undo.RecordObject(player.transform, "Move player to office door");
            player.transform.SetPositionAndRotation(
                new Vector3(0f, 1.1f, 2f), Quaternion.identity);   // facing north
        }

        /// <summary>
        /// The Stage 2 slice (marker -> smell -> vision -> clue) has to keep
        /// running, so the existing marker moves to the fountain instead of
        /// being left floating in the disabled room.
        /// </summary>
        private static void KeepExistingMarkerReachable()
        {
            var marker = GameObject.Find("ScentMarker_01");
            if (marker == null) return;

            Undo.RecordObject(marker.transform, "Move scent marker to the plaza");
            marker.transform.position = new Vector3(0f, 1.2f, 43f);
            Debug.Log("[FLAIR] Moved ScentMarker_01 to the plaza fountain so the " +
                      "Stage 2 slice still runs. Task 24 replaces this properly.");
        }

        // ---------------------------------------------------------------
        // Primitives
        // ---------------------------------------------------------------

        private static Transform Group(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            return go.transform;
        }

        /// <summary>
        /// A box given by its bounds rather than centre and size -- the layout
        /// above is written as min/max edges, and converting by hand is how
        /// greyboxes end up with one-metre gaps in the walls.
        /// </summary>
        private static GameObject Block(string name, Transform parent,
                                        float x0, float x1, float z0, float z1,
                                        float y0, float y1, Material mat = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3((x0 + x1) * 0.5f, (y0 + y1) * 0.5f, (z0 + z1) * 0.5f);
            go.transform.localScale = new Vector3(x1 - x0, y1 - y0, z1 - z0);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat != null ? mat : massMat;
            return go;
        }

        private static void Cylinder(string name, Transform parent, Vector3 centre,
                                     float radius, float height, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = centre;
            // Unity's cylinder is 2 units tall with a 0.5 radius.
            go.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        /// <summary>
        /// Scattered debris, seeded off the pile's name so re-running produces
        /// the identical layout. That matters more than it looks: the scene is
        /// a 17,000-line text file that does not merge, so rubble that
        /// reshuffled on every build would fill every diff with noise.
        /// </summary>
        private static void Rubble(string name, Transform parent, Vector3 centre,
                                   int count, float spread, float maxSize = 1.6f)
        {
            var rng = new System.Random(StableHash(name));
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);

            for (int i = 0; i < count; i++)
            {
                float s = maxSize * (0.4f + (float)rng.NextDouble() * 0.6f);
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"{name}_{i}";
                go.transform.SetParent(g.transform, false);
                go.transform.localPosition = centre + new Vector3(
                    ((float)rng.NextDouble() - 0.5f) * spread,
                    s * 0.4f,
                    ((float)rng.NextDouble() - 0.5f) * spread);
                go.transform.localRotation = Quaternion.Euler(
                    (float)rng.NextDouble() * 40f,
                    (float)rng.NextDouble() * 360f,
                    (float)rng.NextDouble() * 40f);
                go.transform.localScale = new Vector3(s, s * 0.6f, s);
                go.GetComponent<MeshRenderer>().sharedMaterial = propMat;
            }
        }

        /// <summary>
        /// FNV-1a. string.GetHashCode is not guaranteed stable across runs --
        /// on some .NET runtimes it is randomised per process -- which would
        /// mean the same rubble pile landing somewhere new every session.
        /// </summary>
        private static int StableHash(string s)
        {
            unchecked
            {
                int hash = (int)2166136261;
                foreach (char c in s)
                {
                    hash = (hash ^ c) * 16777619;
                }
                return hash & 0x7fffffff;
            }
        }

        private static void Anchor(string name, Transform parent, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
        }

        // ---------------------------------------------------------------
        // Materials
        // ---------------------------------------------------------------

        /// <summary>
        /// Three flat greys, created once as assets. Primitives built in script
        /// get the built-in default material, which renders magenta under URP,
        /// so every renderer is assigned explicitly.
        ///
        /// Separating ground / mass / prop is not decoration: reading form in a
        /// greybox depends on value separation, and these are the values task
        /// 20's noir grade gets tuned against before any texture exists.
        /// </summary>
        private static void LoadMaterials()
        {
            groundMat = GetOrCreateMaterial("Greybox_Ground", new Color(0.16f, 0.16f, 0.17f));
            massMat = GetOrCreateMaterial("Greybox_Mass", new Color(0.34f, 0.34f, 0.35f));
            propMat = GetOrCreateMaterial("Greybox_Prop", new Color(0.55f, 0.55f, 0.56f));
        }

        private static Material GetOrCreateMaterial(string name, Color colour)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            var pipeline = GraphicsSettings.currentRenderPipeline;
            var shader = pipeline != null ? pipeline.defaultShader : Shader.Find("Standard");

            // URP Lit uses _BaseColor; the built-in fallback uses _Color.
            var mat = new Material(shader) { name = name };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", colour);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", colour);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0f);

            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static bool RemoveExisting()
        {
            var existing = GameObject.Find(RootName);
            if (existing == null) return false;

            Undo.DestroyObjectImmediate(existing);
            return true;
        }
    }
}
