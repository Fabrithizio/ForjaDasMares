using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ForjaDasMares.Networking
{
    // First region is a local exploration prototype. No public-session or economy claims.
    [DefaultExecutionOrder(220)]public sealed class RegionalWorld:MonoBehaviour
    {
        [SerializeField] RegionDefinition definition;
        [SerializeField] HeroRoster hero;[SerializeField] EnemyPool enemies;[SerializeField] ArenaCamera follow;
        [SerializeField] SoundscapeLibrary soundscape;
        readonly Dictionary<EnemyActor,int> ownership=new Dictionary<EnemyActor,int>(24);
        float[][] timers;EnemyActor[][] residents;int[] firstKills;bool[] cleared;bool[][] firstDefeated;bool travelling;float poll;
        TownHub town;
        PlayerMotor travelMotor;
        HeroCombat travelCombat;
        GameObject mount;
        Material mountMetal, mountCrystal;
        int mountedAction;
        public bool IsMounted => mount != null && mount.activeSelf;

        public void ToggleMount()
        {
            if (travelling || !hero.Health.IsAlive || hero.GetComponent<ArenaInput>().CombatBlocked) return;
            if (IsMounted) { Dismount(); return; }
            if (IsCatacombInstance || travelCombat.IsBusy || travelMotor.IsDodging || enemies.ThreatNear(hero.transform.position,9)) return;
            if (mount == null) BuildMount();
            mountedAction = travelCombat.ActionVersion;
            mount.SetActive(true);
            travelMotor.TravelSpeedMultiplier = 1.8f;
        }

        public void Dismount()
        {
            if (mount != null) mount.SetActive(false);
            if (travelMotor != null) travelMotor.TravelSpeedMultiplier = 1f;
        }

        void BuildMount()
        {
            mount = new GameObject("Prancha Rúnica");
            mount.transform.SetParent(hero.transform,false);
            mountMetal = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mountMetal.SetColor("_BaseColor",new Color(.16f,.26f,.31f));
            mountCrystal = new Material(mountMetal);
            mountCrystal.SetColor("_BaseColor",new Color(.15f,.75f,.86f));
            mountCrystal.EnableKeyword("_EMISSION");
            mountCrystal.SetColor("_EmissionColor",new Color(.04f,.36f,.44f));

            AddMountPiece(PrimitiveType.Sphere,new Vector3(0,.07f,0),new Vector3(.92f,.14f,1.65f),mountMetal,0);
            AddMountPiece(PrimitiveType.Sphere,new Vector3(0,.08f,.64f),new Vector3(.37f,.16f,.59f),mountCrystal,0);
            AddMountPiece(PrimitiveType.Cube,new Vector3(-.42f,.045f,-.08f),new Vector3(.17f,.09f,.91f),mountCrystal,-13);
            AddMountPiece(PrimitiveType.Cube,new Vector3(.42f,.045f,-.08f),new Vector3(.17f,.09f,.91f),mountCrystal,13);
        }

        void AddMountPiece(PrimitiveType shape,Vector3 position,Vector3 scale,Material surface,float yaw)
        {
            var piece = GameObject.CreatePrimitive(shape);
            piece.layer = 2;
            piece.GetComponent<Collider>().enabled = false;
            piece.transform.SetParent(mount.transform,false);
            piece.transform.localPosition = position;
            piece.transform.localRotation = Quaternion.Euler(0,yaw,0);
            piece.transform.localScale = scale;
            piece.GetComponent<Renderer>().sharedMaterial = surface;
        }
        internal RegionalWorldHud Hud { get; set; }
        RegionalInventory regionalInventory;
        const int GoldCapacity = 20;
        readonly WorldClickTarget[] goldPiles = new WorldClickTarget[GoldCapacity];
        readonly int[] goldAmounts = new int[GoldCapacity];
        readonly int[] killsSinceDrop = new int[32];
        int dropSequence, goldCursor;
        Material goldMaterial;
        readonly Vector3[] goldOrigins = new Vector3[GoldCapacity];
        Material altarMaterial;
        public bool IsCatacombInstance => definition.regionId == "catacumbas-previa";
        public const int CatacombCardCount = 32, CatacombPicks = 2;
        public const int RuneCard = -1, StoneCard = -2;
        readonly int[] cardOrder = new int[CatacombCardCount];
        readonly bool[] revealedCards = new bool[CatacombCardCount];
        bool claimingCard, pendingCards;
        public int CardsRevealed { get; private set; }
        public bool IsCardRevealed(int card) => revealedCards[card];
        public bool CardsFinalized { get; private set; }
        public string CardRewardLabel(int card) => CardsFinalized ? RewardLabel(cardOrder[card]) : "";
        public int VisibleCardReward(int card) => CardsFinalized ? cardOrder[card] : 0;
        static string RewardLabel(int reward) => reward == RuneCard ? "Runa de Coleta" :
            reward == StoneCard ? "Pedra de Têmpera" : reward + " ouro";
        RegionDefinition testRoom;
        Material testRoomWallMaterial;
        public string GoldNotice { get; private set; }
        float goldNoticeUntil;
        public string GoldFeedback => Time.unscaledTime < goldNoticeUntil ? GoldNotice : null;
        public RegionDefinition Definition=>definition;public HeroRoster Hero=>hero;public EnemyPool Enemies=>enemies;
        public RegionalJourney Journey {get;private set;}
        public void SetJourney(RegionalJourney value)=>Journey=value;
        public int ClearedCount{get{int n=0;if(cleared!=null)foreach(bool c in cleared)if(c)n++;return n;}}
        public bool PortalReady=>ClearedCount==definition.camps.Length;
        public bool IsCampCleared(int index)=>cleared!=null&&index>=0&&index<cleared.Length&&cleared[index];
        public float RefugeDistance=>Vector3.Distance(hero.transform.position,definition.refuge);
        public event Action Changed;
        public void Configure(RegionDefinition data,HeroRoster player,EnemyPool pool,ArenaCamera camera,SoundscapeLibrary audio){definition=data;hero=player;enemies=pool;follow=camera;soundscape=audio;}
        void Start()
        {
            if(Journey==null)Journey=GetComponent<RegionalJourney>();
            if (IsCatacombInstance) ConfigureCatacombTestRoom();
            town=GetComponent<TownHub>();
            travelMotor = hero.GetComponent<PlayerMotor>();
            travelCombat = hero.GetComponent<HeroCombat>();
            hero.Health.Damaged += Dismount;
            regionalInventory=GetComponent<RegionalInventory>();
            hero.GetComponent<ArenaInput>().Desktop?.ConfigureAutoAim(enemies);
            var quality=follow.GetComponent<PresentationQuality>();if(quality!=null)quality.Configure(follow.GetComponent<Camera>());
            follow.UseRpgFraming();
            int heroClass = CampaignHome.CharacterClass;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string[] arguments = Environment.GetCommandLineArgs();
            int previewClass = Array.IndexOf(arguments,"-netHero");
            if (Array.IndexOf(arguments,"-netSavePath") >= 0 && previewClass >= 0 && previewClass + 1 < arguments.Length &&
                int.TryParse(arguments[previewClass+1],out int configuredClass)) heroClass = configuredClass;
#endif
            hero.Select(Mathf.Clamp(heroClass,0,hero.Count-1));hero.enabled=false;hero.GetComponent<HeroCombat>().ConfigureFlow(hero.Current.onlineFlow);hero.Changed+=HeroChanged;
            hero.GetComponent<PlayerMotor>().Teleport(definition.refuge,Quaternion.identity);follow.SetFocusBounds(definition.bounds,true);follow.Snap();
            residents=new EnemyActor[definition.camps.Length][];timers=new float[residents.Length][];firstKills=new int[residents.Length];cleared=new bool[residents.Length];firstDefeated=new bool[residents.Length][];
            for(int i=0;i<residents.Length;i++){residents[i]=new EnemyActor[definition.camps[i].enemies.Length];timers[i]=new float[residents[i].Length];firstDefeated[i]=new bool[residents[i].Length];}
            // The old Catacombs (and their altar) left the game: the new places of Chapter 1 replace them.
            if (town != null) { mapStation = BuildMapStation(); BuildProtectorCrystal(); }
            // Gates to the neighbour places (world map passages).
            if (!IsCatacombInstance) PortoesDoMundo.Criar(this);
            // You fell somewhere else: the web pulls you back to the Nó that recorded you.
            if (Historia.RenascerNoNo && mapStation != null) { Historia.RenascerNoNo = false; hero.GetComponent<PlayerMotor>().Teleport(NoSpot,Quaternion.Euler(0,180,0)); follow.Snap(); }
            if (IsCatacombInstance)
            {
                for (int i = 0; i < cardOrder.Length; i++) cardOrder[i] = 15 + (i % 6) * 5;
                cardOrder[0] = RuneCard;
                cardOrder[1] = cardOrder[2] = StoneCard;
                for (int i = cardOrder.Length - 1; i > 0; i--)
                {
                    int other = UnityEngine.Random.Range(0,i+1);
                    (cardOrder[i],cardOrder[other]) = (cardOrder[other],cardOrder[i]);
                }
            }
            enemies.SetLimit(definition.activeEnemyLimit);enemies.Died+=EnemyDied;
            AudioListener.volume=Mathf.Clamp01(PlayerPrefs.GetFloat("CampaignMasterVolume",.8f));
            if(soundscape!=null){Voice("RegionalMusic",town!=null?soundscape.menu.music:soundscape.biomes[0].music,.26f*CampaignSoundscape.Volume(CampaignSoundscape.MusicKey));Voice("RegionalAmbience",soundscape.menu.ambience,.16f*CampaignSoundscape.Volume(CampaignSoundscape.AmbienceKey));}
            Debug.Log("REGION_READY localPrototype=true camps="+residents.Length+" capacity="+enemies.Capacity);
        }
        void Voice(string label,AudioClip clip,float volume){var go=new GameObject(label);go.transform.SetParent(transform,false);var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=true;source.spatialBlend=0;source.clip=clip;source.volume=volume;source.Play();}
        void HeroChanged(){hero.GetComponent<HeroCombat>().ConfigureFlow(hero.Current.onlineFlow);}
        void EnemyDied(EnemyActor actor)
        {
            if(!ownership.TryGetValue(actor,out int camp))return;ownership.Remove(actor);
            regionalInventory?.Service?.GrantRegionalEnemyExperience();
            if (!IsCatacombInstance) TryDropGold(camp, actor.transform.position);
            var members=residents[camp];for(int i=0;i<members.Length;i++)if(members[i]==actor){members[i]=null;timers[camp][i]=definition.camps[camp].respawnSeconds;if(!firstDefeated[camp][i]){firstDefeated[camp][i]=true;firstKills[camp]++;}break;}
            if(!cleared[camp]){if(firstKills[camp]>=members.Length){cleared[camp]=true;if(IsCatacombInstance && PortalReady)pendingCards=true;Changed?.Invoke();}}
        }
        void TryDropGold(int camp, Vector3 position)
        {
            if (regionalInventory?.Service == null) return;
            // Alternate between two and three kills. The first kill starts a pile.
            int counter = camp % killsSinceDrop.Length;
            if (killsSinceDrop[counter] > 0) { killsSinceDrop[counter]--; return; }
            killsSinceDrop[counter] = 1 + (dropSequence++ & 1);
            if (UnityEngine.AI.NavMesh.SamplePosition(position,out var walkable,1.5f,UnityEngine.AI.NavMesh.AllAreas))
                position = walkable.position;
            int amount = 8 + Mathf.Min(4,camp) * 2;
            for (int offset = 0; offset < GoldCapacity; offset++)
            {
                int slot = (goldCursor + offset) % GoldCapacity;
                if (goldPiles[slot] != null && goldPiles[slot].gameObject.activeSelf) continue;
                if (goldPiles[slot] == null) goldPiles[slot] = CreateGoldPile(slot);
                goldAmounts[slot] = amount;
                var pile = goldPiles[slot];
                pile.Configure("Recolher " + amount + " ouro");
                pile.HoverLabel = amount + " ouro";
                goldOrigins[slot] = position;
                pile.transform.position = position;
                pile.gameObject.SetActive(true);
                goldCursor = (slot + 1) % GoldCapacity;
                return;
            }
            int nearest = 0;
            for (int i = 1; i < GoldCapacity; i++)
                if ((goldPiles[i].transform.position-position).sqrMagnitude < (goldPiles[nearest].transform.position-position).sqrMagnitude) nearest = i;
            goldAmounts[nearest] = Mathf.Min(1000,goldAmounts[nearest] + amount);
            goldPiles[nearest].Configure("Recolher " + goldAmounts[nearest] + " ouro");
            goldPiles[nearest].HoverLabel = goldAmounts[nearest] + " ouro";
        }

        WorldClickTarget CreateGoldPile(int slot)
        {
            var root = new GameObject("Ouro no chão");
            root.transform.SetParent(transform,false);
            var hit = root.AddComponent<BoxCollider>();
            var heroCollider = hero.GetComponent<CharacterController>();
            if (heroCollider != null) Physics.IgnoreCollision(hit,heroCollider);
            hit.center = new Vector3(0,.18f,0);
            hit.size = new Vector3(.88f,.36f,.88f);
            var target = root.AddComponent<WorldClickTarget>();
            target.Interacting += actor => CollectGold(slot,actor);
            if (goldMaterial == null)
            {
                goldMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                goldMaterial.SetColor("_BaseColor",new Color(1f,.65f,.12f));
                goldMaterial.SetFloat("_Metallic",.42f);
                goldMaterial.SetFloat("_Smoothness",.72f);
            }
            for (int i = 0; i < 4; i++)
            {
                var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                coin.transform.SetParent(root.transform,false);
                coin.transform.localPosition = new Vector3((i%2-.5f)*.30f,.08f+i*.027f,(i/2-.5f)*.28f);
                coin.transform.localScale = new Vector3(.29f,.04f,.29f);
                coin.GetComponent<Collider>().enabled = false;
                var renderer = coin.GetComponent<Renderer>();
                renderer.sharedMaterial = goldMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return target;
        }

        void CollectGold(int slot, Transform actor)
        {
            if (actor != hero.transform || !hero.Health.IsAlive || goldAmounts[slot] <= 0) return;
            int amount = goldAmounts[slot];
            goldAmounts[slot] = 0;
            if (!regionalInventory.Service.CollectRegionalGold(amount))
            {
                goldAmounts[slot] = amount;
                return;
            }
            goldPiles[slot].gameObject.SetActive(false);
            GoldNotice = "+" + amount + " OURO";
            goldNoticeUntil = Time.unscaledTime + 1.6f;
        }

        WorldClickTarget NearbyGold()
        {
            WorldClickTarget nearest = null;
            float best = float.MaxValue;
            foreach (var pile in goldPiles)
            {
                if (pile == null || !pile.isActiveAndEnabled || !pile.InRange(hero.transform)) continue;
                float distance = (pile.transform.position-hero.transform.position).sqrMagnitude;
                if (distance < best) { nearest = pile; best = distance; }
            }
            return nearest;
        }

        public bool TryTouchGold(Vector2 screenPosition)
        {
            var camera = follow.GetComponent<Camera>();
            if (!Physics.Raycast(camera.ScreenPointToRay(screenPosition),out var hit,150,
                (1<<0)|(1<<8)|(1<<9),QueryTriggerInteraction.Ignore)) return false;
            if (mapStation != null && (hit.point - mapStation.transform.position).sqrMagnitude < 36) return mapStation.TryUse(hero.transform);
            var target = hit.collider.GetComponentInParent<WorldClickTarget>();
            if (target == null || (target != catacombAltar && Array.IndexOf(goldPiles,target) < 0)) return false;
            return target.TryUse(hero.transform);
        }

        void Update()
        {
            if(residents==null||travelling)return;
            if (IsMounted && (travelCombat.ActionVersion != mountedAction || travelMotor.IsDodging || !hero.Health.IsAlive)) Dismount();
            var commands=(IHeroCommands)hero.GetComponent<PlayerMotor>().Commands;
            commands.TakeReset();
            commands.TakeHeroSlot();
            commands.TakeNextHero();
            if(!hero.Health.IsAlive)return;
            if(hero.transform.position.y< -5)Respawn();
            if (pendingCards && Hud?.OpenCatacombCards() == true) pendingCards = false;
            UpdateCollectionRune();
            poll-=Time.deltaTime;if(poll>0)return;float elapsed=.25f-poll;poll=.25f;
            for(int c=0;c<residents.Length;c++)for(int i=0;i<residents[c].Length;i++)
            {
                timers[c][i]=Mathf.Max(0,timers[c][i]-elapsed);
                if(residents[c][i]!=null||timers[c][i]>0 || (IsCatacombInstance && firstDefeated[c][i]))continue;
                // The boss of the room only arrives after its guards fall.
                if(IsCatacombInstance&&definition.camps[c].enemies[i]!=null&&definition.camps[c].enemies[i].kind==EnemyKind.Boss&&firstKills[c]<GuardsOf(c))continue;
                var data=definition.camps[c];var point=data.spawns[i%data.spawns.Length];float distance=(hero.transform.position-point).sqrMagnitude;
                if(distance>75*75||(!IsCatacombInstance&&distance<8*8))continue;
                var actor=enemies.TrySpawn(data.enemies[i],point);if(actor!=null){if(IsCatacombInstance){bool boss=data.enemies[i].kind==EnemyKind.Boss;actor.Health.ApplyAttributes(boss?260:22,0);actor.Health.Restore();actor.Combat.NormalDamageScale=.35f;}residents[c][i]=actor;ownership[actor]=c;}
            }
        }
        public string InteractionHint => IsCatacombInstance && PortalReady ? "F · Abrir cartas" : NearbyGold() is WorldClickTarget pile ? "F · "+pile.Label : mapStation != null && mapStation.InRange(hero.transform) ? "F · Raiz-Mãe do Refúgio (mapa)" : catacombAltar != null && catacombAltar.InRange(hero.transform) ? "F · Altar das Catacumbas" : RefugeDistance<5&&CanRest?"F · Descansar no refúgio":Vector3.Distance(hero.transform.position,definition.portal)<5?(PortalReady?(IsCatacombInstance?"F · Escolher carta":"F · Entrar nos desafios"):"Investigue os três acampamentos para abrir o portal"):"";
        public void Interact()
        {
            if(!hero.Health.IsAlive||travelling)return;
            if (IsCatacombInstance && PortalReady)
            {
                Hud?.OpenCatacombCards();
                return;
            }
            if(NearbyGold()?.TryUse(hero.transform)==true)return;
            if(mapStation!=null&&mapStation.TryUse(hero.transform))return;
            if(town!=null&&town.TryInteractNearest())return;
            if(Journey!=null&&Journey.Interact())return;
            if(RefugeDistance<5&&CanRest){hero.Health.Restore();Changed?.Invoke();}
            else if(Journey==null&&PortalReady&&Vector3.Distance(hero.transform.position,definition.portal)<5)
            {
                if (IsCatacombInstance) Hud?.OpenCatacombCards();
                else ReturnToCampaign();
            }
        }
        void UpdateCollectionRune()
        {
            var service = regionalInventory?.Service;
            int level = service?.CollectionRuneEquipped == true ? service.CollectionRuneLevel : 0;
            float radius = level == 1 ? .65f : level == 2 ? 2.5f : 4.5f;
            for (int i = 0; i < GoldCapacity; i++)
            {
                var pile = goldPiles[i];
                if (pile == null || !pile.isActiveAndEnabled) continue;
                Vector3 destination = hero.transform.position;
                Vector3 delta = destination - goldOrigins[i];
                bool canCollect = level > 0 && delta.sqrMagnitude <= radius * radius &&
                    !Physics.Linecast(goldOrigins[i]+Vector3.up*.25f,destination+Vector3.up*.25f,1<<9,QueryTriggerInteraction.Ignore);
                if (!canCollect)
                {
                    pile.transform.position = goldOrigins[i];
                    continue;
                }
                if (level == 1 || (pile.transform.position-destination).sqrMagnitude <= .22f)
                    CollectGold(i,hero.transform);
                else pile.transform.position = Vector3.MoveTowards(pile.transform.position,destination,12f*Time.deltaTime);
            }
        }

        WorldClickTarget BuildPlazaStation(string label, Vector3 offset, Action open)
        {
            var altar = new GameObject(label);
            altar.transform.SetParent(transform,false);
            altar.transform.position = definition.refuge + offset - Vector3.up * definition.refuge.y;
            if (altarMaterial == null)
            {
                altarMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                altarMaterial.SetColor("_BaseColor",new Color(.26f,.47f,.52f));
            }
            for (int i = 0; i < 3; i++)
            {
                var stone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stone.transform.SetParent(altar.transform,false);
                stone.transform.localPosition = Vector3.up * (.16f+i*.32f);
                stone.transform.localScale = new Vector3(1.25f-i*.31f,.16f,1.25f-i*.31f);
                stone.GetComponent<Renderer>().sharedMaterial = altarMaterial;
            }
            var target = altar.AddComponent<WorldClickTarget>();
            target.Configure(label,3);
            target.Interacting += actor =>
            {
                if (actor == hero.transform && hero.Health.IsAlive)
                    open();
            };
            return target;
        }
        WorldClickTarget catacombAltar;

        // The Nó do Refúgio: the root portal (the tree with the cracked core, beside the Expedições hall).
        // Touching it opens the world map and records the player in the root (falling brings you back here).
        // A ring of light on the ground marks where to stand.
        WorldClickTarget mapStation;
        public Transform No => mapStation != null ? mapStation.transform : null;
        Vector3 NoSpot
        {
            get
            {
                var p = mapStation.transform.position;
                return UnityEngine.AI.NavMesh.SamplePosition(p,out var hit,4,UnityEngine.AI.NavMesh.AllAreas) ? hit.position : p;
            }
        }
        WorldClickTarget BuildMapStation()
        {
            // Where the tree is: its sign (the scene still calls it "Estação Antiga"). Without the sign: beside the instance hall.
            Vector3 place = new Vector3(-13.5f,0,23);
            foreach (var text in FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
                if (text.text != null && text.text.ToUpperInvariant().Contains("ESTAÇÃO ANTIGA"))
                { text.text = "RAIZ-MÃE DO REFÚGIO"; place = new Vector3(text.transform.position.x,0,text.transform.position.z); }
            // The old service that stood there (the "santuário") gives way to the Nó.
            foreach (var old in FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None))
                if (old.Label.IndexOf("santuário",StringComparison.OrdinalIgnoreCase) >= 0) old.enabled = false;
            var station = new GameObject("No do Refugio");
            station.transform.SetParent(transform,false);
            station.transform.position = place;
            var ring = station.AddComponent<LineRenderer>();
            const int Steps = 48; const float Radius = 2.4f;
            ring.loop = true; ring.positionCount = Steps; ring.useWorldSpace = false; ring.widthMultiplier = .14f;
            for (int i = 0; i < Steps; i++) { float a = i * Mathf.PI * 2 / Steps; ring.SetPosition(i,new Vector3(Mathf.Cos(a)*Radius,.07f,Mathf.Sin(a)*Radius)); }
            var glow = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            glow.SetColor("_BaseColor",new Color(.45f,1f,.85f,1));
            ring.sharedMaterial = glow; ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var target = station.AddComponent<WorldClickTarget>();
            target.Configure("Raiz-Mãe do Refúgio",4.5f);
            target.Interacting += actor =>
            {
                if (actor != hero.transform || !hero.Health.IsAlive || MapaDoMundo.Aberto) return;
                DescobertasDoMapa.Descobrir("refugio");
                // The bell is the Nó's signal: it rings when the root records you.
                bool first = Historia.CenaDoNo != SceneManager.GetActiveScene().name;
                Historia.GravarNo(SceneManager.GetActiveScene().name);
                if (first) AudioSource.PlayClipAtPoint(Historia.Sino(true),station.transform.position,.9f);
                MapaDoMundo.Abrir(this,true);
            };
            return target;
        }

        // The protector crystal in the middle of the plaza: pure sap of the web, it keeps the Breu away (that is why
        // the city is safe). It floats over the monument, turning slowly, with its own light.
        void BuildProtectorCrystal()
        {
            float top = 4f;
            var monument = GameObject.Find("Monumento");
            if (monument != null)
            {
                var renderers = monument.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0) { var b = renderers[0].bounds; foreach (var r in renderers) b.Encapsulate(r.bounds); top = b.max.y; }
            }
            var crystal = new GameObject("Cristal Protetor");
            crystal.transform.SetParent(transform,false);
            crystal.transform.position = new Vector3(0,top + 1.6f,0);
            var mesh = new Mesh { name = "Cristal" };
            const int Sides = 6; const float Up = 2.2f, Down = 1.3f, Width = .75f;
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int i = 0; i < Sides; i++)
            {
                float a0 = i * Mathf.PI * 2 / Sides, a1 = (i + 1) * Mathf.PI * 2 / Sides;
                var p0 = new Vector3(Mathf.Cos(a0) * Width,0,Mathf.Sin(a0) * Width); var p1 = new Vector3(Mathf.Cos(a1) * Width,0,Mathf.Sin(a1) * Width);
                int v = vertices.Count; vertices.Add(Vector3.up * Up); vertices.Add(p1); vertices.Add(p0); triangles.AddRange(new[] { v,v + 1,v + 2 });
                v = vertices.Count; vertices.Add(Vector3.down * Down); vertices.Add(p0); vertices.Add(p1); triangles.AddRange(new[] { v,v + 1,v + 2 });
            }
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            crystal.AddComponent<MeshFilter>().sharedMesh = mesh;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            var color = new Color(.45f,1f,.88f);
            material.SetColor("_BaseColor",new Color(color.r,color.g,color.b,1));
            material.SetFloat("_Smoothness",.9f);
            material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor",color * 1.6f);
            var renderer = crystal.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var lamp = new GameObject("Luz do Cristal").AddComponent<Light>();
            lamp.transform.SetParent(crystal.transform,false);
            lamp.type = LightType.Point; lamp.color = color; lamp.range = 14; lamp.intensity = 2.2f; lamp.shadows = LightShadows.None;
            crystal.AddComponent<CristalProtetor>();
        }

        public void EnterCatacombInstance()
        {
            if (travelling || !hero.Health.IsAlive) return; // test phase: always open
            TravelTo("CatacumbasDasMares");
        }

        int GuardsOf(int camp){int count=0;foreach(var e in definition.camps[camp].enemies)if(e!=null&&e.kind!=EnemyKind.Boss)count++;return count;}

        void ConfigureCatacombTestRoom()
        {
            var original = definition;
            testRoom = Instantiate(original);
            definition = testRoom;
            var oldCamp = original.camps[0];
            var center = oldCamp.center;
            definition.refuge = center + new Vector3(0,.06f,-4);
            definition.portal = center + new Vector3(0,.06f,5);
            definition.route = new[] { definition.refuge, center, definition.portal };
            definition.connections = Array.Empty<RegionConnection>();
            definition.landmarks = Array.Empty<RegionLandmark>();
            // With a boss in the camp the room is bigger (room to dodge its charge) and holds the whole camp.
            bool hasBoss = Array.Exists(oldCamp.enemies, e => e != null && e.kind == EnemyKind.Boss);
            float halfX = hasBoss ? 10 : 8, halfZ = hasBoss ? 8.5f : 7.5f;
            definition.bounds = new Bounds(center,new Vector3(halfX*2,10,halfZ*2));
            definition.activeEnemyLimit = hasBoss ? 3 : 2;
            definition.camps = new[]
            {
                new RegionCampDefinition
                {
                    label = oldCamp.label,
                    center = center,
                    spawns = hasBoss ? oldCamp.spawns : new[] { oldCamp.spawns[0],oldCamp.spawns[1] },
                    enemies = hasBoss ? oldCamp.enemies : new[] { oldCamp.enemies[0],oldCamp.enemies[1] }
                }
            };

            testRoomWallMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            testRoomWallMaterial.SetColor("_BaseColor",new Color(.21f,.29f,.32f));
            BuildTestRoomWall(center + Vector3.left * halfX,new Vector3(.45f,2.1f,halfZ*2));
            BuildTestRoomWall(center + Vector3.right * halfX,new Vector3(.45f,2.1f,halfZ*2));
            BuildTestRoomWall(center + Vector3.forward * halfZ,new Vector3(halfX*2,2.1f,.45f));
            BuildTestRoomWall(center + Vector3.back * halfZ,new Vector3(halfX*2,2.1f,.45f));
        }

        void BuildTestRoomWall(Vector3 point, Vector3 size)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Limite da sala de teste";
            wall.layer = 9;
            wall.transform.SetParent(transform,false);
            wall.transform.position = point + Vector3.up * (size.y * .5f);
            wall.transform.localScale = size;
            wall.GetComponent<Renderer>().sharedMaterial = testRoomWallMaterial;
        }

        public bool ClaimCatacombCard(int card)
        {
            if (!IsCatacombInstance || !PortalReady || travelling || CardsFinalized || !hero.Health.IsAlive ||
                CardsRevealed >= CatacombPicks || card < 0 || card >= cardOrder.Length || revealedCards[card])
                return false;
            revealedCards[card] = true;
            CardsRevealed++;
            return true;
        }

        public bool FinalizeCatacombCards()
        {
            if (!IsCatacombInstance || !PortalReady || travelling || CardsFinalized || claimingCard || !hero.Health.IsAlive) return false;
            var service = regionalInventory?.Service;
            if (service == null) return false;
            while (CardsRevealed < CatacombPicks)
            {
                int candidate = UnityEngine.Random.Range(0,cardOrder.Length-CardsRevealed);
                for (int i = 0; i < cardOrder.Length; i++)
                {
                    if (revealedCards[i]) continue;
                    if (candidate-- != 0) continue;
                    ClaimCatacombCard(i);
                    break;
                }
            }

            int first = Array.IndexOf(revealedCards,true);
            int second = Array.IndexOf(revealedCards,true,first+1);
            bool duplicateRune = service.CollectionRuneLevel > 0;
            claimingCard = true;
            bool saved = service.ClaimCatacombCards(cardOrder[first],cardOrder[second]);
            claimingCard = false;
            if (!saved) return false;
            if (duplicateRune)
            {
                if (cardOrder[first] == RuneCard) cardOrder[first] = 60;
                if (cardOrder[second] == RuneCard) cardOrder[second] = 60;
            }
            CardsFinalized = true;
            return true;
        }

        public void LeaveCatacombInstance()
        {
            if (IsCatacombInstance && CardsFinalized) TravelTo("RefugioUltimoSino");
        }

        public void Respawn()
        {
            Dismount();hero.GetComponent<ArenaInput>().Desktop?.Cancel();hero.ResetHero();
            // The root that recorded you pulls you back to its Nó (instances keep their own start).
            string noScene = Historia.CenaDoNo, here = SceneManager.GetActiveScene().name;
            if (!IsCatacombInstance && !string.IsNullOrEmpty(noScene) && noScene != here && Application.CanStreamedLevelBeLoaded(noScene))
            { Historia.RenascerNoNo = true; AreasDoMundo.VindoDe = null; TravelTo(noScene); return; }
            hero.GetComponent<PlayerMotor>().Teleport(noScene == here && mapStation != null ? NoSpot : definition.refuge,Quaternion.identity);
            follow.Snap();Changed?.Invoke();
        }
        public void ReturnToCampaign() => TravelTo("CampaignCoop");
        public void Viajar(string scene) => TravelTo(scene);
        // In the wild there is no free rest (only in cities).
        bool CanRest => !AreasDoMundo.Selvagem(SceneManager.GetActiveScene().name);
        public void ReturnToStartMenu() => TravelTo("Inicio");
        void TravelTo(string scene){if(travelling)return;travelling=true;hero.GetComponent<ArenaInput>().BlockCommands(true);SceneManager.LoadSceneAsync(scene); }
        void OnDestroy(){if(hero!=null&&hero.Health!=null)hero.Health.Damaged-=Dismount;if(mountMetal!=null)Destroy(mountMetal);if(mountCrystal!=null)Destroy(mountCrystal);if(testRoom!=null)Destroy(testRoom);if(testRoomWallMaterial!=null)Destroy(testRoomWallMaterial);if(goldMaterial!=null)Destroy(goldMaterial);if(altarMaterial!=null)Destroy(altarMaterial);if(enemies!=null)enemies.Died-=EnemyDied;if(hero!=null)hero.Changed-=HeroChanged;ownership.Clear();}
    }

    // Turns slowly and floats up and down (the protector crystal of the city).
    public sealed class CristalProtetor:MonoBehaviour
    {
        Vector3 start;
        void Start()=>start=transform.position;
        void Update(){transform.Rotate(0,25*Time.deltaTime,0,Space.World);transform.position=start+Vector3.up*Mathf.Sin(Time.time*1.1f)*.25f;}
    }
}
