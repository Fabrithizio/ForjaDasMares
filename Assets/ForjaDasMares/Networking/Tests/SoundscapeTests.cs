using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class SoundscapeTests
    {
        CoopDirector session;CampaignSoundscape audio;SoundscapeLibrary library;
        readonly string[] keys={CampaignSoundscape.MusicKey,CampaignSoundscape.AmbienceKey};
        readonly float[] prior=new float[2];readonly bool[] existed=new bool[2];
        [UnitySetUp]public IEnumerator Load()
        {
            for(int i=0;i<keys.Length;i++){existed[i]=PlayerPrefs.HasKey(keys[i]);prior[i]=PlayerPrefs.GetFloat(keys[i]);PlayerPrefs.SetFloat(keys[i],.7f);}
            yield return SceneManager.LoadSceneAsync("CampaignCoop");yield return null;yield return null;
            session=Object.FindFirstObjectByType<CoopDirector>();audio=session.GetComponent<CampaignSoundscape>();library=Resources.Load<SoundscapeLibrary>("CampaignSoundscape");Assert.NotNull(audio);Assert.NotNull(library);
        }
        [UnityTearDown]public IEnumerator Close()
        {
            if(session!=null)session.Close();for(int i=0;i<keys.Length;i++){if(existed[i])PlayerPrefs.SetFloat(keys[i],prior[i]);else PlayerPrefs.DeleteKey(keys[i]);}PlayerPrefs.Save();yield return null;
        }
        [Test]public void TwelveRealStereoClipsAreConfigured()
        {
            Assert.AreEqual(3,library.biomes.Length);
            var loops=library.biomes.SelectMany(c=>new[]{c.music,c.ambience}).Concat(new[]{library.menu.music,library.menu.ambience,library.bossMusic}).ToArray();
            Assert.AreEqual(9,loops.Length);foreach(var clip in loops){Assert.NotNull(clip);Assert.AreEqual(2,clip.channels);Assert.Greater(clip.length,30);Assert.Greater(clip.samples,100000);}
            foreach(var clip in new[]{library.start,library.victory,library.defeat}){Assert.NotNull(clip);Assert.Greater(clip.length,1);}
        }
        [Test]public void MenuHasBoundedVoicesWithoutRecreatingOnRefresh()
        {
            audio.RefreshNow();Assert.AreEqual(library.menu.music,audio.CurrentMusic);Assert.AreEqual(library.menu.ambience,audio.CurrentAmbience);Assert.AreEqual("Menu",audio.Context);
            int before=audio.GetComponentsInChildren<AudioSource>().Length;Assert.AreEqual(5,before);
            for(int i=0;i<30;i++)audio.RefreshNow();Assert.AreEqual(before,audio.GetComponentsInChildren<AudioSource>().Length);
        }
        [UnityTest]public IEnumerator EnteringDungeonAndClosingRestoresCorrectTracks()
        {
            session.SelectCampaignLevel(0);Assert.IsTrue(session.HostDirect("AUDIOQA",7892,false));yield return null;session.StartGame();float deadline=Time.realtimeSinceStartup+15;
            while(session.Phase==CoopPhase.Loading&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.AreEqual(CoopPhase.Playing,session.Phase);audio.RefreshNow();Assert.AreEqual(library.biomes[0].music,audio.CurrentMusic);Assert.AreEqual(library.biomes[0].ambience,audio.CurrentAmbience);
            float end=Time.realtimeSinceStartup+3;
            while(Time.realtimeSinceStartup<end){Assert.LessOrEqual(audio.GetComponentsInChildren<AudioSource>().Count(s=>s.name.StartsWith("Music")&&s.isPlaying),1,"Music loops must never overlap");yield return null;}
            session.Close();audio.RefreshNow();Assert.AreEqual(library.menu.music,audio.CurrentMusic);
        }
        [UnityTest]public IEnumerator MusicMuteDoesNotMuteAmbienceAndDisableStopsVoices()
        {
            PlayerPrefs.SetFloat(CampaignSoundscape.MusicKey,0);PlayerPrefs.SetFloat(CampaignSoundscape.AmbienceKey,1);yield return null;yield return null;
            var sources=audio.GetComponentsInChildren<AudioSource>();Assert.IsTrue(sources.Where(s=>s.name.StartsWith("Music")).All(s=>s.volume==0));Assert.Greater(sources.Where(s=>s.name.StartsWith("Ambience")).Sum(s=>s.volume),0);
            audio.enabled=false;yield return null;Assert.IsTrue(sources.All(s=>!s.isPlaying));
        }
        [Test]public void HomeAndSessionAudioControlsChangeRealPreferences()
        {
            var buttons=Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach(string name in new[]{"MusicVolume","AmbienceVolume"})
            {
                var matches=buttons.Where(b=>b.name==name).ToArray();Assert.AreEqual(2,matches.Length,name);string key=name=="MusicVolume"?CampaignSoundscape.MusicKey:CampaignSoundscape.AmbienceKey;
                foreach(var button in matches){float before=CampaignSoundscape.Volume(key);button.onClick.Invoke();Assert.AreNotEqual(before,CampaignSoundscape.Volume(key));StringAssert.Contains("%",button.GetComponentInChildren<Text>().text);}
            }
        }
    }
}
