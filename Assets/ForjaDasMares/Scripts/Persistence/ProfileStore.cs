using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
namespace ForjaDasMares
{
    // This local checksum detects corruption; it does not authenticate an online economy.
    public sealed class ProfileStore
    {
        [Serializable] sealed class Envelope { public int version = 2; public bool hasExpedition, hasLastResult; public string payload, checksum; }
        public string Path { get; }
        public ProfileStore(string path) { Path = System.IO.Path.GetFullPath(path); }
        static string Hash(string value) { using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", ""); }
        public PlayerProfile Load(ProgressionCatalog catalog, out string notice)
        {
            notice = ""; if (!File.Exists(Path) && !File.Exists(Path + ".bak")) return null;
            Exception first = null;
            foreach (string path in new[] { Path, Path + ".bak" })
            {
                if (!File.Exists(path)) continue;
                try
                {
                    var envelope = JsonUtility.FromJson<Envelope>(File.ReadAllText(path, Encoding.UTF8));
                    if (envelope == null || envelope.version < 1 || envelope.version > 2 || envelope.payload == null || Hash(envelope.payload) != envelope.checksum) throw new InvalidDataException("Checksum inválido.");
                    var profile = JsonUtility.FromJson<PlayerProfile>(envelope.payload);
                    if (profile == null || profile.version < 1 || profile.version > 2) throw new InvalidDataException("Versão de salvamento não suportada.");
                    if (envelope.version == 1) ProfileSerialization.NormalizeLegacyNulls(profile);
                    else { if (!envelope.hasExpedition) profile.expedition = null; if (!envelope.hasLastResult) profile.lastResult = null; }
                    if (profile.version == 1) { profile.version = 2; profile.deliveries ??= new List<ItemInstance>(); profile.completedRuns ??= new List<string>(); profile.options ??= new LocalOptions(); }
                    Validate(profile, catalog);
                    if (path != Path) notice = "Salvamento recuperado do backup.";
                    return profile;
                }
                catch (Exception error) { first ??= error; }
            }
            // Preserve damaged files for manual recovery instead of silently overwriting them.
            throw new InvalidDataException("Não foi possível carregar o perfil ou backup. Arquivos preservados em " + Path, first);
        }
        public void Save(PlayerProfile profile, ProgressionCatalog catalog)
        {
            Validate(profile, catalog); string payload = JsonUtility.ToJson(profile);
            var envelope = new Envelope { payload = payload, checksum = Hash(payload), hasExpedition = profile.expedition != null, hasLastResult = profile.lastResult != null };
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)); string temporary = Path + ".tmp";
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(envelope, true));
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None)) { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (File.Exists(Path)) File.Replace(temporary, Path, Path + ".bak"); else File.Move(temporary, Path);
        }
        static void Validate(PlayerProfile profile, ProgressionCatalog catalog)
        {
            if (profile.version != 2 || profile.experience < 0 || profile.experience > 1000000 || profile.coins < 0 || profile.coins > 100000000 || profile.inventory == null || profile.deliveries == null || profile.loadouts == null || profile.completedRuns == null || profile.options == null) throw new InvalidDataException("Perfil inválido.");
            if (profile.inventory.Count > catalog.inventoryCapacity || profile.inventory.Count + profile.deliveries.Count > 512 || profile.completedRuns.Count > 256 || profile.loadouts.Count > catalog.heroes.Length) throw new InvalidDataException("Limites de perfil excedidos.");
            if (float.IsNaN(profile.options.volume) || profile.options.volume < 0 || profile.options.volume > 1) throw new InvalidDataException("Opções inválidas.");
            var ids = new HashSet<string>();
            foreach (var item in profile.inventory) Item(item, catalog, ids);
            foreach (var item in profile.deliveries) Item(item, catalog, ids);
            var heroIds = new HashSet<string>();
            foreach (var set in profile.loadouts)
            {
                if (set == null || !heroIds.Add(set.heroId) || catalog.Hero(set.heroId) == null || set.items == null || set.items.Length != 5) throw new InvalidDataException("Equipamentos inválidos.");
                for (int slot = 0; slot < 5; slot++) if (!string.IsNullOrEmpty(set.items[slot]) && !ProgressionStats.CanEquip(profile, catalog, set.heroId, profile.Item(set.items[slot]), (EquipmentSlot)slot, out _)) throw new InvalidDataException("Equipamento incompatível no salvamento.");
            }
            if (profile.expedition != null)
            {
                var run = profile.expedition;
                if (string.IsNullOrWhiteSpace(run.runId) || catalog.Hero(run.heroId) == null || run.items == null || run.events == null || run.items.Count > catalog.expeditionCapacity || run.events.Count > 512 || run.experience < 0 || run.experience > 100000 || run.coins < 0 || run.coins > 1000000) throw new InvalidDataException("Expedição inválida.");
                foreach (var item in run.items) Item(item, catalog, ids);
                var events = new HashSet<string>(); foreach (var id in run.events) if (string.IsNullOrWhiteSpace(id) || id.Length > 128 || !events.Add(id)) throw new InvalidDataException("Evento duplicado ou inválido.");
            }
            if (profile.lastResult != null && (profile.lastResult.items == null || profile.lastResult.items.Count > catalog.expeditionCapacity)) throw new InvalidDataException("Resultado inválido.");
        }
        static void Item(ItemInstance item, ProgressionCatalog catalog, HashSet<string> ids)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.instanceId) || item.instanceId.Length > 64 || !ids.Add(item.instanceId) || catalog.Find(item.definitionId) == null || !Enum.IsDefined(typeof(ItemRarity), item.rarity) || item.modifiers == null || item.modifiers.Count != catalog.Rarity(item.rarity).affixCount) throw new InvalidDataException("Item inválido ou duplicado.");
            var stats = new HashSet<ItemStat>();
            foreach (var bonus in item.modifiers) if (!Enum.IsDefined(typeof(ItemStat), bonus.stat) || !stats.Add(bonus.stat) || float.IsNaN(bonus.value) || bonus.value < 0 || bonus.value > ProgressionStats.ModifierLimit(bonus.stat)) throw new InvalidDataException("Modificador fora dos limites.");
        }
    }
}
