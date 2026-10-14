using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ForjaDasMares
{
    [DefaultExecutionOrder(150)]
    public sealed class ProgressionSceneBridge : MonoBehaviour
    {
        [SerializeField] ProgressionCatalog catalog;
        [SerializeField] HeroRoster heroes;
        [SerializeField] HeroEquipment equipment;
        [SerializeField] EncounterController encounter;
        [SerializeField] ArenaSession arena;
        [SerializeField] DungeonSession dungeon;
        [SerializeField] SupportCompanion companion;
        [SerializeField] ArenaInput commands;
        readonly Dictionary<EnemyActor, int> spawned = new Dictionary<EnemyActor, int>(24);
        ProgressionProfileRuntime runtime;
        string runId;
        int sequence;
        bool ending, bound;
        public ProfileService Service => runtime != null ? runtime.Service : null;
        public ProgressionCatalog Catalog => catalog;
        public HeroRoster Heroes => heroes;
        public HeroEquipment Equipment => equipment;
        public SupportCompanion Companion => companion;
        public string Error { get; private set; }
        public bool CanManage => Service != null && encounter.State != EncounterState.Spawning && encounter.State != EncounterState.Fighting && !heroes.GetComponent<HeroCombat>().IsBusy && heroes.GetComponent<HeroCombat>().Pool.ActiveProjectiles == 0 && heroes.Health.IsAlive;
        public void Configure(ProgressionCatalog data, HeroRoster roster, HeroEquipment gear, EncounterController fights, ArenaSession reset, DungeonSession map, SupportCompanion pet, ArenaInput input)
        { catalog = data; heroes = roster; equipment = gear; encounter = fights; arena = reset; dungeon = map; companion = pet; commands = input; }
        void Awake()
        {
            try { runtime = ProgressionProfileRuntime.Open(catalog); equipment.Initialize(Service, catalog); companion.Initialize(Service); }
            catch (Exception error) { Error = error.Message; Debug.LogError("Progression initialization failed: " + Error, this); return; }
            encounter.Enemies.Spawned += Spawned; encounter.Enemies.Died += Died; heroes.Health.Changed += HealthChanged;
            if (dungeon != null) { dungeon.Generated += BeginDungeon; dungeon.RoomCompleted += RoomWon; dungeon.ChestRewardRequested = Chest; dungeon.ExitRewardRequested = FinishDungeon; }
            else { encounter.Started += BeginArena; encounter.Completed += ArenaWon; arena.Resetting += Abort; heroes.Changed += Abort; }
            bound = true;
        }
        void OnDestroy()
        {
            if (!bound) return;
            encounter.Enemies.Spawned -= Spawned; encounter.Enemies.Died -= Died; heroes.Health.Changed -= HealthChanged;
            if (dungeon != null) { dungeon.Generated -= BeginDungeon; dungeon.RoomCompleted -= RoomWon; dungeon.ChestRewardRequested = null; dungeon.ExitRewardRequested = null; }
            else { encounter.Started -= BeginArena; encounter.Completed -= ArenaWon; arena.Resetting -= Abort; heroes.Changed -= Abort; }
            Abort();
        }
        void BeginDungeon() => Begin(dungeon.Biome.displayName);
        void BeginArena() => Begin("Arena: " + encounter.Current.displayName);
        void Begin(string location)
        {
            if (Service == null) return;
            if (!Service.BeginExpedition(heroes.Current.heroId, location)) { Error = Service.Message; return; }
            runId = Service.Profile.expedition.runId; spawned.Clear(); sequence = 0; companion.ResetBudget();
        }
        void Spawned(EnemyActor enemy) { if (runId != null) spawned[enemy] = ++sequence; }
        void Died(EnemyActor enemy)
        {
            if (runId == null || !heroes.Health.IsAlive || !spawned.TryGetValue(enemy, out int serial)) return;
            spawned.Remove(enemy); if (enemy.Profile.kind == EnemyKind.Minion) return;
            Grant("enemy:" + serial, enemy.Profile.kind == EnemyKind.Boss ? catalog.boss : catalog.enemy);
        }
        bool Grant(string id, RewardRule rule)
        {
            if (Service == null || runId == null) return false;
            int seed = 17; unchecked { foreach (char character in runId + id) seed = seed * 31 + character; }
            return Service.Grant(runId, id, rule, seed);
        }
        void RoomWon(DungeonRoom room) { if (room.encounter != null) Grant("room:" + room.node.id, catalog.room); }
        bool Chest(int roomId)
        {
            if (dungeon.CurrentRoom == null || dungeon.CurrentRoom.node.id != roomId || !heroes.Health.IsAlive) return false;
            return Grant("chest:" + roomId, catalog.chest);
        }
        bool FinishDungeon()
        {
            if (Service?.Profile.expedition == null || runId == null) return false;
            if (!Service.Profile.expedition.events.Contains("completion") && !Grant("completion", catalog.completion)) return false;
            return Finish(true, "Masmorra concluída");
        }
        void ArenaWon() { if (runId != null) { Grant("completion", catalog.completion); Finish(true, "Encontro concluído"); } }
        void HealthChanged() { if (!ending && !heroes.Health.IsAlive) Finish(false, "Derrota"); }
        public void Abort() => Finish(false, "Abandono");
        bool Finish(bool victory, string outcome)
        {
            if (ending || Service == null || runId == null) return false; ending = true;
            bool saved = Service.FinishExpedition(runId, victory, outcome); if (saved) { runId = null; spawned.Clear(); } ending = false; return saved;
        }
        public bool Equip(string id, EquipmentSlot slot) => CanManage && Service.Equip(heroes.Current.heroId, id, slot);
        public bool Unequip(EquipmentSlot slot) => CanManage && Service.Unequip(heroes.Current.heroId, slot);
        public void BlockCommands(bool value)
        { commands.BlockCommands(value); if (dungeon != null) dungeon.GetComponent<DungeonInteractionInput>().enabled = !value; }
        public void OpenDungeon() { Abort(); SceneManager.LoadScene("ProgressionDungeon"); }
    }
}
