using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class AdventurePolishTests
    {
        CoopDirector session;
        [UnitySetUp]public IEnumerator Load(){SceneManager.LoadScene("CampaignCoop");yield return null;yield return null;session=Object.FindFirstObjectByType<CoopDirector>();}
        [UnityTearDown]public IEnumerator Close(){session.Close();yield return null;yield return null;}
        [Test]public void GearSlotsUseImagesAndKeepTheirInspectionAction()
        {
            var hud=Object.FindFirstObjectByType<CoopInventoryHud>();hud.OpenInventory();
            for(int i=0;i<5;i++){var slot=Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None);Button found=null;foreach(var button in slot)if(button.name=="EquipmentSlot_"+i)found=button;Assert.NotNull(found);Assert.NotNull(found.transform.Find("EquippedItemIcon").GetComponent<Image>());Assert.NotNull(found.transform.Find("EmptySlot").GetComponent<HudIcon>());Assert.AreEqual("",found.GetComponentInChildren<Text>().text);found.onClick.Invoke();}
        }
        [Test]public void CombatFeelHasRealAudioAndSharedMaterial()
        {
            var feel=session.Actor(0).Roster.Current.feedback;Assert.NotNull(feel);foreach(var clip in new[]{feel.impact,feel.swing,feel.heavy,feel.step,feel.reward,feel.perfect}){Assert.NotNull(clip);Assert.Greater(clip.samples,100);Assert.Greater(clip.length,.05f);}Assert.NotNull(feel.trailMaterial);Assert.LessOrEqual(feel.impactPause,.06f);
        }
        [Test]public void VisualPauseNeverStopsGameTime()
        {
            float original=Time.timeScale;var model=session.Actor(0).Presentation.Visible.GetComponentInChildren<HeroModelView>();model.PauseImpact(.04f);Assert.AreEqual(original,Time.timeScale);
        }
    }
}
