using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Flair.EditorTools
{
    /// <summary>
    /// A browser cannot play an imported VideoClip, so web builds load visions by
    /// URL from StreamingAssets instead (see VideoVisionPlayer.AssignSource). This
    /// copies Assets/Visions into StreamingAssets for the length of a web build,
    /// then removes the copy again.
    ///
    /// That keeps one copy of each vision in the repo. Replacing the placeholder
    /// ink video with a drawn one is still a single overwrite in Assets/Visions --
    /// nobody has to remember a second copy.
    ///
    /// Runs for any web build, including Unity's own Build button, not only the
    /// FLAIR build menu.
    /// </summary>
    public class WebVisionBuildStep : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private const string SourceFolder = "Assets/Visions";
        private const string TargetFolder = "Assets/StreamingAssets/Visions";

        private static readonly string[] VideoExtensions = { ".mp4", ".webm", ".mov", ".m4v" };

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // A build that failed last time never reached its cleanup. Start clean.
            RemoveCopy();

            if (report.summary.platform != BuildTarget.WebGL)
            {
                return;
            }

            if (!Directory.Exists(SourceFolder))
            {
                return;
            }

            Directory.CreateDirectory(TargetFolder);

            int copied = 0;
            foreach (string file in Directory.GetFiles(SourceFolder))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (System.Array.IndexOf(VideoExtensions, ext) < 0)
                {
                    continue;
                }

                File.Copy(file, Path.Combine(TargetFolder, Path.GetFileName(file)), true);
                copied++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[WebVisionBuildStep] Copied {copied} vision(s) to StreamingAssets for the web build.");
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.WebGL)
            {
                RemoveCopy();
            }
        }

        private static void RemoveCopy()
        {
            if (!AssetDatabase.IsValidFolder(TargetFolder))
            {
                return;
            }

            AssetDatabase.DeleteAsset(TargetFolder);

            // Remove StreamingAssets too if this step was the only thing using it.
            const string parent = "Assets/StreamingAssets";
            if (AssetDatabase.IsValidFolder(parent) && Directory.GetFileSystemEntries(parent).Length == 0)
            {
                AssetDatabase.DeleteAsset(parent);
            }

            AssetDatabase.Refresh();
        }
    }
}
