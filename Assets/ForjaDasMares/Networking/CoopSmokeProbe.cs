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
        Vector3 initial; EnemyActor sample;
        void Start()
        {
            var args = Environment.GetCommandLineArgs(); directory = Arg(args, "-netProbe");
            if (string.IsNullOrEmpty(directory)) { enabled = false; return; }
            session = GetComponent<CoopDirector>(); int.TryParse(Arg(args, "-netExpected"), out expected); if (expected == 0) expected = 4;
            Directory.CreateDirectory(directory); session.ProbeInput = true;
        }
        static string Arg(string[] args, string key) { for (int i = 0; i < args.Length - 1; i++) if (args[i] == key) return args[i + 1]; return null; }
        void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            if (session.IsHost && session.Phase == CoopPhase.Lobby && session.ConnectedCount >= expected)
            { lobby += Time.unscaledDeltaTime; if (lobby > 3) session.StartGame(); }
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
            clock -= Time.unscaledDeltaTime; if (clock > 0) return; clock = 1;
            string control = Path.Combine(directory, "control.txt");
            if (File.Exists(control))
            {
                string command = File.ReadAllText(control).Trim(); File.Delete(control);
                if (command == "start" && session.IsHost) session.StartGame();
                if (command == "disconnect") session.Close();
                if (command == "reconnect") session.JoinDirect("127.0.0.1", "TEST42", 7777);
                if (command == "kill-host-player" && session.IsHost) session.Actor(0).Health.ApplyDamage(100000);
                if (command == "revive-host-player" && session.IsHost) session.Actor(0).Health.Restore();
                if (command == "chest" && session.IsHost)
                {
                    var room = session.Dungeon.Generator.Rooms[3]; session.Dungeon.Enter(room);
                    for (int i = 0; i < 4; i++) if (session.IsConnected(i)) session.PlaceInRoom(i, room);
                    foreach (var item in room.interactions) if (item.Kind == DungeonInteractionKind.Chest)
                    { var actor = session.Actor(session.LocalSlot); actor.Motor.Teleport(item.transform.position + new Vector3(0, .06f, -1), Quaternion.identity); Debug.Log("COOP_PROBE_CHEST used=" + item.UseBy(actor.Health)); break; }
                }
                if (command == "defeat" && session.IsHost) for (int i = 0; i < 4; i++) if (session.IsConnected(i)) session.Actor(i).Health.ApplyDamage(100000);
                if (command == "encounter" && session.IsHost)
                { var room = session.Dungeon.Generator.Rooms[1]; session.Dungeon.Enter(room); for (int i = 0; i < 4; i++) if (session.IsConnected(i)) session.PlaceInRoom(i, room); }
                if (command == "clear-wave" && session.IsHost) foreach (var enemy in session.Dungeon.Encounter.Enemies.Actors) if (enemy.IsAlive) enemy.Health.ApplyDamage(100000);
                if (command == "exit") Application.Quit();
                Debug.Log("COOP_PROBE_CONTROL " + command);
            }
            var report = new ProbeReport { phase = session.Phase.ToString(), tick = session.LastTick, localSlot = session.LocalSlot, connected = session.ConnectedCount, snapshotBytes = session.LastSnapshotBytes, enemies = session.IsHost ? session.Dungeon.Encounter.Enemies.ActiveCount : session.AppliedEnemyCount, effects = session.AppliedEffectCount, time = elapsed };
            for (int i = 0; i < 4; i++) { var actor = session.Actor(i); if (actor == null) continue; report.positions[i] = actor.transform.position; report.hp[i] = actor.Health.Current; }
            if (session.Dungeon.Generator.Layout != null) report.signature = session.Dungeon.Generator.Layout.Signature();
            report.sampleHp = sample != null ? sample.Health.Current : -1;
            var profiles = session.GetComponent<CoopProgression>();
            for (int i = 0; i < 4; i++) { var profile = session.IsHost ? profiles.Profile(i)?.Profile : i == session.LocalSlot ? session.GetComponent<CoopProfileChannel>().LocalProfile : null; if (profile == null) continue; report.xp[i] = profile.expedition?.experience ?? profile.experience; report.items[i] = profile.inventory.Count; }
            File.WriteAllText(Path.Combine(directory, "probe.json"), JsonUtility.ToJson(report, true));
        }
        [Serializable] sealed class ProbeReport
        { public string phase, signature; public uint tick; public int localSlot, connected, enemies, effects, snapshotBytes, sampleHp; public float time; public Vector3[] positions = new Vector3[4]; public int[] hp = new int[4], xp = new int[4], items = new int[4]; }
#endif
    }
}
