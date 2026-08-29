using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Flair.EditorTools
{
    /// <summary>
    /// One-click setup for the Phase 1 slice: the remaining clue assets, three
    /// scent markers placed in the greybox, and the end-card UI wired up to
    /// LevelEndScreen. This is what you would otherwise click through in the
    /// Inspector -- written down so it is repeatable and reviewable.
    ///
    /// Safe to run more than once. It finds what already exists and leaves it
    /// alone rather than making duplicates, and it never overwrites a clue that
    /// somebody has already authored by hand.
    /// </summary>
    public static class SliceSetup
    {
        private const string CluesFolder = "Assets/Clues";

        [MenuItem("FLAIR/Slice/Set Up Clue Gate + End Card")]
        public static void SetUp()
        {
            ClueData c1 = EnsureClue("Clue_01", "clue_01_hollow_vial", "The hollow vial",
                "The marked vial, present and empty, back among the shelves. The medicine left " +
                "the room without the glass, which is why the seal never fired.");

            ClueData c2 = EnsureClue("Clue_02", "clue_02_four_scents", "The four lingering scents",
                "What the four customers left in the room, matched against the damaged hologram. " +
                "Three read as ordinary people. The fourth is logged now and not understood yet.");

            ClueData c3 = EnsureClue("Clue_03", "clue_03_tobacco", "Death and expensive tobacco",
                "The service alley, where Borlow's trail simply stops. Only one kind of man in " +
                "the Privy smokes that leaf.");

            Chain(c1, c2);
            Chain(c2, c3);

            // Locations are concept.md 5.3, coordinates are DistrictGreybox's:
            // the droggery is z 94..106, the alley dogleg is x -28..-23 / z 72..77.
            EnsureMarker("ScentMarker_01", new Vector3(-3.5f, 1f, 99f), c1);
            EnsureMarker("ScentMarker_02", new Vector3(3.5f, 1f, 101.5f), c2);
            EnsureMarker("ScentMarker_03", new Vector3(-25.5f, 1f, 74.5f), c3);

            BuildEndCard();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            Debug.Log("[SliceSetup] Done. Three clues authored and placed, end card wired. " +
                      "Now save the scene with Ctrl+S.");
        }

        private static ClueData EnsureClue(string assetName, string id, string display,
                                           string description)
        {
            if (!AssetDatabase.IsValidFolder(CluesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Clues");
            }

            string path = CluesFolder + "/" + assetName + ".asset";
            ClueData clue = AssetDatabase.LoadAssetAtPath<ClueData>(path);

            if (clue != null)
            {
                // Already authored by hand. Leave the writing alone.
                return clue;
            }

            clue = ScriptableObject.CreateInstance<ClueData>();
            AssetDatabase.CreateAsset(clue, path);

            SerializedObject so = new SerializedObject(clue);
            so.FindProperty("clueId").stringValue = id;
            so.FindProperty("displayName").stringValue = display;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("isTrueScent").boolValue = true;
            so.FindProperty("visionDuration").floatValue = 3f;
            so.FindProperty("visionId").stringValue = id.Replace("clue_", "vision_");
            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[SliceSetup] Created " + path);
            return clue;
        }

        /// <summary>Points one clue at the next, without clobbering an existing link.</summary>
        private static void Chain(ClueData from, ClueData to)
        {
            if (from == null || to == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(from);
            SerializedProperty leads = so.FindProperty("leadsTo");

            if (leads.objectReferenceValue == null)
            {
                leads.objectReferenceValue = to;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void EnsureMarker(string name, Vector3 position, ClueData clue)
        {
            GameObject go = GameObject.Find(name);

            if (go == null)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = name;
                go.transform.localScale = Vector3.one * 0.4f;

                // The smell radius does the detecting, so a collider here would
                // only be something to bump into.
                Collider col = go.GetComponent<Collider>();
                if (col != null)
                {
                    Object.DestroyImmediate(col);
                }

                Undo.RegisterCreatedObjectUndo(go, "Create scent marker");
                Debug.Log("[SliceSetup] Created " + name);
            }

            go.transform.position = position;

            ScentMarker marker = go.GetComponent<ScentMarker>();
            if (marker == null)
            {
                marker = go.AddComponent<ScentMarker>();
            }

            SerializedObject so = new SerializedObject(marker);
            so.FindProperty("clue").objectReferenceValue = clue;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildEndCard()
        {
            GameObject canvasGo = GameObject.Find("Canvas");
            GameObject systems = GameObject.Find("GameSystems");
            GameObject player = GameObject.Find("Player");

            if (canvasGo == null || systems == null || player == null)
            {
                Debug.LogError("[SliceSetup] Needs Canvas, GameSystems and Player in the scene.");
                return;
            }

            Transform existing = canvasGo.transform.Find("EndCardPanel");
            GameObject panel = existing != null ? existing.gameObject : null;
            Text label;

            if (panel == null)
            {
                panel = new GameObject("EndCardPanel", typeof(RectTransform),
                                       typeof(CanvasRenderer), typeof(Image));
                panel.transform.SetParent(canvasGo.transform, false);
                Stretch(panel.transform as RectTransform);
                panel.GetComponent<Image>().color = Color.black;

                GameObject labelGo = new GameObject("EndCardLabel", typeof(RectTransform),
                                                    typeof(CanvasRenderer), typeof(Text));
                labelGo.transform.SetParent(panel.transform, false);
                Stretch(labelGo.transform as RectTransform);

                label = labelGo.GetComponent<Text>();
                label.font = BuiltinFont();
                label.fontSize = 40;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
                label.text = "END CARD";

                Undo.RegisterCreatedObjectUndo(panel, "Create end card");
                Debug.Log("[SliceSetup] Created EndCardPanel");
            }
            else
            {
                label = panel.GetComponentInChildren<Text>(true);
            }

            // A later sibling draws on top, so the card survives the black fade.
            panel.transform.SetAsLastSibling();
            panel.SetActive(false);

            VisionHud hud = systems.GetComponent<VisionHud>();
            if (hud != null)
            {
                SerializedObject so = new SerializedObject(hud);
                so.FindProperty("endCardPanel").objectReferenceValue = panel;
                so.FindProperty("endCardLabel").objectReferenceValue = label;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            LevelEndScreen end = systems.GetComponent<LevelEndScreen>();
            if (end == null)
            {
                end = Undo.AddComponent<LevelEndScreen>(systems);
                Debug.Log("[SliceSetup] Added LevelEndScreen to GameSystems");
            }

            SerializedObject endSo = new SerializedObject(end);
            endSo.FindProperty("player").objectReferenceValue = player.GetComponent<PlayerController>();
            endSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Font BuiltinFont()
        {
            // Arial.ttf was dropped from the builtin resources in 2022+.
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return f != null ? f : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
