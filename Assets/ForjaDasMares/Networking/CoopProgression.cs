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
        public string HomeCacheRoot=>Path.Combine(directory,"HomeCache",director.ParticipantIdentity);
        public void PrepareHome(ProfileService service){var actor=director.Actor(0);if(actor.Roster.Selected!=director.SelectedHero||!actor.Health.IsAlive)actor.Roster.Select(director.SelectedHero);actor.Presentation.ApplyReplica(HeroState.Idle,0);director.Actor(0).ClearReplicaGear();EnsureEquipment(0,service);director.Actor(0).GetComponent<HeroEquipment>().enabled=true;}
        ProfileService homeProfile;
        public int HomeRevision {get;private set;}
        public ProfileService HomeProfile
        {
            get
            {
                if(homeProfile!=null)return homeProfile;
                homeProfile=new ProfileService(catalog,new ProfileStore(CoopRewardLedger.ParticipantPath(directory,director.ParticipantIdentity)));
                if(homeProfile.Profile.expedition!=null)homeProfile.FinishExpedition(homeProfile.Profile.expedition.runId,false,"Aplicação interrompida");
                homeProfile.Changed+=HomeChanged;HomeRevision++;return homeProfile;
            }
        }
        void HomeChanged()=>HomeRevision++;
        public void ReleaseHome(){if(homeProfile!=null)homeProfile.Changed-=HomeChanged;homeProfile=null;HomeRevision++;}
        string directory, expeditionId; int serial; float retry; bool bound;
        public ProgressionCatalog Catalog => catalog;
        public string Error { get; private set; }
        public bool Active => ledger != null && ledger.Active;
        public bool Participating(int slot)=>ledger!=null&&ledger.Profile(slot)!=null;
        public int PendingCount => pending.Count;
        sealed class Job { public RewardRule rule; public int seed;public bool drop;public Vector3 position;public int[] before=new int[4]; }
        public void Configure(ProgressionCatalog data) => catalog = data;
        void Awake()
        {
            director = GetComponent<CoopDirector>();if(GetComponent<CampaignLootDrops>()==null)gameObject.AddComponent<CampaignLootDrops>();
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
            gear.enabled=true;gear.Initialize(profile, catalog);
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
            spawned.Remove(actor);
            var rule=actor.Profile.kind==EnemyKind.Minion?catalog.summonedEnemy:actor.Profile.miniboss?catalog.MinibossRule(actor.Profile.rewardTheme):actor.Profile.kind==EnemyKind.Boss?catalog.boss:director.Campaign!=null&&director.Campaign.Selected==2?catalog.challengeEnemy:director.Campaign!=null&&director.Campaign.Selected==1?catalog.veteranEnemy:catalog.enemy;
            string key="enemy:"+id;Enqueue(key,rule);var job=pending[key];job.drop=director.Campaign!=null;job.position=actor.transform.position;for(int i=0;i<4;i++)job.before[i]=profiles[i]?.Profile.expedition?.items.Count??0;TryFlush();
        }
        void RoomWon(DungeonRoom room)
        {
            if (!director.Manager.IsServer || !Active || room.encounter == null) return;
            if(director.Campaign==null)Enqueue("room:" + room.node.id, catalog.room); TryFlush();
            for (int i = 0; i < 4; i++)
            {
                var actor = director.Actor(i); if (!director.IsConnected(i) || actor.Health.IsAlive) continue;
                if (actor.Health.Revive(revivalFraction)) director.PlaceInRoom(i, room);
            }
        }
        bool SideReward(int id,bool exploration)
        {
            if(!Active||!director.Manager.IsServer||director.Phase!=CoopPhase.Playing)return false;
            Enqueue("adventure:"+id,id>=3000?catalog.optionalChallenge:id>=2000?catalog.secretCache:exploration?catalog.exploration:catalog.supplies);return TryFlush();
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
            foreach(var pair in pending)if(pair.Value.drop)
            {
                var job=pair.Value;for(int i=0;i<4;i++)if(director.IsConnected(i)&&profiles[i]?.Profile.expedition!=null)
                {
                    var items=profiles[i].Profile.expedition.items;int rarity=items.Count>job.before[i]?(int)items[items.Count-1].rarity:-1;
                    GetComponent<CampaignLootDrops>()?.Send(i,job.position,job.rule.coins,job.rule.experience,rarity);
                }
            }
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
        public bool CanManage(int slot) => ManagementBlockReason(slot) == null;
        public string ManagementBlockReason(int slot)
        {
            if (slot < 0 || slot >= 4 || !director.IsConnected(slot)) return "Aguarde a conexão com a sala.";
            var actor = director.Actor(slot);
            if (actor == null) return "Aguarde o carregamento do herói.";
            if (director.Phase == CoopPhase.Loading || director.Phase == CoopPhase.Connecting)
                return "Aguarde o carregamento da partida.";
            if (actor.Combat.IsBusy) return "Aguarde seu ataque ou habilidade terminar.";
            if (director.Phase == CoopPhase.Lobby) return null;
            if (!actor.Health.IsAlive) return "Reapareça antes de alterar o equipamento.";
            var state = director.Dungeon.Encounter.State;
            if ((state == EncounterState.Spawning || state == EncounterState.Fighting)
                && (!director.Dungeon.OpenLandscape || director.Dungeon.Encounter.Enemies.ThreatNear(actor.transform.position, 12)))
                return "Afaste-se dos inimigos para alterar o equipamento.";
            if (actor.Combat.Pool.HasActiveEffectsOwnedBy(actor.Health))
                return "Aguarde seus projéteis e efeitos terminarem.";
            return null;
        }
        void Update()
        {
            if (!director.Manager.IsServer || !Active || pending.Count == 0 && director.Phase != CoopPhase.Defeat) return;
            retry -= Time.unscaledDeltaTime; if (retry > 0) return; retry = .5f;
            if (director.Phase == CoopPhase.Defeat) Finish(false, "Derrota cooperativa"); else TryFlush();
        }
        void OnDestroy()
        {
            ReleaseHome();
            if (bound && director != null && director.Dungeon != null)
            { director.Dungeon.Encounter.Enemies.Spawned -= Spawned; director.Dungeon.Encounter.Enemies.Died -= Died; director.Dungeon.RoomCompleted -= RoomWon; }
            for (int i = 0; i < profiles.Length; i++) if (profiles[i] != null && callbacks[i] != null) profiles[i].Changed -= callbacks[i];
        }
    }
}
