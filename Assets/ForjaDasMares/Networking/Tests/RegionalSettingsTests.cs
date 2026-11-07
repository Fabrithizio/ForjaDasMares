using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class RegionalSettingsTests
    {
        RegionalSettingsMenu menu;RegionalWorld world;float oldMaster;bool hadMaster;
        [UnitySetUp]public IEnumerator Load(){hadMaster=PlayerPrefs.HasKey("CampaignMasterVolume");oldMaster=PlayerPrefs.GetFloat("CampaignMasterVolume",.8f);yield return SceneManager.LoadSceneAsync("RefugioUltimoSino");yield return null;yield return null;yield return null;menu=Object.FindFirstObjectByType<RegionalSettingsMenu>();world=Object.FindFirstObjectByType<RegionalWorld>();}
        [UnityTearDown]public IEnumerator Close(){if(menu!=null)menu.Close();if(hadMaster)PlayerPrefs.SetFloat("CampaignMasterVolume",oldMaster);else PlayerPrefs.DeleteKey("CampaignMasterVolume");AudioListener.volume=oldMaster;PlayerPrefs.Save();yield return null;}
        Button Find(string name)=>menu.GetComponentsInChildren<Button>(true).Single(b=>b.name==name);
        [UnityTest]public IEnumerator SettingsOpenAndCloseWithoutFreezingTime(){float scale=Time.timeScale;Find("RegionalSettingsButton").onClick.Invoke();yield return null;Assert.IsTrue(menu.IsOpen);Assert.IsTrue(world.Hero.GetComponent<ArenaInput>().CombatBlocked);Assert.AreEqual(scale,Time.timeScale);Find("CloseRegionalSettings").onClick.Invoke();Assert.IsFalse(menu.IsOpen);Assert.IsFalse(world.Hero.GetComponent<ArenaInput>().CombatBlocked);}
        [UnityTest]public IEnumerator AudioButtonsChangeTheRealSavedMasterVolume(){menu.Open();float before=PlayerPrefs.GetFloat("CampaignMasterVolume",.8f);Find("AudioDown0").onClick.Invoke();yield return null;Assert.AreEqual(Mathf.Clamp01(before-.1f),AudioListener.volume,.001f);Assert.AreEqual(AudioListener.volume,PlayerPrefs.GetFloat("CampaignMasterVolume"),.001f);}
        [UnityTest]public IEnumerator InventoryAndSettingsNeverLeaveTwoOverlaysOpen(){var inv=menu.GetComponent<CoopInventoryHud>();menu.Open();inv.OpenInventory();yield return null;Assert.IsFalse(menu.IsOpen);Assert.IsTrue(inv.IsOpen);menu.Open();Assert.IsFalse(inv.IsOpen);Assert.IsTrue(menu.IsOpen);}
    }
}
