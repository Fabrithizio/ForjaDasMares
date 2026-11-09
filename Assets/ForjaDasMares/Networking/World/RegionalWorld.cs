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
        readonly int[] cardOrder = {0,1,2};
        public static readonly (int gold,int xp)[] CatacombRewards = {(45,15),(25,45),(35,30)};
        bool cardClaimed;
        public int RevealedCard { get; private set; } = -1;
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
            town=GetComponent<TownHub>();
            regionalInventory=GetComponent<RegionalInventory>();
            hero.GetComponent<ArenaInput>().Desktop?.ConfigureAutoAim(enemies);
            var quality=follow.GetComponent<PresentationQuality>();if(quality!=null)quality.Configure(follow.GetComponent<Camera>());
            follow.UseRpgFraming();hero.Select(Mathf.Clamp(CampaignHome.CharacterClass,0,hero.Count-1));hero.enabled=false;hero.GetComponent<HeroCombat>().ConfigureFlow(hero.Current.onlineFlow);hero.Changed+=HeroChanged;
            hero.GetComponent<PlayerMotor>().Teleport(definition.refuge,Quaternion.identity);follow.SetFocusBounds(definition.bounds,true);follow.Snap();
            residents=new EnemyActor[definition.camps.Length][];timers=new float[residents.Length][];firstKills=new int[residents.Length];cleared=new bool[residents.Length];firstDefeated=new bool[residents.Length][];
            for(int i=0;i<residents.Length;i++){residents[i]=new EnemyActor[definition.camps[i].enemies.Length];timers[i]=new float[residents[i].Length];firstDefeated[i]=new bool[residents[i].Length];}
            if (Journey != null && definition.regionId == "costa-ato1")
                BuildCatacombAltar();
            if (IsCatacombInstance)
                for (int i = cardOrder.Length - 1; i > 0; i--)
                {
                    int other = UnityEngine.Random.Range(0,i+1);
                    (cardOrder[i],cardOrder[other]) = (cardOrder[other],cardOrder[i]);
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
            TryDropGold(camp, actor.transform.position);
            var members=residents[camp];for(int i=0;i<members.Length;i++)if(members[i]==actor){members[i]=null;timers[camp][i]=definition.camps[camp].respawnSeconds;if(!firstDefeated[camp][i]){firstDefeated[camp][i]=true;firstKills[camp]++;}break;}
            if(!cleared[camp]){if(firstKills[camp]>=members.Length){cleared[camp]=true;Changed?.Invoke();}}
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
            var commands=(IHeroCommands)hero.GetComponent<PlayerMotor>().Commands;
            commands.TakeReset();
            commands.TakeHeroSlot();
            commands.TakeNextHero();
            if(!hero.Health.IsAlive)return;
            if(hero.transform.position.y< -5)Respawn();
            UpdateCollectionRune();
            poll-=Time.deltaTime;if(poll>0)return;float elapsed=.25f-poll;poll=.25f;
            for(int c=0;c<residents.Length;c++)for(int i=0;i<residents[c].Length;i++)
            {
                timers[c][i]=Mathf.Max(0,timers[c][i]-elapsed);
                if(residents[c][i]!=null||timers[c][i]>0 || (IsCatacombInstance && firstDefeated[c][i]))continue;
                var data=definition.camps[c];var point=data.spawns[i%data.spawns.Length];float distance=(hero.transform.position-point).sqrMagnitude;
                if(distance>75*75||distance<8*8)continue;
                var actor=enemies.TrySpawn(data.enemies[i],point);if(actor!=null){residents[c][i]=actor;ownership[actor]=c;}
            }
        }
        public string InteractionHint => NearbyGold() is WorldClickTarget pile ? "F · "+pile.Label : catacombAltar != null && catacombAltar.InRange(hero.transform) ? "F · Altar das Catacumbas" : RefugeDistance<5?"F · Descansar no refúgio":Vector3.Distance(hero.transform.position,definition.portal)<5?(PortalReady?(IsCatacombInstance?"F · Escolher carta":"F · Entrar nos desafios"):"Investigue os três acampamentos para abrir o portal"):"";
        public void Interact()
        {
            if(!hero.Health.IsAlive||travelling)return;
            if (IsCatacombInstance && PortalReady && Vector3.Distance(hero.transform.position,definition.portal)<5)
            {
                Hud?.OpenCatacombCards();
                return;
            }
            if(NearbyGold()?.TryUse(hero.transform)==true)return;
            if(catacombAltar?.TryUse(hero.transform)==true)return;
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

        void BuildCatacombAltar()
        {
            var point = Array.Find(Journey.Definition.points, entry => entry.interaction == RegionalInteraction.Catacombs);
            var altar = new GameObject("Altar das Catacumbas");
            altar.transform.SetParent(transform,false);
            altar.transform.position = point.position + Vector3.right * 1.7f;
            altarMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            altarMaterial.SetColor("_BaseColor",new Color(.26f,.47f,.52f));
            for (int i = 0; i < 3; i++)
            {
                var stone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stone.transform.SetParent(altar.transform,false);
                stone.transform.localPosition = Vector3.up * (.16f+i*.32f);
                stone.transform.localScale = new Vector3(1.25f-i*.31f,.16f,1.25f-i*.31f);
                stone.GetComponent<Renderer>().sharedMaterial = altarMaterial;
            }
            var target = altar.AddComponent<WorldClickTarget>();
            target.Configure("Altar das Catacumbas",3);
            target.Interacting += actor =>
            {
                if (actor == hero.transform && hero.Health.IsAlive)
                    Hud?.OpenCatacombEntrance();
            };
            catacombAltar = target;
        }
        WorldClickTarget catacombAltar;

        public void EnterCatacombInstance()
        {
            if (travelling || !hero.Health.IsAlive || Journey?.Completed != true) return;
            TravelTo("CatacumbasDasMares");
        }

        public bool ClaimCatacombCard(int card)
        {
            if (!IsCatacombInstance || !PortalReady || travelling || cardClaimed || !hero.Health.IsAlive ||
                Vector3.Distance(hero.transform.position,definition.portal) >= 5 || card < 0 || card >= cardOrder.Length)
                return false;
            cardClaimed = true;
            int index = cardOrder[card];
            var reward = CatacombRewards[index];
            if (regionalInventory?.Service?.ClaimCatacombCard(reward.gold,reward.xp) != true)
            {
                cardClaimed = false;
                return false;
            }
            RevealedCard = index;
            return true;
        }

        public void LeaveCatacombInstance()
        {
            if (IsCatacombInstance && RevealedCard >= 0) TravelTo("CostaAto1");
        }

        public void Respawn(){hero.GetComponent<ArenaInput>().Desktop?.Cancel();hero.ResetHero();hero.GetComponent<PlayerMotor>().Teleport(definition.refuge,Quaternion.identity);follow.Snap();Changed?.Invoke();}
        public void ReturnToCampaign() => TravelTo("CampaignCoop");
        void TravelTo(string scene){if(travelling)return;travelling=true;hero.GetComponent<ArenaInput>().BlockCommands(true);SceneManager.LoadSceneAsync(scene); }
        void OnDestroy(){if(goldMaterial!=null)Destroy(goldMaterial);if(altarMaterial!=null)Destroy(altarMaterial);if(enemies!=null)enemies.Died-=EnemyDied;if(hero!=null)hero.Changed-=HeroChanged;ownership.Clear();}
    }
}
