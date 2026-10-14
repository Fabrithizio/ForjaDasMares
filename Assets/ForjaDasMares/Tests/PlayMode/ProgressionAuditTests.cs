using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;
namespace ForjaDasMares.Tests
{
    public sealed class ProgressionAuditTests
    {
        readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
        ProgressionCatalog catalog;
        ProfileStore store;
        ProfileService service;
        string directory;
        T Asset<T>() where T : ScriptableObject { var value = ScriptableObject.CreateInstance<T>(); created.Add(value); return value; }
        [SetUp] public void Setup()
        {
            directory = Path.Combine(Application.temporaryCachePath, "ProgressionAudit", Guid.NewGuid().ToString("N"));
            store = new ProfileStore(Path.Combine(directory, "profile.json")); catalog = Asset<ProgressionCatalog>(); catalog.inventoryCapacity = 8; catalog.expeditionCapacity = 8;
            catalog.heroes = new[] { new HeroEquipmentRule { heroId = "warrior", affinity = HeroAffinity.Warrior } };
            catalog.affixes = new[] { new AffixRule { stat = ItemStat.Damage, minimum = .01f, maximum = .1f }, new AffixRule { stat = ItemStat.Health, minimum = 4, maximum = 20 }, new AffixRule { stat = ItemStat.Defense, minimum = 2, maximum = 10 } };
            catalog.rarities = new RarityRule[5]; int[] weights = { 55, 30, 10, 4, 1 };
            for (int i = 0; i < 5; i++) catalog.rarities[i] = new RarityRule { rarity = (ItemRarity)i, weight = weights[i], power = 1 + i * .08f, affixCount = Math.Min(i, 3) };
            catalog.equipment = new EquipmentDefinition[12]; var attack = Asset<AttackDefinition>();
            for (int i = 0; i < 12; i++) { var item = Asset<EquipmentDefinition>(); item.itemId = "item" + i; item.displayName = item.itemId; item.category = WeaponCategory.Sword; item.attacks = new[] { attack }; item.allowsOffHand = true; catalog.equipment[i] = item; }
            catalog.equipment[1].slot = EquipmentSlot.OffHand; catalog.equipment[1].category = WeaponCategory.Shield; catalog.equipment[1].bonuses = new[] { new StatModifier(ItemStat.Defense, 20) };
            catalog.equipment[2].category = WeaponCategory.Hammer; catalog.equipment[2].twoHanded = true; catalog.equipment[2].allowsOffHand = false;
            catalog.equipment[3].slot = EquipmentSlot.Armor; catalog.equipment[3].bonuses = new[] { new StatModifier(ItemStat.Health, 40) };
            catalog.starterItems = new[] { "item0", "item1", "item2", "item3" };
            catalog.enemy = new RewardRule { rewardId = "enemy", experience = 12, coins = 2 };
            catalog.boss = new RewardRule { rewardId = "boss", experience = 100, coins = 20, guaranteedItems = 2, minimumRarity = ItemRarity.Rare };
            catalog.chest = new RewardRule { rewardId = "chest", experience = 10, coins = 10, guaranteedItems = 1 };
            catalog.room = new RewardRule { rewardId = "room", experience = 15, coins = 4 };
            catalog.completion = new RewardRule { rewardId = "completion", experience = 40, coins = 15 };
            service = new ProfileService(catalog, store);
        }
        [TearDown] public void Cleanup()
        {
            foreach (var item in created) if (item != null) UnityEngine.Object.DestroyImmediate(item); created.Clear();
            string prefix = Path.GetFullPath(Path.Combine(Application.temporaryCachePath, "ProgressionAudit")) + Path.DirectorySeparatorChar;
            if (directory != null && Path.GetFullPath(directory).StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && Directory.Exists(directory)) Directory.Delete(directory, true);
        }
        string Begin() { Assert.That(service.BeginExpedition("warrior", "Audit"), Is.True); return service.Profile.expedition.runId; }
        string Item(string definition) => service.Profile.inventory.Find(value => value.definitionId == definition).instanceId;
        [Test] public void GrantAndCompletionAreAppliedOnceAndSurviveReload()
        {
            string run = Begin(); Assert.That(service.Grant(run, "boss1", catalog.boss, 13), Is.True); Assert.That(service.Grant(run, "boss1", catalog.boss, 13), Is.False);
            Assert.That(service.FinishExpedition(run, true, "Victory"), Is.True); Assert.That(service.FinishExpedition(run, true, "Victory"), Is.False);
            var loaded = new ProfileService(catalog, new ProfileStore(store.Path)); Assert.That(loaded.Profile.experience, Is.EqualTo(100)); Assert.That(loaded.Profile.coins, Is.EqualTo(20)); Assert.That(loaded.Profile.inventory.Count, Is.EqualTo(6)); Assert.That(loaded.FinishExpedition(run, true, "Replay"), Is.False);
        }
        [Test] public void DeathAndInterruptedExpeditionKeepItemsAndHalfCurrency()
        {
            string run = Begin(); service.Grant(run, "boss", catalog.boss, 20);
            var reloaded = new ProfileService(catalog, new ProfileStore(store.Path)); Assert.That(reloaded.Profile.expedition.items.Count, Is.EqualTo(2));
            Assert.That(reloaded.BeginExpedition("warrior", "Next scene"), Is.True); Assert.That(reloaded.Profile.inventory.Count, Is.EqualTo(6)); Assert.That(reloaded.Profile.experience, Is.EqualTo(50)); Assert.That(reloaded.Profile.coins, Is.EqualTo(10)); Assert.That(reloaded.Profile.lastResult.outcome, Is.EqualTo("Abandono"));
            Assert.That(reloaded.FinishExpedition(run, false, "Duplicate"), Is.False);
        }
        [Test] public void SecondaryRulesAndTwoHandPreviewMatchEffectiveStats()
        {
            Assert.That(service.Equip("warrior", Item("item1"), EquipmentSlot.OffHand), Is.False);
            Assert.That(service.Equip("warrior", Item("item0"), EquipmentSlot.MainHand), Is.True); Assert.That(service.Equip("warrior", Item("item1"), EquipmentSlot.OffHand), Is.True);
            var hero = Asset<HeroDefinition>(); hero.heroId = "warrior";
            var preview = ProgressionStats.Calculate(hero, service.Profile, catalog, Item("item2"), EquipmentSlot.MainHand, true);
            Assert.That(service.Equip("warrior", Item("item2"), EquipmentSlot.MainHand), Is.True); Assert.That(service.Profile.Loadout("warrior").items[1], Is.Null.Or.Empty); Assert.That(service.Profile.inventory.Count, Is.EqualTo(4));
            Assert.That(ProgressionStats.Calculate(hero, service.Profile, catalog).defense, Is.EqualTo(preview.defense)); Assert.That(service.Equip("warrior", Item("item3"), EquipmentSlot.MainHand), Is.False);
        }
        [Test] public void AttributeChangeCannotHealOrResurrect()
        {
            var go = new GameObject("AuditHealth"); created.Add(go); var hp = go.AddComponent<Health>(); hp.Configure(140, CombatFaction.Player); hp.ApplyDamage(20);
            hp.ApplyAttributes(180, .2f); Assert.That(hp.Current, Is.EqualTo(120)); hp.ApplyDamage(9999); hp.ApplyAttributes(220, .3f); Assert.That(hp.Current, Is.Zero); Assert.That(hp.Heal(100), Is.False);
        }
        [Test] public void OverflowDeliveryAndConfirmedDiscardDoNotLoseItems()
        {
            for (int i = 0; i < 3; i++) { string run = Begin(); service.Grant(run, "boss", catalog.boss, i); service.FinishExpedition(run, true, "Victory"); }
            Assert.That(service.Profile.inventory.Count, Is.EqualTo(8)); Assert.That(service.Profile.deliveries.Count, Is.EqualTo(2)); string delivery = service.Profile.deliveries[0].instanceId;
            Assert.That(service.ClaimDelivery(delivery), Is.False); string discard = Item("item3"); Assert.That(service.Discard(discard, false), Is.False); Assert.That(service.Profile.Item(discard), Is.Not.Null);
            Assert.That(service.Discard(discard, true), Is.True); Assert.That(service.ClaimDelivery(delivery), Is.True); Assert.That(service.Profile.inventory.Count, Is.EqualTo(8));
            var loaded = new ProfileService(catalog, new ProfileStore(store.Path)); Assert.That(loaded.Profile.deliveries.Count, Is.EqualTo(1));
        }
        [Test] public void EquippedItemCannotBeDiscarded()
        { string item = Item("item0"); service.Equip("warrior", item, EquipmentSlot.MainHand); Assert.That(service.Discard(item, true), Is.False); service.Unequip("warrior", EquipmentSlot.MainHand); Assert.That(service.Discard(item, true), Is.True); }
        [Test] public void WriteFailureRollsBackTransaction()
        {
            using (var locked = new FileStream(store.Path, FileMode.Open, FileAccess.Read, FileShare.None)) Assert.That(service.BeginExpedition("warrior", "Locked"), Is.False);
            Assert.That(service.Profile.expedition, Is.Null); Assert.That(service.Message, Does.Contain("Não foi possível salvar"));
        }
        [Test] public void DamagedPrimaryRecoversValidBackup()
        {
            string run = Begin(); service.Grant(run, "enemy", catalog.enemy, 1); File.WriteAllText(store.Path, "broken"); var recovered = store.Load(catalog, out string message);
            Assert.That(recovered.expedition.runId, Is.EqualTo(run)); Assert.That(message, Does.Contain("backup")); File.WriteAllText(store.Path + ".bak", "also broken"); Assert.Throws<InvalidDataException>(() => store.Load(catalog, out _));
        }
        [Serializable] sealed class Envelope { public int version = 1; public string payload, checksum; }
        [Test] public void VersionOneMigratesAndFutureVersionIsRejected()
        {
            var profile = service.Profile; profile.version = 1; Write(profile); var loaded = store.Load(catalog, out _); Assert.That(loaded.version, Is.EqualTo(2));
            profile.version = 99; Write(profile); Assert.Throws<InvalidDataException>(() => store.Load(catalog, out _));
        }
        void Write(PlayerProfile profile)
        {
            string payload = JsonUtility.ToJson(profile); string hash; using (var sha = SHA256.Create()) hash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))).Replace("-", "");
            File.WriteAllText(store.Path, JsonUtility.ToJson(new Envelope { payload = payload, checksum = hash }));
        }
        [Test] public void InvalidModifierAndRarityAreRejectedBeforeUse()
        {
            catalog.equipment[0].bonuses = new[] { new StatModifier((ItemStat)90, 1) }; Assert.Throws<InvalidOperationException>(() => catalog.Validate()); catalog.equipment[0].bonuses = Array.Empty<StatModifier>();
            catalog.rarities[1].power = float.NaN; Assert.Throws<InvalidOperationException>(() => catalog.Validate());
        }
        [Test] public void RarityRollsUseWeightsAndDoNotRepeatAffixes()
        {
            var random = new System.Random(18); int[] counts = new int[5];
            for (int i = 0; i < 5000; i++) { var rarity = LootGenerator.RollRarity(catalog, random); counts[(int)rarity]++; var item = LootGenerator.Create(catalog, "item0", rarity, random); var unique = new HashSet<ItemStat>(); foreach (var bonus in item.modifiers) Assert.That(unique.Add(bonus.stat), Is.True); }
            Assert.That(counts[0], Is.InRange(2400, 3100)); Assert.That(counts[4], Is.InRange(20, 100));
            for (int i = 0; i < 100; i++) Assert.That(LootGenerator.RollRarity(catalog, random, ItemRarity.Rare), Is.GreaterThanOrEqualTo(ItemRarity.Rare));
        }
        [Test] public void FullTemporaryLootStillGrantsEnemyExperienceOnce()
        {
            catalog.enemy.itemChance = 1; string run = Begin();
            for (int i = 0; i < 8; i++) Assert.That(service.Grant(run, "enemy" + i, catalog.enemy, i), Is.True);
            Assert.That(service.Profile.expedition.items.Count, Is.EqualTo(5)); Assert.That(service.Profile.expedition.experience, Is.EqualTo(96)); Assert.That(service.Grant(run, "enemy7", catalog.enemy, 7), Is.False);
            Assert.That(service.Grant(run, "boss", catalog.boss, 10), Is.True); Assert.That(service.Profile.expedition.items.Count, Is.EqualTo(7));
        }
        [Test] public void EmptyAbandonPreservesLastMeaningfulResult()
        {
            string run=Begin(); service.Grant(run,"boss",catalog.boss,1); service.FinishExpedition(run,true,"Victory"); string result=service.Profile.lastResult.runId;
            run=Begin(); service.FinishExpedition(run,false,"Abandono"); Assert.That(service.Profile.lastResult.runId,Is.EqualTo(result));
            var loaded=new ProfileService(catalog,new ProfileStore(store.Path)); Assert.That(loaded.Profile.lastResult.runId,Is.EqualTo(result));
        }
    }
}
