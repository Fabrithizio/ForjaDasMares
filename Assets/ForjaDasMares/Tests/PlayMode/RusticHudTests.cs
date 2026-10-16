using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Tests
{
    public sealed class RusticHudTests
    {
        RusticDungeonHud hud; ProgressionHud inventory; DungeonSession dungeon;
        [UnitySetUp] public IEnumerator Load()
        {
            SceneManager.LoadScene("GaleriasDungeon"); yield return null; yield return null; yield return null;
            hud=Object.FindFirstObjectByType<RusticDungeonHud>();inventory=Object.FindFirstObjectByType<ProgressionHud>();dungeon=Object.FindFirstObjectByType<DungeonSession>();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            inventory.Bridge.Abort();var profile=Object.FindFirstObjectByType<ProgressionProfileRuntime>();
            SceneManager.LoadScene("ArenaTest");if(profile!=null)Object.Destroy(profile.gameObject);yield return null;
        }
        [Test] public void CompactHudPreservesRealMenuAndInventoryActions()
        {
            Assert.NotNull(hud);Assert.IsFalse(hud.MenuOpen);
            var root=hud.SafeRoot;Assert.IsTrue(root.Find("HeroCard").gameObject.activeInHierarchy);
            Assert.That(((RectTransform)root.Find("HeroCard/ExperienceFillTrack/ExperienceFill")).rect.height,Is.GreaterThan(0),"Thin XP bars must still have a visible fill.");
            root.Find("HudMenu").GetComponent<Button>().onClick.Invoke();Assert.IsTrue(hud.MenuOpen);
            root.Find("AdventureMenu/Menu_Inventory").GetComponent<Button>().onClick.Invoke();Assert.IsTrue(inventory.IsOpen);Assert.IsFalse(hud.MenuOpen);
            inventory.Close();Assert.IsFalse(inventory.IsOpen);
            root.Find("HudInventory").GetComponent<Button>().onClick.Invoke();Assert.IsTrue(inventory.IsOpen);
            Assert.IsFalse(hud.transform.Find("Biome").gameObject.activeSelf,"Debug tools do not occupy combat view.");
        }
        [UnityTest] public IEnumerator HealthCooldownAndHeroActionsUseLiveData()
        {
            int selected=dungeon.Heroes.Selected;
            hud.SafeRoot.Find("HudHero").GetComponent<Button>().onClick.Invoke();yield return null;yield return new WaitForSeconds(.15f);
            Assert.That(dungeon.Heroes.Selected,Is.EqualTo((selected+1)%4));
            dungeon.Heroes.Health.ApplyDamage(12);yield return new WaitForSeconds(.15f);
            string expected=dungeon.Heroes.Health.Current+" / "+dungeon.Heroes.Health.Maximum;
            StringAssert.Contains(expected,hud.SafeRoot.Find("HeroCard/Health").GetComponent<Text>().text);
            hud.SafeRoot.Find("ActionDock/Special").GetComponent<Button>().onClick.Invoke();yield return new WaitForSeconds(.15f);
            Assert.That(dungeon.Heroes.GetComponent<HeroCombat>().SpecialCooldownRemaining,Is.GreaterThan(0));
            StringAssert.Contains("s",hud.SafeRoot.Find("ActionDock/Special/Cooldown").GetComponent<Text>().text);
        }
        [UnityTest] public IEnumerator MenuCanCloseAndResizingKeepsSafeRootInsideCanvas()
        {
            hud.ToggleMenu();Assert.IsTrue(hud.MenuOpen);hud.ToggleMenu();Assert.IsFalse(hud.MenuOpen);
            Canvas.ForceUpdateCanvases();yield return null;
            Assert.That(hud.SafeRoot.anchorMin.x,Is.InRange(0f,1f));Assert.That(hud.SafeRoot.anchorMax.x,Is.InRange(0f,1f));
            foreach(var button in hud.SafeRoot.GetComponentsInChildren<Button>(true))Assert.NotNull(button.targetGraphic);
            var glyphs=hud.SafeRoot.GetComponentsInChildren<HudIcon>(true);Assert.That(glyphs.Length,Is.GreaterThanOrEqualTo(7));
        }
    }
}
