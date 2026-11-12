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
            if (town != null && Journey != null)
                catacombAltar = BuildPlazaStation("Altar das Catacumbas",new Vector3(5,0,-4),() => Hud?.OpenCatacombEntrance());
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
                var data=definition.camps[c];var point=data.spawns[i%data.spawns.Length];float distance=(hero.transform.position-point).sqrMagnitude;
                if(distance>75*75||(!IsCatacombInstance&&distance<8*8))continue;
                var actor=enemies.TrySpawn(data.enemies[i],point);if(actor!=null){if(IsCatacombInstance){actor.Health.ApplyAttributes(22,0);actor.Health.Restore();actor.Combat.NormalDamageScale=.35f;}residents[c][i]=actor;ownership[actor]=c;}
            }
        }
        public string InteractionHint => IsCatacombInstance && PortalReady ? "F · Abrir cartas" : NearbyGold() is WorldClickTarget pile ? "F · "+pile.Label : catacombAltar != null && catacombAltar.InRange(hero.transform) ? "F · Altar das Catacumbas" : RefugeDistance<5?"F · Descansar no refúgio":Vector3.Distance(hero.transform.position,definition.portal)<5?(PortalReady?(IsCatacombInstance?"F · Escolher carta":"F · Entrar nos desafios"):"Investigue os três acampamentos para abrir o portal"):"";
        public void Interact()
        {
            if(!hero.Health.IsAlive||travelling)return;
            if (IsCatacombInstance && PortalReady)
            {
                Hud?.OpenCatacombCards();
                return;
            }
            if(NearbyGold()?.TryUse(hero.transform)==true)return;
            if(town!=null&&town.TryInteractNearest())return;
            if(Journey!=null&&Journey.Interact())return;
            if(RefugeDistance<5){hero.Health.Restore();Changed?.Invoke();}
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

        public void EnterCatacombInstance()
        {
            if (travelling || !hero.Health.IsAlive || Journey?.Completed != true) return;
            TravelTo("CatacumbasDasMares");
        }

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
            definition.bounds = new Bounds(center,new Vector3(16,10,15));
            definition.activeEnemyLimit = 2;
            definition.camps = new[]
            {
                new RegionCampDefinition
                {
                    label = oldCamp.label,
                    center = center,
                    spawns = new[] { oldCamp.spawns[0],oldCamp.spawns[1] },
                    enemies = new[] { oldCamp.enemies[0],oldCamp.enemies[1] }
                }
            };

            testRoomWallMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            testRoomWallMaterial.SetColor("_BaseColor",new Color(.21f,.29f,.32f));
            BuildTestRoomWall(center + Vector3.left * 8,new Vector3(.45f,2.1f,15));
            BuildTestRoomWall(center + Vector3.right * 8,new Vector3(.45f,2.1f,15));
            BuildTestRoomWall(center + Vector3.forward * 7.5f,new Vector3(16,2.1f,.45f));
            BuildTestRoomWall(center + Vector3.back * 7.5f,new Vector3(16,2.1f,.45f));
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

        public void Respawn(){Dismount();hero.GetComponent<ArenaInput>().Desktop?.Cancel();hero.ResetHero();hero.GetComponent<PlayerMotor>().Teleport(definition.refuge,Quaternion.identity);follow.Snap();Changed?.Invoke();}
        public void ReturnToCampaign() => TravelTo("CampaignCoop");
        void TravelTo(string scene){if(travelling)return;travelling=true;hero.GetComponent<ArenaInput>().BlockCommands(true);SceneManager.LoadSceneAsync(scene); }
        void OnDestroy(){if(hero!=null&&hero.Health!=null)hero.Health.Damaged-=Dismount;if(mountMetal!=null)Destroy(mountMetal);if(mountCrystal!=null)Destroy(mountCrystal);if(testRoom!=null)Destroy(testRoom);if(testRoomWallMaterial!=null)Destroy(testRoomWallMaterial);if(goldMaterial!=null)Destroy(goldMaterial);if(altarMaterial!=null)Destroy(altarMaterial);if(enemies!=null)enemies.Died-=EnemyDied;if(hero!=null)hero.Changed-=HeroChanged;ownership.Clear();}
    }
}
