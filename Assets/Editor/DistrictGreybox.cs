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
    /// DEVIATES FROM Part 3.5 in two ways, both from walking the first pass:
    /// the street, alley and plaza are all wider than "claustrophobic" wanted,
    /// and there is now a four-way crossroads in the lower street feeding a
    /// second north-south street. Part 3.5 still says "a loop with one spur"
    /// and needs updating on dev to match.
    /// </summary>
    public static class DistrictGreybox
    {
        private const string RootName = "District";
        private const string MaterialFolder = "Assets/Materials";

        // ---------------------------------------------------------------
        // The layout, in metres.
        //
        //                        droggery          z 90..102
        //                        stoop             z 86..90
        //                        upper street      z 62..86
        //   west street  <->     PLAZA             z 38..62   <-> blocked arch
        //        |               lower street      z 32..38
        //        +-------------  CROSSROADS  ------------+    z 20..32
        //                        lower street      z -1..20
        //                        office            z -8..-1
        // ---------------------------------------------------------------

        private const float StreetW = 6f;      // main street runs x -6..6
        private const float PlazaW = 15f;      // plaza widens to x -15..15
        private const float MassH = 18f;       // buildings: 5-6 storeys at ~3m
        private const float DomeY = 40f;       // dome ceiling ~40m up

        // South-to-north landmarks, as z coordinates.
        private const float OfficeZ0 = -8f;    // back of the office recess
        private const float OfficeZ1 = -1f;    // office doorway meets the street
        private const float CrossZ0 = 20f;     // crossroads, south edge
        private const float CrossZ1 = 32f;     // crossroads, north edge
        private const float StreetZ1 = 38f;    // lower street ends at the plaza
        private const float PlazaZ1 = 62f;     // plaza ends, upper street begins
        private const float UpperZ1 = 86f;     // upper street ends at the stoop
        private const float StoopZ1 = 90f;     // stoop ends, droggery front wall
        private const float DrogZ1 = 102f;     // back of the droggery

        // Building rows sit 12m apart either side of the main street.
        private const float RowW = -18f, RowE = 18f;

        // The second, western street: x -26..-21 up to the dogleg, then it jogs
        // east and runs x -23..-18 to the droggery's back yard.
        private const float WestStX0 = -26f, WestStX1 = -21f;
        private const float DogZ0 = 70f, DogZ1 = 75f;
        private const float NorthLegX0 = -23f, NorthLegX1 = -18f;

        // Ground / dome extents, wide enough to cover both streets and the arch.
        private const float GroundX0 = -33f, GroundX1 = 29f;
        private const float GroundZ0 = -9f, GroundZ1 = 107f;

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
            BuildCrossroads();
            BuildPlaza();
            BuildBlockedArch();
            BuildUpperStreet();
            BuildWestStreet();
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

            Block("Ground", g, GroundX0, GroundX1, GroundZ0, GroundZ1, -1f, 0f, groundMat);

            // The map has a roof and you can see it (Part 3.5).
            Block("DomeCeiling", g, GroundX0, GroundX1, GroundZ0, GroundZ1, DomeY, DomeY + 2f);

            // Ribs, so the ceiling reads as structure rather than a lid.
            for (float z = GroundZ0 + 6f; z < GroundZ1; z += 12f)
            {
                Block("DomeRib", g, GroundX0, GroundX1, z, z + 1.2f, DomeY - 1.2f, DomeY);
            }
        }

        /// <summary>
        /// South end: a narrow doorway under a dead neon sign, three steps down
        /// to street level. The start point, and the map's southern pinch.
        /// </summary>
        private static void BuildOffice()
        {
            var g = Group("South - Bunks Office");

            // The recess is only 6m wide where the street is 12 -- Part 3.5
            // wants the block pinched at both ends.
            Block("OfficeBlock_W", g, RowW, -3f, OfficeZ0, OfficeZ1, 0f, MassH);
            Block("OfficeBlock_E", g, 3f, RowE, OfficeZ0, OfficeZ1, 0f, MassH);
            Block("OfficeBlock_Back", g, -3f, 3f, OfficeZ0, OfficeZ0 + 4f, 0f, MassH);
            Block("OfficeLintel", g, -3f, 3f, OfficeZ0 + 4f, OfficeZ1, 4f, MassH);

            // Three steps DOWN: the door sits below street level. Each rise is
            // 0.15, well inside the CharacterController's 0.3 step offset.
            for (int i = 0; i < 3; i++)
            {
                float y = -0.15f * (i + 1);
                float z = OfficeZ1 - (i + 1);
                Block($"OfficeStep_{i + 1}", g, -2.5f, 2.5f, z, z + 1f, y, 0f, propMat);
            }

            Block("DeadNeonSign", g, -1.8f, 1.8f, OfficeZ1 - 0.4f, OfficeZ1, 4.2f, 5.4f, propMat);
            Block("LitWindow", g, -1.2f, 1.2f, OfficeZ1 - 0.3f, OfficeZ1, 6.5f, 8.5f, propMat);
        }

        /// <summary>
        /// Lower street: cracked asphalt, tram rails ending in rubble,
        /// burnt-out chassis shoved against the kerb. Split by the crossroads.
        /// </summary>
        private static void BuildLowerStreet()
        {
            var g = Group("Lower Street");

            Block("Row_W_S", g, RowW, -StreetW, OfficeZ1, CrossZ0, 0f, MassH);
            Block("Row_E_S", g, StreetW, RowE, OfficeZ1, CrossZ0, 0f, MassH);
            Block("Row_W_N", g, RowW, -StreetW, CrossZ1, StreetZ1, 0f, MassH);
            Block("Row_E_N", g, StreetW, RowE, CrossZ1, StreetZ1, 0f, MassH);

            Block("Kerb_W", g, -StreetW, -StreetW + 0.4f, OfficeZ1, CrossZ0, 0f, 0.25f, propMat);
            Block("Kerb_E", g, StreetW - 0.4f, StreetW, OfficeZ1, CrossZ0, 0f, 0.25f, propMat);

            // Rusted tram rails, ending in rubble rather than going anywhere.
            Block("TramRail_W", g, -2.4f, -2.1f, 2f, 17f, 0f, 0.15f, propMat);
            Block("TramRail_E", g, 2.1f, 2.4f, 2f, 17f, 0f, 0.15f, propMat);
            Rubble("RailRubble", g, new Vector3(0f, 0f, 18f), 4, 2.4f);

            // Barricades against the kerbs. They break the sightline north so
            // the plaza is not visible from the office door (6.1 wants exploring).
            Block("Chassis_W", g, -5.6f, -2.8f, 8f, 12.5f, 0f, 1.5f, propMat);
            Block("Chassis_E", g, 2.8f, 5.6f, 33f, 37.5f, 0f, 1.4f, propMat);

            // Pipework vented at ankle height, cable bundles swagged overhead.
            Block("Pipes_W", g, -StreetW, -StreetW + 0.5f, 4f, 18f, 0.4f, 0.9f, propMat);
            Block("Cables_Overhead", g, -StreetW, StreetW, 14f, 14.6f, 7f, 7.4f, propMat);
        }

        /// <summary>
        /// The four-way crossroads. West arm runs into the second north-south
        /// street; east arm is a short service spur sealed at the far end.
        /// </summary>
        private static void BuildCrossroads()
        {
            var g = Group("Crossroads");

            // South and north sides of both arms.
            Block("CrossWall_SW", g, GroundX0 + 7f, RowW, GroundZ0, CrossZ0, 0f, MassH);
            Block("CrossWall_NW", g, WestStX1, RowW, CrossZ1, StreetZ1, 0f, MassH);
            Block("CrossWall_SE", g, RowE, GroundX1, GroundZ0, CrossZ0, 0f, MassH);
            Block("CrossWall_NE", g, RowE, GroundX1, CrossZ1, StreetZ1, 0f, MassH);

            // East arm dead-ends at a sealed service gate.
            Block("EastArm_End", g, 22f, GroundX1, CrossZ0, CrossZ1, 0f, MassH);
            Rubble("EastArmRubble", g, new Vector3(20.5f, 0f, 26f), 6, 5f);

            Block("StreetPlate", g, -0.4f, 0.4f, CrossZ1 - 0.5f, CrossZ1, 3.4f, 4.6f, propMat);
        }

        /// <summary>
        /// Centre: the plaza, the hub. Four routes meet -- street south,
        /// upper street north, the alley mouth west, the blocked arch east.
        /// </summary>
        private static void BuildPlaza()
        {
            var g = Group("Centre - Plaza");

            // West side, split by the mouth that links to the western street.
            Block("PlazaWall_W_S", g, WestStX1, -PlazaW, StreetZ1, 54f, 0f, MassH);
            Block("PlazaWall_W_N", g, WestStX1, -PlazaW, 59f, PlazaZ1, 0f, MassH);

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

            // The mouth: open below 7m, solid mass above.
            Block("ArchLintel", g, PlazaW, GroundX1, 46f, 54f, 7f, MassH);

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

            Block("Row_W", g, RowW, -StreetW, PlazaZ1, UpperZ1, 0f, MassH);
            Block("Row_W_Back", g, WestStX1, RowW, PlazaZ1, DogZ0, 0f, MassH);

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
        /// The western street: the second north-south route, reached from the
        /// crossroads at its south end and from the plaza through the alley
        /// mouth halfway up. One sharp dogleg makes a blind corner, then it
        /// runs on to the droggery's back yard -- so the crime scene has a
        /// front and a back way in.
        /// </summary>
        private static void BuildWestStreet()
        {
            var g = Group("West Street and Service Alley");

            // Outer wall, the whole length.
            Block("WestOuter", g, GroundX0, WestStX0, CrossZ0, 92f, 0f, MassH);

            // East wall of the southern stretch, between it and the plaza.
            Block("WestInner_S", g, WestStX1, RowW, CrossZ1, StreetZ1, 0f, MassH);

            // The blind corner: coming north you hit this and must jog east.
            Block("DoglegMass", g, WestStX0, NorthLegX0, DogZ1, 92f, 0f, MassH);

            // North of the jog the street is x -23..-18, so wall off the rest.
            Block("StoopFlank_W", g, RowW, -10f, UpperZ1, 92f, 0f, MassH);

            // Back yard walls.
            Block("YardWall_W", g, GroundX0, WestStX0, 92f, 98f, 0f, MassH);
            Block("YardWall_N", g, GroundX0, -10f, 98f, GroundZ1 - 1f, 0f, MassH);

            // Bins and crates. They hug the walls on purpose: the player capsule
            // is 1.16m across including skin width, so anything that leaves less
            // than ~1.7m stops being clutter and starts being a wall.
            Block("Bin_1", g, WestStX0, WestStX0 + 1.1f, 44f, 45.2f, 0f, 1.3f, propMat);
            Block("Crate_1", g, WestStX0, WestStX0 + 1.1f, 62f, 63.4f, 0f, 1.1f, propMat);
            Block("Crate_2", g, WestStX0 + 0.1f, WestStX0 + 1f, 62.7f, 63.9f, 1.1f, 2f, propMat);
            Block("Bin_2", g, NorthLegX0, NorthLegX0 + 1.1f, 80f, 81.2f, 0f, 1.3f, propMat);
            Block("Crate_3", g, NorthLegX0, NorthLegX0 + 1.1f, 86f, 87.4f, 0f, 1.2f, propMat);

            // Fire escapes, and the raised catwalk that casts a hard bar of
            // shadow across the street once task 20's lighting lands.
            Block("Catwalk", g, WestStX0, WestStX1, 56f, 58f, 5.5f, 5.8f, propMat);
            Block("FireEscape_A", g, WestStX1 - 0.6f, WestStX1, 46f, 50f, 3f, 3.3f, propMat);
            Block("FireEscape_B", g, WestStX1 - 0.6f, WestStX1, 50f, 54f, 6f, 6.3f, propMat);
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
            Block("Front_Pier_Mid", g, 2f, 3f, StoopZ1, StoopZ1 + 1f, 0f, wallY);
            Block("Front_Pier_E", g, 8f, 10f, StoopZ1, StoopZ1 + 1f, 0f, wallY);
            Block("Front_WindowSill", g, 3f, 8f, StoopZ1, StoopZ1 + 1f, 0f, 1f);
            Block("Front_Lintel", g, -2f, 8f, StoopZ1, StoopZ1 + 1f, 4.2f, wallY);

            // Glass on the pavement rather than inside -- it broke outward.
            Rubble("ShatteredGlass", g, new Vector3(5.5f, 0.45f, 88.5f), 6, 3.2f, 0.25f);

            // Hanging iron bracket sign with the witch's mark.
            Block("BracketArm", g, 7.8f, 9.6f, StoopZ1 - 0.2f, StoopZ1 + 0.1f, 5.2f, 5.5f, propMat);
            Block("WitchMarkSign", g, 8.6f, 9.6f, StoopZ1 - 0.3f, StoopZ1, 3.9f, 5.2f, propMat);

            // Shell. Rear door gap on the west wall at z 93..96.
            Block("Wall_W_S", g, -10f, -9f, StoopZ1 + 1f, 93f, 0f, wallY);
            Block("Wall_W_N", g, -10f, -9f, 96f, DrogZ1, 0f, wallY);
            Block("Wall_E", g, 9f, 10f, StoopZ1 + 1f, DrogZ1, 0f, wallY);
            Block("Wall_N", g, -10f, 10f, DrogZ1 - 1f, DrogZ1, 0f, wallY);
            Block("Ceiling", g, -10f, 10f, StoopZ1, DrogZ1, wallY, wallY + 0.5f);

            // The building above the shop: widest and most ornate on the block.
            Block("UpperFacade", g, -10f, 10f, StoopZ1, DrogZ1, wallY + 0.5f, MassH);

            // Interior. Floor-to-ceiling bottles, long counter, overturned
            // stool, wall cabinet standing open and emptied.
            Block("Shelves_E", g, 8f, 9f, 92f, DrogZ1 - 1f, 0f, wallY, propMat);
            Block("Shelves_N", g, -9f, 8f, DrogZ1 - 2f, DrogZ1 - 1f, 0f, wallY, propMat);
            Block("Counter", g, 3f, 4.4f, 92f, 99f, 0f, 1.1f, propMat);
            Cylinder("OverturnedStool", g, new Vector3(1.4f, 0.22f, 95f), 0.45f, 0.44f, propMat);
            Block("EmptiedCabinet", g, -9f, -8.2f, 97.5f, 100.5f, 0.8f, 3.4f, propMat);
            Block("HangingHerbs", g, -6f, 1f, 93f, 97f, 5.2f, wallY, propMat);
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

            Anchor("ScentAnchor_Droggery_Cabinet", g, new Vector3(-7f, 1.2f, 99f));
            Anchor("ScentAnchor_Alley_Dogleg", g, new Vector3(-22f, 1.2f, 72.5f));
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

        /// <summary>Scattered debris. Deterministic, so re-running does not reshuffle it.</summary>
        private static void Rubble(string name, Transform parent, Vector3 centre,
                                   int count, float spread, float maxSize = 1.6f)
        {
            var rng = new System.Random(name.GetHashCode());
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
