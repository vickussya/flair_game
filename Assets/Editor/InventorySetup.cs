using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Flair.EditorTools
{
    /// <summary>
    /// One-click setup for the scent inventory: builds the overlay under the
    /// Canvas, adds InventoryHud and ScentInventory, wires them to the existing
    /// ClueLog and VisionDirector, and fills the case list from Assets/Clues.
    ///
    /// Same contract as SliceSetup -- safe to run more than once, finds what is
    /// already there instead of duplicating it, and never overwrites a reference
    /// somebody has set by hand.
    /// </summary>
    public static class InventorySetup
    {
        private const string CluesFolder = "Assets/Clues";
        private const string PanelName = "InventoryPanel";
        private const string HintName = "InventoryHint";
        private const string CountdownName = "EndCountdown";
        private const string GlowName = "ScentGlow";

        [MenuItem("FLAIR/Slice/Set Up Scent Inventory")]
        public static void SetUp()
        {
            GameObject canvasGo = GameObject.Find("Canvas");
            GameObject systems = GameObject.Find("GameSystems");
            GameObject player = GameObject.Find("Player");

            if (canvasGo == null || systems == null || player == null)
            {
                Debug.LogError("[InventorySetup] Needs Canvas, GameSystems and Player in the scene.");
                return;
            }

            ClueLog log = systems.GetComponent<ClueLog>();
            VisionDirector director = systems.GetComponent<VisionDirector>();
            VisionHud visionHud = systems.GetComponent<VisionHud>();

            if (log == null)
            {
                Debug.LogError("[InventorySetup] No ClueLog on GameSystems. Run the slice setup first.");
                return;
            }

            Text hint = BuildHint(canvasGo);
            BuildCountdown(canvasGo, visionHud);

            BuildPanel(canvasGo, visionHud, out GameObject panel,
                       out RectTransform rowParent, out Text header);

            InventoryHud hud = systems.GetComponent<InventoryHud>();
            if (hud == null)
            {
                hud = Undo.AddComponent<InventoryHud>(systems);
                Debug.Log("[InventorySetup] Added InventoryHud to GameSystems");
            }

            SerializedObject hudSo = new SerializedObject(hud);
            hudSo.FindProperty("panel").objectReferenceValue = panel;
            hudSo.FindProperty("rowParent").objectReferenceValue = rowParent;
            hudSo.FindProperty("headerLabel").objectReferenceValue = header;
            hudSo.FindProperty("hintLabel").objectReferenceValue = hint;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            ScentInventory inventory = player.GetComponent<ScentInventory>();
            if (inventory == null)
            {
                inventory = Undo.AddComponent<ScentInventory>(player);
                Debug.Log("[InventorySetup] Added ScentInventory to Player");
            }

            SerializedObject invSo = new SerializedObject(inventory);
            invSo.FindProperty("clueLog").objectReferenceValue = log;
            invSo.FindProperty("hud").objectReferenceValue = hud;
            invSo.FindProperty("visionDirector").objectReferenceValue = director;
            FillScents(invSo.FindProperty("allScents"));
            Retune(invSo.FindProperty("hintDelay"), 3f, 5f, "Hint delay");
            invSo.ApplyModifiedPropertiesWithoutUndo();

            // The interactor suppresses its prompt while the overlay is up.
            SmellInteractor interactor = player.GetComponent<SmellInteractor>();
            if (interactor != null)
            {
                SerializedObject smellSo = new SerializedObject(interactor);
                smellSo.FindProperty("inventory").objectReferenceValue = inventory;
                RetireHoldE(smellSo.FindProperty("promptFormat"));
                smellSo.ApplyModifiedPropertiesWithoutUndo();
            }

            TintMarkers();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            Debug.Log("[InventorySetup] Done. E opens the scent inventory, F smells. " +
                      "Now save the scene with Ctrl+S.");
        }

        /// <summary>
        /// The prompt was authored in the scene back when smelling was on E, and a
        /// serialized string beats the field default in the script -- so changing
        /// SmellInteractor.cs alone would still have drawn "Hold E" in play mode.
        ///
        /// Only the old wording is replaced. A prompt somebody has since reworded
        /// is left alone, same as every other reference this tool touches.
        /// </summary>
        private static void RetireHoldE(SerializedProperty promptFormat)
        {
            if (promptFormat == null || promptFormat.stringValue == null)
            {
                return;
            }

            if (!promptFormat.stringValue.Contains("Hold E"))
            {
                return;
            }

            promptFormat.stringValue = promptFormat.stringValue.Replace("Hold E", "Hold F");
            Debug.Log("[InventorySetup] Smell prompt updated to " + promptFormat.stringValue);
        }

        /// <summary>
        /// Puts the faint red scent material on every marker in the scene.
        ///
        /// One material for all of them, deliberately. 6.3 is explicit that a true
        /// scent and a red herring look identical -- telling them apart is the
        /// player's job, and a marker that betrayed its own answer would take the
        /// game away.
        /// </summary>
        private static void TintMarkers()
        {
            Material scent = ScentMaterial();

            ScentMarker[] markers = Object.FindObjectsByType<ScentMarker>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            int tinted = 0;

            foreach (ScentMarker marker in markers)
            {
                RemoveGlow(marker);

                ScentDrift drift = marker.GetComponent<ScentDrift>();

                if (drift == null)
                {
                    drift = Undo.AddComponent<ScentDrift>(marker.gameObject);
                    Debug.Log($"[InventorySetup] Added ScentDrift to {marker.name}", marker);
                }

                SerializedObject driftSo = new SerializedObject(drift);
                // Every speed this tool has shipped, so a marker set up at any
                // point along the way still lands on the current one.
                Retune(driftSo.FindProperty("speed"),
                       new[] { 0.06f, 0.12f, 0.16f }, 0.3f, "Drift speed");
                Retune(driftSo.FindProperty("halfExtents"),
                       new Vector3(0.35f, 0.2f, 0.35f),
                       new Vector3(0.7f, 0.45f, 0.7f), "Drift box");
                driftSo.ApplyModifiedPropertiesWithoutUndo();

                foreach (Renderer r in marker.GetComponentsInChildren<Renderer>(true))
                {
                    if (r.sharedMaterial == scent)
                    {
                        continue;
                    }

                    Undo.RecordObject(r, "Tint scent marker");
                    r.sharedMaterial = scent;
                    tinted++;
                }
            }

            if (tinted > 0)
            {
                Debug.Log($"[InventorySetup] Tinted {tinted} scent marker renderer(s)");
            }
        }

        /// <summary>
        /// Red, per the black/white/red palette in Part 1 where red is crime and
        /// scent. Emission sits deliberately above the grade's bloom threshold: the
        /// noir grade drains most colour, so the sphere has to carry its own light,
        /// and letting bloom draw the halo is what makes every marker look the same
        /// whether it stands against a wall or out in the open.
        /// </summary>
        private static Material ScentMaterial()
        {
            const string path = "Assets/Materials/Greybox_Scent.mat";

            // Values this tool has shipped before: too dark to find in an unlit
            // corner, then too hot once the glow light was added underneath it.
            // Anything still sitting on one of these is ours to move.
            Color[] superseded =
            {
                new Color(0.65f, 0.05f, 0.08f),
                new Color(2f, 0.16f, 0.22f),
                new Color(0.9f, 0.07f, 0.10f),
                new Color(3f, 0.22f, 0.30f),
                new Color(1.8f, 0.14f, 0.19f),
            };

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool isNew = mat == null;

            if (isNew)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                {
                    AssetDatabase.CreateFolder("Assets", "Materials");
                }

                RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
                Shader shader = pipeline != null ? pipeline.defaultShader : Shader.Find("Standard");
                mat = new Material(shader) { name = "Greybox_Scent" };
            }
            else if (!IsSuperseded(mat.GetColor("_EmissionColor"), superseded))
            {
                // Somebody has tuned this since. Their numbers win -- but the
                // keyword still has to be on, or their emission does nothing.
                EnableEmission(mat);
                EditorUtility.SetDirty(mat);
                return mat;
            }

            // Pushed above the grade's bloom threshold (0.85 in
            // SampleSceneProfile) so the halo comes from bloom rather than from a
            // light hitting whatever happens to be nearby. Bloom is screen-space,
            // so a marker against a wall and a marker in open air glow the same --
            // which a point light can never do.
            // Red pushed up and green/blue pulled down: saturation, not just
            // brightness. The grade drains most colour on its way to black and
            // white, so a red that is merely bright comes out pink-grey -- it has
            // to go in far more saturated than it needs to come out.
            var albedo = new Color(0.85f, 0.03f, 0.05f);

            // Well over the 0.85 bloom threshold so the halo stays, and nearly pure
            // red so what blooms is red rather than a hot white core.
            var emission = new Color(2.6f, 0.06f, 0.10f);

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", albedo);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", albedo);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);

            mat.SetColor("_EmissionColor", emission);
            EnableEmission(mat);

            if (isNew)
            {
                AssetDatabase.CreateAsset(mat, path);
                Debug.Log("[InventorySetup] Created " + path);
            }
            else
            {
                EditorUtility.SetDirty(mat);
                Debug.Log("[InventorySetup] Brightened " + path);
            }

            return mat;
        }

        /// <summary>
        /// Turns emission on so that it actually renders.
        ///
        /// EnableKeyword alone did not survive being written to the asset -- the
        /// material saved with _EmissionColor set but m_ValidKeywords empty, which
        /// in URP means emission is simply off. Assigning shaderKeywords directly
        /// is what serialises, so the value in the Inspector and the thing on
        /// screen finally agree.
        /// </summary>
        private static void EnableEmission(Material mat)
        {
            mat.EnableKeyword("_EMISSION");

            var keywords = new List<string>(mat.shaderKeywords);

            if (!keywords.Contains("_EMISSION"))
            {
                keywords.Add("_EMISSION");
                mat.shaderKeywords = keywords.ToArray();
            }

            // Nothing here is lightmapped, so keep emission out of GI -- same as
            // the fixtures in NoirGrade. This also clears EmissiveIsBlack, which
            // would otherwise cull the emission before it ever drew.
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        }

        /// <summary>
        /// As above, but accepting every value this tool has previously written.
        /// A number the tool has never set is somebody's own, and stays.
        /// </summary>
        private static void Retune(SerializedProperty property, float[] from, float to,
                                   string label)
        {
            if (property == null)
            {
                return;
            }

            foreach (float old in from)
            {
                if (Mathf.Approximately(property.floatValue, old))
                {
                    Debug.Log($"[InventorySetup] {label}: {old} -> {to}");
                    property.floatValue = to;
                    return;
                }
            }
        }

        /// <summary>Vector3 form of the same "move it only off the old default" rule.</summary>
        private static void Retune(SerializedProperty property, Vector3 from, Vector3 to,
                                   string label)
        {
            if (property == null)
            {
                return;
            }

            Vector3 current = property.vector3Value;

            if (!Mathf.Approximately(current.x, from.x)
                || !Mathf.Approximately(current.y, from.y)
                || !Mathf.Approximately(current.z, from.z))
            {
                return;
            }

            property.vector3Value = to;
            Debug.Log($"[InventorySetup] {label}: {from} -> {to}");
        }

        private static bool IsSuperseded(Color current, Color[] known)
        {
            foreach (Color c in known)
            {
                if (Mathf.Approximately(current.r, c.r)
                    && Mathf.Approximately(current.g, c.g)
                    && Mathf.Approximately(current.b, c.b))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// A small red point light on each marker, so a scent in an unlit corner
        /// still announces itself. Deliberately weak and short-ranged: it should
        /// tell you something is there, not light the room for you.
        ///
        /// ScentMarker switches it off with the renderers, so a collected scent
        /// does not leave its glow hanging in the air.
        /// </summary>
        /// <summary>
        /// Removes the point light earlier versions of this tool added.
        ///
        /// A light paints its glow onto whatever happens to be standing nearby, and
        /// the three markers do not stand in comparable places: 01 sits about a
        /// metre off the droggery's north shelves, 03 is boxed into the alley
        /// dogleg, and 02 hangs in open air with only the floor beneath it. One
        /// light, three different amounts of surface to catch it, three different
        /// looks -- and no intensity fixes that, because the difference is the room
        /// rather than the setting.
        ///
        /// Emission and the grade's bloom do the whole job instead. Bloom haloes
        /// the bright pixels themselves, so a marker reads the same against a wall
        /// as it does in the middle of the street. The cost is that nothing spills
        /// onto the ground any more; that is the trade, and uniformity wins because
        /// 6.3 needs every scent to look alike.
        /// </summary>
        private static void RemoveGlow(ScentMarker marker)
        {
            Transform existing = marker.transform.Find(GlowName);

            if (existing == null)
            {
                return;
            }

            Undo.DestroyObjectImmediate(existing.gameObject);
            Debug.Log($"[InventorySetup] Removed {GlowName} from {marker.name}", marker);
        }

        /// <summary>
        /// Moves a serialized tuning value off an old default and onto a new one.
        ///
        /// A value already written into the scene beats the field default in the
        /// script, so changing the script alone never reaches a component that is
        /// already in the scene. Only the exact old default is moved -- anything
        /// else is a number somebody chose, and it stays.
        /// </summary>
        private static void Retune(SerializedProperty property, float from, float to,
                                   string label)
        {
            if (property == null || !Mathf.Approximately(property.floatValue, from))
            {
                return;
            }

            property.floatValue = to;
            Debug.Log($"[InventorySetup] {label}: {from} -> {to}");
        }

        /// <summary>
        /// Every clue asset in the case, in filename order so Clue_01 comes first
        /// and the empty slots sit where the player will eventually fill them.
        /// </summary>
        private static void FillScents(SerializedProperty list)
        {
            string[] guids = AssetDatabase.FindAssets("t:ClueData", new[] { CluesFolder });
            List<string> paths = new List<string>();

            foreach (string guid in guids)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            paths.Sort(string.CompareOrdinal);

            // Appended, never reordered. Whatever order is already in the list was
            // curated by somebody; a new clue asset just joins the end of it.
            int added = 0;

            foreach (string path in paths)
            {
                var clue = AssetDatabase.LoadAssetAtPath<ClueData>(path);

                if (clue == null || Contains(list, clue))
                {
                    continue;
                }

                int index = list.arraySize;
                list.InsertArrayElementAtIndex(index);
                list.GetArrayElementAtIndex(index).objectReferenceValue = clue;
                added++;
            }

            if (added > 0)
            {
                Debug.Log($"[InventorySetup] Added {added} scent(s) to the inventory list");
            }
        }

        private static bool Contains(SerializedProperty list, Object value)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == value)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The first-time nudge, top-left. A sibling of the panel rather than a
        /// child of it: the panel is inactive while the inventory is closed, which
        /// is precisely when the hint needs to be on screen.
        ///
        /// world.md puts the case-file tab in this corner eventually. Nothing is
        /// there yet, and by the time it is the player will have learned E.
        /// </summary>
        private static Text BuildHint(GameObject canvasGo)
        {
            Transform existing = canvasGo.transform.Find(HintName);

            if (existing != null)
            {
                return existing.GetComponent<Text>();
            }

            var go = new GameObject(HintName, typeof(RectTransform),
                                    typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(canvasGo.transform, false);

            RectTransform rt = go.transform as RectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(40f, -34f);
            rt.sizeDelta = new Vector2(460f, 40f);

            Text text = go.GetComponent<Text>();
            text.font = BuiltinFont();
            text.fontSize = 20;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.text = "Press E to open Scent Inventory";
            text.color = new Color(1f, 1f, 1f, 0.35f);

            Undo.RegisterCreatedObjectUndo(go, "Create inventory hint");
            Debug.Log("[InventorySetup] Created " + HintName);

            return text;
        }

        /// <summary>
        /// The think-time timer, top-right. Belongs to VisionHud rather than the
        /// inventory, because LevelEndScreen is what counts it down -- this tool
        /// only builds it so the whole HUD comes from one menu item.
        ///
        /// world.md eventually wants the units counter and the minimap in this
        /// corner. Neither exists yet.
        /// </summary>
        private static void BuildCountdown(GameObject canvasGo, VisionHud visionHud)
        {
            if (visionHud == null)
            {
                return;
            }

            Transform existing = canvasGo.transform.Find(CountdownName);
            Text text;

            if (existing != null)
            {
                text = existing.GetComponent<Text>();
            }
            else
            {
                var go = new GameObject(CountdownName, typeof(RectTransform),
                                        typeof(CanvasRenderer), typeof(Text));
                go.transform.SetParent(canvasGo.transform, false);

                RectTransform rt = go.transform as RectTransform;
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-40f, -30f);
                rt.sizeDelta = new Vector2(140f, 44f);

                text = go.GetComponent<Text>();
                text.font = BuiltinFont();
                text.fontSize = 26;
                text.alignment = TextAnchor.UpperRight;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.text = "0:20";
                text.color = Color.white;

                go.SetActive(false);

                Undo.RegisterCreatedObjectUndo(go, "Create end countdown");
                Debug.Log("[InventorySetup] Created " + CountdownName);
            }

            SerializedObject so = new SerializedObject(visionHud);
            SerializedProperty prop = so.FindProperty("countdownLabel");

            if (prop != null && prop.objectReferenceValue == null)
            {
                prop.objectReferenceValue = text;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void BuildPanel(GameObject canvasGo, VisionHud visionHud,
                                       out GameObject panel, out RectTransform rowParent,
                                       out Text header)
        {
            Transform existing = canvasGo.transform.Find(PanelName);

            if (existing != null)
            {
                panel = existing.gameObject;
                rowParent = panel.transform.Find("Rows") as RectTransform;

                Transform headerTransform = panel.transform.Find("Header");
                header = headerTransform != null ? headerTransform.GetComponent<Text>() : null;

                OrderOverlays(panel, visionHud);
                panel.SetActive(false);
                return;
            }

            panel = new GameObject(PanelName, typeof(RectTransform),
                                   typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(canvasGo.transform, false);
            Stretch(panel.transform as RectTransform);

            // world.md asks for black at ~85%, not solid -- the street stays
            // faintly visible behind it, so the overlay reads as Bunk thinking
            // rather than as a menu the game paused for.
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

            var headerGo = new GameObject("Header", typeof(RectTransform),
                                          typeof(CanvasRenderer), typeof(Text));
            headerGo.transform.SetParent(panel.transform, false);
            RectTransform headerRt = headerGo.transform as RectTransform;
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(1f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.offsetMin = new Vector2(60f, -110f);
            headerRt.offsetMax = new Vector2(-60f, -50f);

            header = headerGo.GetComponent<Text>();
            header.font = BuiltinFont();
            header.fontSize = 30;
            header.alignment = TextAnchor.UpperLeft;
            header.color = Color.white;
            header.text = "SCENTS   0/0";

            var rowsGo = new GameObject("Rows", typeof(RectTransform));
            rowsGo.transform.SetParent(panel.transform, false);
            rowParent = rowsGo.transform as RectTransform;
            rowParent.anchorMin = Vector2.zero;
            rowParent.anchorMax = Vector2.one;
            rowParent.offsetMin = new Vector2(60f, 60f);
            rowParent.offsetMax = new Vector2(-60f, -130f);

            var layout = rowsGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            Undo.RegisterCreatedObjectUndo(panel, "Create scent inventory");
            Debug.Log("[InventorySetup] Created " + PanelName);

            OrderOverlays(panel, visionHud);
            panel.SetActive(false);
        }

        /// <summary>
        /// Sibling order is draw order. The inventory must sit under the vision
        /// fade, or a vision starting on the same frame would cross into black
        /// with the overlay still painted on top of it.
        /// </summary>
        private static void OrderOverlays(GameObject panel, VisionHud visionHud)
        {
            panel.transform.SetAsLastSibling();

            if (visionHud == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(visionHud);
            PushToTop(so.FindProperty("fadeOverlay").objectReferenceValue);
            PushToTop(so.FindProperty("endCardPanel").objectReferenceValue);
        }

        private static void PushToTop(Object reference)
        {
            if (reference is Component component)
            {
                component.transform.SetAsLastSibling();
            }
            else if (reference is GameObject go)
            {
                go.transform.SetAsLastSibling();
            }
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
