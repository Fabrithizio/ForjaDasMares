using UnityEditor;

namespace ForjaDasMares.Networking.Editor
{
    // World map art and its icons: full quality (up to 4096 px, high-quality compression, no mipmaps for UI).
    sealed class MapaTexturas : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("/Resources/Mapa/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            importer.filterMode = UnityEngine.FilterMode.Bilinear;
            importer.alphaIsTransparency = importer.DoesSourceTextureHaveAlpha();
            importer.sRGBTexture = true;
            // MapaAreas is data (which place owns each pixel), not a picture: exact values, readable by the game.
            if (assetPath.EndsWith("MapaAreas.png"))
            {
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = UnityEngine.FilterMode.Point;
                importer.sRGBTexture = false;
                importer.alphaIsTransparency = false;
                importer.maxTextureSize = 1024;
            }
        }
    }
}
