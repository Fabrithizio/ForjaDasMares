using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ForjaDasMares.Networking.Editor
{
    public static class SoundscapeInstaller
    {
        const string Root="Assets/ForjaDasMares/Audio/Soundscapes/";
        public static void Apply()
        {
            AssetDatabase.Refresh();
            foreach(string path in Directory.GetFiles(Root,"*.wav"))
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));bool loop=path.Contains("Amb_")||path.Contains("Menu_")||path.Contains("Catacumbas_")||path.Contains("Floresta_")||path.Contains("Fortaleza_")||path.Contains("Chefe_");
                var settings=importer.defaultSampleSettings;settings.loadType=loop?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.65f;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;importer.defaultSampleSettings=settings;importer.forceToMono=false;importer.loadInBackground=true;importer.SaveAndReimport();
            }
            string asset="Assets/ForjaDasMares/Resources/CampaignSoundscape.asset";var data=AssetDatabase.LoadAssetAtPath<SoundscapeLibrary>(asset);
            if(data==null){data=ScriptableObject.CreateInstance<SoundscapeLibrary>();AssetDatabase.CreateAsset(data,asset);}
            data.menu=Cue("Menu","Menu_Mares","Amb_Porto");
            data.biomes=new[]{Cue("Catacumbas","Catacumbas_Ecos","Amb_Catacumbas"),Cue("Floresta subterrânea","Floresta_Raizes","Amb_Floresta"),Cue("Fortaleza vulcânica","Fortaleza_Brasas","Amb_Fortaleza")};
            data.bossMusic=Clip("Chefe_Ferro");data.start=Clip("Partida");data.victory=Clip("Vitoria");data.defeat=Clip("Derrota");data.crossfade=2.5f;
            EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();Debug.Log("SOUNDSCAPE_READY music5 ambience4 accents3 stereo streaming loops");
        }
        static SoundscapeCue Cue(string name,string music,string ambience)=>new SoundscapeCue{displayName=name,music=Clip(music),ambience=Clip(ambience)};
        static AudioClip Clip(string name){var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+name+".wav");if(clip==null)throw new InvalidOperationException("Missing soundscape "+name);return clip;}
    }
}
