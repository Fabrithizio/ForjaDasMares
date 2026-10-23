using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class CampaignPresentationTests
    {
        CoopDirector director;CampaignHud hud;bool hadQuality;int previousQuality;
        Button Button(string name)=>hud.GetComponentsInChildren<Button>(true).Single(x=>x.name==name);
        [UnitySetUp]public IEnumerator Setup()
        {
            hadQuality=PlayerPrefs.HasKey(PresentationQuality.Preference);previousQuality=PlayerPrefs.GetInt(PresentationQuality.Preference,1);PlayerPrefs.SetInt(PresentationQuality.Preference,1);
            yield return SceneManager.LoadSceneAsync("CampaignCoop");yield return null;yield return null;
            director=UnityEngine.Object.FindFirstObjectByType<CoopDirector>();hud=UnityEngine.Object.FindFirstObjectByType<CampaignHud>();
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            director?.Close();if(hadQuality)PlayerPrefs.SetInt(PresentationQuality.Preference,previousQuality);else PlayerPrefs.DeleteKey(PresentationQuality.Preference);PlayerPrefs.Save();yield return null;
        }
        [Test]public void OriginalArtworkIsPresentAndDoesNotInterceptClicks()
        {var art=hud.GetComponentsInChildren<MenuArtwork>(true);Assert.That(art.Length,Is.GreaterThanOrEqualTo(3));foreach(var image in art){Assert.NotNull(image.texture,image.name);Assert.IsFalse(image.raycastTarget);Assert.That(image.uvRect.width,Is.GreaterThan(0).And.LessThanOrEqualTo(1));Assert.That(image.uvRect.height,Is.GreaterThan(0).And.LessThanOrEqualTo(1));}}
        [UnityTest]public IEnumerator HomeCampaignAndHeroCardsUseRealSelection()
        {
            Button("Campaign").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);Assert.IsFalse(hud.AtHome);
            Button("Hero").onClick.Invoke();var picker=hud.GetComponent<CampaignHeroPicker>();Assert.IsTrue(picker.IsOpen);
            Assert.That(director.HeroCount,Is.GreaterThanOrEqualTo(4));for(int i=0;i<director.HeroCount;i++)Assert.NotNull(Button("HeroChoice_"+i));
            Button("HeroChoice_2").onClick.Invoke();Assert.AreEqual(2,director.SelectedHero);Assert.AreEqual(director.HeroAt(2).heroId,director.Actor(0).Roster.Current.heroId);Assert.IsFalse(picker.IsOpen);
        }
        [UnityTest]public IEnumerator StageButtonsSelectRealCampaignAndInputRequiresValidCode()
        {
            Button("Campaign").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);Button("Level_3").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);Assert.AreEqual(2,director.Campaign.Selected);
            var input=hud.GetComponentsInChildren<InputField>(true).Single(x=>x.name=="InvitationCode");input.text="";yield return new WaitForSecondsRealtime(.2f);Assert.IsFalse(Button("JoinCode").interactable);
            input.text="ABCD12";yield return new WaitForSecondsRealtime(.2f);Assert.IsTrue(Button("JoinCode").interactable);Assert.IsTrue(Button("Solo").interactable);Assert.IsTrue(Button("Private").interactable);
        }
        [TestCase(null,false)][TestCase("",false)][TestCase("   ",false)][TestCase("ABC",false)][TestCase("AB-CD",false)][TestCase("ÁBCD",false)][TestCase("ABCD12",true)][TestCase(" abcd ",true)][TestCase("1234567890123",false)]
        public void InvitationCodeValidationMatchesDisplayedRules(string value,bool valid)=>Assert.AreEqual(valid,CampaignHud.ValidCode(value));
        [UnityTest]public IEnumerator UnconfiguredSoloDoesNotQueueAnUnexpectedStart()
        {
            var cloud=director.GetComponent<CoopCloud>();var field=typeof(CoopCloud).GetField("settings",BindingFlags.NonPublic|BindingFlags.Instance);var old=field.GetValue(cloud);
            try{field.SetValue(cloud,null);hud.BeginSolo();Assert.IsFalse((bool)typeof(CampaignHud).GetField("soloPending",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(hud));}
            finally{field.SetValue(cloud,old);}
            Assert.IsTrue(director.HostDirect("NOPEND",7814,false));yield return new WaitForSecondsRealtime(.3f);Assert.AreEqual(CoopPhase.Lobby,director.Phase);
        }
        [UnityTest]public IEnumerator LobbyShowsConnectedHeroesAndHostStartActuallyLoadsGame()
        {
            Assert.IsTrue(director.HostDirect("PRESENT",7811,false));yield return new WaitForSecondsRealtime(.3f);
            var slot=hud.GetComponentsInChildren<Transform>(true).Single(x=>x.name=="PartySlot_0");Assert.IsTrue(slot.gameObject.activeInHierarchy);StringAssert.Contains(director.Actor(0).Roster.Current.displayName,slot.GetComponentInChildren<Text>().text);
            Assert.IsTrue(Button("Start").interactable);Button("Start").onClick.Invoke();Assert.AreEqual(CoopPhase.Loading,director.Phase);
            float end=Time.realtimeSinceStartup+10;while(director.Phase!=CoopPhase.Playing&&Time.realtimeSinceStartup<end)yield return null;
            Assert.AreEqual(CoopPhase.Playing,director.Phase);yield return new WaitForSecondsRealtime(.2f);var loading=hud.GetComponentsInChildren<Transform>(true).Single(x=>x.name=="CampaignLoading");Assert.IsFalse(loading.gameObject.activeSelf);
        }
        [UnityTest]public IEnumerator GraphicsButtonCyclesRealRenderScaleAndKeepsPreference()
        {
            var home=hud.GetComponentInChildren<CampaignHome>();home.GetComponentsInChildren<Button>(true).Single(x=>x.name=="Settings").onClick.Invoke();
            var button=home.GetComponentsInChildren<Button>(true).Single(x=>x.name=="GraphicsQuality");button.onClick.Invoke();Assert.AreEqual(2,PresentationQuality.Selected);button.onClick.Invoke();Assert.AreEqual(0,PresentationQuality.Selected);
            Assert.That(hud.GetComponent<PresentationQuality>().RenderScale,Is.EqualTo(.8f).Within(.001f));yield return null;
        }
        [UnityTest]public IEnumerator ClosedRoomReturnsHomeWithActualReason()
        {
            Assert.IsTrue(director.HostDirect("CLOSED",7812,false));yield return null;director.Close();yield return new WaitForSecondsRealtime(.3f);Assert.IsTrue(hud.AtHome);
            var notice=hud.GetComponentsInChildren<Text>(true).Single(x=>x.name=="ConnectionNotice");StringAssert.Contains(director.Message,notice.text);
        }
        [Test]public void HostShutdownNoticeRequiresNewRoomInsteadOfPromisingReconnection()
        {var message=CampaignHud.ClosedNotice("Conexão encerrada. Use o código para retornar ao mesmo host. Disconnected due to host shutting down.");StringAssert.Contains("novo código",message);Assert.IsFalse(message.Contains("retornar ao mesmo host"));Assert.IsFalse(message.Contains("Disconnected"));Assert.AreEqual("Sala encerrada.",CampaignHud.ClosedNotice("Sala encerrada."));}
        [Test]public void ConnectionErrorsDoNotExposeSdkMessagesToPlayers()
        {StringAssert.Contains("tempo",CoopCloud.FriendlyConnectionError(new OperationCanceledException()));var error=CoopCloud.FriendlyConnectionError(new Exception("internal-sdk-detail"));Assert.IsFalse(error.Contains("internal-sdk-detail"));StringAssert.Contains("internet",error);}
        [Test]public void RoundedBordersAndIconLinesContainTransparentCoverageVertices()
        {
            void Check(Graphic graphic)
            {
                using(var helper=new VertexHelper())
                {
                    graphic.GetType().GetMethod("OnPopulateMesh",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.DeclaredOnly).Invoke(graphic,new object[]{helper});
                    Assert.That(helper.currentVertCount,Is.GreaterThan(0));bool transparent=false,visible=false;var vertex=new UIVertex();for(int i=0;i<helper.currentVertCount;i++){helper.PopulateUIVertex(ref vertex,i);transparent|=vertex.color.a==0;visible|=vertex.color.a>0;}Assert.IsTrue(transparent);Assert.IsTrue(visible);
                }
            }
            Check(Button("Campaign").GetComponent<RoundedHudGraphic>());Check(hud.GetComponentsInChildren<HudIcon>(true).First());
        }
    }
}
