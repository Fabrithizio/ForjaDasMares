using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class CoopRewardLedgerTests
    {
        ProgressionCatalog catalog;
        CoopRewardLedger ledger;
        string directory;
        readonly ProfileService[] profiles = new ProfileService[4];
        readonly string[] paths = new string[4];
        readonly string[] heroes = { "warrior", "ranger", "arcanist", "rune-knight" };
        [SetUp] public void Setup()
        {
#if UNITY_EDITOR
            catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ProgressionCatalog>("Assets/ForjaDasMares/Data/Progression/Catalog.asset");
#else
            Assert.Ignore("This fixture uses the real editor catalog; player process tests cover runtime integration.");
#endif
            Assert.IsNotNull(catalog); directory = Path.Combine(Application.temporaryCachePath, "CoopRewards", Guid.NewGuid().ToString("N"));
            ledger = new CoopRewardLedger(catalog);
            for (int i = 0; i < 4; i++)
            { paths[i] = CoopRewardLedger.ParticipantPath(directory, Guid.NewGuid().ToString("N")); profiles[i] = new ProfileService(catalog, new ProfileStore(paths[i])); ledger.Assign(i, profiles[i]); }
        }
        [TearDown] public void Cleanup()
        {
            string prefix = Path.GetFullPath(Path.Combine(Application.temporaryCachePath, "CoopRewards")) + Path.DirectorySeparatorChar;
            if (directory != null && Path.GetFullPath(directory).StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && Directory.Exists(directory)) Directory.Delete(directory, true);
        }
        void Begin() => Assert.IsTrue(ledger.Begin(heroes, "Masmorra cooperativa"), ledger.Error);
        [Test] public void FourParticipantsReceiveBossAndCompletionOnceAcrossReload()
        {
            Begin(); Assert.IsTrue(ledger.Grant("boss:1", catalog.boss, 99)); Assert.IsTrue(ledger.Grant("boss:1", catalog.boss, 99));
            Assert.IsTrue(ledger.Grant("completion", catalog.completion, 100)); Assert.IsTrue(ledger.Finish(true, "Vitória")); Assert.IsFalse(ledger.Finish(true, "Duplicada"));
            for (int i = 0; i < 4; i++)
            {
                var loaded = new ProfileService(catalog, new ProfileStore(paths[i]));
                Assert.AreEqual(catalog.boss.experience + catalog.completion.experience, loaded.Profile.experience);
                Assert.AreEqual(catalog.boss.coins + catalog.completion.coins, loaded.Profile.coins);
                Assert.AreEqual(3, loaded.Profile.lastResult.items.Count); Assert.AreEqual(1, loaded.Profile.completedRuns.Count);
                Assert.AreEqual(catalog.starterItems.Length + 3, loaded.Profile.inventory.Count);
            }
        }
        [Test] public void PartialDiskFailureRetriesOnlyUncommittedParticipants()
        {
            Begin(); Directory.CreateDirectory(paths[3] + ".tmp");
            Assert.IsFalse(ledger.Grant("chest:3", catalog.chest, 17));
            for (int i = 0; i < 3; i++) { Assert.AreEqual(1, profiles[i].Profile.expedition.items.Count); Assert.AreEqual(1, profiles[i].Profile.expedition.events.Count); }
            Assert.AreEqual(0, profiles[3].Profile.expedition.items.Count);
            Directory.Delete(paths[3] + ".tmp"); Assert.IsTrue(ledger.Grant("chest:3", catalog.chest, 17));
            for (int i = 0; i < 4; i++) { Assert.AreEqual(1, profiles[i].Profile.expedition.items.Count); Assert.AreEqual(catalog.chest.coins, profiles[i].Profile.expedition.coins); }
        }
        [Test] public void PartialCompletionRetryCannotDuplicateCurrencyOrItems()
        {
            Begin(); Assert.IsTrue(ledger.Grant("room:1", catalog.room, 8)); Directory.CreateDirectory(paths[3] + ".tmp");
            Assert.IsFalse(ledger.Finish(true, "Vitória")); Assert.IsTrue(ledger.Active);
            Directory.Delete(paths[3] + ".tmp"); Assert.IsTrue(ledger.Finish(true, "Vitória"));
            foreach (var profile in profiles) { Assert.AreEqual(catalog.room.coins, profile.Profile.coins); Assert.AreEqual(catalog.room.experience, profile.Profile.experience); Assert.AreEqual(1, profile.Profile.completedRuns.Count); }
        }
        [Test] public void AuthorityRejectsArbitraryRewardRulesAndParticipantReplacement()
        {
            Begin(); Assert.IsFalse(ledger.Grant("fake", new RewardRule { experience = 100000, coins = 1000000 }, 1));
            Assert.IsFalse(ledger.Grant(new string('x', 129), catalog.enemy, 1));
            Assert.Throws<InvalidOperationException>(() => ledger.Assign(0, profiles[1]));
            foreach (var profile in profiles) Assert.AreEqual(0, profile.Profile.expedition.experience);
        }
        [Test] public void DefeatKeepsItemsAndHalvesExperienceAndCoinsForEveryParticipant()
        {
            Begin(); Assert.IsTrue(ledger.Grant("chest:1", catalog.chest, 9)); Assert.IsTrue(ledger.Finish(false, "Derrota"));
            foreach (var profile in profiles)
            { Assert.AreEqual(catalog.chest.experience / 2, profile.Profile.experience); Assert.AreEqual(catalog.chest.coins / 2, profile.Profile.coins); Assert.AreEqual(1, profile.Profile.lastResult.items.Count); }
        }
        [Test] public void ParticipantPathsRejectTraversalAndRemainInsideOwnedDirectory()
        {
            Assert.Throws<ArgumentException>(() => CoopRewardLedger.ParticipantPath(directory, "../../other"));
            string path = CoopRewardLedger.ParticipantPath(directory, "11111111111111111111111111111111");
            Assert.IsTrue(Path.GetFullPath(path).StartsWith(Path.GetFullPath(directory) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
        }
    }
}
