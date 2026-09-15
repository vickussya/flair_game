using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Flair.EditorTools
{
    /// <summary>
    /// One-click builds for sharing the demo: a web build to play in the browser
    /// and a Windows build to download. Each is zipped and ready to upload to
    /// itch.io -- see docs/build-and-publish.md.
    ///
    /// Everything lands in Builds/, which is gitignored, so builds never end up in
    /// the repo.
    /// </summary>
    public static class BuildTools
    {
        private const string BuildsRoot = "Builds";

        [MenuItem("FLAIR/Build/Web (play in browser)")]
        public static void BuildWeb()
        {
            // itch.io cannot serve Brotli-compressed Unity builds with the headers
            // they need, and the page hangs on a loading bar forever. Gzip with the
            // decompression fallback loads everywhere, at the cost of a little speed.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;

            Build(BuildTarget.WebGL, Path.Combine(BuildsRoot, "Web"), "Flair-Web.zip");
        }

        [MenuItem("FLAIR/Build/Windows (.exe download)")]
        public static void BuildWindows()
        {
            string folder = Path.Combine(BuildsRoot, "Windows");
            Build(BuildTarget.StandaloneWindows64, folder, "Flair-Windows.zip",
                  Path.Combine(folder, PlayerSettings.productName + ".exe"));
        }

        private static void Build(BuildTarget target, string folder, string zipName, string locationOverride = null)
        {
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildTools] No scenes enabled in Build Settings.");
                return;
            }

            // Start from nothing, so files from an older build never sneak into the zip.
            FileUtil.DeleteFileOrDirectory(folder);
            Directory.CreateDirectory(folder);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                target = target,
                locationPathName = locationOverride ?? folder,
                options = BuildOptions.None,
            };

            Debug.Log($"[BuildTools] Building {target}. The first build for a platform re-imports " +
                      "assets and can take several minutes.");

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[BuildTools] {target} build {report.summary.result} with " +
                               $"{report.summary.totalErrors} error(s). See the Console above.");
                return;
            }

            string zipPath = Path.Combine(BuildsRoot, zipName);
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            // Contents at the root of the zip: itch.io looks for index.html there.
            ZipFile.CreateFromDirectory(folder, zipPath, System.IO.Compression.CompressionLevel.Optimal, false);

            float mb = new FileInfo(zipPath).Length / (1024f * 1024f);
            Debug.Log($"[BuildTools] {target} build done: {zipPath} ({mb:0.0} MB). Ready to upload to itch.io.");

            EditorUtility.RevealInFinder(zipPath);
        }
    }
}
