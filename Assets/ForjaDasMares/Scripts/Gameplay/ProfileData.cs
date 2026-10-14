using System;
using System.Collections.Generic;
namespace ForjaDasMares
{
    [Serializable] public sealed class ItemInstance
    {
        public string instanceId, definitionId;
        public ItemRarity rarity;
        public List<StatModifier> modifiers = new List<StatModifier>();
    }
    [Serializable] public sealed class HeroLoadout
    { public string heroId; public string[] items = new string[5]; }
    [Serializable] public sealed class LocalOptions
    { public float volume = .8f; public bool companionEnabled = true, largeText; }
    [Serializable] public sealed class ExpeditionData
    {
        public string runId, heroId, location;
        public int experience, coins;
        public List<ItemInstance> items = new List<ItemInstance>();
        public List<string> events = new List<string>();
    }
    [Serializable] public sealed class ExpeditionResult
    {
        public string runId, outcome, location;
        public int experience, coins;
        public List<ItemInstance> items = new List<ItemInstance>();
    }
    [Serializable] public sealed class PlayerProfile
    {
        public int version = 2, experience, coins;
        public List<ItemInstance> inventory = new List<ItemInstance>();
        public List<ItemInstance> deliveries = new List<ItemInstance>();
        public List<HeroLoadout> loadouts = new List<HeroLoadout>();
        public List<string> completedRuns = new List<string>();
        public ExpeditionData expedition;
        public ExpeditionResult lastResult;
        public LocalOptions options = new LocalOptions();
        public ItemInstance Item(string id) { foreach (var item in inventory) if (item.instanceId == id) return item; return null; }
        public HeroLoadout Loadout(string heroId) { foreach (var set in loadouts) if (set.heroId == heroId) return set; return null; }
    }
    internal static class ProfileSerialization
    {
        // JsonUtility materializes null custom objects as empty objects. Preserve presence explicitly.
        public static PlayerProfile Clone(PlayerProfile source)
        {
            var copy = UnityEngine.JsonUtility.FromJson<PlayerProfile>(UnityEngine.JsonUtility.ToJson(source));
            if (source.expedition == null) copy.expedition = null;
            if (source.lastResult == null) copy.lastResult = null;
            return copy;
        }
        public static void NormalizeLegacyNulls(PlayerProfile profile)
        {
            var run = profile.expedition;
            if (run != null && string.IsNullOrEmpty(run.runId) && run.coins == 0 && run.experience == 0 && (run.items == null || run.items.Count == 0) && (run.events == null || run.events.Count == 0)) profile.expedition = null;
            var result = profile.lastResult;
            if (result != null && string.IsNullOrEmpty(result.runId) && result.coins == 0 && result.experience == 0 && (result.items == null || result.items.Count == 0)) profile.lastResult = null;
        }
    }
}
