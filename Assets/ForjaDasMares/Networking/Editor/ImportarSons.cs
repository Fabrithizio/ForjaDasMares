using UnityEditor;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    // How the sounds in Resources/Som are imported, so they stay light on the phone:
    // music is read from disk while it plays (Streaming, Vorbis) instead of sitting decompressed in memory;
    // short effects are mono and ready in memory (no delay when a blow lands).
    public sealed class ImportarSons : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            if (!assetPath.Replace('\\', '/').Contains("/Resources/Som/")) return;
            var importer = (AudioImporter)assetImporter;
            var settings = importer.defaultSampleSettings;
            if (assetPath.Contains("/Som/Musicas/"))
            {
                importer.forceToMono = false; importer.loadInBackground = true;
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis; settings.quality = .6f;
            }
            else
            {
                importer.forceToMono = true;
                bool longo = assetPath.Contains("Uivo");
                settings.loadType = longo ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.Vorbis; settings.quality = .7f;
            }
            importer.defaultSampleSettings = settings;
        }
    }
}
