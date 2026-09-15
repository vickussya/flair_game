using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Video;

namespace Flair.EditorTools
{
    /// <summary>
    /// Swaps the placeholder vision player for the real video one (task 19), and
    /// points VisionDirector at it.
    ///
    /// Safe to run before any video exists: VideoVisionPlayer falls back to the
    /// placeholder panel for any clue whose visionId is not mapped to a clip, so
    /// the game plays exactly as it does now until the first vision is exported.
    /// </summary>
    public static class VisionPlayerSetup
    {
        private const string VisionsFolder = "Assets/Visions";

        /// <summary>
        /// Maps every video in Assets/Visions to the clue with the same name: a file
        /// called vision_01_hollow_vial.mp4 plays for the clue whose visionId is
        /// vision_01_hollow_vial.
        ///
        /// The name is the contract. That is what lets a finished vision replace a
        /// placeholder by overwriting the file in place -- same name, same GUID,
        /// nothing to rewire.
        /// </summary>
        [MenuItem("FLAIR/Vision/Map Videos From Assets-Visions")]
        public static void MapVideos()
        {
            GameObject systems = GameObject.Find("GameSystems");
            VideoVisionPlayer player = systems != null ? systems.GetComponent<VideoVisionPlayer>() : null;

            if (player == null)
            {
                Debug.LogError("[VisionPlayerSetup] No VideoVisionPlayer on GameSystems. " +
                               "Run FLAIR > Vision > Install Video Vision Player first.");
                return;
            }

            SerializedObject so = new SerializedObject(player);
            SerializedProperty list = so.FindProperty("visions");

            int mapped = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:VideoClip", new[] { VisionsFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>(path);
                string visionId = System.IO.Path.GetFileNameWithoutExtension(path);

                SerializedProperty entry = FindOrAddEntry(list, visionId);
                entry.FindPropertyRelative("visionId").stringValue = visionId;
                entry.FindPropertyRelative("clip").objectReferenceValue = clip;

                Debug.Log($"[VisionPlayerSetup] {visionId} -> {path} ({clip.length:0.0}s)");
                mapped++;
            }

            so.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log(mapped == 0
                ? "[VisionPlayerSetup] No videos found in " + VisionsFolder + "."
                : $"[VisionPlayerSetup] Mapped {mapped} vision(s). Save with Ctrl+S.");
        }

        private static SerializedProperty FindOrAddEntry(SerializedProperty list, string visionId)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty existing = list.GetArrayElementAtIndex(i);
                if (existing.FindPropertyRelative("visionId").stringValue == visionId)
                {
                    return existing;
                }
            }

            list.arraySize++;
            return list.GetArrayElementAtIndex(list.arraySize - 1);
        }

        [MenuItem("FLAIR/Vision/Install Video Vision Player")]
        public static void Install()
        {
            GameObject systems = GameObject.Find("GameSystems");

            if (systems == null)
            {
                Debug.LogError("[VisionPlayerSetup] No GameSystems object in the scene.");
                return;
            }

            VideoPlayer video = systems.GetComponent<VideoPlayer>();
            if (video == null)
            {
                video = Undo.AddComponent<VideoPlayer>(systems);
                Debug.Log("[VisionPlayerSetup] Added VideoPlayer");
            }

            VideoVisionPlayer player = systems.GetComponent<VideoVisionPlayer>();
            if (player == null)
            {
                player = Undo.AddComponent<VideoVisionPlayer>(systems);
                Debug.Log("[VisionPlayerSetup] Added VideoVisionPlayer");
            }

            // Remove the placeholder, otherwise VisionDirector's GetComponent
            // fallback could pick up whichever it finds first.
            PlaceholderVisionPlayer placeholder = systems.GetComponent<PlaceholderVisionPlayer>();
            if (placeholder != null)
            {
                Undo.DestroyObjectImmediate(placeholder);
                Debug.Log("[VisionPlayerSetup] Removed PlaceholderVisionPlayer");
            }

            VisionDirector director = systems.GetComponent<VisionDirector>();
            if (director != null)
            {
                SerializedObject so = new SerializedObject(director);
                so.FindProperty("visionPlayer").objectReferenceValue = player;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[VisionPlayerSetup] Done. Add each vision to the Visions list on " +
                      "VideoVisionPlayer: visionId must match ClueData.visionId " +
                      "(clue_01 -> vision_01_hollow_vial). Save with Ctrl+S.");
        }

        [MenuItem("FLAIR/Vision/Restore Placeholder Vision Player")]
        public static void Restore()
        {
            GameObject systems = GameObject.Find("GameSystems");
            if (systems == null)
            {
                return;
            }

            VideoVisionPlayer player = systems.GetComponent<VideoVisionPlayer>();
            if (player != null)
            {
                Undo.DestroyObjectImmediate(player);
            }

            PlaceholderVisionPlayer placeholder = systems.GetComponent<PlaceholderVisionPlayer>();
            if (placeholder == null)
            {
                placeholder = Undo.AddComponent<PlaceholderVisionPlayer>(systems);
            }

            VisionDirector director = systems.GetComponent<VisionDirector>();
            if (director != null)
            {
                SerializedObject so = new SerializedObject(director);
                so.FindProperty("visionPlayer").objectReferenceValue = placeholder;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[VisionPlayerSetup] Placeholder restored.");
        }
    }
}