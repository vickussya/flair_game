using UnityEditor;

namespace Flair.EditorTools
{
    /// <summary>
    /// Import settings for every video under Assets/Visions, enforced in code so
    /// both machines agree.
    ///
    /// Visions are transcoded to VP8 on import. Unity decodes VP8 itself, so
    /// playback no longer depends on the operating system's H.264 decoder -- which
    /// is exactly the kind of dependency that works on one machine and silently
    /// shows nothing on the next. The MP4 in the repo stays as rendered; only
    /// Unity's imported copy changes.
    /// </summary>
    public class VisionImportRules : AssetPostprocessor
    {
        private const string VisionsRoot = "Assets/Visions/";

        private void OnPreprocessAsset()
        {
            if (!assetPath.StartsWith(VisionsRoot))
            {
                return;
            }

            if (!(assetImporter is VideoClipImporter importer))
            {
                return;
            }

            VideoImporterTargetSettings settings = importer.defaultTargetSettings;
            settings.enableTranscoding = true;
            settings.codec = VideoCodec.VP8;
            importer.defaultTargetSettings = settings;

            // Visions carry no sound; audio arrives separately with task 22.
            importer.importAudio = false;
        }
    }
}
