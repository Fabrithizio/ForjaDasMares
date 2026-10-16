using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class CoopProgressionTests
    {
        CoopDirector session; CoopProgression progression; CoopProfileChannel channel;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("CoopDungeon"); yield return null; yield return null;
            session = Object.FindFirstObjectByType<CoopDirector>(); progression = session.GetComponent<CoopProgression>(); channel = session.GetComponent<CoopProfileChannel>();
            Assert.IsNotNull(progression.Catalog); Assert.IsTrue(session.HostDirect("REWARDQA", 7791, false)); yield return null;
        }
        [UnityTearDown] public IEnumerator Close() { if (session != null) session.Close(); yield return null; yield return null; }
        [Test] public void EquipmentIntentChangesRealCombatAndRejectsIncompatibleItems()
        {
            var actor = session.Actor(0); var profile = channel.LocalProfile;
            var axe = profile.inventory.Find(i => i.definitionId == "fenda-axe"); var bow = profile.inventory.Find(i => i.definitionId == "vento-bow");
            actor.Health.ApplyDamage(30); int hp = actor.Health.Current;
            channel.Request(CoopInventoryAction.Equip, axe.instanceId, EquipmentSlot.MainHand);
            Assert.AreEqual(175, actor.Combat.BasicAttack.arc); Assert.AreEqual(hp, actor.Health.Current);
            channel.Request(CoopInventoryAction.Equip, bow.instanceId, EquipmentSlot.MainHand);
            Assert.AreEqual(axe.instanceId, channel.LocalProfile.Loadout("warrior").items[0]);
            for (int i = 0; i < 4; i++) Assert.AreEqual(1, actor.GetComponents<HeroEquipment>().Length);
        }
        [UnityTest] public IEnumerator RealChestDefeatAndReloadDoNotDuplicateItems()
        {
            session.StartGame(); yield return null; yield return null;
            Assert.AreEqual(CoopPhase.Playing, session.Phase); var actor = session.Actor(0); var room = session.Dungeon.Generator.Rooms[3];
            session.Dungeon.Enter(room); DungeonInteractable chest = null;
            foreach (var item in room.interactions) if (item.Kind == DungeonInteractionKind.Chest) chest = item;
            actor.Motor.Teleport(chest.transform.position + new Vector3(0, .06f, -1), Quaternion.identity);
            Assert.IsTrue(chest.UseBy(actor.Health)); Assert.IsFalse(chest.UseBy(actor.Health));
            Assert.AreEqual(1, channel.LocalProfile.expedition.items.Count);
            actor.Health.ApplyDamage(100000); yield return null; yield return null;
            Assert.AreEqual(CoopPhase.Defeat, session.Phase); Assert.IsNull(channel.LocalProfile.expedition); Assert.AreEqual(13, channel.LocalProfile.inventory.Count);
            var service = progression.Profile(0); var result = service.Profile.lastResult;
            Assert.IsFalse(service.FinishExpedition(result.runId, true, "Replay")); Assert.AreEqual(13, service.Profile.inventory.Count);
            var loaded = new ProfileService(progression.Catalog, new ProfileStore(progression.SavePath(0)));
            Assert.AreEqual(13, loaded.Profile.inventory.Count); Assert.AreEqual(result.runId, loaded.Profile.lastResult.runId); Assert.IsNull(loaded.Profile.expedition);
        }
        [UnityTest] public IEnumerator EnemyRewardComesOnceFromAuthorityPoolAndInventoryIsLockedInCombat()
        {
            session.StartGame(); yield return null; yield return null;
            var actor = session.Actor(0); var room = session.Dungeon.Generator.Rooms[1]; session.Dungeon.Enter(room); Assert.IsTrue(session.PlaceInRoom(0, room));
            yield return new WaitForSeconds(.6f); EnemyActor target = null;
            foreach (var enemy in session.Dungeon.Encounter.Enemies.Actors) if (enemy.IsAlive) { target = enemy; break; }
            Assert.IsNotNull(target); Assert.IsFalse(progression.CanManage(0));
            var item = channel.LocalProfile.inventory.Find(i => i.definitionId == "fenda-axe"); channel.Request(CoopInventoryAction.Equip, item.instanceId, EquipmentSlot.MainHand);
            Assert.IsNull(actor.GetComponent<HeroEquipment>().MainWeapon);
            int before = channel.LocalProfile.expedition.experience; target.Health.ApplyDamage(9999); int after = channel.LocalProfile.expedition.experience;
            Assert.AreEqual(before + progression.Catalog.enemy.experience, after); target.Health.ApplyDamage(9999); Assert.AreEqual(after, channel.LocalProfile.expedition.experience);
        }
        [Test] public void RevivalHasExplicitHealthFractionAndDoesNotGiveInvulnerability()
        {
            var actor = session.Actor(0); actor.Health.ApplyDamage(100000);
            Assert.IsFalse(actor.Health.Heal(100)); Assert.IsFalse(actor.Health.Revive(float.NaN)); Assert.IsTrue(actor.Health.Revive(.5f));
            Assert.AreEqual(70, actor.Health.Current); Assert.IsFalse(actor.Health.Revive(.5f)); Assert.IsFalse(actor.Motor.IsInvulnerable); Assert.IsTrue(actor.Health.ApplyDamage(10));
        }
        [UnityTest] public IEnumerator AuthorityCompletesRealCombatsBossAndUniqueSavedVictory()
        {
            session.SelectHero(3); var actor = session.Actor(0);
            foreach (string id in new[] { "cais-blade", "laje-shield", "brasa-armor", "estrada-charm", "retorno-seal" })
            { var item = channel.LocalProfile.inventory.Find(i => i.definitionId == id); channel.Request(CoopInventoryAction.Equip, item.instanceId, progression.Catalog.Find(id).slot); }
            session.StartGame(); yield return null; yield return null; session.SetInputBlocked(true);
            actor.Motor.enabled = actor.Combat.enabled = false;
            var navigation = new UnityEngine.AI.NavMeshPath(); var corners = new Vector3[32];
            var filter = new UnityEngine.AI.NavMeshQueryFilter { agentTypeID = 0, areaMask = UnityEngine.AI.NavMesh.AllAreas };
            float pathClock = 0; int corner = 0, cornerCount = 0, reportedRoom = -1; EnemyActor pathTarget = null;
            for (int id = 1; id <= 6; id++)
            {
                var room = session.Dungeon.Generator.Rooms[id]; session.Dungeon.Enter(room); Assert.IsTrue(session.PlaceInRoom(0, room));
                float elapsed = 0;
                while (room.encounter != null && !room.completed && actor.Health.IsAlive && elapsed < 100)
                {
                    EnemyActor nearest = null; float best = float.MaxValue;
                    foreach (var enemy in session.Dungeon.Encounter.Enemies.Actors) if (enemy.IsAlive) { float distance = (enemy.transform.position - actor.transform.position).sqrMagnitude; if (distance < best) { best = distance; nearest = enemy; } }
                    Vector3 travel = Vector3.zero;
                    if (nearest != null)
                    {
                        Vector3 delta = nearest.transform.position - actor.transform.position; delta.y = 0; float distance = delta.magnitude; Vector3 direction = delta.normalized;
                        if (!actor.Motor.FacingLocked && direction.sqrMagnitude > .1f) actor.transform.rotation = Quaternion.LookRotation(direction);
                        travel = distance > 2.1f ? direction : distance < 1.4f ? -direction : Vector3.zero;
                        // The regression bot previously pushed straight into crates for100s.
                        // Follow real navigation around blocked sightlines; keep collisions,
                        // combat, health, rewards and all victory assertions intact.
                        pathClock -= Time.deltaTime;
                        if (Physics.Linecast(actor.transform.position + Vector3.up, nearest.transform.position + Vector3.up, 1 << 9, QueryTriggerInteraction.Ignore))
                        {
                            if (pathClock <= 0 || pathTarget != nearest)
                            {
                                pathClock = .25f; pathTarget = nearest; cornerCount = 0; corner = 1;
                                if (UnityEngine.AI.NavMesh.CalculatePath(actor.transform.position, nearest.transform.position, filter, navigation) && navigation.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                                    cornerCount = navigation.GetCornersNonAlloc(corners);
                                if (reportedRoom != id) { reportedRoom = id; Debug.Log("COOP_TEST_BOT_OBSTACLE_PATH room=" + id + " corners=" + cornerCount + " actor=" + actor.transform.position + " target=" + nearest.transform.position); }
                            }
                            while (corner < cornerCount && Vector3.Distance(actor.transform.position, corners[corner]) < .35f) corner++;
                            if (corner < cornerCount) { travel = corners[corner] - actor.transform.position; travel.y = 0; travel.Normalize(); }
                        }
                        bool warning = nearest.Combat.WarningRemaining > 0 && (nearest.Combat.CurrentBossAttack == BossAttackKind.GroundMarks || nearest.Combat.WarningRemaining < .45f && distance < 4);
                        if (warning) { travel = Vector3.Cross(Vector3.up, direction); if (nearest.Combat.WarningRemaining < .3f) actor.Motor.TryDodge(Vector2.right); }
                        if (distance < actor.Combat.BasicAttack.range - .1f) actor.Combat.TryAttack(); if (distance < 3.2f) actor.Combat.TrySpecial();
                    }
                    Vector3 forward = Camera.main.transform.forward; forward.y = 0; forward.Normalize(); Vector3 right = Vector3.Cross(Vector3.up, forward);
                    actor.Motor.Simulate(Time.deltaTime, new Vector2(Vector3.Dot(travel, right), Vector3.Dot(travel, forward))); actor.Combat.Simulate(Time.deltaTime);
                    elapsed += Time.deltaTime; yield return null;
                }
                Assert.IsTrue(actor.Health.IsAlive, "Death in room " + id); if (room.encounter != null) Assert.IsTrue(room.completed, "Incomplete combat " + id);
                foreach (var interaction in room.interactions) if (interaction.Kind != DungeonInteractionKind.Exit)
                { actor.Motor.Teleport(interaction.transform.position + new Vector3(0, .06f, -1.3f), Quaternion.identity); Assert.IsTrue(interaction.UseBy(actor.Health)); }
                Assert.IsTrue(room.completed, "Missing objective " + id);
            }
            var exit = session.Dungeon.CurrentRoom.interactions[0]; actor.Motor.Teleport(exit.transform.position + new Vector3(0, .06f, -1.3f), Quaternion.identity);
            Assert.IsTrue(exit.UseBy(actor.Health)); yield return null; Assert.AreEqual(CoopPhase.Victory, session.Phase);
            var profile = channel.LocalProfile; Assert.IsNull(profile.expedition); Assert.GreaterOrEqual(profile.lastResult.items.Count, 4); Assert.GreaterOrEqual(profile.lastResult.experience, 380);
            int coins = profile.coins; int items = profile.inventory.Count; Assert.IsFalse(exit.UseBy(actor.Health)); Assert.AreEqual(coins, channel.LocalProfile.coins); Assert.AreEqual(items, channel.LocalProfile.inventory.Count);
            var loaded = new ProfileService(progression.Catalog, new ProfileStore(progression.SavePath(0))); Assert.AreEqual(coins, loaded.Profile.coins); Assert.AreEqual(profile.lastResult.runId, loaded.Profile.lastResult.runId);
        }
    }
}
