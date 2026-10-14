using UnityEngine;
using UnityEngine.SceneManagement;
namespace ForjaDasMares
{
    [DefaultExecutionOrder(100)]
    public sealed class DungeonSession : MonoBehaviour
    {
        [SerializeField] DungeonGenerator generator;
        [SerializeField] EncounterController encounter;
        [SerializeField] HeroRoster heroes;
        [SerializeField] ArenaSession arena;
        [SerializeField] ArenaCamera cameraFollow;
        [SerializeField] DungeonInteractionInput input;
        [SerializeField] TelegraphPool warnings;
        [SerializeField] BiomeDefinition[] biomes;
        [SerializeField] Light mainLight;
        [SerializeField] int seed = 12345;
        [SerializeField, Range(7, 12)] int maximumRooms = 9;
        [SerializeField] bool fixedMap = true;
        int biomeIndex;
        bool regenerate, generating;
        float detection;
        public DungeonRoom CurrentRoom { get; private set; }
        public DungeonGenerator Generator => generator;
        public HeroRoster Heroes => heroes;
        public EncounterController Encounter => encounter;
        public BiomeDefinition Biome => biomes[biomeIndex];
        public int Seed => seed;
        public bool FixedMap => fixedMap;
        public int Coins { get; private set; }
        public bool HasKey { get; private set; }
        public bool Finished { get; private set; }
        public DungeonInteractable Nearby { get; private set; }
        public event System.Action Generated;
        public event System.Action<DungeonRoom> RoomCompleted;
        public System.Func<int, bool> ChestRewardRequested { get; set; }
        public System.Func<bool> ExitRewardRequested { get; set; }
        [SerializeField] string arenaSceneName = "ArenaTest";
        public void ConfigureArenaScene(string name) => arenaSceneName = name;
        public void Configure(DungeonGenerator map, EncounterController fights, HeroRoster player, ArenaSession reset, ArenaCamera follow, DungeonInteractionInput controls, TelegraphPool signs, BiomeDefinition[] data, Light sun)
        { generator = map; encounter = fights; heroes = player; arena = reset; cameraFollow = follow; input = controls; warnings = signs; biomes = data; mainLight = sun; }
        void Awake() { encounter.Completed += EncounterWon; encounter.Failed += EncounterFailed; arena.Resetting += RequestReset; heroes.Changed += RequestReset; }
        void OnDestroy() { if (encounter != null) { encounter.Completed -= EncounterWon; encounter.Failed -= EncounterFailed; } if (arena != null) arena.Resetting -= RequestReset; if (heroes != null) heroes.Changed -= RequestReset; }
        void Start() => Generate();
        void RequestReset() { if (!generating) regenerate = true; }
        public void Generate()
        {
            generating = true; regenerate = false; encounter.Cancel(); heroes.ResetHero(); Coins = 0; HasKey = Finished = false; Nearby = null;
            generator.Generate(this, Biome, seed, maximumRooms, fixedMap, warnings);
            RenderSettings.ambientLight = Biome.ambient; mainLight.color = Biome.light; Camera.main.backgroundColor = Biome.background;
            CurrentRoom = generator.Rooms[0]; CurrentRoom.entered = CurrentRoom.completed = CurrentRoom.encounterComplete = true;
            arena.Spawn.position = CurrentRoom.Center + new Vector3(0, 0.06f, -3);
            heroes.GetComponent<PlayerMotor>().Teleport(arena.Spawn.position, Quaternion.identity);
            RefreshDoors(); UpdateVisibility(); cameraFollow.Snap(); generating = false;
            Debug.Log("DUNGEON_RUNTIME_READY biome=" + Biome.biomeId + " seed=" + seed);
            Generated?.Invoke();
        }
        void Update()
        {
            if (regenerate) { Generate(); return; }
            if (generating || Finished || generator.Rooms == null) return;
            detection -= Time.deltaTime;
            if (detection <= 0)
            {
                detection = 0.08f; Vector3 position = heroes.transform.position;
                foreach (var room in generator.Rooms) if (room != CurrentRoom && room.Contains(position)) { Enter(room); break; }
                Nearby = null; float best = 2.1f * 2.1f;
                if (CurrentRoom != null) foreach (var interaction in CurrentRoom.interactions)
                {
                    float distance = (interaction.transform.position - position).sqrMagnitude;
                    if (interaction.CanUse && distance < best) { Nearby = interaction; best = distance; }
                }
            }
            if (input.TakeInteraction() && Nearby != null && (Nearby.transform.position - heroes.transform.position).sqrMagnitude <= 2.1f * 2.1f) Nearby.Use();
        }
        public void Enter(DungeonRoom room)
        {
            if (!heroes.Health.IsAlive || room == CurrentRoom) return;
            CurrentRoom = room;
            if (!room.entered)
            {
                room.entered = true;
                if (room.encounter != null) { encounter.SetSpawnPoints(room.spawnPoints); encounter.Begin(room.encounter, false); }
                else room.encounterComplete = true;
                TryComplete(room);
            }
            RefreshDoors(); UpdateVisibility();
        }
        void EncounterWon() { if (CurrentRoom == null) return; CurrentRoom.encounterComplete = true; TryComplete(CurrentRoom); }
        void EncounterFailed() { RefreshDoors(); }
        void TryComplete(DungeonRoom room)
        {
            if (room.completed || !room.encounterComplete || !room.objectiveComplete) return;
            room.completed = true; if (room.encounter != null) Coins += Biome.rewards.victoryCoins; RefreshDoors(); RoomCompleted?.Invoke(room);
        }
        public void CompleteObjective(DungeonRoom room, bool grantKey)
        { room.objectiveComplete = true; if (grantKey && Biome.rewards.eventGrantsOptionalKey) HasKey = true; TryComplete(room); RefreshDoors(); }
        public void DisableTrap(DungeonRoom room)
        { foreach (var trap in generator.Traps) if (trap.transform.IsChildOf(room.root.transform)) trap.Disarm(); CompleteObjective(room, false); }
        public void OpenChest(int roomId)
        { TryOpenChest(roomId); }
        public bool TryOpenChest(int roomId)
        {
            if (ChestRewardRequested != null && !ChestRewardRequested(roomId)) return false;
            var random = new System.Random(unchecked(seed * 397 ^ roomId * 7919)); int minimum = Mathf.Max(0, Biome.rewards.minimumCoins), maximum = Mathf.Max(minimum, Biome.rewards.maximumCoins);
            Coins += random.Next(minimum, maximum + 1);
            return true;
        }
        public void RefreshDoors()
        {
            foreach (var door in generator.Doors)
            {
                var a = generator.Rooms[door.A]; var b = generator.Rooms[door.B];
                bool key = (!a.entered && a.node.category == RoomCategory.Treasure || !b.entered && b.node.category == RoomCategory.Treasure) && !HasKey;
                door.SetClosed(a.Locked || b.Locked || key || !(a.completed || b.completed));
            }
        }
        void UpdateVisibility()
        {
            foreach (var room in generator.Rooms) room.SetVisible(room == CurrentRoom || CurrentRoom.node.connections.Contains(room.node.id));
            foreach (var corridor in generator.Corridors) corridor.SetVisible(corridor.a == CurrentRoom.node.id || corridor.b == CurrentRoom.node.id);
            foreach (var door in generator.Doors) door.SetVisible(door.A == CurrentRoom.node.id || door.B == CurrentRoom.node.id || CurrentRoom.node.connections.Contains(door.A) || CurrentRoom.node.connections.Contains(door.B));
            cameraFollow.SetFocusBounds(new Bounds(CurrentRoom.Center + Vector3.up * 0.9f, new Vector3(9, 100, 9)), true);
        }
        public void ExitDungeon() { TryExitDungeon(); }
        public bool TryExitDungeon()
        { if (Finished || CurrentRoom == null || CurrentRoom.node.category != RoomCategory.Boss || !CurrentRoom.completed || !heroes.Health.IsAlive) return false; if (ExitRewardRequested != null && !ExitRewardRequested()) return false; Finished = true; encounter.Cancel(); heroes.Victory(); return true; }
        public void NextBiome() { biomeIndex = (biomeIndex + 1) % biomes.Length; Generate(); }
        public void NextSeed() { seed = unchecked(seed + 1); Generate(); }
        public void ToggleFixed() { fixedMap = !fixedMap; Generate(); }
        public void ConfigureTest(int newSeed, bool fixedLayout, int biome = 0) { seed = newSeed; fixedMap = fixedLayout; biomeIndex = Mathf.Clamp(biome, 0, biomes.Length - 1); Generate(); }
        public void BackToArena() => SceneManager.LoadScene(arenaSceneName);
    }
}
