using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class RegionalInventoryTests
    {
        RegionalInventory inventory;CoopInventoryHud hud;string directory;
        [UnitySetUp]public IEnumerator Load()
        {
            directory=Path.Combine(Application.temporaryCachePath,"RegionalInventoryFixture",Guid.NewGuid().ToString("N"));RegionalInventory.TestDirectory=directory;
            yield return SceneManager.LoadSceneAsync("RefugioUltimoSino");yield return null;yield return null;yield return null;
            inventory=UnityEngine.Object.FindFirstObjectByType<RegionalInventory>();hud=UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>();Assert.NotNull(inventory);Assert.NotNull(hud);
        }
        [UnityTearDown]public IEnumerator Close(){if(hud!=null)hud.CloseInventory();RegionalInventory.TestDirectory=null;yield return null;}
        Button Button(string name)=>GameObject.Find("CoopInventory").GetComponentsInChildren<Button>(true).Single(b=>b.name==name);
        [UnityTest]public IEnumerator MerchantFacesCustomerAndSavesPurchase()
        {
            var merchant=UnityEngine.Object.FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None)
                .Single(target=>target.name.StartsWith("Téo"));
            var hero=inventory.World.Hero;
            hero.GetComponent<PlayerMotor>().Teleport(merchant.transform.position+Vector3.back,Quaternion.identity);
            inventory.Service.Profile.coins=50;
            int before=inventory.Service.Profile.inventory.Count;
            Assert.IsTrue(merchant.TryUse(hero.transform));
            yield return null;
            var regionalHud=UnityEngine.Object.FindFirstObjectByType<RegionalWorldHud>();
            Assert.IsTrue(regionalHud.ShopOpen);
            Assert.Greater(Vector3.Dot(merchant.transform.forward,Vector3.back),.95f);
            var buy=GameObject.Find("Buy0").GetComponent<Button>();
            Assert.IsTrue(buy.interactable);
            buy.onClick.Invoke();
            Assert.AreEqual(5,inventory.Service.Profile.coins);
            Assert.AreEqual(before+1,inventory.Service.Profile.inventory.Count);
            Assert.IsFalse(buy.interactable);
            var saved=new ProfileService(inventory.Catalog,new ProfileStore(inventory.SavePath));
            Assert.AreEqual(5,saved.Profile.coins);
            Assert.AreEqual(before+1,saved.Profile.inventory.Count);
            regionalHud.CloseShop();
        }

        [UnityTest]public IEnumerator NewHeroReceivesFirstWeaponFromDaro()
        {
            var hero=inventory.World.Hero;
            var combat=hero.GetComponent<HeroCombat>();
            Assert.IsFalse(combat.CanUseWeapon);
            Assert.IsFalse(combat.TryAttack());
            Assert.IsFalse(combat.TrySpecial());
            var daro=UnityEngine.Object.FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None)
                .Single(target=>target.name.StartsWith("Daro"));
            hero.GetComponent<PlayerMotor>().Teleport(daro.transform.position+Vector3.back,Quaternion.identity);
            yield return null;
            Assert.IsTrue(daro.TryUse(hero.transform));
            Assert.IsTrue(combat.CanUseWeapon);
            int count=inventory.Service.Profile.inventory.Count;
            daro.TryUse(hero.transform);
            Assert.AreEqual(count,inventory.Service.Profile.inventory.Count);
        }

        ItemInstance AddStaff()
        {
            var staff=inventory.Catalog.equipment.First(d=>d.category==WeaponCategory.Staff&&!d.starter&&d.minimumLevel<=1);
            var item=LootGenerator.Create(inventory.Catalog,staff.itemId,ItemRarity.Rare,new System.Random(42));inventory.Service.Profile.inventory.Add(item);inventory.SetNotice("Item de teste");return item;
        }
        [UnityTest]public IEnumerator ExistingInventoryOpensInWorldAndBlocksCommandsUntilClosed()
        {
            var input=inventory.World.Hero.GetComponent<ArenaInput>();input.SetTouchMove(Vector2.right,true);
            var button=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name=="RegionalInventoryButton");button.onClick.Invoke();yield return null;
            Assert.IsTrue(hud.IsOpen);Assert.IsTrue(input.CombatBlocked);Assert.AreEqual(Vector2.zero,input.Move);
            hud.NavigateProfile(2);Assert.AreEqual(2,hud.ProfileArea);hud.CloseInventory();yield return null;Assert.IsFalse(input.CombatBlocked);Assert.AreEqual(Vector2.zero,input.Move);
        }
        [UnityTest]public IEnumerator KeyboardOpensAndEscapeClosesInventory()
        {
            var old=InputSystem.settings.backgroundBehavior;var editor=InputSystem.settings.editorInputBehaviorInPlayMode;Keyboard keyboard=null;
            try
            {
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.EnableDevice(keyboard);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.I));InputSystem.Update();yield return null;Assert.IsTrue(hud.IsOpen);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));InputSystem.Update();yield return null;Assert.IsFalse(hud.IsOpen);Assert.IsFalse(inventory.World.Hero.GetComponent<ArenaInput>().CombatBlocked);
            }
            finally{if(keyboard!=null)InputSystem.RemoveDevice(keyboard);InputSystem.settings.backgroundBehavior=old;InputSystem.settings.editorInputBehaviorInPlayMode=editor;}
        }
        [UnityTest]public IEnumerator UiEquipAndUnequipChangeTheRealHeroAndPersist()
        {
            var item=AddStaff();var definition=inventory.Catalog.Find(item.definitionId);hud.OpenInventory();yield return null;
            var row=GameObject.Find("CoopInventory").GetComponentsInChildren<Button>(true).First(b=>b.name.StartsWith("InventoryItem_")&&b.gameObject.activeSelf&&b.GetComponentInChildren<Text>().text==definition.displayName);row.onClick.Invoke();
            Button("EQUIPAR").onClick.Invoke();yield return null;
            var gear=inventory.World.Hero.GetComponent<HeroEquipment>();Assert.AreEqual(definition,gear.MainWeapon,inventory.Notice);
            Assert.AreEqual(item.instanceId,inventory.Service.Profile.Loadout("arcanist").items[0]);
            var loaded=new ProfileService(inventory.Catalog,new ProfileStore(inventory.SavePath));Assert.AreEqual(item.instanceId,loaded.Profile.Loadout("arcanist").items[0]);
            Assert.AreEqual(ProgressionStats.Calculate(inventory.World.Hero.Current,inventory.Service.Profile,inventory.Catalog).maximumHealth,inventory.World.Hero.Health.Maximum);
            Button("DESEQUIPAR").onClick.Invoke();yield return null;Assert.IsNull(gear.MainWeapon);Assert.IsTrue(string.IsNullOrEmpty(inventory.Service.Profile.Loadout("arcanist").items[0]));Assert.IsNotNull(inventory.Service.Profile.Item(item.instanceId));
        }
        [UnityTest]public IEnumerator TravellingRetainsItemsAndDoesNotGrantStartersTwice()
        {
            var item=AddStaff();inventory.Request(CoopInventoryAction.Equip,item.instanceId,EquipmentSlot.MainHand,false,0);
            string path=inventory.SavePath;int count=inventory.Service.Profile.inventory.Count,coins=inventory.Service.Profile.coins;
            yield return SceneManager.LoadSceneAsync("CostaAto1");yield return null;yield return null;yield return null;
            inventory=UnityEngine.Object.FindFirstObjectByType<RegionalInventory>();hud=UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>();
            Assert.AreEqual(path,inventory.SavePath);Assert.AreEqual(count,inventory.Service.Profile.inventory.Count);Assert.AreEqual(coins,inventory.Service.Profile.coins);Assert.AreEqual(item.instanceId,inventory.Service.Profile.Loadout("arcanist").items[0]);
            Assert.AreEqual(CoopRewardLedger.ParticipantPath(directory,"11111111111111111111111111111111"),inventory.SavePath);
        }
        [UnityTest]public IEnumerator RegionalUsesExactlyTheCampaignCatalog()
        {
            var expected=inventory.Catalog;Assert.IsTrue(expected.equipment.Any(e=>e.visualMesh!=null));
            yield return SceneManager.LoadSceneAsync("CampaignCoop");yield return null;yield return null;
            Assert.AreSame(expected,UnityEngine.Object.FindFirstObjectByType<CoopProfileChannel>().Catalog);
            inventory=null;hud=UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>();
        }
        [UnityTest]public IEnumerator DeadHeroSeesWhyEquipmentIsBlocked()
        {
            AddStaff();inventory.World.Hero.Health.ApplyDamage(100000);hud.OpenInventory();yield return null;
            var texts=GameObject.Find("CoopInventory").GetComponentsInChildren<Text>(true);
            Assert.IsTrue(texts.Any(t=>t.text.Contains("Reapareça")),"A UI deve explicar o bloqueio, não apenas desativar botões.");
            hud.CloseInventory();
        }
        [Test]public void ReadyProfileHasNoBlockingReason()
        {Assert.IsTrue(inventory.CanManage,inventory.BlockReason);Assert.AreEqual("",inventory.BlockReason);}
        [Test]public void DeadHeroCannotMutateEquipment()
        {
            var item=AddStaff();inventory.World.Hero.Health.ApplyDamage(100000);string before=JsonUtility.ToJson(inventory.Service.Profile);
            inventory.Request(CoopInventoryAction.Equip,item.instanceId,EquipmentSlot.MainHand,false,0);Assert.AreEqual(before,JsonUtility.ToJson(inventory.Service.Profile));StringAssert.Contains("Reapareça",inventory.Notice);
        }
    }
}

