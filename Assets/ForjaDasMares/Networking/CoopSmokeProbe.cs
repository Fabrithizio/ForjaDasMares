using System;
using System.IO;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    // Explicit development-only process probe. Never enabled by network messages.
    public sealed class CoopSmokeProbe : MonoBehaviour
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        CoopDirector session; string directory; int expected; float clock, lobby, playing, elapsed; bool spawned, recordedMovement, killRecorded;
        Vector3 initial; EnemyActor sample;ArenaInput arsenalInput;float arsenalUntil;int arsenalVersion=-1,arsenalBudget;
        void Start()
        {
            var args = Environment.GetCommandLineArgs(); directory = Arg(args, "-netProbe");
            if (string.IsNullOrEmpty(directory)) { enabled = false; return; }
            session = GetComponent<CoopDirector>(); int.TryParse(Arg(args, "-netExpected"), out expected); if (expected == 0) expected = 4;
            Directory.CreateDirectory(directory); session.ProbeInput = true;
            if(session.Campaign!=null&&int.TryParse(Arg(args,"-netCampaignLevel"),out int level))session.SelectCampaignLevel(level);
            if(int.TryParse(Arg(args,"-netHero"),out int hero))session.SelectHero(hero);
            var cloud = session.GetComponent<CoopCloud>();
            if(Arg(args,"-netCloudMode")=="find")cloud.FindCampaignGroup();
            if (Arg(args, "-netCloudMode") == "host") cloud.Create();
            if (Arg(args, "-netCloudMode") == "client") cloud.Join(Arg(args, "-netCloudCode"));
        }
        static string Arg(string[] args, string key) { for (int i = 0; i < args.Length - 1; i++) if (args[i] == key) return args[i + 1]; return null; }
        void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            if (session.IsHost && session.Phase == CoopPhase.Lobby && session.ConnectedCount >= expected && !session.GetComponent<CoopCloud>().Busy)
            { lobby += Time.unscaledDeltaTime; if (lobby > 3) session.StartGame(); }
            if(session.Phase!=CoopPhase.Lobby)lobby=0;
            if (session.Phase == CoopPhase.Playing && session.LocalSlot >= 0)
            {
                var actor = session.Actor(session.LocalSlot); if (playing == 0) initial = actor.transform.position;
                playing += Time.unscaledDeltaTime;
                session.ProbeMove = playing < 1.2f ? new Vector2(.25f, 0) : Vector2.zero;
                session.ProbeAim = new Vector2(actor.transform.forward.x, actor.transform.forward.z);
                if (!recordedMovement && playing > 2) { recordedMovement = true; Debug.Log("COOP_PROBE_MOVEMENT slot=" + session.LocalSlot + " distance=" + Vector3.Distance(initial, actor.transform.position)); }
                if (session.IsHost && !spawned && playing > 3)
                {
                    spawned = true; var victim = session.Actor(1); var desired = victim.transform.position + victim.transform.forward * 1.35f;
                    var pool = session.Dungeon.Encounter.Enemies;
                    for (int i = 0; i < pool.Capacity; i++) if (pool.DefinitionAt(i).kind == EnemyKind.Raider)
                    { sample = pool.TrySpawn(pool.DefinitionAt(i), desired); break; }
                    Debug.Log("COOP_PROBE_TARGET spawned=" + (sample != null));
                }
                if (session.LocalSlot == 1 && playing > 3)
                {
                    var pool = session.Dungeon.Encounter.Enemies;
                    foreach (var enemy in pool.Actors)
                        if (enemy.IsSpawned && enemy.Health.IsAlive)
                        { Vector3 direction = enemy.transform.position - actor.transform.position; direction.y = 0; direction.Normalize(); session.ProbeAim = new Vector2(direction.x, direction.z); session.Request(HeroButtons.Attack); break; }
                }
                if (session.IsHost && sample != null && playing > 6 && !sample.Health.IsAlive && !killRecorded) { killRecorded = true; Debug.Log("COOP_PROBE_REMOTE_KILL confirmed"); }
            }
            if(arsenalUntil>0)
            {
                if(Time.unscaledTime>=arsenalUntil||session.Phase!=CoopPhase.Playing){arsenalInput?.SetUiAttackHeld(false);arsenalUntil=0;}
                else if(session.LocalSlot>=0&&arsenalBudget>0)
                {
                    var actor=session.Actor(session.LocalSlot);var view=actor.GetComponent<HeroAttackArea>();var p=actor.Presentation;
                    if(p.State==HeroState.Attack&&p.ActionProgress>.65f&&p.ActionVersion!=arsenalVersion&&view!=null&&(view.Visible||p.EquippedWeapon!=null&&p.EquippedWeapon.attacks[0].shape==AttackShape.Projectile))
                    {
                        arsenalVersion=p.ActionVersion;arsenalBudget--;string weapon=p.EquippedWeapon!=null?p.EquippedWeapon.itemId:"default";string file="arsenal-"+weapon+"-"+arsenalVersion+"-"+p.ActionVariant;ScreenCapture.CaptureScreenshot(Path.Combine(directory,file+".png"));
                        var model=p.Visible.GetComponentInChildren<HeroModelView>();var clips=model!=null?model.Animator.GetCurrentAnimatorClipInfo(0):new AnimatorClipInfo[0];File.WriteAllText(Path.Combine(directory,file+".json"),JsonUtility.ToJson(new ArsenalEvidence{weapon=weapon,variant=p.ActionVariant,version=p.ActionVersion,time=p.ActionProgress,area=view.Visible,clip=clips.Length>0?clips[0].clip.name:"geometric"},true));
                    }
                }
            }
            clock -= Time.unscaledDeltaTime; if (clock > 0) return; clock = 1;
            string control = Path.Combine(directory, "control.txt");
            if (File.Exists(control))
            {
                string command = File.ReadAllText(control).Trim(); File.Delete(control);
                if(command=="qa-altar"&&session.IsHost&&session.Phase==CoopPhase.Playing){var altar=session.Campaign.Features.Nodes[0];for(int slot=0;slot<4;slot++)if(session.IsConnected(slot))session.Actor(slot).Motor.Teleport(altar.root.position+Quaternion.Euler(0,slot*90,0)*Vector3.forward*1.2f+Vector3.up*.06f,Quaternion.identity);}
                if(command.StartsWith("qa-boon:")&&session.LocalSlot>=0)session.GetComponent<CampaignAdventureChannel>().Request(AdventureAction.ChooseBoon,0,int.Parse(command.Substring(8)));
                if(command.StartsWith("qa-ping:")&&session.LocalSlot>=0)session.GetComponent<CampaignAdventureChannel>().Request(AdventureAction.Ping,0,int.Parse(command.Substring(8)));
                if(command=="arsenal-hold"&&session.Phase==CoopPhase.Playing){arsenalInput=session.LocalInput;arsenalInput.SetUiAttackHeld(true);arsenalUntil=Time.unscaledTime+5;arsenalVersion=-1;arsenalBudget=6;}
                if(command=="arsenal-release"){arsenalInput?.SetUiAttackHeld(false);arsenalUntil=0;}
                if(command.StartsWith("arsenal-equip:")&&session.LocalSlot>=0)
                {
                    string definition=command.Substring(14);var channel=session.GetComponent<CoopProfileChannel>();var p=channel.LocalProfile;var item=p.inventory.Find(x=>x.definitionId==definition);
                    // Asset preview grants are allowed only in explicitly isolated QA profiles.
                    if(item==null&&session.IsHost&&Arg(Environment.GetCommandLineArgs(),"-netSavePath")!=null&&channel.Catalog.Find(definition)!=null)
                    {item=LootGenerator.Create(channel.Catalog,definition,ItemRarity.Common,new System.Random(91));p.inventory.Add(item);}
                    if(item!=null)channel.Request(CoopInventoryAction.Equip,item.instanceId,EquipmentSlot.MainHand);
                }
                if(command=="cast-special")session.Request(HeroButtons.Special);
                if(command=="capture")ScreenCapture.CaptureScreenshot(Path.Combine(directory,"screen.png"));
                if(command=="profile")UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>()?.OpenInventory();
                if(command=="profile-character")UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>()?.NavigateProfile(0);
                if(command=="profile-status")UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>()?.NavigateProfile(2);
                if(command=="profile-resources")UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>()?.NavigateProfile(4);
                if(command=="close-profile")UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>()?.CloseInventory();
                if(command=="manual")session.ProbeInput=false;
                if(command=="quiet"){session.ProbeInput=true;session.ProbeMove=Vector2.zero;spawned=true;}
                if(command=="publish"&&session.IsHost)session.GetComponent<CoopCloud>().ToggleVisibility();
                if(command=="admission-limit"&&session.IsHost&&session.Campaign!=null)
                {
                    int completed=0,target=Mathf.CeilToInt((session.Campaign.Level.route.Length-1)*.2f);
                    foreach(var room in session.Dungeon.Generator.Rooms)if(room.node.category!=RoomCategory.Entrance){room.completed=true;if(++completed>=target)break;}
                }
                if (command == "start" && session.IsHost) session.StartGame();
                if (command == "disconnect") session.Close();
                if (command == "reconnect") session.JoinDirect("127.0.0.1", "TEST42", 7777);
                if (command == "kill-host-player" && session.IsHost) session.Actor(0).Health.ApplyDamage(100000);
                if (command == "revive-host-player" && session.IsHost) session.Actor(0).Health.Restore();
                if (command == "chest" && session.IsHost && session.Phase == CoopPhase.Playing)
                {
                    var room = session.Dungeon.Generator.Rooms[3];
                    if(session.Campaign!=null)foreach(var candidate in session.Dungeon.Generator.Rooms)if(candidate.node.category==RoomCategory.Event){room=candidate;break;}
                    session.Dungeon.Enter(room);
                    for (int i = 0; i < 4; i++) if (session.IsConnected(i)) session.PlaceInRoom(i, room);
                    foreach (var item in room.interactions) if (item.Kind == DungeonInteractionKind.Chest)
                    { var actor = session.Actor(session.LocalSlot); actor.Motor.Teleport(item.transform.position + new Vector3(0, .06f, -1), Quaternion.identity); Debug.Log("COOP_PROBE_CHEST used=" + item.UseBy(actor.Health)); break; }
                }
                if (command == "defeat" && session.IsHost && session.Phase == CoopPhase.Playing) for (int i = 0; i < 4; i++) if (session.IsConnected(i)) session.Actor(i).Health.ApplyDamage(100000);
                if (command == "encounter" && session.IsHost && session.Phase == CoopPhase.Playing)
                { var room = session.Dungeon.Generator.Rooms[1]; session.Dungeon.Enter(room); for (int i = 0; i < 4; i++) if (session.IsConnected(i)) session.PlaceInRoom(i, room); }
                if(command=="rescue-check"&&session.IsHost&&session.Phase==CoopPhase.Playing&&session.ConnectedCount>=2)
                {var fallen=session.Actor(1);fallen.Health.ApplyDamage(100000);session.Actor(0).Motor.Teleport(fallen.transform.position+Vector3.right*.9f,Quaternion.identity);session.Request(HeroButtons.Interact);}
                if (command == "clear-wave" && session.IsHost && session.Phase == CoopPhase.Playing) foreach (var enemy in session.Dungeon.Encounter.Enemies.Actors) if (enemy.IsAlive) enemy.Health.ApplyDamage(100000);
                if(command=="win-campaign"&&session.IsHost&&session.Campaign!=null&&session.Phase==CoopPhase.Playing)
                {
                    session.Dungeon.Encounter.Cancel();DungeonRoom boss=null;
                    foreach(var room in session.Dungeon.Generator.Rooms){room.entered=room.completed=room.objectiveComplete=room.encounterComplete=true;if(room.node.category==RoomCategory.Boss)boss=room;}
                    session.Dungeon.Enter(boss);
                    foreach(var item in boss.interactions)if(item.Kind==DungeonInteractionKind.Exit)
                    {var actor=session.Actor(0);actor.Motor.Teleport(item.transform.position+new Vector3(0,.06f,-1),Quaternion.identity);Debug.Log("ADMIN_CAMPAIGN_EXIT "+item.UseBy(actor.Health));break;}
                }
                if(command=="continue-campaign"&&session.IsHost&&session.Campaign!=null)
                {session.ReturnToCampaignMap();session.SelectCampaignLevel((session.Campaign.Selected+1)%session.Campaign.Count);}
                if(command=="adventure-seals"&&session.IsHost&&session.Campaign?.Adventure!=null)
                {foreach(var p in session.Campaign.Adventure.Props)if(p.Kind==AdventurePropKind.Seal)p.Health.ApplyDamage(p.Health.Maximum);Debug.Log("ADMIN_ADVENTURE_SEALS");}
                if (command == "exit") Application.Quit();
                Debug.Log("COOP_PROBE_CONTROL " + command);
            }
            var report = new ProbeReport { phase = session.Phase.ToString(), tick = session.LastTick, localSlot = session.LocalSlot, connected = session.ConnectedCount, snapshotBytes = session.LastSnapshotBytes, enemies = session.IsHost ? session.Dungeon.Encounter.Enemies.ActiveCount : session.AppliedEnemyCount, effects = session.AppliedEffectCount, time = elapsed };
            var cloudState = session.GetComponent<CoopCloud>(); report.code = session.Pin; report.cloudMessage = cloudState.Message; report.cloudBusy = cloudState.Busy;
            var soundscape=session.GetComponent<CampaignSoundscape>();if(soundscape!=null){report.soundscape=soundscape.Context;report.musicPlaying=soundscape.MusicPlaying;report.ambiencePlaying=soundscape.AmbiencePlaying;report.musicSample=soundscape.MusicSamplePosition;report.audioRate=AudioSettings.outputSampleRate;}
            report.publicRoom=cloudState.PublicRoom;report.solo=session.Campaign!=null&&session.Campaign.Solo;report.completion=session.Campaign!=null?session.Campaign.Completion:0;
            var adventure=session.Campaign?.Adventure;if(adventure!=null){report.seals=adventure.SealCount;report.sealGoal=adventure.SealGoal;report.explorationReward=adventure.Rewarded;report.propHealth=new int[adventure.Props.Count];for(int p=0;p<report.propHealth.Length;p++)report.propHealth[p]=adventure.Props[p].Health.Current;}
            report.profileReady=session.GetComponent<CoopProfileChannel>().LocalProfile!=null;
            for(int i=0;i<4;i++)report.maximum[i]=session.Actor(i).Health.Maximum;
            for (int i = 0; i < 4; i++) { var actor = session.Actor(i); if (actor == null) continue; report.positions[i] = actor.transform.position; report.hp[i] = actor.Health.Current;report.heroes[i]=actor.Roster.Current.heroId;report.variants[i]=actor.Presentation.ActionVariant;
                var model=actor.Presentation.Visible.GetComponentInChildren<HeroModelView>();if(model!=null){var clips=model.Animator.GetCurrentAnimatorClipInfo(0);report.clips[i]=clips.Length>0?clips[0].clip.name:"none";}
                foreach(var line in actor.Presentation.Visible.GetComponentsInChildren<LineRenderer>())if(line.enabled&&line.name.StartsWith("PrismaGlyph_"))report.glyphs[i]++;
                report.recoveries[i]=actor.Motor.FallRecoveries; }
            if (session.Dungeon.Generator.Layout != null) report.signature = session.Dungeon.Generator.Layout.Signature();
            report.sampleHp = sample != null ? sample.Health.Current : -1;
            var profiles = session.GetComponent<CoopProgression>();
            for (int i = 0; i < 4; i++) { var profile = session.IsHost ? profiles.Profile(i)?.Profile : i == session.LocalSlot ? session.GetComponent<CoopProfileChannel>().LocalProfile : null; if (profile == null) continue; report.xp[i] = profile.expedition?.experience ?? profile.experience; report.items[i] = profile.inventory.Count; }
            for(int i=0;i<4;i++)report.boons[i]=session.Actor(i)?.GetComponent<CombatBuildEffects>()?.PackedBoons??0;if(session.Campaign?.Features!=null){var features=session.Campaign.Features;report.featureStages=new int[features.Nodes.Count];for(int i=0;i<features.Nodes.Count;i++)report.featureStages[i]=features.Nodes[i].state.stage;report.pingKinds=new int[4];for(int i=0;i<4;i++)report.pingKinds[i]=features.Pings!=null&&features.Pings[i].remaining>0?features.Pings[i].kind:-1;}
            File.WriteAllText(Path.Combine(directory, "probe.json"), JsonUtility.ToJson(report, true));
        }
        void OnDisable(){arsenalInput?.SetUiAttackHeld(false);}
        [Serializable]sealed class ArsenalEvidence {public string weapon,clip;public int variant,version;public float time;public bool area;}
        [Serializable] sealed class ProbeReport
        {public string soundscape;public bool musicPlaying,ambiencePlaying;public int musicSample,audioRate;public string[] heroes=new string[4],clips=new string[4];public int[] variants=new int[4],glyphs=new int[4]; public int[] recoveries=new int[4],boons=new int[4],featureStages,pingKinds;public string phase, signature, code, cloudMessage; public int seals,sealGoal;public int[] propHealth;public bool explorationReward,profileReady; public bool cloudBusy,publicRoom,solo; public uint tick; public int localSlot, connected, enemies, effects, snapshotBytes, sampleHp; public float time,completion; public Vector3[] positions = new Vector3[4]; public int[] hp = new int[4],maximum=new int[4], xp = new int[4], items = new int[4]; }
#endif
    }
}
