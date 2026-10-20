using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class RpgTests
    {
        CoopDirector session;CoopProgression progression;CoopProfileChannel channel;ProgressionCatalog catalog;
        [UnitySetUp]public IEnumerator Load(){SceneManager.LoadScene("CampaignCoop");yield return null;yield return null;session=UnityEngine.Object.FindFirstObjectByType<CoopDirector>();progression=session.GetComponent<CoopProgression>();channel=session.GetComponent<CoopProfileChannel>();catalog=progression.Catalog;}
        [UnityTearDown]public IEnumerator Close(){session.Close();yield return null;yield return null;}
        [UnityTest]public IEnumerator BagExcludesEquippedAndUnequipRestoresExactlyOne()
        {
            var p=channel.LocalProfile;Assert.AreEqual(0,InventoryRules.BagCount(p));int owned=p.inventory.Count;string id=p.Loadout("warrior").items[0];
            channel.Request(CoopInventoryAction.Unequip,null,EquipmentSlot.MainHand);Assert.AreEqual(1,InventoryRules.BagCount(channel.LocalProfile));
            channel.Request(CoopInventoryAction.Equip,id,EquipmentSlot.MainHand);Assert.AreEqual(0,InventoryRules.BagCount(channel.LocalProfile));Assert.AreEqual(owned,channel.LocalProfile.inventory.Count);yield return null;
        }
        [UnityTest]public IEnumerator ImmediateRewardsAreUniqueAndSurviveDeathAndRestart()
        {
            Assert.IsTrue(session.HostDirect("RPG1",7891,false));yield return null;session.StartGame();yield return null;var service=progression.Profile(0);string run=service.Profile.expedition.runId;int coins=service.Profile.coins,xp=service.Profile.experience;
            Assert.IsTrue(service.Grant(run,"unique-kill",catalog.enemy,45));Assert.IsFalse(service.Grant(run,"unique-kill",catalog.enemy,45));Assert.AreEqual(coins+catalog.enemy.coins,service.Profile.coins);Assert.AreEqual(xp+catalog.enemy.experience,service.Profile.experience);
            Assert.IsTrue(service.FinishExpedition(run,false,"Teste"));Assert.AreEqual(coins+catalog.enemy.coins,service.Profile.coins);Assert.AreEqual(xp+catalog.enemy.experience,service.Profile.experience);
            var reopened=new ProfileService(catalog,new ProfileStore(progression.SavePath(0)));Assert.AreEqual(service.Profile.coins,reopened.Profile.coins);Assert.AreEqual(service.Profile.experience,reopened.Profile.experience);
        }
        [UnityTest]public IEnumerator TalentPointsAffectRealStatsAndCostGoldToReset()
        {
            Assert.IsTrue(session.HostDirect("RPG2",7892,false));yield return null;var service=progression.Profile(0);var actor=session.Actor(0);service.Profile.experience=ProgressionStats.RequiredExperience(1);service.Profile.coins=100;
            var before=ProgressionStats.Calculate(actor.Roster.Current,service.Profile,catalog);Assert.AreEqual(1,HeroTalents.Available(service.Profile,"warrior",catalog));
            Assert.IsTrue(service.SpendTalent("warrior",TalentId.Vitality));Assert.IsFalse(service.SpendTalent("warrior",TalentId.Might));Assert.IsFalse(service.SpendTalent("warrior",(TalentId)99));
            Assert.AreEqual(before.maximumHealth+8,ProgressionStats.Calculate(actor.Roster.Current,service.Profile,catalog).maximumHealth);Assert.AreEqual(before.maximumHealth+8,actor.Health.Maximum);
            Assert.IsFalse(service.ResetTalents("warrior",false));Assert.IsTrue(service.ResetTalents("warrior",true));Assert.AreEqual(75,service.Profile.coins);Assert.AreEqual(1,HeroTalents.Available(service.Profile,"warrior",catalog));Assert.IsFalse(service.ResetTalents("warrior",true));
            Assert.IsTrue(service.SpendTalent("warrior",TalentId.Might));var reopen=new ProfileService(catalog,new ProfileStore(progression.SavePath(0)));Assert.AreEqual(1,HeroTalents.Rank(reopen.Profile,"warrior",TalentId.Might));
        }
        [UnityTest]public IEnumerator RecyclePaysOnlyOnceAndNeverSellsEquippedItems()
        {
            Assert.IsTrue(session.HostDirect("RPG3",7893,false));yield return null;var service=progression.Profile(0);string id=service.Profile.Loadout("warrior").items[0];int gold=service.Profile.coins;int price=InventoryRules.SellValue(catalog,service.Profile.Item(id));
            Assert.IsFalse(service.Discard(id,true));Assert.IsTrue(service.Unequip("warrior",EquipmentSlot.MainHand));Assert.IsFalse(service.Discard(id,false));Assert.IsTrue(service.Discard(id,true));Assert.AreEqual(gold+price,service.Profile.coins);Assert.IsFalse(service.Discard(id,true));Assert.AreEqual(gold+price,service.Profile.coins);
        }
        [UnityTest]public IEnumerator LiveEnemyDeathDropsImmediatelyAndPoolIsBounded()
        {
            Assert.IsTrue(session.HostDirect("RPG4",7894,false));yield return null;session.StartGame();yield return null;var service=progression.Profile(0);var pool=session.Dungeon.Encounter.Enemies;var def=Enumerable.Range(0,pool.Capacity).Select(pool.DefinitionAt).First(x=>x.kind==EnemyKind.Raider);int coins=service.Profile.coins;var enemy=pool.TrySpawn(def,session.Actor(0).transform.position+Vector3.forward*4);Assert.NotNull(enemy);enemy.Health.ApplyDamage(100000);
            Assert.AreEqual(coins+catalog.enemy.coins,service.Profile.coins);enemy.Health.ApplyDamage(100000);Assert.AreEqual(coins+catalog.enemy.coins,service.Profile.coins);Assert.That(session.GetComponent<CampaignLootDrops>().ActiveCount,Is.InRange(1,32));yield return new WaitForSeconds(1.3f);Assert.AreEqual(0,session.GetComponent<CampaignLootDrops>().ActiveCount);
        }
        [Test]public void DifficultyRewardsAndItemDescriptionsHaveRealData()
        {
            Assert.Greater(catalog.challengeEnemy.itemChance,catalog.veteranEnemy.itemChance);Assert.Greater(catalog.veteranEnemy.itemChance,catalog.enemy.itemChance);Assert.AreEqual(ItemRarity.Uncommon,catalog.challengeEnemy.minimumRarity);Assert.That(catalog.summonedEnemy.coins,Is.GreaterThan(0));
            var definition=catalog.equipment.First(x=>x.slot==EquipmentSlot.Artifact);var item=LootGenerator.Create(catalog,definition.itemId,ItemRarity.Rare,new System.Random(22));string description=ItemDescriptions.Describe(catalog,item);Assert.IsTrue(description.Contains(definition.displayName));foreach(var b in definition.bonuses)Assert.IsTrue(description.Contains(ItemDescriptions.Name(b.stat)));Assert.IsTrue(description.Contains("Reciclagem"));Assert.AreEqual(10,ItemDescriptions.Bonuses(catalog,item).Length);
        }
        [UnityTest]public IEnumerator InventoryDetailsAndTalentButtonsOpenRealPanels()
        {
            var hud=UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>();hud.OpenInventory();yield return null;
            var all=hud.GetComponentsInChildren<Button>(true);var slot=all.First(x=>x.name=="EquipmentSlot_0");slot.onClick.Invoke();yield return null;
            var popup=hud.GetComponentsInChildren<Transform>(true).First(x=>x.name=="ItemComparison");Assert.IsTrue(popup.gameObject.activeInHierarchy);Assert.IsTrue(popup.GetComponent<RoundedHudGraphic>().raycastTarget);Assert.AreEqual(1,popup.GetComponent<RoundedHudGraphic>().color.a);Assert.IsTrue(popup.GetComponentsInChildren<Text>().Any(x=>x.text.Contains("Reciclagem")));
            all.First(x=>x.name=="CloseItem").onClick.Invoke();all.First(x=>x.name=="Talents").onClick.Invoke();yield return null;Assert.IsTrue(hud.GetComponentsInChildren<Transform>(true).First(x=>x.name=="CharacterTalents").gameObject.activeInHierarchy);hud.CloseInventory();Assert.IsFalse(popup.gameObject.activeInHierarchy);
        }

        [UnityTest]public IEnumerator FullBagDoesNotLoseEquipmentWhenUnequipFails()
        {
            Assert.IsTrue(session.HostDirect("RPG5",7895,false));yield return null;var service=progression.Profile(0);var definition=catalog.equipment.First(x=>x.slot==EquipmentSlot.Armor);string id=service.Profile.Loadout("warrior").items[0];
            for(int i=0;i<catalog.inventoryCapacity;i++)service.Profile.inventory.Add(LootGenerator.Create(catalog,definition.itemId,ItemRarity.Common,new System.Random(i)));
            Assert.AreEqual(catalog.inventoryCapacity,InventoryRules.BagCount(service.Profile));Assert.IsFalse(service.Unequip("warrior",EquipmentSlot.MainHand));Assert.AreEqual(id,service.Profile.Loadout("warrior").items[0]);Assert.NotNull(service.Profile.Item(id));
        }
        [UnityTest]public IEnumerator TemporaryEquipmentSurvivesFinishAndOccupiedSlotsRejectDuplicateUse()
        {
            Assert.IsTrue(session.HostDirect("RPG6",7896,false));yield return null;session.StartGame();yield return null;var service=progression.Profile(0);var definition=catalog.equipment.First(x=>x.slot==EquipmentSlot.Armor&&x.compatibleHeroes==HeroAffinity.All);var item=LootGenerator.Create(catalog,definition.itemId,ItemRarity.Common,new System.Random(6));service.Profile.expedition.items.Add(item);
            Assert.IsTrue(service.Equip("warrior",item.instanceId,EquipmentSlot.Armor));Assert.IsFalse(service.Equip("ranger",item.instanceId,EquipmentSlot.Armor));Assert.IsTrue(service.FinishExpedition(service.Profile.expedition.runId,true,"Teste"));Assert.AreEqual(1,service.Profile.inventory.Count(x=>x.instanceId==item.instanceId));Assert.IsTrue(InventoryRules.Equipped(service.Profile,item.instanceId));
        }
        [UnityTest]public IEnumerator ExperienceWithinSameLevelDoesNotRebuildEquipment()
        {
            Assert.IsTrue(session.HostDirect("RPG7",7897,false));yield return null;session.StartGame();yield return null;var actor=session.Actor(0);var service=progression.Profile(0);int updates=0;Action changed=()=>updates++;actor.Health.Changed+=changed;
            Assert.IsTrue(service.Grant(service.Profile.expedition.runId,"small-xp",catalog.enemy,7));actor.Health.Changed-=changed;Assert.AreEqual(0,updates,"Ouro/XP sem mudança de nível não deve reconstruir visuais e atributos.");Assert.AreEqual(catalog.enemy.experience,service.Profile.experience);
        }
        [Test]public void MinimapIsCompactAtTopLeft()
        {
            var map=UnityEngine.Object.FindFirstObjectByType<CampaignMinimap>();var panel=(RectTransform)map.transform.parent;Assert.AreEqual(new Vector2(0,1),panel.anchorMin);Assert.AreEqual(160,panel.rect.width);Assert.AreEqual(165,panel.rect.height);
        }
    }
}
