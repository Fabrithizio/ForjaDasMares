using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Tests
{
    public sealed class DungeonAcceptanceTests
    {
        DungeonSession dungeon; PlayerMotor motor; HeroCombat combat; HeroRoster heroes;
        [UnitySetUp] public IEnumerator Load()
        {
            SceneManager.LoadScene("DungeonTest"); yield return null; yield return null; yield return new WaitForSeconds(0.2f);
            dungeon = Object.FindFirstObjectByType<DungeonSession>(); heroes = dungeon.Heroes; motor = heroes.GetComponent<PlayerMotor>(); combat = heroes.GetComponent<HeroCombat>();
            motor.enabled = heroes.enabled = combat.enabled = false; motor.GetComponent<ArenaInput>().enabled = false; motor.GetComponent<PlayerAim>().enabled = false;
        }
        [Test] public void ThousandsOfSeedsAreConnectedBoundedAndReproducible()
        {
            for (int seed = -1000; seed < 1000; seed++) for (int maximum = 7; maximum <= 12; maximum++)
            {
                var map = DungeonLayout.Generate(seed, maximum, false); Assert.That(map.Validate(maximum, out string error), Is.True, error);
                Assert.That(map.Signature(), Is.EqualTo(DungeonLayout.Generate(seed, maximum, false).Signature()));
                int combats = 0; foreach (var room in map.rooms) if (room.category == RoomCategory.Combat || room.category == RoomCategory.HardCombat) combats++;
                Assert.That(combats, Is.EqualTo(3));
            }
        }
        [UnityTest] public IEnumerator ThreeBiomesHaveRealNavigationAndDistinctConfiguredArt()
        {
            var colors = new System.Collections.Generic.HashSet<Color>();
            for (int biome = 0; biome < 3; biome++) for (int seed = 0; seed < 4; seed++)
            {
                dungeon.ConfigureTest(seed * 937 + 12345, false, biome); yield return new WaitForSeconds(0.15f);
                Assert.That(dungeon.Generator.NavigationConnected, Is.True); Assert.That(dungeon.Generator.Rooms.Length, Is.InRange(7, 9)); Assert.That(dungeon.Biome.decorationMesh, Is.Not.Null); colors.Add(dungeon.Biome.floor.GetColor("_BaseColor"));
                foreach (var room in dungeon.Generator.Rooms) Assert.That(room.root.GetComponentsInChildren<Collider>().Length, Is.GreaterThan(0));
            }
            Assert.That(colors.Count, Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator ClosedDoorBlocksPlayerAndEncounterCompletionOpensIt()
        {
            var room = dungeon.Generator.Rooms[1]; motor.Teleport(room.Center + new Vector3(0, 0.06f, -3), Quaternion.identity); dungeon.Enter(room);
            foreach (var door in room.doors) Assert.That(door.IsClosed, Is.True);
            var blocked = room.doors[0]; motor.Teleport(blocked.transform.position + new Vector3(0, -0.84f, 1), Quaternion.LookRotation(Vector3.back));
            Vector3 forward = Camera.main.transform.forward; forward.y = 0; forward.Normalize(); var input = new Vector2(Vector3.Dot(Vector3.back, Vector3.Cross(Vector3.up, forward)), Vector3.Dot(Vector3.back, forward));
            for (int i = 0; i < 20; i++) { motor.Simulate(0.02f, input); yield return null; }
            Assert.That(motor.transform.position.z, Is.GreaterThan(blocked.transform.position.z + 0.45f));
            float elapsed = 0; while (!room.completed && elapsed < 8) { foreach (var enemy in dungeon.Encounter.Enemies.Actors) if (enemy.IsAlive) enemy.Health.ApplyDamage(10000); elapsed += Time.deltaTime; yield return null; }
            Assert.That(room.completed, Is.True); foreach (var door in room.doors) if (dungeon.Generator.Rooms[door.A == room.node.id ? door.B : door.A].node.category != RoomCategory.Treasure) Assert.That(door.IsClosed, Is.False);
        }
        [UnityTest] public IEnumerator ChestKeyObjectiveRestTrapAndExitRequireTheirConditions()
        {
            var eventRoom = dungeon.Generator.Rooms[3]; motor.Teleport(eventRoom.Center + Vector3.up * 0.06f, Quaternion.identity); dungeon.Enter(eventRoom);
            Assert.That(eventRoom.completed, Is.False); int coins = dungeon.Coins;
            foreach (var item in eventRoom.interactions) if (item.Kind == DungeonInteractionKind.Chest) { motor.Teleport(item.transform.position + new Vector3(0, 0.06f, -1.3f), Quaternion.identity); Assert.That(item.Use(), Is.True); Assert.That(item.Use(), Is.False); }
            Assert.That(dungeon.Coins, Is.GreaterThan(coins)); Assert.That(dungeon.HasKey, Is.False);
            foreach (var item in eventRoom.interactions) if (item.Kind == DungeonInteractionKind.Crystal) { motor.Teleport(item.transform.position + new Vector3(0, 0.06f, -1.3f), Quaternion.identity); item.Use(); } Assert.That(eventRoom.completed, Is.True); Assert.That(dungeon.HasKey, Is.True);
            var trapRoom = dungeon.Generator.Rooms[5]; motor.Teleport(trapRoom.Center + Vector3.up * 0.06f, Quaternion.identity); dungeon.Enter(trapRoom);
            int hp = heroes.Health.Current; yield return new WaitForSeconds(0.7f); Assert.That(heroes.Health.Current, Is.EqualTo(hp)); yield return new WaitForSeconds(1.5f); Assert.That(heroes.Health.Current, Is.LessThan(hp));
            foreach (var item in trapRoom.interactions) { motor.Teleport(item.transform.position + new Vector3(0, 0.06f, -1.3f), Quaternion.identity); item.Use(); } Assert.That(trapRoom.completed, Is.True); foreach (var trap in dungeon.Generator.Traps) Assert.That(trap.Disarmed, Is.True);
            var bossRoom = dungeon.Generator.Rooms[6]; motor.Teleport(bossRoom.Center + new Vector3(0, 0.06f, -3), Quaternion.identity); dungeon.Enter(bossRoom); Assert.That(bossRoom.interactions[0].Use(), Is.False);
        }
        [UnityTest] public IEnumerator ResetSameSeedAndHeroSwapStartSafeNewRun()
        {
            string before = dungeon.Generator.Layout.Signature(); dungeon.OpenChest(3); Object.FindFirstObjectByType<ArenaSession>().ResetArena(); yield return null; yield return null;
            Assert.That(dungeon.Generator.Layout.Signature(), Is.EqualTo(before)); Assert.That(dungeon.Coins, Is.Zero); Assert.That(dungeon.CurrentRoom.node.id, Is.Zero); Assert.That(motor.transform.position.y, Is.GreaterThan(0));
            heroes.Select(1); yield return null; Assert.That(dungeon.CurrentRoom.node.id, Is.Zero); Assert.That(heroes.Selected, Is.EqualTo(1)); Assert.That(heroes.Health.Current, Is.EqualTo(105));
        }
        [Test] public void DungeonCameraKeepsHeroVisibleAcrossAspectRatios()
        {
            var follow = Object.FindFirstObjectByType<ArenaCamera>(); var camera = follow.GetComponent<Camera>();
            foreach (float aspect in new[] { 16f / 9f, 4f / 3f, 9f / 16f }) foreach (Vector3 offset in new[] { new Vector3(0, 0.06f, -3), new Vector3(4.2f, 0.06f, 4.2f), new Vector3(-4.2f, 0.06f, -4.2f) })
            {
                camera.aspect = aspect; motor.Teleport(dungeon.CurrentRoom.Center + offset, Quaternion.identity); follow.Snap(); Vector3 screen = camera.WorldToViewportPoint(motor.transform.position + Vector3.up);
                Assert.That(screen.x, Is.InRange(0.05f, 0.95f)); Assert.That(screen.y, Is.InRange(0.05f, 0.95f)); Assert.That(screen.z, Is.GreaterThan(0));
            }
        }
        [UnityTest] public IEnumerator InputSystemInteractionActivatesNearbyRequiredCrystal()
        {
            var room = dungeon.Generator.Rooms[3]; motor.Teleport(room.Center + Vector3.up * 0.06f, Quaternion.identity); dungeon.Enter(room);
            DungeonInteractable crystal = null; foreach (var item in room.interactions) if (item.Kind == DungeonInteractionKind.Crystal) crystal = item;
            motor.Teleport(crystal.transform.position + new Vector3(0, 0.06f, -1.3f), Quaternion.identity); yield return new WaitForSeconds(0.15f);
            Assert.That(dungeon.Nearby, Is.SameAs(crystal)); var reader = dungeon.GetComponent<DungeonInteractionInput>(); Assert.That(reader.InteractionEnabled, Is.True);
            var originalBackground = InputSystem.settings.backgroundBehavior; var originalEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
            var keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                // Batch mode has no focused Game View. Route this synthetic device to the player only during this test.
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                Assert.That(reader.InteractionAction.controls, Does.Contain(keyboard.fKey));
                InputState.Change(keyboard, new KeyboardState(Key.F), InputUpdateType.Dynamic);
                // Dynamic state monitors invoke the actual action callback; the session consumes it in its normal Update.
                yield return null; yield return null; yield return null; Assert.That(crystal.Used, Is.True, "Use after action"); Assert.That(room.completed, Is.True);
            }
            finally { InputSystem.RemoveDevice(keyboard); InputSystem.settings.backgroundBehavior = originalBackground; InputSystem.settings.editorInputBehaviorInPlayMode = originalEditor; }
        }
        [UnityTest] public IEnumerator CompleteFixedRouteWinsThreeCombatsChestObjectiveTrapBossAndExit()
        {
            heroes.Select(3); yield return null; yield return null;
            for (int id = 1; id <= 6; id++)
            {
                var room = dungeon.Generator.Rooms[id]; yield return WalkTo(room);
                if (room.encounter != null)
                {
                    float elapsed = 0;
                    while (!room.completed && heroes.Health.IsAlive && elapsed < 65)
                    {
                        EnemyActor nearest = null; float best = float.MaxValue; foreach (var enemy in dungeon.Encounter.Enemies.Actors) if (enemy.IsAlive) { float distance = (enemy.transform.position - motor.transform.position).sqrMagnitude; if (distance < best) { best = distance; nearest = enemy; } }
                        if (nearest != null) Bot(nearest); else { motor.Simulate(Time.deltaTime, Vector2.zero); combat.Simulate(Time.deltaTime); }
                        elapsed += Time.deltaTime; yield return null;
                    }
                    Assert.That(heroes.Health.IsAlive, Is.True, "Died in room " + id); Assert.That(room.completed, Is.True, "Encounter not won in room " + id);
                }
                foreach (var item in room.interactions) if (item.Kind != DungeonInteractionKind.Exit) { yield return WalkNear(item.transform.position); Assert.That(item.Use(), Is.True); }
                Assert.That(room.completed, Is.True, "Objective missing " + id);
                if (id == 2)
                {
                    DungeonRoom rest = null; foreach (var candidate in dungeon.Generator.Rooms) if (candidate.node.category == RoomCategory.Rest) rest = candidate;
                    if (rest != null) { yield return WalkTo(rest); foreach (var item in rest.interactions) { yield return WalkNear(item.transform.position); item.Use(); } yield return WalkTo(room); }
                }
            }
            var exit = dungeon.CurrentRoom.interactions[0]; yield return WalkNear(exit.transform.position); Assert.That(exit.Use(), Is.True); Assert.That(dungeon.Finished, Is.True); Assert.That(dungeon.Coins, Is.GreaterThan(12));
        }
        [UnityTest] public IEnumerator ProgressionRouteCommitsBossChestLootExperienceAndVictoryOnce()
        {
            SceneManager.LoadScene("ProgressionDungeon"); yield return null; yield return null;
            dungeon=Object.FindFirstObjectByType<DungeonSession>(); heroes=dungeon.Heroes; motor=heroes.GetComponent<PlayerMotor>(); combat=heroes.GetComponent<HeroCombat>(); motor.enabled=false; combat.enabled=false; motor.GetComponent<ArenaInput>().enabled=false; motor.GetComponent<PlayerAim>().enabled=false;
            var bridge=Object.FindFirstObjectByType<ProgressionSceneBridge>(); heroes.Select(3); yield return null; yield return null;
            foreach(string id in new[]{"cais-blade","laje-shield","brasa-armor","estrada-charm","retorno-seal"})
            { var item=bridge.Service.Profile.inventory.Find(value=>value.definitionId==id); Assert.That(bridge.Equip(item.instanceId,bridge.Catalog.Find(id).slot),Is.True,bridge.Service.Message); }
            var profile=Object.FindFirstObjectByType<ProgressionProfileRuntime>();
            try
            {
                yield return CompleteFixedRouteWinsThreeCombatsChestObjectiveTrapBossAndExit();
                Assert.That(bridge.Service.Profile.expedition,Is.Null); var result=bridge.Service.Profile.lastResult; Assert.That(result.outcome,Is.EqualTo("Masmorra concluída")); Assert.That(result.items.Count,Is.GreaterThanOrEqualTo(4)); Assert.That(result.experience,Is.GreaterThanOrEqualTo(380)); Assert.That(bridge.Equipment.Stats.level,Is.GreaterThan(1));
                int coins=bridge.Service.Profile.coins,count=bridge.Service.Profile.inventory.Count; Assert.That(dungeon.TryExitDungeon(),Is.False); Assert.That(bridge.Service.FinishExpedition(result.runId,true,"Replay"),Is.False); Assert.That(bridge.Service.Profile.coins,Is.EqualTo(coins)); Assert.That(bridge.Service.Profile.inventory.Count,Is.EqualTo(count));
                var loaded=new ProfileService(bridge.Catalog,new ProfileStore(profile.SavePath)); Assert.That(loaded.Profile.coins,Is.EqualTo(coins)); Assert.That(loaded.Profile.lastResult.runId,Is.EqualTo(result.runId));
            }
            finally { if(profile!=null) Object.Destroy(profile.gameObject); }
        }
        IEnumerator WalkTo(DungeonRoom room)
        {
            Vector3 currentCenter = dungeon.CurrentRoom.Center, direction = (room.Center - currentCenter).normalized;
            // Follow the center line through both connection sockets; walls and doors remain active.
            yield return WalkNear(currentCenter); yield return WalkNear(currentCenter + direction * 8); yield return WalkNear(room.Center);
            yield return new WaitForSeconds(0.12f); Assert.That(dungeon.CurrentRoom, Is.SameAs(room));
        }
        IEnumerator WalkNear(Vector3 point)
        {
            yield return new WaitForSeconds(0.2f);
            var filter = new NavMeshQueryFilter { agentTypeID = 0, areaMask = NavMesh.AllAreas };
            var path = new NavMeshPath(); Assert.That(NavMesh.SamplePosition(point, out NavMeshHit destination, 1.5f, filter), Is.True);
            Assert.That(NavMesh.CalculatePath(motor.transform.position, destination.position, filter, path), Is.True); Assert.That(path.status, Is.EqualTo(NavMeshPathStatus.PathComplete), "From " + motor.transform.position + " to " + point + " sampled " + destination.position + " room " + dungeon.CurrentRoom.node.id);
            Vector3[] corners = path.corners; int corner = corners.Length > 1 ? 1 : 0; float elapsed = 0;
            while (EnemyBrain.PlanarDistance(motor.transform.position, point) > 1.1f && elapsed < 10)
            {
                if (corner < corners.Length - 1 && EnemyBrain.PlanarDistance(motor.transform.position, corners[corner]) < 0.3f) corner++;
                Vector3 direction = corners.Length > 0 ? corners[corner] - motor.transform.position : point - motor.transform.position; direction.y = 0; Move(direction.normalized); combat.Simulate(Time.deltaTime); elapsed += Time.deltaTime; yield return null;
            }
            Assert.That(EnemyBrain.PlanarDistance(motor.transform.position, point), Is.LessThan(1.2f), "Passage/interaction unreachable " + point);
        }
        void Move(Vector3 direction)
        {
            Vector3 forward = Camera.main.transform.forward; forward.y = 0; forward.Normalize(); Vector3 right = Vector3.Cross(Vector3.up, forward);
            motor.Simulate(Time.deltaTime, new Vector2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward)));
        }
        void Bot(EnemyActor enemy)
        {
            Vector3 delta = enemy.transform.position - motor.transform.position; delta.y = 0; float distance = delta.magnitude; Vector3 direction = delta.normalized;
            if (!motor.FacingLocked && direction.sqrMagnitude > 0.1f) motor.transform.rotation = Quaternion.LookRotation(direction);
            Vector3 travel = distance > 2.1f ? direction : distance < 1.4f ? -direction : Vector3.zero;
            bool warning = enemy.Combat.WarningRemaining > 0 && (enemy.Combat.CurrentBossAttack == BossAttackKind.GroundMarks || enemy.Combat.WarningRemaining < 0.45f && distance < 4);
            if (warning) travel = Vector3.Cross(Vector3.up, direction);
            if (warning && enemy.Combat.WarningRemaining < 0.3f) motor.TryDodge(new Vector2(1, 0));
            Move(travel); combat.Simulate(Time.deltaTime); if (distance <= heroes.Current.attacks[0].range - 0.1f) combat.TryAttack(); if (distance < 3.2f) combat.TrySpecial();
        }
    }
}
