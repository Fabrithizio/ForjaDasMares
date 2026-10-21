using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.AI;
namespace ForjaDasMares.Networking
{
    [DefaultExecutionOrder(200)]
    public sealed class CoopDirector : MonoBehaviour
    {
        [SerializeField] NetworkManager network;
        [SerializeField] UnityTransport transport;
        [SerializeField] DungeonSession dungeon;
        [SerializeField] ArenaSession arena;
        [SerializeField] HeroRoster template;
        [SerializeField] ArenaCamera follow;
        [SerializeField] ArenaInput input;
        [SerializeField] DungeonInteractionInput interaction;
        [SerializeField] CombatEffectPool effects;
        [SerializeField] TelegraphPool warnings;
        [SerializeField, Range(5, 20)] int snapshotRate = 10;
        [SerializeField, Range(10, 40)] int inputRate = 30;
        [SerializeField, Range(10, 180)] float reconnectWindow = 90;
        readonly CoopActor[] actors = new CoopActor[4];
        readonly Seat[] seats = { new Seat(), new Seat(), new Seat(), new Seat() };
        readonly Dictionary<ulong, int> connections = new Dictionary<ulong, int>(4);
        readonly WorldFrame frame = new WorldFrame();
        readonly ActorFrame[] playerFrames = new ActorFrame[4];
        readonly List<ActorFrame> enemyFrames = new List<ActorFrame>(64);
        CombatParty party; CoopReplicaView replica;
        CoopProgression progression; CoopProfileChannel profiles;
        bool inputBlocked,transportLost;
        int expedition,loadedExpedition=-1;
        uint sequence, tick; int localSlot = -1, selectedHero, biome;
        float inputClock, snapshotClock, roomClock;
        string identity = Guid.NewGuid().ToString("N");
        bool handlers, closing, ready, cloudMode, localLoaded;
        float loadingDeadline;
        HeroButtons touchButtons;
        public CoopPhase Phase { get; private set; } = CoopPhase.Offline;
        public string Message { get; private set; } = "Sala privada: teste local ou rede local.";
        public string Pin { get; private set; } = "";
        public bool IsHost => network.IsHost;
        public int LocalSlot => localSlot;
        public ArenaInput LocalInput => input;
        public string ParticipantIdentity=>identity;
        public int SelectedHero => selectedHero;
        public uint LastTick => tick;
        public int LastSnapshotBytes { get; private set; }
        public int AppliedEnemyCount { get; private set; }
        int replicaEncounter;
        public EncounterState EncounterStatus=>network.IsServer?dungeon.Encounter.State:(EncounterState)replicaEncounter;
        public int AppliedEffectCount { get; private set; }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public bool ProbeInput { get; set; }
        public Vector2 ProbeMove { get; set; }
        public Vector2 ProbeAim { get; set; }
#endif
        CampaignRun campaign;
        public CampaignRun Campaign=>campaign;
        bool CampaignReservation(int slot)=>Campaign!=null&&Phase==CoopPhase.Playing&&progression.Participating(slot);
        bool SeatFree(int slot)=>seats[slot].identity==null||!seats[slot].connected&&seats[slot].expires<=Time.unscaledTime&&(Phase!=CoopPhase.Playing||!progression.Participating(slot));
        public bool HasAdmissionSeat {get{for(int i=0;i<4;i++)if(SeatFree(i))return true;return false;}}
        public bool CanAdmitNewPlayer => Campaign!=null&&Phase==CoopPhase.Playing&&Campaign.Completion<.2f&&HasAdmissionSeat;
        void UpdateSoloBonus()
        {
            if(Campaign==null)return;
            Campaign.Solo=ConnectedCount==1;
            for(int i=0;i<4;i++)
            {var gear=actors[i]?.GetComponent<HeroEquipment>();if(gear!=null)gear.SetCampaignSolo(false);}
        }
        public int HeroCount => template.Count;
        public string HeroName => actors[localSlot >= 0 ? localSlot : 0].Roster.Current.displayName;
        public int ConnectedCount { get { int value = 0; foreach (var seat in seats) if (seat.connected) value++; return value; } }
        public CoopActor Actor(int slot) => slot >= 0 && slot < actors.Length ? actors[slot] : null;
        public NetworkManager Manager => network;
        public DungeonSession Dungeon => dungeon;
        public bool IsConnected(int slot) => slot >= 0 && slot < 4 && seats[slot].connected;
        public bool IsParticipant(int slot) => slot >= 0 && slot < 4 && seats[slot].identity != null && (seats[slot].connected || seats[slot].expires > Time.unscaledTime||CampaignReservation(slot));
        public bool TryGetSlot(ulong client, out int slot) => connections.TryGetValue(client, out slot);
        public event Action Changed;
        sealed class Seat { public string identity; public ulong client; public bool connected, loaded; public float expires, rateTime; public int commands; }
        public void Configure(NetworkManager manager, UnityTransport wire, DungeonSession map, ArenaSession reset, HeroRoster player, ArenaCamera camera, ArenaInput controls, DungeonInteractionInput interactions, CombatEffectPool pool, TelegraphPool signs)
        { network = manager; transport = wire; dungeon = map; arena = reset; template = player; follow = camera; input = controls; interaction = interactions; effects = pool; warnings = signs; }
        void Awake() { campaign=GetComponent<CampaignRun>(); dungeon.ExternallyDriven = true; arena.enabled = false; dungeon.Encounter.enabled = false; }
        void Start()
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, dungeon.gameObject.scene);
            party = gameObject.AddComponent<CombatParty>(); dungeon.Encounter.Enemies.Party = party; dungeon.Party = party;
            for (int i = 0; i < actors.Length; i++)
            {
                GameObject root;
                if (i == 0) root = template.gameObject;
                else
                {
                    root = Instantiate(template.gameObject); root.name = "CoopHero_" + i;
                    root.GetComponent<ArenaInput>().enabled = false;
                }
                actors[i] = root.GetComponent<CoopActor>(); if (actors[i] == null) actors[i] = root.AddComponent<CoopActor>(); actors[i].Configure(follow.transform);if(Campaign!=null)actors[i].Combat.AddTargets(1<<CampaignAdventure.PropLayer); playerFrames[i] = new ActorFrame { slot = i };
                if (i > 0) root.SetActive(false);
            }
            foreach(var actor in actors){var aura=actor.GetComponent<CampaignFuryVisual>();if(aura==null)aura=actor.gameObject.AddComponent<CampaignFuryVisual>();aura.Configure(actor);}
            for (int i = 0; i < dungeon.Encounter.Enemies.Capacity; i++) enemyFrames.Add(new ActorFrame { slot = i });
            replica = gameObject.AddComponent<CoopReplicaView>(); replica.Configure(dungeon.Encounter.Enemies, effects, warnings);
            progression = GetComponent<CoopProgression>(); profiles = GetComponent<CoopProfileChannel>();
            profiles.Configure(this, progression);if(Campaign!=null){var adventures=GetComponent<CampaignAdventureChannel>()??gameObject.AddComponent<CampaignAdventureChannel>();adventures.Configure(this);}
            network.ConnectionApprovalCallback = Approve;
            network.OnClientConnectedCallback += Connected; network.OnClientDisconnectCallback += Disconnected;
            network.OnTransportFailure+=TransportFailed;network.OnServerStopped+=ServerStopped;
            ready = true;
            if (GetComponent<CoopCloud>() == null) gameObject.AddComponent<CoopCloud>();
            var args = Environment.GetCommandLineArgs();
            if (Arg(args, "-netProbe") != null && GetComponent<CoopSmokeProbe>() == null) gameObject.AddComponent<CoopSmokeProbe>();
            string savedIdentity = PlayerPrefs.GetString("CoopParticipantIdentity", "");
            if (!Guid.TryParseExact(savedIdentity, "N", out _)) { savedIdentity = Guid.NewGuid().ToString("N"); PlayerPrefs.SetString("CoopParticipantIdentity", savedIdentity); PlayerPrefs.Save(); }
            identity = Arg(args, "-netIdentity") ?? savedIdentity;
            string mode = Arg(args, "-netMode");
            if (mode == "host") HostDirect(Arg(args, "-netPin") ?? "TEST42", ParsePort(Arg(args, "-netPort")), Arg(args, "-netLan") == "true");
            if (mode == "client") JoinDirect(Arg(args, "-netAddress") ?? "127.0.0.1", Arg(args, "-netPin") ?? "TEST42", ParsePort(Arg(args, "-netPort")));
            Changed?.Invoke();
        }
        static string Arg(string[] args, string key) { for (int i = 0; i < args.Length - 1; i++) if (args[i] == key) return args[i + 1]; return null; }
        static ushort ParsePort(string value) => ushort.TryParse(value, out ushort port) && port > 0 ? port : (ushort)7777;
        void BindMessages()
        {
            if (handlers) return;
            network.CustomMessagingManager.RegisterNamedMessageHandler("fdm.command.v1", ReceiveCommand);
            network.CustomMessagingManager.RegisterNamedMessageHandler("fdm.frame.v1", ReceiveFrame);
            network.CustomMessagingManager.RegisterNamedMessageHandler("fdm.hero.v1", ReceiveHero);
            network.CustomMessagingManager.RegisterNamedMessageHandler("fdm.ready.v1", ReceiveReady);
            profiles.Bind();GetComponent<CampaignAdventureChannel>()?.Bind();
            handlers = true;
        }
        public bool HostDirect(string pin, ushort port, bool lan = true)
        {
            if (!ready || network.IsListening || string.IsNullOrWhiteSpace(pin) || pin.Length > 12) return false;
            Pin = pin.Trim().ToUpperInvariant(); PrepareConnection(); transport.SetConnectionData("127.0.0.1", port, lan ? "0.0.0.0" : "127.0.0.1");
            network.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new JoinIntent { identity = identity, pin = Pin, hero = selectedHero, campaign=Campaign!=null }));
            if (!network.StartHost()) { SetPhase(CoopPhase.Closed, "Não foi possível abrir a porta da sala."); return false; }
            BindMessages(); SetPhase(CoopPhase.Lobby, "Sala local aberta. Compartilhe IP, porta e código com o grupo."); return true;
        }
        public bool JoinDirect(string address, string pin, ushort port)
        {
            if (!ready || network.IsListening || !System.Net.IPAddress.TryParse(address, out _) || string.IsNullOrWhiteSpace(pin) || pin.Length > 12) return false;
            Pin = pin.Trim().ToUpperInvariant(); PrepareConnection(); transport.SetConnectionData(address, port);
            network.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new JoinIntent { identity = identity, pin = Pin, hero = selectedHero, campaign=Campaign!=null }));
            if (!network.StartClient()) { SetPhase(CoopPhase.Closed, "Não foi possível iniciar a conexão."); return false; }
            BindMessages(); SetPhase(CoopPhase.Connecting, "Conectando à sala privada…"); return true;
        }
        public void PrepareConnection() { closing = false; transportLost=false; cloudMode = localLoaded = false; expedition=0;loadedExpedition=-1; sequence = tick = 0; localSlot = -1; inputClock = snapshotClock = 0; handlers = false; }
        public void PrepareCloudConnection(bool constrainCampaign=false)
        {
            PrepareConnection(); cloudMode = true; Pin = "";
            network.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new JoinIntent { identity = identity, pin = "", hero = selectedHero, campaign=Campaign!=null,campaignLevel=constrainCampaign&&Campaign!=null?Campaign.Selected:-1 }));
            SetPhase(CoopPhase.Connecting, "Conectando pelo Relay…");
        }
        public void CompleteCloudConnection(string code,bool privateRoom=true)
        { Pin = code; BindMessages(); if (network.IsHost) SetPhase(CoopPhase.Lobby, (privateRoom?"Sala privada pela internet. Código ":"Grupo aberto desta fase. Código ") + code); }
        void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.CreatePlayerObject = false; response.Pending = false; response.Approved = false;
            if (request.Payload == null || request.Payload.Length > 512) { response.Reason = "Pedido inválido."; return; }
            JoinIntent join;
            try { join = JsonUtility.FromJson<JoinIntent>(Encoding.UTF8.GetString(request.Payload)); }
            catch (Exception) { response.Reason = "Pedido inválido."; return; }
            if (!CoopLimits.ValidJoin(join, template.Count) || join.campaign!=(Campaign!=null) || join.campaignLevel>=0&&(Campaign==null||join.campaignLevel!=Campaign.Selected) || join.pin != (cloudMode ? "" : Pin)) { response.Reason = "Código ou versão incompatível."; return; }
            int slot = -1;
            for (int i = 0; i < seats.Length; i++) if (seats[i].identity == join.identity && !seats[i].connected && (seats[i].expires > Time.unscaledTime || CampaignReservation(i))) { slot = i; break; }
            bool returning = slot >= 0;
            if (!returning && Phase != CoopPhase.Offline && Phase != CoopPhase.Lobby && Phase != CoopPhase.Connecting && !CanAdmitNewPlayer) { response.Reason = "Entrada encerrada: fase com 20% ou mais de progresso."; return; }
            if (!returning) for (int i = 0; i < seats.Length; i++) if (SeatFree(i)) { slot = i; break; }
            if (slot < 0) { response.Reason = "Sala cheia: máximo de quatro jogadores."; return; }
            foreach (var seat in seats) if (seat.connected && seat.identity == join.identity) { response.Reason = "Jogador já conectado."; return; }
            if(!returning)actors[slot].Roster.Select(join.hero);
            if (!progression.Attach(slot, join.identity,join.hero)) { response.Reason = progression.Error; return; }
            var claim = seats[slot]; claim.identity = join.identity; claim.client = request.ClientNetworkId; claim.connected = true;claim.loaded=false;claim.expires=Time.unscaledTime+30; claim.commands = 0; claim.rateTime = Time.unscaledTime;
            connections[request.ClientNetworkId] = slot;
            if (!returning) actors[slot].Roster.Select(join.hero);
            response.Approved = true;
        }
        void Connected(ulong client)
        {
            if (!network.IsServer) return;
            // NGO approves its own host synchronously before this callback.
            if (!connections.TryGetValue(client, out int slot)) return;
            if (client == network.LocalClientId) localSlot = slot;
            actors[slot].Commands.Clear(); actors[slot].gameObject.SetActive(true); actors[slot].SetAuthority(false,network.IsServer);
            party.Set(slot, Phase==CoopPhase.Playing?null:actors[slot].Health);
            UpdateSoloBonus();
            profiles.Resend(slot);
            if (Phase == CoopPhase.Playing && dungeon.CurrentRoom != null) PlaceInRoom(slot, dungeon.CurrentRoom);
            Debug.Log("COOP_CONNECTED slot=" + slot + " count=" + ConnectedCount); Changed?.Invoke();
        }
        void Disconnected(ulong client)
        {
            if (network.IsServer && connections.TryGetValue(client, out int slot))
            {
                connections.Remove(client); seats[slot].connected = false; seats[slot].expires = Time.unscaledTime + reconnectWindow;
                actors[slot].Commands.Clear(); actors[slot].SetAuthority(false,network.IsServer); actors[slot].gameObject.SetActive(false); party.Set(slot, null);
                UpdateSoloBonus();Debug.Log("COOP_DISCONNECTED slot=" + slot); Changed?.Invoke();
            }
            else if (!network.IsServer && client == network.LocalClientId && !closing)
            { string reason=network.DisconnectReason;Close();Message="Conexão encerrada. Use o código para retornar ao mesmo host. "+reason; }
        }
        void TransportFailed(){if(!closing)transportLost=true;}
        void ServerStopped(bool wasHost){if(!closing&&Phase!=CoopPhase.Offline&&Phase!=CoopPhase.Closed)transportLost=true;}
        public void SelectHero(int index)
        {
            if (index < 0 || index >= HeroCount || Phase != CoopPhase.Offline && Phase != CoopPhase.Closed && Phase != CoopPhase.Lobby) return;
            selectedHero = index;
            if (!network.IsListening) actors[0].Roster.Select(index);
            else if (network.IsServer && localSlot >= 0) actors[localSlot].Roster.Select(index);
            else { using var writer = new FastBufferWriter(4, Allocator.Temp); writer.WriteValueSafe(index); network.CustomMessagingManager.SendNamedMessage("fdm.hero.v1", NetworkManager.ServerClientId, writer); }
            Changed?.Invoke();
        }
        void ReceiveHero(ulong sender, FastBufferReader reader)
        {
            if (!network.IsServer || Phase != CoopPhase.Lobby || reader.Length - reader.Position != 4 || !connections.TryGetValue(sender, out int slot)) return;
            reader.ReadValueSafe(out int index); if (index < 0 || index >= HeroCount) return; actors[slot].Roster.Select(index);
        }
        public void SelectCampaignLevel(int index)
        {
            if(Campaign==null||network.IsListening&&!(network.IsServer&&Phase==CoopPhase.Lobby)||index<0||index>=Campaign.Count)return;
            Campaign.Select(index);Changed?.Invoke();
        }
        public void ReturnToCampaignMap()
        {
            if(Campaign==null||!network.IsServer||Phase!=CoopPhase.Victory&&Phase!=CoopPhase.Defeat)return;
            Freeze();dungeon.Encounter.Cancel();SetPhase(CoopPhase.Lobby,"Grupo mantido. O host pode escolher a próxima fase e iniciar com o mesmo código.");
        }
        public void StartGame()
        {
            if (!network.IsServer || Phase != CoopPhase.Lobby || ConnectedCount == 0) return;
            expedition++;
            if(Campaign!=null){Campaign.Authority=true;Campaign.Prepare(Campaign.Selected);biome=Campaign.Level.biomeIndex;}
            SetPhase(CoopPhase.Loading, "Gerando a masmorra compartilhada…");
            dungeon.ConfigureTest(Campaign!=null?Campaign.Level.seed:12345, true, biome); dungeon.Encounter.enabled = false; loadingDeadline = Time.unscaledTime + 30;
            if (!progression.Begin()) { SetPhase(CoopPhase.Lobby, progression.Error); return; }
            for (int i = 0; i < actors.Length; i++) if (seats[i].connected)
            { seats[i].loaded = i == localSlot; actors[i].GetComponent<CombatBuildEffects>()?.ResetRun();actors[i].Roster.ResetHero(); actors[i].SetAuthority(false,network.IsServer); actors[i].Motor.Teleport(arena.Spawn.position + Vector3.right * (i * .9f), Quaternion.identity); }
            UpdateSoloBonus();for(int i=0;i<4;i++)if(seats[i].connected)actors[i].Health.Restore();
            follow.Configure(actors[localSlot].transform, 1 << 9); follow.Snap();
        }
        void ReceiveReady(ulong sender, FastBufferReader reader)
        {
            if (!network.IsServer || !connections.TryGetValue(sender, out int slot) || reader.Length - reader.Position < 4 || dungeon.Generator.Layout == null) return;
            reader.ReadValueSafe(out int count); if (count <= 0 || count > 512 || count != reader.Length - reader.Position) return;
            byte[] bytes = new byte[count]; reader.ReadBytesSafe(ref bytes, count);
            if (Encoding.UTF8.GetString(bytes) != dungeon.Generator.Layout.Signature()) { network.DisconnectClient(sender, "Layout incompatível."); return; }
            seats[slot].loaded = true;profiles.Resend(slot); Debug.Log("COOP_MAP_READY slot=" + slot);
            if(Phase==CoopPhase.Playing){if(!PlaceInRoom(slot,dungeon.CurrentRoom)){network.DisconnectClient(sender,"Sem ponto seguro de entrada.");return;}party.Set(slot,actors[slot].Health);actors[slot].SetAuthority(true);}
        }
        void SendReady()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(dungeon.Generator.Layout.Signature()); using var writer = new FastBufferWriter(bytes.Length + 4, Allocator.Temp);
            writer.WriteValueSafe(bytes.Length); writer.WriteBytesSafe(bytes); network.CustomMessagingManager.SendNamedMessage("fdm.ready.v1", NetworkManager.ServerClientId, writer); localLoaded = true;
        }
        public void SetInputBlocked(bool value) { inputBlocked = value; input.BlockCommands(value); touchButtons = HeroButtons.None; }
        public void Request(HeroButtons buttons) { if (!inputBlocked) touchButtons |= buttons; }
        void Update()
        {
            if(Campaign!=null){Campaign.Authority=network.IsServer;Campaign.Running=Phase==CoopPhase.Playing;}
            if(ready&&!closing&&(transportLost||!network.IsListening&&(Phase==CoopPhase.Playing||Phase==CoopPhase.Loading||Phase==CoopPhase.Lobby)))
            { if(progression.Active)progression.Finish(false,"Conexão interrompida");Close();Message="Conexão interrompida. Crie uma nova sala ou retorne pelo código do host.";return;}
            if (!ready || !network.IsListening) return;
            int hero = input.TakeHeroSlot(); if (hero >= 0) SelectHero(hero); if (input.TakeNextHero()) SelectHero((selectedHero + 1) % HeroCount);
            if (Phase == CoopPhase.Playing && localSlot >= 0)
            {
                if (input.TakeAttack() || input.AttackHeld) touchButtons |= HeroButtons.Attack;
                if (input.TakeDodge()) touchButtons |= HeroButtons.Dodge;
                if (input.TakeSpecial()) touchButtons |= HeroButtons.Special;
                if(input.TakeSuper())touchButtons|=HeroButtons.Super;
                if (interaction.TakeInteraction()) touchButtons |= HeroButtons.Interact;
                inputClock -= Time.unscaledDeltaTime;
                if (inputClock <= 0) { inputClock = 1f / inputRate; SendInput(); }
            }
            if (!network.IsServer) return;
            if (Phase == CoopPhase.Loading)
            {
                bool loaded = true; foreach (var seat in seats) if (seat.connected && !seat.loaded) loaded = false;
                if (loaded)
                {
                    dungeon.Encounter.enabled = true; foreach (var actor in actors) if (actor.gameObject.activeSelf) actor.SetAuthority(true);
                    SetPhase(CoopPhase.Playing, Campaign!=null?"Campanha cooperativa. F para interagir; entrem juntos nos setores.":"Masmorra cooperativa. F para interagir; grupo reunido ao entrar em cada sala."); Debug.Log("COOP_STARTED signature=" + dungeon.Generator.Layout.Signature());
                }
                else if (Time.unscaledTime > loadingDeadline) { Close(); Message = "Um cliente não confirmou o mapa a tempo. Sala encerrada."; return; }
            }
            if (Phase == CoopPhase.Playing)
            {
                for(int i=0;i<4;i++)if(seats[i].connected&&!seats[i].loaded&&Time.unscaledTime>seats[i].expires)network.DisconnectClient(seats[i].client,"Mapa não confirmado a tempo.");
                roomClock -= Time.deltaTime;
                if (roomClock <= 0) { roomClock = .08f; AdvanceRoomAndInteract(); }
                if (!party.AnyAlive || dungeon.Encounter.State == EncounterState.Failed) { Freeze();dungeon.Encounter.Cancel(); progression.Finish(false, "Derrota cooperativa"); SetPhase(CoopPhase.Defeat, "Todos os heróis caíram. Consulte o resultado e encerre a sala."); }
                else if (dungeon.Finished) { Freeze(); SetPhase(CoopPhase.Victory, "Masmorra concluída pelo grupo."); }
            }
            snapshotClock -= Time.unscaledDeltaTime;
            if (snapshotClock <= 0) { snapshotClock = 1f / snapshotRate; Broadcast(); }
        }
        Vector2 Aim()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (ProbeInput) return Vector2.ClampMagnitude(ProbeAim, 1);
#endif
            var actor = actors[localSlot]; Vector3 forward = actor.Motor.WorldDirection(input.Move);
            if (input.HasPointer)
            { Ray ray = follow.GetComponent<Camera>().ScreenPointToRay(input.PointerPosition); if (new Plane(Vector3.up, Vector3.up).Raycast(ray, out float distance)) forward = ray.GetPoint(distance) - actor.transform.position; }
            forward.y = 0; if (forward.sqrMagnitude < .01f) forward = actor.transform.forward;
            if(Application.isMobilePlatform&&!input.HasPointer)
            {
                float range=Mathf.Min(12,actor.AimRange+1),best=range*range;
                foreach(var enemy in dungeon.Encounter.Enemies.Actors)
                {
                    if(!enemy.IsAlive)continue;Vector3 offset=enemy.transform.position-actor.transform.position;offset.y=0;float distance=offset.sqrMagnitude;
                    if(distance<best&&!Physics.Linecast(actor.transform.position+Vector3.up,enemy.transform.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore)){best=distance;forward=offset;}
                }
            }
            forward.Normalize(); return new Vector2(forward.x, forward.z);
        }
        void SendInput()
        {
            Vector2 aim = Aim(), move = input.Move; sequence++;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (ProbeInput) move = Vector2.ClampMagnitude(ProbeMove, 1);
#endif
            if (network.IsServer) Accept(localSlot, sequence, move, aim, touchButtons);
            else
            {
                using var writer = new FastBufferWriter(CoopLimits.CommandBytes, Allocator.Temp);
                writer.WriteValueSafe(sequence); writer.WriteValueSafe(move.x); writer.WriteValueSafe(move.y); writer.WriteValueSafe(aim.x); writer.WriteValueSafe(aim.y); writer.WriteValueSafe((byte)touchButtons);
                network.CustomMessagingManager.SendNamedMessage("fdm.command.v1", NetworkManager.ServerClientId, writer, NetworkDelivery.ReliableSequenced);
            }
            touchButtons = HeroButtons.None;
        }
        void ReceiveCommand(ulong sender, FastBufferReader reader)
        {
            if (!network.IsServer || Phase != CoopPhase.Playing || reader.Length - reader.Position != CoopLimits.CommandBytes || !connections.TryGetValue(sender, out int slot)) return;
            var seat = seats[slot]; if (Time.unscaledTime - seat.rateTime >= 1) { seat.rateTime = Time.unscaledTime; seat.commands = 0; }
            if (++seat.commands > 45) return;
            reader.ReadValueSafe(out uint serial); reader.ReadValueSafe(out float x); reader.ReadValueSafe(out float y); reader.ReadValueSafe(out float aimX); reader.ReadValueSafe(out float aimY); reader.ReadValueSafe(out byte buttons);
            Accept(slot, serial, new Vector2(x, y), new Vector2(aimX, aimY), (HeroButtons)buttons);
        }
        void Accept(int slot, uint serial, Vector2 move, Vector2 aim, HeroButtons buttons)
        {
            var actor = actors[slot]; if (!seats[slot].connected || !seats[slot].loaded || !actor.Health.IsAlive || !actor.Commands.Accept(serial, move, aim, buttons)) return;
            if (!actor.Motor.FacingLocked && aim.sqrMagnitude > .01f) actor.transform.rotation = Quaternion.LookRotation(new Vector3(aim.x, 0, aim.y));
        }
        void AdvanceRoomAndInteract()
        {
            foreach (var actor in actors)
            {
                if (!actor.gameObject.activeSelf || !actor.Motor.enabled || !actor.Health.IsAlive) continue;
                foreach (var room in dungeon.Generator.Rooms)
                    if ((room != dungeon.CurrentRoom||dungeon.OpenLandscape) && room.Contains(actor.transform.position) && (Campaign==null||dungeon.OpenLandscape||PartyInside(room)))
                    {
                        dungeon.Enter(room);
                        // One encounter controller: assemble the group inside before doors close.
                        for (int i = 0; i < actors.Length; i++) if (seats[i].connected && Campaign==null)
                            PlaceInRoom(i, room);
                        break;
                    }
                if (actor.Commands.TakeInteraction())
                {
                    if(Campaign!=null&&(TryReviveAlly(actor)||Campaign.Adventure!=null&&Campaign.Adventure.TryUse(actor.Health)))continue;
                    foreach(var sector in dungeon.Generator.Rooms)if(sector.Contains(actor.transform.position))foreach (var item in sector.interactions) if ((item.transform.position - actor.transform.position).sqrMagnitude <= 4.41f && item.UseBy(actor.Health)) break;
                }
            }
        }
        bool TryReviveAlly(CoopActor rescuer)
        {
            if(!network.IsServer||dungeon.Encounter.Enemies.ThreatNear(rescuer.transform.position,6))return false;
            for(int i=0;i<actors.Length;i++)
            {
                var fallen=actors[i];if(!seats[i].connected||fallen==rescuer||fallen.Health.IsAlive)continue;
                if((fallen.transform.position-rescuer.transform.position).sqrMagnitude>4.41f)continue;
                if(Physics.Linecast(rescuer.transform.position+Vector3.up,fallen.transform.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore))continue;
                if(fallen.Health.Revive(.35f)){Message="Aliado levantado. Avancem juntos.";return true;}
            }
            return false;
        }
        bool PartyInside(DungeonRoom room)
        {
            foreach(var actor in actors)if(actor.gameObject.activeSelf&&actor.Motor.enabled&&actor.Health.IsAlive&&!room.Contains(actor.transform.position))return false;
            return true;
        }
        public bool PlaceInRoom(int slot, DungeonRoom room)
        {
            if (slot < 0 || slot >= 4 || room == null) return false;
            // Sample the baked room navigation and check the actual capsule against walls/props.
            for (int step = 0; step < 36; step++)
            {
                Vector3 desired = room.Center + new Vector3(-3 + (step + slot) % 6 * 1.2f, .06f, -3 + (step + slot) / 6 * 1.2f);
                if (!NavMesh.SamplePosition(desired, out var hit, .65f, NavMesh.AllAreas) || !room.Contains(hit.position)) continue;
                Vector3 point = hit.position + Vector3.up * .06f;
                if (Physics.CheckCapsule(point + Vector3.up * .4f, point + Vector3.up * 1.45f, .34f, 1 << 9, QueryTriggerInteraction.Ignore)) continue;
                actors[slot].Motor.Teleport(point, Quaternion.identity); return true;
            }
            Debug.LogError("No safe cooperative respawn point in room " + room.node.id); return false;
        }
        void Broadcast()
        {
            if (!handlers) BindMessages(); frame.tick = ++tick; frame.phase = (int)Phase; frame.seed = dungeon.Seed; frame.fixedMap = dungeon.FixedMap; frame.biome = biome;
            // A lobby may retain the previous dungeon locally, but must not advertise it
            // with the next selected campaign level to a newly joining client.
            bool hasWorld=Phase!=CoopPhase.Lobby&&dungeon.Generator.Layout!=null;
            frame.expedition=expedition;frame.campaignLevel=Campaign!=null?Campaign.Selected:-1;frame.campaign=hasWorld&&Campaign!=null?Campaign.Capture():null;
            frame.room = hasWorld&&dungeon.CurrentRoom != null ? dungeon.CurrentRoom.node.id : -1; frame.signature = hasWorld ? dungeon.Generator.Layout.Signature() : "";
            frame.encounter = (int)dungeon.Encounter.State; frame.finished = dungeon.Finished; frame.hasKey = dungeon.HasKey;
            frame.players.Clear(); frame.enemies.Clear(); frame.effects.Clear();
            for (int i = 0; i < actors.Length; i++)
            {
                var actor = actors[i]; var value = playerFrames[i]; value.connected = seats[i].connected; value.hero = actor.Roster.Selected; Fill(value, actor.Health, actor.transform);
                value.counter=actor.GetComponent<CombatBuildEffects>()?.CounterReady??false;value.boons=actor.GetComponent<CombatBuildEffects>()?.PackedBoons??0;value.charge=actor.Combat.ChargeProgress;value.super=actor.Combat.SuperProgress;value.attackRange=actor.Combat.BasicAttack!=null?actor.Combat.BasicAttack.range:3;
                value.variant=actor.Combat.AnimationVariant;value.actionVersion=actor.Combat.ActionVersion;value.impact=actor.Combat.ImpactTime;
                value.state = (int)actor.Presentation.State; value.animation = actor.Combat.NormalizedTime; value.specialCooldown = actor.Combat.SpecialCooldownRemaining; value.dodgeCooldown = actor.Motor.DodgeCooldownRemaining;value.fury=actor.Combat.FuryRemaining; frame.players.Add(value);
                var profile = progression.Profile(i)?.Profile; var loadout = profile?.Loadout(actor.Roster.Current.heroId);
                value.mainItem = profile?.Item(loadout?.items[0])?.definitionId;
                value.secondaryItem = profile?.Item(loadout?.items[1])?.definitionId;
            }
            var enemies = dungeon.Encounter.Enemies.Actors;
            for (int i = 0; i < enemies.Length; i++) if (enemies[i].IsSpawned)
            { var value = enemyFrames[i]; value.opening=enemies[i].Profile.miniboss&&enemies[i].Combat.IsRecovering;value.ailments=enemies[i].GetComponent<CombatAilments>()?.Bits??0;value.spawnWarning=enemies[i].SpawnRemaining;value.kind = (int)enemies[i].Profile.kind; value.state = (int)enemies[i].Brain.State; Fill(value, enemies[i].Health, enemies[i].transform); frame.enemies.Add(value); }
            replica.CaptureEffects(frame.effects); frame.world = dungeon.CaptureReplica();
            foreach (var link in connections)
            {
                profiles.Sync(link.Key, link.Value);
                if (link.Key == network.LocalClientId) continue; frame.localSlot = link.Value;
                byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(frame));
                LastSnapshotBytes = bytes.Length;
                if (bytes.Length > CoopLimits.MaximumSnapshotBytes) { Debug.LogError("Snapshot exceeds the configured network budget."); continue; }
                using var writer = new FastBufferWriter(bytes.Length + 4, Allocator.Temp); writer.WriteValueSafe(bytes.Length); writer.WriteBytesSafe(bytes);
                network.CustomMessagingManager.SendNamedMessage("fdm.frame.v1", link.Key, writer, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
        static void Fill(ActorFrame value, Health hp, Transform root) { value.hp = hp.Current; value.maximum = hp.Maximum; value.position = root.position; value.yaw = root.eulerAngles.y; }
        void ReceiveFrame(ulong sender, FastBufferReader reader)
        {
            if (network.IsServer || sender != NetworkManager.ServerClientId || reader.Length - reader.Position < 4) return;
            reader.ReadValueSafe(out int length); if (length <= 0 || length > CoopLimits.MaximumSnapshotBytes || length != reader.Length - reader.Position) return;
            byte[] bytes = new byte[length]; reader.ReadBytesSafe(ref bytes, length);
            WorldFrame update;
            try { update = JsonUtility.FromJson<WorldFrame>(Encoding.UTF8.GetString(bytes)); } catch (Exception) { return; }
            if (update == null || update.players == null || update.players.Count != 4 || update.localSlot < 0 || update.localSlot >= 4 || unchecked((int)(update.tick - tick)) <= 0) return;
            if(Campaign!=null&&(update.campaignLevel<0||update.campaignLevel>=Campaign.Count)){Close();Message="Fase incompatível.";return;}
            if(Campaign!=null)Campaign.Select(update.campaignLevel);
            tick = update.tick; localSlot = update.localSlot;replicaEncounter=update.encounter;
            LastSnapshotBytes = length; AppliedEnemyCount = update.enemies.Count; AppliedEffectCount = update.effects.Count;
            if (update.room >= 0 && (dungeon.Generator.Layout == null || dungeon.Generator.Layout.Signature() != update.signature||loadedExpedition!=update.expedition))
            {
                localLoaded=false;loadedExpedition=update.expedition;
                SetPhase(CoopPhase.Loading, "Carregando o mapa da autoridade…"); if(Campaign!=null)Campaign.Prepare(update.campaignLevel); dungeon.ConfigureTest(update.seed, update.fixedMap, update.biome);
                if (dungeon.Generator.Layout.Signature() != update.signature) { Close(); Message = "Layout incompatível: conexão encerrada."; return; }
                foreach (var trap in dungeon.Generator.Traps) trap.enabled = false;
            }
            if (update.room >= 0 && !localLoaded) SendReady();
            for (int i = 0; i < actors.Length; i++)
            { var value = update.players[i]; seats[i].connected = value.connected; actors[i].gameObject.SetActive(value.connected); if (value.connected) { actors[i].Apply(value); actors[i].ApplyGear(value, progression.Catalog); } }
            if (follow.Target != actors[localSlot].transform) { follow.Configure(actors[localSlot].transform, 1 << 9); follow.Snap(); }
            replica.Apply(update); if (update.room >= 0) dungeon.ApplyReplica(update.world);if(Campaign!=null&&update.room>=0)Campaign.Apply(update.campaign); selectedHero = update.players[localSlot].hero;
            var next = (CoopPhase)update.phase; if (Phase != next) SetPhase(next, next == CoopPhase.Playing ? "Sessão conectada: simulação validada pelo host." : next.ToString());
        }
        void Freeze() { dungeon.Encounter.enabled = false; foreach (var actor in actors) if (actor != null) { actor.Commands.Clear(); actor.SetAuthority(false,network.IsServer); } }
        void SetPhase(CoopPhase value, string text) { Phase = value;if(Campaign!=null)Campaign.Running=value==CoopPhase.Playing; Message = text; Changed?.Invoke(); Debug.Log("COOP_PHASE " + value); }
        public void Close()
        {
            if (closing) return; closing = true;
            if (network.IsServer && progression != null && progression.Active) progression.Finish(false, "Sala encerrada");
            Freeze(); dungeon.Encounter.Cancel(); replica.Clear(); network.Shutdown(); handlers = false; SetInputBlocked(false);
            foreach (var seat in seats) { seat.connected = false; seat.identity = null; } connections.Clear(); localSlot = -1;progression.ReleaseHome();
            actors[0].Roster.Select(selectedHero);
            SetPhase(CoopPhase.Closed, "Sala encerrada. O progresso local anterior foi preservado.");
        }
        void OnDestroy()
        {
            if (network != null && network.IsServer && progression != null && progression.Active) progression.Finish(false, "Aplicação encerrada");
            if (network == null) return; network.OnClientConnectedCallback -= Connected; network.OnClientDisconnectCallback -= Disconnected;network.OnTransportFailure-=TransportFailed;network.OnServerStopped-=ServerStopped;
            if (network.IsListening) network.Shutdown();
        }
        void OnApplicationQuit() { if (network != null && network.IsServer && progression != null && progression.Active) progression.Finish(false, "Aplicação encerrada"); }
    }
}
