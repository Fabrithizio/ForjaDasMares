using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Tests
{
    public sealed class PveAcceptanceTests
    {
        EncounterController encounter; HeroRoster heroes; PlayerMotor motor; HeroCombat combat;
        [UnitySetUp] public IEnumerator Load()
        {
            SceneManager.LoadScene("ArenaTest"); yield return null; yield return null; yield return new WaitForSeconds(0.15f);
            encounter = Object.FindFirstObjectByType<EncounterController>(); heroes = encounter.Heroes; motor = heroes.GetComponent<PlayerMotor>(); combat = heroes.GetComponent<HeroCombat>();
            motor.enabled = heroes.enabled = combat.enabled = false; motor.GetComponent<ArenaInput>().enabled = false; motor.GetComponent<PlayerAim>().enabled = false;
            motor.Teleport(new Vector3(0, 0.06f, -7), Quaternion.identity);
        }
        EnemyDefinition Definition(int slot) => encounter.Presets[slot + 1].waves[0].entries[0].enemy;
        EnemyActor Spawn(int slot, Vector3 position) { var actor = encounter.Enemies.TrySpawn(Definition(slot), position); Assert.That(actor, Is.Not.Null); return actor; }
        [Test] public void DefinitionsNavigationAndPoolsAreComplete()
        {
            Assert.That(encounter.Enemies.Capacity, Is.EqualTo(24));
            for (int i = 0; i < 5; i++) { var e = Definition(i); Assert.That(e.prefab.GetComponent<EnemyActor>(), Is.Not.Null); Assert.That(e.attack.damage, Is.GreaterThan(0)); }
            var boss = encounter.Presets[6].waves[0].entries[0].enemy; Assert.That(boss.boss.attacks.Length, Is.EqualTo(3));
            Assert.That(NavMesh.SamplePosition(new Vector3(0, 0, -5), out _, 1, NavMesh.AllAreas), Is.True);
        }
        [UnityTest] public IEnumerator FrontShieldBlocksMoreThanRearAndDeathCleansMinions()
        {
            var guard = Spawn(1, new Vector3(0, 0.06f, -3)); guard.Brain.enabled = false; guard.transform.rotation = Quaternion.identity;
            var hit = heroes.Current.attacks[0]; guard.Health.ApplyHit(hit, Vector3.back, heroes.Health); int front = guard.Health.Maximum - guard.Health.Current;
            guard.Health.Restore(); yield return new WaitForSeconds(0.2f); guard.Health.ApplyHit(hit, Vector3.forward, heroes.Health); int rear = guard.Health.Maximum - guard.Health.Current;
            Assert.That(front, Is.LessThan(rear)); encounter.Enemies.Clear();
            var summoner = Spawn(4, new Vector3(0, 0.06f, -1)); summoner.Brain.enabled = false;
            Assert.That(summoner.Combat.TryAttack(), Is.True); summoner.Combat.Simulate(1); Assert.That(encounter.Enemies.CountOwned(summoner), Is.EqualTo(1));
            summoner.Health.ApplyDamage(10000); Assert.That(encounter.Enemies.CountOwned(summoner), Is.Zero); Assert.That(encounter.Enemies.ActiveCount, Is.Zero);
        }
        [UnityTest] public IEnumerator ExplosionWarnsBeforeDamageAndHitsOnlyOnce()
        {
            var bomb = Spawn(3, new Vector3(0, 0.06f, -5)); bomb.Brain.enabled = false;
            Assert.That(bomb.Combat.TryAttack(), Is.True); var warnings = Object.FindFirstObjectByType<TelegraphPool>(); Assert.That(warnings.ActiveCount, Is.GreaterThan(0));
            var sign = Object.FindFirstObjectByType<Telegraph>(); Assert.That(sign.HasSound, Is.True);
            int before = heroes.Health.Current; bomb.Combat.Simulate(0.4f); Assert.That(heroes.Health.Current, Is.EqualTo(before)); bomb.Combat.Simulate(0.8f);
            Assert.That(heroes.Health.Current, Is.LessThan(before)); int after = heroes.Health.Current; yield return new WaitForSeconds(0.5f); Assert.That(heroes.Health.Current, Is.EqualTo(after)); Assert.That(bomb.IsAlive, Is.False);
        }
        [UnityTest] public IEnumerator ArcherShotsStopAtImpactAndGeometryBlocksAttack()
        {
            var archer = Spawn(2, new Vector3(0, 0.06f, -1)); archer.Brain.enabled = false;
            Assert.That(archer.Combat.TryAttack(), Is.True); archer.Combat.Simulate(0.8f); yield return new WaitForSeconds(1);
            int after = heroes.Health.Current; Assert.That(after, Is.LessThan(heroes.Health.Maximum)); yield return new WaitForSeconds(0.5f); Assert.That(heroes.Health.Current, Is.EqualTo(after));
            archer.Combat.Cancel(); archer.Combat.Simulate(2);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.layer = 9; wall.transform.position = new Vector3(0, 1, -4); wall.transform.localScale = new Vector3(3, 2, 0.3f); Physics.SyncTransforms();
            Assert.That(archer.Combat.TryAttack(), Is.False); Object.Destroy(wall);
        }
        [UnityTest] public IEnumerator NavigationRoutesAroundExistingWall()
        {
            motor.Teleport(new Vector3(-2, 0.06f, 3), Quaternion.identity);
            var enemy = Spawn(0, new Vector3(-6, 0.06f, 3)); enemy.Brain.enabled = false; enemy.Motor.MoveTo(motor.transform.position);
            float elapsed = 0;
            while (elapsed < 7 && EnemyBrain.PlanarDistance(enemy.transform.position, motor.transform.position) > 1.5f) { enemy.Motor.Tick(Time.deltaTime); elapsed += Time.deltaTime; yield return null; }
            Assert.That(EnemyBrain.PlanarDistance(enemy.transform.position, motor.transform.position), Is.LessThan(1.5f));
            Assert.That(enemy.transform.position.x, Is.GreaterThan(-3));
        }
        [UnityTest] public IEnumerator ActiveCapAndSummonLifetimeBudgetAreEnforced()
        {
            encounter.Enemies.SetLimit(2); var summoner = Spawn(4, new Vector3(0, 0.06f, -1)); summoner.Brain.enabled = false;
            for (int i = 0; i < 8; i++) { summoner.Combat.Simulate(5); summoner.Combat.TryAttack(); summoner.Combat.Simulate(1); Assert.That(encounter.Enemies.ActiveCount, Is.LessThanOrEqualTo(2)); foreach (var actor in encounter.Enemies.Actors) if (actor.IsAlive && actor.Summoner == summoner) actor.Health.ApplyDamage(1000); yield return new WaitForSeconds(0.7f); }
            Assert.That(encounter.Enemies.Capacity, Is.EqualTo(24)); encounter.Enemies.Clear(); Assert.That(encounter.Enemies.ActiveCount, Is.Zero);
        }
        [UnityTest] public IEnumerator AllFourHeroesCanDefeatEachArchetypeWithTheirActualAttacks()
        {
            for (int hero = 0; hero < 4; hero++) for (int kind = 0; kind < 5; kind++)
            {
                encounter.Cancel(); heroes.Select(hero); combat.ResetCombat(); motor.Teleport(new Vector3(0, 0.06f, -8), Quaternion.identity);
                var enemy = Spawn(kind, new Vector3(0, 0.06f, -4)); yield return Fight(enemy, 22);
                Assert.That(enemy.IsAlive, Is.False, "Enemy survived: hero " + hero + " kind " + kind); Assert.That(heroes.Health.IsAlive, Is.True, "Hero died: " + hero + "/" + kind);
            }
        }
        IEnumerator Fight(EnemyActor enemy, float maximum)
        {
            float elapsed = 0;
            while (enemy.IsAlive && heroes.Health.IsAlive && elapsed < maximum)
            {
                Vector3 toward = enemy.transform.position - motor.transform.position; toward.y = 0; float distance = toward.magnitude; Vector3 direction = toward.normalized;
                if (!motor.FacingLocked && direction.sqrMagnitude > 0.1f) motor.transform.rotation = Quaternion.LookRotation(direction);
                float desired = heroes.Selected == 1 || heroes.Selected == 2 ? 6 : 1.7f;
                Vector3 travel = distance > desired + 0.4f ? direction : distance < desired - 0.4f ? -direction : Vector3.zero;
                bool ground = enemy.Combat.CurrentBossAttack == BossAttackKind.GroundMarks;
                if (enemy.Combat.WarningRemaining > 0 && (ground || enemy.Combat.WarningRemaining < 0.45f && distance < 4)) travel = Vector3.Cross(Vector3.up, direction);
                Vector3 forward = Camera.main.transform.forward; forward.y = 0; forward.Normalize(); Vector3 right = Vector3.Cross(Vector3.up, forward);
                var input = new Vector2(Vector3.Dot(travel, right), Vector3.Dot(travel, forward));
                if (enemy.Combat.WarningRemaining > 0 && enemy.Combat.WarningRemaining < 0.3f && (ground || distance < 4)) motor.TryDodge(input);
                motor.Simulate(Time.deltaTime, input); combat.Simulate(Time.deltaTime);
                if (distance <= heroes.Current.attacks[0].range - 0.1f) combat.TryAttack(); if (distance < 3.2f && combat.SpecialCooldownRemaining <= 0) combat.TrySpecial();
                elapsed += Time.deltaTime; yield return null;
            }
        }
        [UnityTest] public IEnumerator PassivePlayerCanDieAndEncounterFailsThenResets()
        {
            encounter.SetSpawnPoints(new[] { new Vector3(0, 0.06f, -4) }); encounter.Begin(encounter.Presets[1]);
            float elapsed = 0; while (heroes.Health.IsAlive && elapsed < 35) { motor.Simulate(Time.deltaTime, Vector2.zero); elapsed += Time.deltaTime; yield return null; }
            yield return null; Assert.That(heroes.Health.IsAlive, Is.False); Assert.That(encounter.State, Is.EqualTo(EncounterState.Failed)); Assert.That(encounter.Enemies.ActiveCount, Is.Zero);
            Object.FindFirstObjectByType<ArenaSession>().ResetArena(); Assert.That(heroes.Health.IsAlive, Is.True); Assert.That(encounter.State, Is.EqualTo(EncounterState.Ready));
        }
        [UnityTest] public IEnumerator BossHasThreeAvoidablePatternsPhaseAndSingleReward()
        {
            heroes.Select(3); encounter.SetSpawnPoints(new[] { new Vector3(0, 0.06f, -3) }); encounter.Begin(encounter.Presets[6]);
            yield return new WaitForSeconds(1.2f); var boss = encounter.Boss; Assert.That(boss, Is.Not.Null); boss.Brain.enabled = false; boss.Combat.Cancel();
            foreach (var move in boss.Profile.boss.attacks)
            {
                motor.Teleport(new Vector3(0, 0.06f, -6), Quaternion.identity); boss.Combat.Simulate(3);
                Assert.That(boss.Combat.TryBossAttack(move, false), Is.True); int hp = heroes.Health.Current;
                Assert.That(Object.FindFirstObjectByType<TelegraphPool>().ActiveCount, Is.GreaterThan(0)); boss.Combat.Simulate(0.2f); Assert.That(heroes.Health.Current, Is.EqualTo(hp));
                motor.Teleport(new Vector3(7, 0.06f, -7), Quaternion.identity); boss.Combat.Simulate(2); Assert.That(heroes.Health.Current, Is.EqualTo(hp)); boss.Combat.Cancel();
            }
            boss.Health.ApplyDamage(270); Assert.That(boss.Brain.BossPhase, Is.EqualTo(2)); boss.Health.ApplyDamage(10000); yield return null; yield return null;
            Assert.That(encounter.State, Is.EqualTo(EncounterState.Won)); Assert.That(encounter.Seals, Is.EqualTo(25)); yield return new WaitForSeconds(1); Assert.That(encounter.Seals, Is.EqualTo(25));
        }
        [UnityTest] public IEnumerator EveryHeroCanDefeatTheBoss()
        {
            for (int hero = 0; hero < 4; hero++)
            {
                encounter.Cancel(); heroes.Select(hero); motor.Teleport(new Vector3(0, 0.06f, -8), Quaternion.identity); var boss = encounter.Enemies.TrySpawn(encounter.Presets[6].waves[0].entries[0].enemy, new Vector3(0, 0.06f, -4)); Assert.That(boss, Is.Not.Null);
                yield return Fight(boss, 60); Assert.That(boss.IsAlive, Is.False, "Boss survived hero " + hero); Assert.That(heroes.Health.IsAlive, Is.True, "Boss killed hero " + hero);
            }
        }
        [UnityTest] public IEnumerator WavesWaitForEverySpawnAndRelevantEnemyThenComplete()
        {
            encounter.Begin(encounter.Presets[0]); int spawned = 0; encounter.Enemies.Spawned += Count;
            float elapsed = 0;
            while (encounter.State != EncounterState.Won && elapsed < 15)
            {
                foreach (var actor in encounter.Enemies.Actors) if (actor.IsAlive) actor.Health.ApplyDamage(10000);
                elapsed += Time.deltaTime; yield return null;
            }
            encounter.Enemies.Spawned -= Count;
            Assert.That(spawned, Is.EqualTo(9)); Assert.That(encounter.State, Is.EqualTo(EncounterState.Won)); Assert.That(encounter.Enemies.ActiveCount, Is.Zero);
            void Count(EnemyActor actor) => spawned++;
        }
        [Test] public void MultiplePlayerCollidersDoNotDuplicateAreaDamage()
        {
            var enemy = Spawn(0, new Vector3(0, 0.06f, -5)); enemy.Brain.enabled = false;
            var extra = motor.gameObject.AddComponent<CapsuleCollider>(); extra.center = Vector3.up * 0.9f; extra.height = 1.8f; extra.radius = 0.4f; Physics.SyncTransforms();
            int before = heroes.Health.Current; var resolver = new DamageResolver();
            int hits = resolver.Area(enemy.transform.position + Vector3.up, 4, Vector3.back, 360, enemy.Profile.attack, enemy.Health, 1 << 11, 1 << 9);
            Assert.That(hits, Is.EqualTo(1)); Assert.That(before - heroes.Health.Current, Is.EqualTo(10)); Object.Destroy(extra);
        }
    }
}
