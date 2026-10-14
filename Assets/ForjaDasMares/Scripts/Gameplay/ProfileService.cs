using System;
using UnityEngine;
namespace ForjaDasMares
{
    public sealed class ProfileService
    {
        readonly ProgressionCatalog catalog;
        readonly ProfileStore store;
        public PlayerProfile Profile { get; private set; }
        public string Message { get; private set; }
        public event Action Changed;
        public ProfileService(ProgressionCatalog data, ProfileStore persistence)
        {
            catalog = data; store = persistence; catalog.Validate();
            Profile = store.Load(catalog, out string notice); Message = notice;
            if (Profile == null)
            {
                Profile = new PlayerProfile(); var random = new System.Random(607);
                foreach (var id in catalog.starterItems) Profile.inventory.Add(LootGenerator.Create(catalog, id, ItemRarity.Common, random));
                store.Save(Profile, catalog);
            }
        }
        bool Transaction(Action<PlayerProfile> mutate, string message)
        {
            var next = ProfileSerialization.Clone(Profile);
            try { mutate(next); store.Save(next, catalog); }
            catch (Exception error) { Message = "Não foi possível salvar: " + error.Message; return false; }
            Profile = next; Message = message; Changed?.Invoke(); return true;
        }
        public bool Equip(string heroId, string itemId, EquipmentSlot slot)
        {
            var item = Profile.Item(itemId); if (!ProgressionStats.CanEquip(Profile, catalog, heroId, item, slot, out string reason)) { Message = reason; return false; }
            return Transaction(next =>
            {
                var set = next.Loadout(heroId); if (set == null) { set = new HeroLoadout { heroId = heroId }; next.loadouts.Add(set); }
                set.items[(int)slot] = itemId;
                if (slot == EquipmentSlot.MainHand)
                {
                    var secondary = next.Item(set.items[1]); if (secondary != null && !ProgressionStats.CanEquip(next, catalog, heroId, secondary, EquipmentSlot.OffHand, out _)) set.items[1] = null;
                }
            }, "Equipado. Secundária incompatível foi guardada no inventário.");
        }
        public bool Unequip(string heroId, EquipmentSlot slot)
        {
            if (Profile.Loadout(heroId) == null) return false;
            return Transaction(next => { var set = next.Loadout(heroId); set.items[(int)slot] = null; if (slot == EquipmentSlot.MainHand) set.items[1] = null; }, "Item guardado no inventário.");
        }
        public bool Discard(string itemId, bool confirmed)
        {
            if (!confirmed) { Message = "Confirme o descarte."; return false; }
            if (Profile.Item(itemId) == null) return false;
            foreach (var set in Profile.loadouts) foreach (var id in set.items) if (id == itemId) { Message = "Desequipe de todos os heróis antes de descartar."; return false; }
            return Transaction(next => next.inventory.RemoveAll(item => item.instanceId == itemId), "Item descartado.");
        }
        public bool ClaimDelivery(string id)
        {
            if (Profile.inventory.Count >= catalog.inventoryCapacity) { Message = "Inventário cheio. Libere um espaço."; return false; }
            if (!Profile.deliveries.Exists(item => item.instanceId == id)) return false;
            return Transaction(next => { var item = next.deliveries.Find(value => value.instanceId == id); next.deliveries.Remove(item); next.inventory.Add(item); }, "Entrega recebida.");
        }
        public bool SetOptions(float volume, bool companion, bool largeText) => Transaction(next => next.options = new LocalOptions { volume = Mathf.Clamp01(volume), companionEnabled = companion, largeText = largeText }, "Opções salvas.");
        public bool BeginExpedition(string heroId, string location)
        {
            if (catalog.Hero(heroId) == null || string.IsNullOrWhiteSpace(location)) return false;
            if (Profile.expedition != null && !FinishExpedition(Profile.expedition.runId, false, "Abandono")) return false;
            if (Profile.inventory.Count + Profile.deliveries.Count > 512 - catalog.expeditionCapacity) { Message = "Estoque de entregas cheio. Receba e libere itens antes de partir."; return false; }
            return Transaction(next => next.expedition = new ExpeditionData { runId = Guid.NewGuid().ToString("N"), heroId = heroId, location = location }, "Expedição iniciada.");
        }
        public bool Grant(string runId, string eventId, RewardRule rule, int seed)
        {
            if (rule != catalog.enemy && rule != catalog.boss && rule != catalog.chest && rule != catalog.room && rule != catalog.completion) return false;
            var run = Profile.expedition;
            if (run == null || run.runId != runId || run.events.Contains(eventId) || string.IsNullOrWhiteSpace(eventId) || eventId.Length > 128 || run.events.Count >= 512) return false;
            var loot = LootGenerator.RewardItems(catalog, rule, new System.Random(seed));
            int capacity = rule == catalog.boss || rule == catalog.completion ? catalog.expeditionCapacity : catalog.expeditionCapacity - 3;
            if (rule == catalog.enemy && loot.Count + run.items.Count > capacity) loot.Clear();
            if (loot.Count + run.items.Count > capacity || Profile.inventory.Count + Profile.deliveries.Count + run.items.Count + loot.Count > 512) { Message = "Espaço de saque cheio; libere espaço antes de abrir outro baú."; return false; }
            return Transaction(next => { var current = next.expedition; current.events.Add(eventId); current.coins += rule.coins; current.experience += rule.experience; current.items.AddRange(loot); }, "Recompensa registrada: " + rule.rewardId);
        }
        public bool FinishExpedition(string runId, bool victory, string outcome)
        {
            if (Profile.expedition == null || Profile.expedition.runId != runId || Profile.completedRuns.Contains(runId)) return false;
            return Transaction(next =>
            {
                var run = next.expedition; int xp = victory ? run.experience : run.experience / 2, coins = victory ? run.coins : run.coins / 2;
                next.experience = Mathf.Min(1000000, next.experience + xp); next.coins = Mathf.Min(100000000, next.coins + coins);
                foreach (var item in run.items) { if (next.inventory.Count < catalog.inventoryCapacity) next.inventory.Add(item); else next.deliveries.Add(item); }
                next.lastResult = new ExpeditionResult { runId = run.runId, location = run.location, outcome = outcome, experience = xp, coins = coins, items = run.items };
                next.completedRuns.Add(run.runId); if (next.completedRuns.Count > 256) next.completedRuns.RemoveAt(0); next.expedition = null;
            }, victory ? "Vitória! Recompensas salvas." : "Expedição encerrada: itens guardados, metade de XP/moedas.");
        }
    }
}
