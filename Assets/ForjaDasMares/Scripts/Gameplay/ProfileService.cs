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
                // Preserve unreadable originals before making a replacement profile.
                if(System.IO.File.Exists(store.Path))
                {
                    string suffix=".preserved-"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");System.IO.File.Copy(store.Path,store.Path+suffix);
                    if(System.IO.File.Exists(store.Path+".bak"))System.IO.File.Copy(store.Path+".bak",store.Path+".bak"+suffix);
                }
                Profile = new PlayerProfile(); var random = new System.Random(607);
                foreach (var id in catalog.starterItems) Profile.inventory.Add(LootGenerator.Create(catalog, id, ItemRarity.Common, random));
                store.Save(Profile, catalog);
            }
            Profile.starterGranted??=new System.Collections.Generic.List<string>();
            EnsureClassStarters();if(!string.IsNullOrEmpty(notice))Message=notice;
        }
        public void EnsureClassStarters()
        {
            if(catalog.classStarters==null||catalog.classStarters.Length==0)return;
            Profile.starterGranted??=new System.Collections.Generic.List<string>();
            bool missing=false;foreach(var rule in catalog.classStarters)if(!Profile.starterGranted.Contains(rule.heroId))missing=true;
            if(!missing)return;
            Transaction(next=>
            {
                next.starterGranted??=new System.Collections.Generic.List<string>();
                foreach(var rule in catalog.classStarters)
                {
                    if(next.starterGranted.Contains(rule.heroId))continue;
                    if(next.inventory.Count+next.deliveries.Count>=510)continue;
                    var item=LootGenerator.Create(catalog,rule.itemId,ItemRarity.Common,new System.Random(607));
                    bool space=InventoryRules.BagCount(next)<catalog.inventoryCapacity||string.IsNullOrEmpty(next.Loadout(rule.heroId)?.items[0]);
                    if(space)next.inventory.Add(item);else next.deliveries.Add(item);
                    var set=next.Loadout(rule.heroId);if(set==null){set=new HeroLoadout{heroId=rule.heroId};next.loadouts.Add(set);}
                    if(space&&string.IsNullOrEmpty(set.items[0]))set.items[0]=item.instanceId;
                    next.starterGranted.Add(rule.heroId);
                }
            },"Armas comuns de iniciação disponíveis no perfil.");
        }
        bool Transaction(Action<PlayerProfile> mutate, string message)
        {
            var next = ProfileSerialization.Clone(Profile);
            try { mutate(next); store.Save(next, catalog); }
            catch (Exception error) { Message = "Não foi possível salvar: " + error.Message; return false; }
            Profile = next; Message = message; Changed?.Invoke(); return true;
        }
        public bool MarkCampaignClear(int level)
        {if(level<0||level>23)return false;uint bit=1u<<level;if((Profile.campaignCleared&bit)!=0)return true;return Transaction(next=>next.campaignCleared|=bit,"Fase concluída na campanha.");}
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
            }, "Item equipado.");
        }
        public bool Unequip(string heroId, EquipmentSlot slot)
        {
            if (!Enum.IsDefined(typeof(EquipmentSlot),slot)||Profile.Loadout(heroId) == null) return false;
            var current=Profile.Loadout(heroId);
            if(string.IsNullOrEmpty(current.items[(int)slot])){Message="Este slot já está vazio.";return false;}
            int released=0;for(int i=0;i<5;i++)if(i==(int)slot||slot==EquipmentSlot.MainHand&&i==1)
                if(!string.IsNullOrEmpty(current.items[i])&&Profile.inventory.Exists(x=>x.instanceId==current.items[i]))released++;
            bool overflow=InventoryRules.BagCount(Profile)+released>catalog.inventoryCapacity;
            return Transaction(next =>
            {
                var set=next.Loadout(heroId);
                // Remove loadout references first; capacity counts only loose items.
                var primary=next.Item(set.items[(int)slot]);
                var secondary=slot==EquipmentSlot.MainHand?next.Item(set.items[1]):null;
                set.items[(int)slot]=null;if(slot==EquipmentSlot.MainHand)set.items[1]=null;
                // Move existing instances, never clone them or turn temporary loot into
                // permanent rewards. Expedition items keep their original lifecycle.
                foreach(var item in new[]{primary,secondary})
                    if(item!=null&&!InventoryRules.Equipped(next,item.instanceId)
                        &&InventoryRules.BagCount(next)>catalog.inventoryCapacity
                        &&next.inventory.Remove(item))next.deliveries.Add(item);
            }, overflow?"Item desequipado. Mochila cheia: a peça excedente foi guardada em ENTREGAS.":"Item desequipado. Consulte a mochila ou o saque atual.");
        }
        public bool Discard(string itemId, bool confirmed)
        {
            if (!confirmed) { Message = "Confirme o descarte."; return false; }
            if (Profile.Item(itemId) == null) return false;
            foreach (var set in Profile.loadouts) foreach (var id in set.items) if (id == itemId) { Message = "Desequipe de todos os heróis antes de descartar."; return false; }
            return Transaction(next => {next.inventory.RemoveAll(item => item.instanceId == itemId);next.expedition?.items.RemoveAll(item=>item.instanceId==itemId);next.coins=Mathf.Min(100000000,next.coins+InventoryRules.SellValue(catalog,Profile.Item(itemId)));}, "Item reciclado por "+InventoryRules.SellValue(catalog,Profile.Item(itemId))+" ouro.");
        }
        public bool ClaimDelivery(string id)
        {
            if (InventoryRules.BagCount(Profile) >= catalog.inventoryCapacity) { Message = "Inventário cheio. Libere um espaço."; return false; }
            if (!Profile.deliveries.Exists(item => item.instanceId == id)) return false;
            return Transaction(next => { var item = next.deliveries.Find(value => value.instanceId == id); next.deliveries.Remove(item); next.inventory.Add(item); }, "Entrega recebida.");
        }
        public bool ClaimAllDeliveries()
        {
            int count=Mathf.Min(Mathf.Max(0,catalog.inventoryCapacity-InventoryRules.BagCount(Profile)),Profile.deliveries.Count);
            if(Profile.deliveries.Count==0){Message="Nenhuma entrega para receber.";return false;}
            if(count==0){Message="Mochila cheia. Libere espaço para receber as entregas.";return false;}
            int remaining=Profile.deliveries.Count-count;
            return Transaction(next=>
            {
                var received=next.deliveries.GetRange(0,count);
                next.deliveries.RemoveRange(0,count);next.inventory.AddRange(received);
            },(count==1?"Recebido 1 item na mochila. ":"Recebidos "+count+" itens na mochila. ")+(remaining==0?"Todas as entregas foram recebidas.":remaining==1?"1 entrega continua guardada por falta de espaço.":remaining+" entregas continuam guardadas por falta de espaço."));
        }
        public bool SpendTalent(string heroId,TalentId id)
        {
            if(catalog.Hero(heroId)==null||!Enum.IsDefined(typeof(TalentId),id))return false;
            var rule=catalog.talents[(int)id];if(HeroTalents.Available(Profile,heroId,catalog)<=0||HeroTalents.Rank(Profile,heroId,id)>=rule.maximumRank){Message="Sem pontos disponíveis ou melhoria no limite.";return false;}
            return Transaction(next=>{next.talents??=new System.Collections.Generic.List<HeroTalentAllocation>();var set=HeroTalents.Allocation(next,heroId);if(set==null){set=new HeroTalentAllocation{heroId=heroId};next.talents.Add(set);}set.ranks[(int)id]++;},"Melhoria aprendida: "+rule.displayName);
        }
        public bool ResetTalents(string heroId,bool confirmed)
        {
            if(catalog.Hero(heroId)==null)return false;int cost=HeroTalents.ResetCost(Profile,heroId,catalog);if(!confirmed||cost<=0||Profile.coins<cost){Message="Confirme a redistribuição e tenha "+cost+" ouro.";return false;}
            return Transaction(next=>{next.coins-=cost;next.talents.RemoveAll(x=>x.heroId==heroId);},"Pontos devolvidos. Custo: "+cost+" ouro.");
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
            if (!catalog.IsRewardRule(rule)) return false;
            var run = Profile.expedition;
            if (run == null || run.runId != runId || run.events.Contains(eventId) || string.IsNullOrWhiteSpace(eventId) || eventId.Length > 128 || run.events.Count >= 2048) return false;
            string discovery="discovery:"+run.heroId;
            if(rule==catalog.firstVictory&&Profile.starterGranted.Contains(discovery))return Transaction(next=>next.expedition.events.Add(eventId),"Encontro concluído.");
            var loot = LootGenerator.RewardItems(catalog, rule, new System.Random(seed),Profile.expedition.heroId);
            int capacity = rule == catalog.boss || rule == catalog.completion ? catalog.expeditionCapacity : rule==catalog.exploration?catalog.expeditionCapacity-1:catalog.expeditionCapacity - 3;
            if ((rule == catalog.enemy||rule==catalog.veteranEnemy||rule==catalog.challengeEnemy||rule==catalog.summonedEnemy||rule==catalog.supplies||rule==catalog.miniboss) && loot.Count + run.items.Count > capacity) loot.Clear();
            if (loot.Count>0&&loot.Count + run.items.Count > capacity || Profile.inventory.Count + Profile.deliveries.Count + run.items.Count + loot.Count > 512) { Message = "Espaço de saque cheio; libere espaço antes de abrir outro baú."; return false; }
            return Transaction(next => { var current = next.expedition; current.events.Add(eventId); current.coins += rule.coins; current.experience += rule.experience;if(rule.instantCurrency){next.experience=Mathf.Min(1000000,next.experience+rule.experience);next.coins=Mathf.Min(100000000,next.coins+rule.coins);current.bankedExperience+=rule.experience;current.bankedCoins+=rule.coins;} current.items.AddRange(loot);if(rule==catalog.firstVictory)next.starterGranted.Add(discovery); }, rule==catalog.firstVictory?"Primeira arma encontrada. Consulte SAQUE ATUAL no personagem.":"Recompensas recebidas.");
        }
        public bool FinishExpedition(string runId, bool victory, string outcome)
        {
            if (Profile.expedition == null || Profile.expedition.runId != runId || Profile.completedRuns.Contains(runId)) return false;
            return Transaction(next =>
            {
                var run = next.expedition; int xp=run.bankedExperience+(victory?run.experience-run.bankedExperience:(run.experience-run.bankedExperience)/2),coins=run.bankedCoins+(victory?run.coins-run.bankedCoins:(run.coins-run.bankedCoins)/2);
                next.experience = Mathf.Min(1000000, next.experience + xp-run.bankedExperience); next.coins = Mathf.Min(100000000, next.coins + coins-run.bankedCoins);
                foreach (var item in run.items)
                {
                    if(InventoryRules.Equipped(next,item.instanceId)||InventoryRules.BagCount(next)<catalog.inventoryCapacity)next.inventory.Add(item);
                    else next.deliveries.Add(item);
                }
                if (xp > 0 || coins > 0 || run.items.Count > 0 || outcome != "Abandono" && outcome != "Aplicação interrompida")
                    next.lastResult = new ExpeditionResult { runId = run.runId, location = run.location, outcome = outcome, experience = xp, coins = coins, items = run.items };
                next.completedRuns.Add(run.runId); if (next.completedRuns.Count > 256) next.completedRuns.RemoveAt(0); next.expedition = null;
            }, victory ? "Vitória! Recompensas salvas." : "Expedição encerrada. Consulte os valores guardados no resultado.");
        }
        static bool Equipped(PlayerProfile profile,string id)
        {foreach(var set in profile.loadouts)foreach(var equipped in set.items)if(equipped==id)return true;return false;}
    }
}
