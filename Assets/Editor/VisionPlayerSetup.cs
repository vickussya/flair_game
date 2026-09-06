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