using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    public sealed class CoopProgression : MonoBehaviour
    {
        [SerializeField] ProgressionCatalog catalog;
        [SerializeField] string profileDirectory="PrivateCoopProfiles";
        [SerializeField, Range(.1f, 1)] float revivalFraction = .5f;
        CoopDirector director; CoopRewardLedger ledger;
        readonly ProfileService[] profiles = new ProfileService[4];
        readonly string[] identities = new string[4];
        readonly Action[] callbacks = new Action[4];
        readonly int[] revisions = new int[4];
        readonly Dictionary<EnemyActor, int> spawned = new Dictionary<EnemyActor, int>(64);
        readonly Dictionary<string, Job> pending = new Dictionary<string, Job>(64);
        string directory, expeditionId; int serial; float retry; bool bound;
        public ProgressionCatalog Catalog => catalog;
        public string Error { get; private set; }
        public bool Active => ledger != null && ledger.Active;
        public bool Participating(int slot)=>ledger!=null&&ledger.Profile(slot)!=null;
        public int PendingCount => pending.Count;
        sealed class Job { public RewardRule rule; public int seed; }
        public void Configure(ProgressionCatalog data) => catalog = data;
        void Awake()
        {
            director = GetComponent<CoopDirector>();
            if (catalog == null) { Error = "Catálogo de progressão ausente."; enabled = false; return; }
            ledger = new CoopRewardLedger(catalog); directory = Path.Combine(Application.persistentDataPath, profileDirectory);
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-runTests") directory = Path.Combine(Application.temporaryCachePath, "CoopIntegration", Guid.NewGuid().ToString("N"));
                if (args[i] == "-netSavePath" && i + 1 < args.Length) directory = Path.GetFullPath(args[++i]);
            }
        }
        public ProfileService Profile(int slot) => slot >= 0 && slot < profiles.Length ? profiles[slot] : null;
        public string SavePath(int slot) => CoopRewardLedger.ParticipantPath(directory, identities[slot]);
        public int Revision(int slot) => slot >= 0 && slot < revisions.Length ? revisions[slot] : 0;
        public bool Attach(int slot, string identity,int hero=-1)
        {
            if (ledger == null) return false;
            if(Active)for(int i=0;i<4;i++)if(i!=slot&&identities[i]==identity&&ledger.Profile(i)!=null)
            {Error="Este perfil já possui uma vaga nesta expedição.";return false;}
            if (identities[slot] == identity && profiles[slot] != null && (!Active||ledger.Profile(slot)==profiles[slot])) return true;
            if (Active && (ledger.Profile(slot)!=null||hero<0||hero>=director.HeroCount)) { Error = "Vaga reservada nesta expedição."; return false; }
            if(Active&&!TryFlush())return false;
            try
            {
                var profile = new ProfileService(catalog, new ProfileStore(CoopRewardLedger.ParticipantPath(directory, identity)));
                if (profile.Profile.expedition != null && !profile.FinishExpedition(profile.Profile.expedition.runId, false, "Aplicação interrompida")) { Error = profile.Message; return false; }
                if(Active&&!ledger.Join(slot,profile,director.Actor(slot).Roster.Current.heroId,director.Dungeon.Biome.displayName)){Error=ledger.Error??"Falha ao registrar participante.";return false;}
                if (profiles[slot] != null && callbacks[slot] != null) profiles[slot].Changed -= callbacks[slot];
                profiles[slot] = profile; identities[slot] = identity; revisions[slot]++;
                callbacks[slot] = () => revisions[slot]++; profile.Changed += callbacks[slot]; ledger.Assign(slot, profile);
                EnsureEquipment(slot, profile); return true;
            }
            catch (Exception error) { Error = "Perfil cooperativo não pôde ser aberto: " + error.Message; return false; }
        }
        void EnsureEquipment(int slot, ProfileService profile)
        {
            var actor = director.Actor(slot); var gear = actor.GetComponent<HeroEquipment>();
            if (gear == null)
            {
                gear = actor.gameObject.AddComponent<HeroEquipment>();
                Material material = actor.Presentation.Visible.GetComponentInChildren<Renderer>().sharedMaterial;
                gear.Configure(actor.Roster, actor.Motor, actor.Combat, Visual(actor.transform, "CoopEquippedMain", new Vector3(.55f, 1, .3f), material), Visual(actor.transform, "CoopEquippedSecondary", new Vector3(-.55f, 1, .3f), material));
            }
            gear.Initialize(profile, catalog);
        }
        static MeshRenderer Visual(Transform parent, string label, Vector3 position, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = label; go.transform.SetParent(parent, false); go.transform.localPosition = position;
            Destroy(go.GetComponent<Collider>()); var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material; return renderer;
        }
        public bool Begin()
        {
            var heroes = new string[4];
            for (int i = 0; i < heroes.Length; i++)
            {
                if (!director.IsParticipant(i)) { ledger.Assign(i, null); continue; }
                ledger.Assign(i, profiles[i]); heroes[i] = director.Actor(i).Roster.Current.heroId;
            }
            if (!ledger.Begin(heroes, director.Dungeon.Biome.displayName)) { Error = ledger.Error; return false; }
            expeditionId = Guid.NewGuid().ToString("N"); serial = 0; spawned.Clear(); pending.Clear();
            if (!bound)
            {
                director.Dungeon.Encounter.Enemies.Spawned += Spawned; director.Dungeon.Encounter.Enemies.Died += Died;
                director.Dungeon.RoomCompleted += RoomWon; bound = true;
            }
            if(director.Campaign!=null)director.Campaign.SideRewardRequested=SideReward;
            director.Dungeon.ChestRewardRequested = Chest; director.Dungeon.ExitRewardRequested = Exit;
            return true;
        }
        void Spawned(EnemyActor actor) { if (director.Manager.IsServer && Active) spawned[actor] = ++serial; }
        void Died(EnemyActor actor)
        {
            if (!director.Manager.IsServer || !Active || !spawned.TryGetValue(actor, out int id)) return;
            spawned.Remove(actor); if (actor.Profile.kind == EnemyKind.Minion) return;
            Enqueue("enemy:" + id, actor.Profile.kind == EnemyKind.Boss ? catalog.boss : catalog.enemy); TryFlush();
        }
        void RoomWon(DungeonRoom room)
        {
            if (!director.Manager.IsServer || !Active || room.encounter == null) return;
            Enqueue("room:" + room.node.id, catalog.room); TryFlush();
            for (int i = 0; i < 4; i++)
            {
                var actor = director.Actor(i); if (!director.IsConnected(i) || actor.Health.IsAlive) continue;
                if (actor.Health.Revive(revivalFraction)) director.PlaceInRoom(i, room);
            }
        }
        bool SideReward(int id,bool exploration)
        {
            if(!Active||!director.Manager.IsServer||director.Phase!=CoopPhase.Playing)return false;
            Enqueue("adventure:"+id,exploration?catalog.exploration:catalog.supplies);return TryFlush();
        }
        bool Chest(int room) { if (!Active || !director.Manager.IsServer) return false; Enqueue("chest:" + room, catalog.chest); return TryFlush(); }
        bool Exit()
        {
            if (!Active || !director.Manager.IsServer) return false; Enqueue("completion", catalog.completion);
            return TryFlush() && Finish(true, "Vitória cooperativa");
        }
        void Enqueue(string id, RewardRule rule)
        {
            if (pending.ContainsKey(id)) return; int seed = 17;
            unchecked { foreach (char character in expeditionId + id) seed = seed * 31 + character; }
            pending.Add(id, new Job { rule = rule, seed = seed });
        }
        public bool TryFlush()
        {
            foreach (var pair in pending) if (!ledger.Grant(pair.Key, pair.Value.rule, pair.Value.seed)) { Error = ledger.Error; return false; }
            pending.Clear(); Error = null; return true;
        }
        public bool Finish(bool victory, string outcome)
        {
            if (!Active) return false;
            if (!TryFlush() || !ledger.Finish(victory, outcome)) { Error ??= ledger.Error; return false; }
            if(victory&&director.Campaign!=null)for(int i=0;i<4;i++)if(director.IsParticipant(i)&&profiles[i]!=null)
                if(!profiles[i].MarkCampaignClear(director.Campaign.Selected))Error=profiles[i].Message;
            spawned.Clear(); return true;
        }
        public bool CanManage(int slot)
        {
            var actor = director.Actor(slot); var state = director.Dungeon.Encounter.State;
            return actor != null && director.IsConnected(slot) && actor.Health.IsAlive && !actor.Combat.IsBusy
                && director.Phase != CoopPhase.Loading && director.Phase != CoopPhase.Connecting
                && state != EncounterState.Spawning && state != EncounterState.Fighting && actor.Combat.Pool.ActiveProjectiles == 0 && actor.Combat.Pool.ActiveZones == 0;
        }
        void Update()
        {
            if (!director.Manager.IsServer || !Active || pending.Count == 0 && director.Phase != CoopPhase.Defeat) return;
            retry -= Time.unscaledDeltaTime; if (retry > 0) return; retry = .5f;
            if (director.Phase == CoopPhase.Defeat) Finish(false, "Derrota cooperativa"); else TryFlush();
        }
        void OnDestroy()
        {
            if (bound && director != null && director.Dungeon != null)
            { director.Dungeon.Encounter.Enemies.Spawned -= Spawned; director.Dungeon.Encounter.Enemies.Died -= Died; director.Dungeon.RoomCompleted -= RoomWon; }
            for (int i = 0; i < profiles.Length; i++) if (profiles[i] != null && callbacks[i] != null) profiles[i].Changed -= callbacks[i];
        }
    }
}
