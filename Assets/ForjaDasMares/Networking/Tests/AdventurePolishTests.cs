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
        [Test]public void FourOverlappingPingsFitDifferentAspectRatiosWithoutCoveringEachOther()
        {
            var points=new Vector2[4];var size=new Vector2(190,48);
            foreach(float width in new[]{720f,1280f,1600f})foreach(Vector2 corner in new[]{Vector2.zero,new Vector2(-5000,-5000),new Vector2(5000,5000)})
            {
                var bounds=new Rect(-width*.5f,-215,width,415);
                for(int i=0;i<4;i++)
                {
                    points[i]=CampaignAdventureHud.PlaceMarker(corner,bounds,size,points,i);
                    Assert.GreaterOrEqual(points[i].x-size.x*.5f,bounds.xMin);Assert.LessOrEqual(points[i].x+size.x*.5f,bounds.xMax);
                    Assert.GreaterOrEqual(points[i].y-size.y*.5f,bounds.yMin);Assert.LessOrEqual(points[i].y+size.y*.5f,bounds.yMax);
                    for(int j=0;j<i;j++)Assert.IsFalse(Mathf.Abs(points[i].x-points[j].x)<size.x&&Mathf.Abs(points[i].y-points[j].y)<size.y,"Overlapping ally pings");
                }
            }
        }
        [UnityTest]public IEnumerator LocalInputRemainsTheActualReaderWhenAnotherActorBecomesActive()
        {
            var reader=session.LocalInput;Assert.AreSame(session.Actor(0).GetComponent<ArenaInput>(),reader);var other=session.Actor(1);other.gameObject.SetActive(true);yield return null;
            Assert.AreSame(reader,session.LocalInput);Assert.AreNotSame(other.GetComponent<ArenaInput>(),reader);Assert.IsTrue(reader.enabled);other.gameObject.SetActive(false);
        }
        [UnityTest]public IEnumerator FixedPingButtonsAreAbsentDuringGameplay()
        {
            Assert.IsTrue(session.HostDirect("CLEANHUD",7897,false));yield return null;session.StartGame();
            float until=Time.realtimeSinceStartup+12;
            while(session.Phase!=CoopPhase.Playing&&Time.realtimeSinceStartup<until)yield return null;
            Assert.AreEqual(CoopPhase.Playing,session.Phase);yield return null;
            foreach(var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                Assert.IsFalse(button.name.StartsWith("Ping_"),"Permanent ping button remained in HUD");
            foreach(var rect in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                Assert.AreNotEqual("QuickPings",rect.name,"Removed toolbar must not occupy or intercept the screen");
        }
        [Test]public void VisualPauseNeverStopsGameTime()
        {
            float original=Time.timeScale;var model=session.Actor(0).Presentation.Visible.GetComponentInChildren<HeroModelView>();model.PauseImpact(.04f);Assert.AreEqual(original,Time.timeScale);
        }
    }
}
