using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class InventoryClarityTests
    {
        CoopDirector session;CoopInventoryHud hud;CoopProfileChannel channel;ProgressionCatalog catalog;
        ItemInstance common,rare,otherClass;EquipmentDefinition staff;
        Transform Root=>GameObject.Find("CoopInventory").transform;
        Button Button(string name)=>Root.GetComponentsInChildren<Button>(true).Single(x=>x.name==name);
        Text Own=>Root.Find("ItemComparison/SelectedItem/Content").GetComponent<Text>();
        ItemInstance NewItem(EquipmentDefinition definition,ItemRarity rarity=ItemRarity.Common)=>LootGenerator.Create(catalog,definition.itemId,rarity,new System.Random(77));
        int VisibleRows()=>Root.GetComponentsInChildren<Button>(true).Count(x=>x.name.StartsWith("InventoryItem_")&&x.gameObject.activeSelf);
        [UnitySetUp]public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("CampaignCoop");yield return null;yield return null;
            session=UnityEngine.Object.FindFirstObjectByType<CoopDirector>();session.SelectHero(2);yield return null;
            hud=UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>();channel=session.GetComponent<CoopProfileChannel>();catalog=channel.Catalog;
            staff=catalog.equipment.First(x=>x.category==WeaponCategory.Staff&&!x.starter);
            common=NewItem(staff);rare=NewItem(staff,ItemRarity.Rare);
            otherClass=NewItem(catalog.equipment.First(x=>x.category==WeaponCategory.Axe&&!x.starter));
            channel.LocalProfile.inventory.Add(common);channel.LocalProfile.inventory.Add(rare);channel.LocalProfile.inventory.Add(otherClass);
            hud.OpenInventory();yield return null;
        }
        [UnityTearDown]public IEnumerator Close(){if(hud!=null)hud.CloseInventory();if(session!=null)session.Close();yield return null;yield return null;}
        [Test]public void ProfileAreasSeparateBagStatusResourcesAndPreserveEquipment()
        {
            string before=JsonUtility.ToJson(channel.LocalProfile);
            hud.NavigateProfile(0);Assert.IsTrue(Root.Find("CharacterPage").gameObject.activeSelf);Assert.IsFalse(Root.Find("InventoryColumn").gameObject.activeSelf);
            Assert.AreEqual(5,Root.Find("CharacterPage").GetComponentsInChildren<Button>(true).Count(b=>b.name.StartsWith("InspectSlot_")));
            hud.NavigateProfile(2);Assert.IsTrue(Root.Find("StatusPage").gameObject.activeSelf);Assert.IsFalse(Root.Find("CharacterPage").gameObject.activeSelf);
            Button("StatusTab_1").onClick.Invoke();StringAssert.Contains("Defesa",string.Join(" ",Root.Find("StatusPage").GetComponentsInChildren<Text>().Select(t=>t.text)));
            hud.NavigateProfile(4);Assert.IsTrue(Root.Find("ResourcesPage").gameObject.activeSelf);
            hud.NavigateProfile(1);Assert.IsTrue(Root.Find("InventoryColumn").gameObject.activeSelf);Assert.IsFalse(Root.Find("ResourcesPage").gameObject.activeSelf);
            hud.NavigateProfile(3);var talents=Root.GetComponent<CampaignCharacterSheet>();Assert.IsTrue(talents.IsOpen);talents.Hide();hud.SendMessage("LateUpdate");Assert.AreEqual(0,hud.ProfileArea,"Closing talents returns to the character page");
            Assert.AreEqual(before,JsonUtility.ToJson(channel.LocalProfile),"Navigation must not mutate inventory, equipment or coins");
        }
        [UnityTest]public IEnumerator PerformanceSamplesOnlyGameplayAndResetsForInventory()
        {
            var diagnostic=UnityEngine.Object.FindFirstObjectByType<CampaignPerformanceHud>();Assert.NotNull(diagnostic);
            yield return null;Assert.AreEqual(0,diagnostic.SampleCount,"Inventory frames must not count as gameplay.");
            hud.CloseInventory();Assert.IsTrue(session.HostDirect("PERFQA",7895,false));
            yield return null;Assert.AreEqual(0,diagnostic.SampleCount,"Lobby frames must not count as gameplay.");
            session.StartGame();float deadline=Time.realtimeSinceStartup+15;
            while(session.Phase==CoopPhase.Loading&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.AreEqual(CoopPhase.Playing,session.Phase);yield return null;yield return null;
            Assert.Greater(diagnostic.SampleCount,0);
            hud.OpenInventory();yield return null;yield return null;Assert.AreEqual(0,diagnostic.SampleCount);
        }
        [Test]public void ProjectileDescriptionsDoNotInventAreaDamageAndShowActualSpread()
        {
            var attack=ScriptableObject.CreateInstance<AttackDefinition>();
            try
            {
                attack.shape=AttackShape.Projectile;attack.maximumTargets=0;attack.splashRadius=0;attack.projectileCount=1;
                StringAssert.Contains("primeiro impacto",ItemDescriptions.Pattern(attack));StringAssert.DoesNotContain("explosão",ItemDescriptions.Pattern(attack));
                attack.projectileCount=3;StringAssert.Contains("3 projéteis",ItemDescriptions.Pattern(attack));
                attack.splashRadius=2;StringAssert.Contains("explosão de 2 m",ItemDescriptions.Pattern(attack));
            }finally{UnityEngine.Object.DestroyImmediate(attack);}
        }
        [Test]public void SummaryIdentifiesClassSlotLevelAndNumericalWeaponDamage()
        {
            string text=ItemDescriptions.Summary(catalog,rare);
            StringAssert.Contains("Arcanista",text);StringAssert.Contains("Arma principal",text);StringAssert.Contains("nível "+staff.minimumLevel,text);
            StringAssert.Contains(Mathf.RoundToInt(staff.attacks[0].damage*catalog.Rarity(rare.rarity).power)+" dano da arma",text);
            StringAssert.DoesNotContain("SEQUÊNCIA DE ATAQUES",text);
        }
        [Test]public void QuickComparisonUsesTheRealAttributeFormulaWithoutMutatingTheProfile()
        {
            var profile=channel.LocalProfile;string before=JsonUtility.ToJson(profile);var hero=session.Actor(0).Roster.Current;
            var a=ProgressionStats.Calculate(hero,profile,catalog);var b=ProgressionStats.Calculate(hero,profile,catalog,rare.instanceId,EquipmentSlot.MainHand,true);
            string text=ItemDescriptions.QuickPreview(hero,profile,catalog,rare);
            StringAssert.Contains("Dano: "+a.damage+" → "+b.damage,text);Assert.AreEqual(before,JsonUtility.ToJson(profile));
            StringAssert.Contains("Incompatível",ItemDescriptions.QuickPreview(hero,profile,catalog,otherClass));
        }
        [Test]public void SummaryAndAdvancedDetailsAreConnectedToAWorkingToggle()
        {
            Assert.AreEqual(2,VisibleRows());Button("InventoryItem_0").onClick.Invoke();
            StringAssert.Contains("ONDE EQUIPA",Own.text);StringAssert.DoesNotContain("SEQUÊNCIA DE ATAQUES",Own.text);
            Button("ToggleAdvanced").onClick.Invoke();StringAssert.Contains("SEQUÊNCIA DE ATAQUES",Own.text);
            Button("ToggleAdvanced").onClick.Invoke();StringAssert.DoesNotContain("SEQUÊNCIA DE ATAQUES",Own.text);
        }
        [UnityTest]public IEnumerator EquippingMovesThePieceOutOfTheBagAndUnequippingReturnsIt()
        {
            Button("InventoryItem_0").onClick.Invoke();Button("EQUIPAR").onClick.Invoke();yield return null;
            Assert.AreEqual(rare.instanceId,channel.LocalProfile.Loadout("arcanist").items[0],channel.Message);
            // The old starter returns to the bag; the chosen item only appears in its slot.
            Assert.AreEqual(1,Root.GetComponentsInChildren<Button>(true).Count(x=>x.name.StartsWith("InventoryItem_")&&x.gameObject.activeSelf&&x.GetComponentInChildren<Text>().text==staff.displayName));
            var store=(ProfileStore)typeof(ProfileService).GetField("store",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(channel.Hub.Service);
            var reloaded=new ProfileService(catalog,new ProfileStore(store.Path));Assert.AreEqual(rare.instanceId,reloaded.Profile.Loadout("arcanist").items[0]);
            Assert.IsTrue(Button("DESEQUIPAR").interactable);Button("DESEQUIPAR").onClick.Invoke();yield return null;
            Assert.IsTrue(string.IsNullOrEmpty(channel.LocalProfile.Loadout("arcanist").items[0]));Assert.IsNotNull(channel.LocalProfile.Item(rare.instanceId));
        }
        [UnityTest]public IEnumerator RecyclingRequiresConfirmationAndGrantsTheDisplayedGoldOnce()
        {
            int coins=channel.LocalProfile.coins;int value=InventoryRules.SellValue(catalog,common);
            Button("InventoryItem_1").onClick.Invoke();Button("DESCARTAR").onClick.Invoke();yield return null;
            Assert.IsNotNull(channel.LocalProfile.Item(common.instanceId));Assert.AreEqual(coins,channel.LocalProfile.coins);
            StringAssert.Contains("CONFIRMAR",Button("DESCARTAR").GetComponentInChildren<Text>().text);
            StringAssert.Contains("ouro",Root.Find("ItemComparison/EquipmentNotice").GetComponent<Text>().text);
            Button("DESCARTAR").onClick.Invoke();yield return null;
            Assert.IsNull(channel.LocalProfile.Item(common.instanceId),channel.Message);Assert.AreEqual(coins+value,channel.LocalProfile.coins);
        }
        [UnityTest]public IEnumerator ReceivingADeliveryOpensItInTheBagWithoutAutomaticallyEquipping()
        {
            var delivery=NewItem(staff,ItemRarity.Epic);channel.LocalProfile.deliveries.Add(delivery);
            Button("DeliverySource").onClick.Invoke();Button("InventoryItem_0").onClick.Invoke();
            StringAssert.Contains("receba",Own.text);Button("EQUIPAR").onClick.Invoke();yield return null;yield return null;
            Assert.IsFalse(channel.LocalProfile.deliveries.Any(x=>x.instanceId==delivery.instanceId));
            Assert.IsNotNull(channel.LocalProfile.Item(delivery.instanceId));Assert.IsFalse(InventoryRules.Equipped(channel.LocalProfile,delivery.instanceId));
            Assert.IsFalse(Button("BagSource").interactable);Assert.IsTrue(Root.Find("ItemComparison").gameObject.activeSelf||Root.Find("SelectedItemColumn")?.gameObject.activeSelf==true);
            StringAssert.Contains(staff.displayName,Own.text);
        }
        [Test]public void EmptyCategoryExplainsTheFilterAndCanShowAllItems()
        {
            Button("ARTEFATO").onClick.Invoke();Assert.AreEqual(0,VisibleRows());
            Assert.IsTrue(Root.Find("EmptyInventory").gameObject.activeSelf);Assert.IsTrue(Button("ClearInventoryFilters").gameObject.activeSelf);
            Button("ClearInventoryFilters").onClick.Invoke();Assert.AreEqual(3,VisibleRows());
            Assert.IsTrue(Root.GetComponentsInChildren<Text>(true).Any(x=>x.text=="OUTRA CLASSE"));
        }
        [Test]public void EmptyEquipmentSlotFiltersTheCorrectCategoryAndNamesRemainIcons()
        {
            Button("EquipmentSlot_2").onClick.Invoke();Assert.IsFalse(Button("ARMADURA").interactable);
            StringAssert.Contains("ARMADURA",Button("EquipmentSlot_2").GetComponentInChildren<Text>().text);
            Assert.IsTrue(Button("EquipmentSlot_0").transform.Find("EquippedItemIcon").GetComponent<Image>().enabled);
        }
        [Test]public void OffHandItemIsRecognizedAsEquippedRatherThanShownAsAnInvalidMainHand()
        {
            session.SelectHero(1);var hero=session.Actor(0).Roster.Current;
            var dagger=catalog.equipment.First(x=>x.category==WeaponCategory.Daggers&&!x.starter);
            var main=NewItem(dagger);var item=NewItem(dagger);var copy=JsonUtility.FromJson<PlayerProfile>(JsonUtility.ToJson(channel.LocalProfile));
            copy.inventory.Add(main);copy.inventory.Add(item);copy.Loadout("ranger").items[0]=main.instanceId;copy.Loadout("ranger").items[1]=item.instanceId;
            Assert.IsTrue(ProgressionStats.CanEquip(copy,catalog,"ranger",item,EquipmentSlot.OffHand,out string reason),reason);
            Assert.IsTrue(InventoryRules.EquippedBy(copy,"ranger",item.instanceId));
            StringAssert.Contains("já estão",ItemDescriptions.QuickPreview(hero,copy,catalog,item));
            StringAssert.Contains("Equipado neste herói",ItemDescriptions.EquipmentState(copy,catalog,hero,item));
        }
        [Test]public void BrowsingSourcesFiltersAndComparisonsDoesNotChangeSavedItemData()
        {
            string before=JsonUtility.ToJson(channel.LocalProfile);
            Button("DeliverySource").onClick.Invoke();Button("BagSource").onClick.Invoke();
            Button("InventoryItem_0").onClick.Invoke();Button("CompareOther").onClick.Invoke();Button("CompareEquipped").onClick.Invoke();Button("ToggleAdvanced").onClick.Invoke();
            Assert.AreEqual(before,JsonUtility.ToJson(channel.LocalProfile));
        }
        [Test]public void LongNamesRemainAvailableInTheScrollableItemPanel()
        {
            string old=staff.displayName;
            try
            {
                staff.displayName="Cajado ancestral de uma descrição de nome excepcionalmente longa para conferir a leitura completa";
                hud.CloseInventory();hud.OpenInventory();Button("InventoryItem_0").onClick.Invoke();
                StringAssert.Contains(staff.displayName,Own.text);Canvas.ForceUpdateCanvases();
                var popup=Root.Find("ItemComparison").GetComponent<RectTransform>();Assert.LessOrEqual(popup.sizeDelta.x,Root.GetComponent<RectTransform>().sizeDelta.x);
                Assert.IsNotNull(Own.GetComponentInParent<ScrollRect>(true));
                var inline=Root.Find("SelectedItemColumn/ItemInformationScroll/Content/Information");
                if(inline!=null){StringAssert.Contains(staff.displayName,inline.GetComponent<Text>().text);Assert.IsNotNull(inline.GetComponentInParent<ScrollRect>());}
            }finally{staff.displayName=old;}
        }
    }
}
