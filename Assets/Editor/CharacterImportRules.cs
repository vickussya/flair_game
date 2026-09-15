using UnityEditor;
using UnityEngine;

namespace Flair.EditorTools
{
    /// <summary>
    /// Import settings for everything under Assets/Characters, enforced in code so
    /// they are identical on every machine.
    ///
    /// Humanoid, because a Humanoid clip retargets onto any humanoid rig: the
    /// Mixamo walk and idle keep working on the Bunk we sculpt ourselves, long after
    /// the placeholder body is gone. It also matters that this is code rather than
    /// a hand-set Inspector value -- Bunk.fbx.meta is gitignored, so without a rule
    /// here Leta's copy would import as Generic and no clip would fit it.
    ///
    /// Only fills in clip settings the first time. Anything adjusted by hand in the
    /// Animation tab is left alone.
    /// </summary>
    public class CharacterImportRules : AssetPostprocessor
    {
        private const string CharactersRoot = "Assets/Characters/";

        private bool IsCharacterAsset => assetPath.StartsWith(CharactersRoot);

        private void OnPreprocessModel()
        {
            if (!IsCharacterAsset)
            {
                return;
            }

            ModelImporter importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
        }

        private void OnPreprocessAnimation()
        {
            if (!IsCharacterAsset)
            {
                return;
            }

            ModelImporter importer = (ModelImporter)assetImporter;

            // Respect hand edits. Only seed the clips if nobody has touched them.
            if (importer.clipAnimations != null && importer.clipAnimations.Length > 0)
            {
                return;
            }

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            string file = System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();

            // Mixamo clips arrive with looping off, so idle and walk would play one
            // cycle and freeze. Actions like the sniff should play once.
            bool loops = file.Contains("idle") || file.Contains("walk") || file.Contains("run");

            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].loopTime = loops;

                // The CharacterController moves Bunk. If the clip also moves him,
                // he drifts forward and snaps back every cycle -- the most common
                // reason Mixamo walks look broken in Unity. Bake it into the pose.
                clips[i].lockRootRotation = true;
                clips[i].lockRootHeightY = true;
                clips[i].lockRootPositionXZ = true;
                clips[i].keepOriginalPositionY = true;
            }

            importer.clipAnimations = clips;
        }
    }
}
