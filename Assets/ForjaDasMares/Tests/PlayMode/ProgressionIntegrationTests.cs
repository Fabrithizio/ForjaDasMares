using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace ForjaDasMares.Tests
{
    public sealed class ProgressionIntegrationTests
    {
        ProgressionSceneBridge bridge;
        DungeonSession dungeon;
        HeroRoster heroes;
        HeroCombat combat;
        PlayerMotor motor;
        [UnitySetUp] public IEnumerator Load()
        { yield return SceneManager.LoadSceneAsync("ProgressionDungeon"); yield return null; Bind(); }
        void Bind()
        {
            bridge = Object.FindFirstObjectByType<ProgressionSceneBridge>(); Assert.That(bridge, Is.Not.Null); Assert.That(bridge.Error, Is.Null); dungeon = Object.FindFirstObjectByType<DungeonSession>(); heroes = bridge.Heroes; combat = heroes.GetComponent<HeroCombat>(); motor = heroes.GetComponent<PlayerMotor>();
            motor.enabled = false; combat.enabled = false; motor.GetComponent<ArenaInput>().enabled = false; motor.GetComponent<PlayerAim>().enabled = false;
        }
        [UnityTearDown] public IEnumerator Unload()
        { yield return SceneManager.LoadSceneAsync("ArenaTest"); var profile = Object.FindFirstObjectByType<ProgressionProfileRuntime>(); if (profile != null) Object.Destroy(profile.gameObject); yield return null; }
        ItemInstance Item(string id) => bridge.Service.Profile.inventory.Find(value => value.definitionId == id);
        void Equip(string id, EquipmentSlot slot = EquipmentSlot.MainHand) => Assert.That(bridge.Equip(Item(id).instanceId, slot), Is.True, bridge.Service.Message);
        [Test] public void RealCatalogContainsAllCategoriesRaritiesAndValidComposition()
        {
            bridge.Catalog.Validate(); var categories = new HashSet<WeaponCategory>(); foreach (var item in bridge.Catalog.equipment) if (item.category != WeaponCategory.None) categories.Add(item.category);
            Assert.That(categories.Count, Is.EqualTo(9)); Assert.That(bridge.Catalog.rarities.Length, Is.EqualTo(5)); Assert.That(bridge.Service.Profile.inventory.Count, Is.EqualTo(12)); Assert.That(bridge.Service.Profile.expedition, Is.Not.Null); Assert.That(dungeon.Generator.NavigationConnected, Is.True);
            Assert.That(bridge.Companion.GetComponentsInChildren<Collider>(true), Is.Empty); Assert.That(Object.FindObjectsByType<ProgressionProfileRuntime>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        }
        [Test] public void EquippingChangesActualHealthSpeedAndAttackWithoutHealingOrAccumulating()
        {
            heroes.Health.ApplyDamage(40); int before = heroes.Health.Current; Equip("brasa-armor", EquipmentSlot.Armor); Assert.That(heroes.Health.Maximum, Is.GreaterThan(140)); Assert.That(heroes.Health.Current, Is.EqualTo(before));
            float speed = motor.Speed; Equip("estrada-charm", EquipmentSlot.Accessory); Assert.That(motor.Speed, Is.GreaterThan(speed)); float once = motor.Speed;
            for (int i=0;i<8;i++) Equip("estrada-charm", EquipmentSlot.Accessory); Assert.That(motor.Speed, Is.EqualTo(once));
            Equip("fenda-axe"); Assert.That(combat.BasicAttack.arc, Is.EqualTo(175)); Assert.That(combat.BasicAttack.recovery, Is.GreaterThan(heroes.Current.attacks[0].recovery)); Assert.That(bridge.Equipment.MainWeapon.category, Is.EqualTo(WeaponCategory.Axe));
            Assert.That(bridge.Equip(Item("vento-bow").instanceId, EquipmentSlot.MainHand), Is.False);
        }
        [UnityTest] public IEnumerator WeaponPatternsCauseRealDistinctHitsAndProjectiles()
        {
            yield return SceneManager.LoadSceneAsync("ProgressionArena"); yield return null; Bind(); var arena = Object.FindFirstObjectByType<ArenaSession>(); arena.enabled = false;
            var rear = new GameObject("RearTarget", typeof(CapsuleCollider), typeof(Health)); rear.layer = 10; rear.transform.position = new Vector3(0, .06f, -1.2f); rear.GetComponent<CapsuleCollider>().center = Vector3.up; rear.GetComponent<Health>().Configure(100, CombatFaction.Training);
            try
            {
                motor.Teleport(new Vector3(0,.06f,0),Quaternion.identity); Equip("cais-blade"); Assert.That(combat.TryAttack(), Is.True); combat.Simulate(1); Assert.That(rear.GetComponent<Health>().Current, Is.EqualTo(100)); Assert.That(arena.TrainingTarget.Current, Is.LessThan(100));
                arena.ResetArena(); motor.Teleport(new Vector3(0,.06f,0),Quaternion.identity); Equip("basalto-hammer"); combat.TryAttack(); combat.Simulate(2); Assert.That(rear.GetComponent<Health>().Current, Is.LessThan(100));
                rear.transform.position=new Vector3(2,.06f,.5f); rear.GetComponent<Health>().Restore(); Physics.SyncTransforms(); Equip("cais-blade"); combat.TryAttack(); combat.Simulate(1); Assert.That(rear.GetComponent<Health>().Current,Is.EqualTo(100)); Equip("fenda-axe"); combat.TryAttack(); combat.Simulate(1); Assert.That(rear.GetComponent<Health>().Current,Is.LessThan(100)); rear.SetActive(false);
                heroes.Select(1); Equip("vidro-daggers"); arena.TrainingTarget.Restore(); motor.Teleport(new Vector3(0,.06f,.5f),Quaternion.identity); combat.TryAttack(); combat.Simulate(.06f); int first=arena.TrainingTarget.Current; combat.Simulate(.08f); Assert.That(arena.TrainingTarget.Current, Is.LessThan(first));
                combat.Simulate(1); arena.ResetArena(); Equip("vento-bow"); motor.Teleport(new Vector3(0,.06f,-3),Quaternion.identity); combat.TryAttack(); combat.Simulate(.12f); Assert.That(combat.Pool.ActiveProjectiles, Is.EqualTo(1)); yield return new WaitForSeconds(.5f); Assert.That(arena.TrainingTarget.Current, Is.LessThan(100));
                heroes.Select(2); Equip("centelha-book"); motor.Teleport(new Vector3(0,.06f,-3),Quaternion.identity); combat.TryAttack(); combat.Simulate(.2f); Assert.That(combat.Pool.ActiveProjectiles, Is.EqualTo(3));
                arena.ResetArena(); Equip("sal-staff"); motor.Teleport(new Vector3(0,.06f,-3),Quaternion.identity); combat.TryAttack(); combat.Simulate(.2f); yield return new WaitForSeconds(.55f); Assert.That(arena.TrainingTarget.ControlMultiplier,Is.LessThan(1));
                arena.ResetArena(); Equip("mare-orb"); motor.Teleport(new Vector3(0,.06f,-3),Quaternion.identity); combat.TryAttack(); combat.Simulate(.2f); yield return new WaitForSeconds(.65f); Assert.That(arena.TrainingTarget.ControlMultiplier,Is.Zero);
            }
            finally { Object.Destroy(rear); }
        }
        [Test] public void ShieldReducesFrontalDamageWithoutInvulnerability()
        {
            Equip("cais-blade"); Equip("laje-shield",EquipmentSlot.OffHand); var source=new GameObject("HitSource").AddComponent<Health>(); source.Configure(100,CombatFaction.Enemy);
            try { var attack=bridge.Catalog.Find("cais-blade").attacks[0]; heroes.Health.Restore(); int hp=heroes.Health.Current; heroes.Health.ApplyHit(attack,-heroes.transform.forward,source); int front=hp-heroes.Health.Current; heroes.Health.Restore(); heroes.Health.ApplyHit(attack,heroes.transform.forward,source); int back=hp-heroes.Health.Current; Assert.That(front,Is.GreaterThan(0)); Assert.That(front,Is.LessThan(back)); Assert.That(motor.IsInvulnerable,Is.False); }
            finally { Object.Destroy(source.gameObject); }
        }
        [UnityTest] public IEnumerator LegendaryEchoTriggersOnceOnThirdAttackWithMultipleColliders()
        {
            yield return SceneManager.LoadSceneAsync("ProgressionArena"); yield return null; Bind(); var arena=Object.FindFirstObjectByType<ArenaSession>(); arena.enabled=false;
            var item=LootGenerator.Create(bridge.Catalog,"cais-blade",ItemRarity.Legendary,new System.Random(1)); bridge.Service.Profile.inventory.Add(item); Assert.That(bridge.Equip(item.instanceId,EquipmentSlot.MainHand),Is.True);
            var target=arena.TrainingTarget; target.Configure(1000,CombatFaction.Training); var extra=new GameObject("SecondCollider",typeof(SphereCollider)); extra.layer=10; extra.transform.SetParent(target.transform,false); extra.transform.localPosition=Vector3.up;
            int hits=0; System.Action callback=()=>hits++; target.Damaged+=callback;
            try { motor.Teleport(new Vector3(0,.06f,0),Quaternion.identity); for(int i=0;i<3;i++) { Assert.That(combat.TryAttack(),Is.True); combat.Simulate(1); if(i<2) Assert.That(target.ControlMultiplier,Is.EqualTo(1)); } Assert.That(target.ControlMultiplier,Is.LessThan(1)); Assert.That(hits,Is.EqualTo(4)); }
            finally { target.Damaged-=callback; Object.Destroy(extra); }
        }
        [UnityTest] public IEnumerator ChestDeathAndSceneTransitionPreserveUniqueRewards()
        {
            var room=dungeon.Generator.Rooms[3]; motor.Teleport(room.Center+Vector3.up*.06f,Quaternion.identity); dungeon.Enter(room); DungeonInteractable chest=null;
            foreach(var item in room.interactions) if(item.Kind==DungeonInteractionKind.Chest) chest=item;
            motor.Teleport(chest.transform.position+new Vector3(0,.06f,-1),Quaternion.identity); Assert.That(chest.Use(),Is.True); Assert.That(chest.Use(),Is.False); Assert.That(bridge.Service.Profile.expedition.items.Count,Is.EqualTo(1));
            heroes.Health.ApplyDamage(9999); Assert.That(bridge.Service.Profile.expedition,Is.Null); Assert.That(bridge.Service.Profile.lastResult.outcome,Is.EqualTo("Derrota")); Assert.That(bridge.Service.Profile.inventory.Count,Is.EqualTo(13)); Assert.That(heroes.Health.IsAlive,Is.False);
            var service=bridge.Service; string result=service.Profile.lastResult.runId; yield return SceneManager.LoadSceneAsync("ProgressionArena"); yield return null; Bind(); Assert.That(bridge.Service,Is.SameAs(service)); Assert.That(service.Profile.inventory.Count,Is.EqualTo(13)); Assert.That(service.FinishExpedition(result,true,"Replay"),Is.False);
            var profile=Object.FindFirstObjectByType<ProgressionProfileRuntime>(); var loaded=new ProfileService(bridge.Catalog,new ProfileStore(profile.SavePath)); Assert.That(loaded.Profile.inventory.Count,Is.EqualTo(13)); Assert.That(loaded.Profile.lastResult.runId,Is.EqualTo(result));
        }
        [UnityTest] public IEnumerator CompanionUsesFiniteCooldownChargesAndDoesNotRevive()
        {
            var pet=bridge.Companion; pet.enabled=false; int start=pet.RemainingCharges;
            for(int i=0;i<start;i++) { heroes.Health.ApplyDamage(6); int before=heroes.Health.Current; pet.Simulate(bridge.Catalog.companion.cooldown); Assert.That(heroes.Health.Current,Is.GreaterThan(before)); int after=heroes.Health.Current; pet.Simulate(.01f); Assert.That(heroes.Health.Current,Is.EqualTo(after)); }
            Assert.That(pet.RemainingCharges,Is.Zero); heroes.Health.ApplyDamage(10); int exhausted=heroes.Health.Current; pet.Simulate(100); Assert.That(heroes.Health.Current,Is.EqualTo(exhausted)); heroes.Health.ApplyDamage(9999); pet.ResetBudget(); pet.Simulate(100); Assert.That(heroes.Health.IsAlive,Is.False); yield return null;
        }
        [UnityTest] public IEnumerator InventoryButtonsEquipAndRequireExplicitDiscardConfirmation()
        {
            var hud=Object.FindFirstObjectByType<ProgressionHud>(); hud.OpenInventory(); yield return null; Assert.That(hud.IsOpen,Is.True); var panel=hud.transform.Find("InventoryOverlay/InventoryPanel"); Assert.That(panel,Is.Not.Null);
            foreach(string name in new[]{"Inventory","RunResult","ProfileOptions"}) Assert.That(hud.transform.Find(name).GetComponentInChildren<Text>().text,Is.Not.Empty,name);
            foreach(var button in panel.GetComponentsInChildren<Button>(true)) if(!button.name.StartsWith("ItemRow_")) Assert.That(button.GetComponentInChildren<Text>().text,Is.Not.Empty,button.name);
            panel.Find("ItemRow_0").GetComponent<Button>().onClick.Invoke(); panel.Find("ApplyItem").GetComponent<Button>().onClick.Invoke(); Assert.That(bridge.Equipment.MainWeapon.itemId,Is.EqualTo("cais-blade"));
            panel.Find("ItemRow_1").GetComponent<Button>().onClick.Invoke(); int count=bridge.Service.Profile.inventory.Count; panel.Find("DiscardItem").GetComponent<Button>().onClick.Invoke(); Assert.That(bridge.Service.Profile.inventory.Count,Is.EqualTo(count)); Assert.That(panel.Find("ConfirmDiscard").gameObject.activeSelf,Is.True);
            panel.Find("ConfirmDiscard/Dialog/Cancel").GetComponent<Button>().onClick.Invoke(); Assert.That(bridge.Service.Profile.inventory.Count,Is.EqualTo(count)); panel.Find("DiscardItem").GetComponent<Button>().onClick.Invoke(); panel.Find("ConfirmDiscard/Dialog/Confirm").GetComponent<Button>().onClick.Invoke(); Assert.That(bridge.Service.Profile.inventory.Count,Is.EqualTo(count-1)); hud.Close(); Assert.That(hud.IsOpen,Is.False);
        }
        [UnityTest] public IEnumerator EncounterSpawnsAndEnemyDeathGrantsOnceFromRealPool()
        {
            var room=dungeon.Generator.Rooms[1]; motor.Teleport(room.Center+Vector3.up*.06f,Quaternion.identity); dungeon.Enter(room); Assert.That(bridge.CanManage,Is.False);
            yield return new WaitForSeconds(.8f); EnemyActor enemy=null; foreach(var actor in dungeon.Encounter.Enemies.Actors) if(actor.IsAlive) { enemy=actor; break; } Assert.That(enemy,Is.Not.Null);
            int before=bridge.Service.Profile.expedition.experience; enemy.Health.ApplyDamage(9999); int after=bridge.Service.Profile.expedition.experience; Assert.That(after,Is.EqualTo(before+bridge.Catalog.enemy.experience)); enemy.Health.ApplyDamage(9999); Assert.That(bridge.Service.Profile.expedition.experience,Is.EqualTo(after));
        }
    }
}
