using UnityEngine;
namespace ForjaDasMares.Networking
{
    // Music and ambience volumes (settings). The regional world plays the sounds itself (RegionalWorld).
    public static class CampaignSoundscape
    {
        public const string MusicKey="CampaignMusicVolume",AmbienceKey="CampaignAmbienceVolume";
        public static float Volume(string key)=>Mathf.Clamp01(PlayerPrefs.GetFloat(key,key==MusicKey?.70f:.65f));
        public static float CycleVolume(string key)
        {float value=Volume(key);value=value<.01f?1:Mathf.Max(0,value-.2f);PlayerPrefs.SetFloat(key,value);PlayerPrefs.Save();return value;}
    }
}
