using UnityEditor;
using UnityEngine;
namespace ForjaDasMares.Networking.Editor
{
    public sealed class MenuArtImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/ForjaDasMares/Resources/MenuArt/"))return;
            var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.mipmapEnabled=false;importer.isReadable=false;importer.maxTextureSize=2048;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.alphaSource=TextureImporterAlphaSource.None;
        }
    }
}
