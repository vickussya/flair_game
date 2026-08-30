using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Flair.EditorTools
{
    /// <summary>
    /// Puts the Bunk model on the Player, in place of the greybox capsule.
    ///
    /// The game is first person, so the model is barely seen while walking -- it
    /// exists for the observation shot in VisionDirector, where the camera leaves
    /// Bunk's eyes and turns back to frame him. That shot is the only close-up in
    /// the level, so scale and footing have to be right.
    ///
    /// Scale is measured rather than assumed: FBX units vary wildly between
    /// exporters, so this fits the model to the CharacterController's own height
    /// using its renderer bounds. Safe to run again after re-exporting the model.
    /// </summary>
    public static class BunkSetup
    {
        private const string ModelPath = "Assets/Characters/Bunk/Bunk.fbx";
        private const string ChildName = "BunkModel";

        /// <summary>
        /// Degrees to turn the model about Y. If Bunk faces the camera in the
        /// observation shot when he should face away (or vice versa), set 180.
        /// </summary>
        private const float YawOffset = 0f;

        [MenuItem("FLAIR/Character/Set Bunk As Player Model")]
        public static void SetUp()
        {
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("[BunkSetup] No object called Player in the scene.");
                return;
            }

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller == null)
            {
                Debug.LogError("[BunkSetup] Player has no CharacterController to fit to.");
                return;
            }

            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (source == null)
            {
                Debug.LogError("[BunkSetup] No model at " + ModelPath +
                               ". Has Unity finished importing it?");
                return;
            }

            // Replace rather than stack, so re-running after a re-export is safe.
            Transform old = player.transform.Find(ChildName);
            if (old != null)
            {
                Undo.DestroyObjectImmediate(old.gameObject);
            }

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(source);
            model.name = ChildName;
            Undo.RegisterCreatedObjectUndo(model, "Add Bunk model");

            model.transform.SetParent(player.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0f, YawOffset, 0f);
            model.transform.localScale = Vector3.one;

            if (!TryGetBounds(model, out Bounds bounds))
            {
                Debug.LogError("[BunkSetup] The model has no renderers, so it cannot be fitted.");
                return;
            }

            // 1. Match the controller's height.
            float targetHeight = controller.height;
            float modelHeight = bounds.size.y;

            if (modelHeight < 0.0001f)
            {
                Debug.LogError("[BunkSetup] The model measures no height. Check the export.");
                return;
            }

            float scale = targetHeight / modelHeight;
            model.transform.localScale = Vector3.one * scale;

            // 2. Stand him on the controller's base rather than trusting the pivot,
            //    which may sit at the hips, the head or the origin depending on
            //    who exported it.
            TryGetBounds(model, out bounds);
            float baseY = player.transform.position.y + controller.center.y - controller.height * 0.5f;
            float feetY = bounds.min.y;
            model.transform.position += new Vector3(0f, baseY - feetY, 0f);

            // 3. Hide the greybox capsule. Kept rather than deleted -- it is the
            //    fallback if the model needs pulling out again.
            MeshRenderer capsule = player.GetComponent<MeshRenderer>();
            if (capsule != null && capsule.enabled)
            {
                Undo.RecordObject(capsule, "Hide capsule");
                capsule.enabled = false;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"[BunkSetup] Bunk fitted to {targetHeight:0.##}m " +
                      $"(model was {modelHeight:0.##} units, scaled by {scale:0.####}). " +
                      "Capsule renderer hidden. Save the scene with Ctrl+S.", model);
        }

        [MenuItem("FLAIR/Character/Restore Greybox Capsule")]
        public static void Restore()
        {
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                return;
            }

            Transform model = player.transform.Find(ChildName);
            if (model != null)
            {
                Undo.DestroyObjectImmediate(model.gameObject);
            }

            MeshRenderer capsule = player.GetComponent<MeshRenderer>();
            if (capsule != null)
            {
                Undo.RecordObject(capsule, "Show capsule");
                capsule.enabled = true;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[BunkSetup] Model removed, capsule visible again.");
        }

        /// <summary>World-space bounds of every renderer under the object.</summary>
        private static bool TryGetBounds(GameObject go, out Bounds bounds)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                bounds = default;
                return false;
            }

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return true;
        }
    }
}
