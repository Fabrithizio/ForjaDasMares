using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class ArsenalTests
    {
        CoopDirector session;ProgressionCatalog catalog;
        float previousCapture;
        [UnitySetUp]public IEnumerator Load(){previousCapture=Time.captureDeltaTime;Time.captureDeltaTime=1f/60;SceneManager.LoadScene("CampaignCoop");yield return null;yield return null;session=UnityEngine.Object.FindFirstObjectByType<CoopDirector>();session.SelectHero(0);catalog=session.GetComponent<CoopProgression>().Catalog;var profile=session.GetComponent<CoopProfileChannel>().LocalProfile;}
        [UnityTearDown]public IEnumerator Close(){Time.captureDeltaTime=previousCapture;session.Close();yield return null;yield return null;}
        [Test]public void TwentyDamageAgainstTenPercentDefenseDealsEighteen()
        {
            var go=new GameObject("DefenseMathQA");var attack=ScriptableObject.CreateInstance<AttackDefinition>();try{var hp=go.AddComponent<Health>();hp.Configure(100,CombatFaction.Enemy,.10f);attack.damage=20;attack.knockback=0;Assert.IsTrue(hp.ApplyHit(attack,Vector3.forward,session.Actor(0).Health));Assert.AreEqual(82,hp.Current);hp.Configure(100,CombatFaction.Enemy,.70f);hp.ApplyHit(attack,Vector3.forward,session.Actor(0).Health);Assert.AreEqual(94,hp.Current);}finally{UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(attack);}
        }
        [Test]public void NumericDamageAndCriticalUseTheSameBaseAndModifiers()
        {
            var original=catalog.equipment.First(x=>x.category==WeaponCategory.Sword);var data=UnityEngine.Object.Instantiate(original);var hit=UnityEngine.Object.Instantiate(original.attacks[0]);var c=UnityEngine.Object.Instantiate(catalog);var hero=UnityEngine.Object.Instantiate(session.Actor(0).Roster.Current);
            try{hit.damage=20;data.attacks=new[]{hit};data.bonuses=new[]{new StatModifier(ItemStat.Damage,.25f),new StatModifier(ItemStat.CriticalChance,.10f),new StatModifier(ItemStat.CriticalDamage,.25f)};c.equipment=c.equipment.Select(x=>x==original?data:x).ToArray();var item=LootGenerator.Create(c,data.itemId,ItemRarity.Common,new System.Random(1));var p=new PlayerProfile();p.inventory.Add(item);var set=new HeroLoadout{heroId=hero.heroId};set.items[0]=item.instanceId;p.loadouts.Add(set);hero.armor=.70f;
            var s=ProgressionStats.Calculate(hero,p,c);Assert.AreEqual(20,s.baseDamage);Assert.AreEqual(25,s.damage);Assert.AreEqual(44,s.criticalDamage);Assert.AreEqual(1.75f,s.criticalMultiplier,.001f);Assert.AreEqual(.1f,s.criticalChance,.001f);Assert.AreEqual(70,s.defensePoints,.01f);Assert.AreEqual(.7f,s.defense,.001f);Assert.AreEqual(s.criticalDamage,Mathf.RoundToInt(hit.damage*ProgressionStats.StrikeMultiplier(s.damageMultiplier,0,true,s.criticalMultiplier)));}
            finally{UnityEngine.Object.DestroyImmediate(data);UnityEngine.Object.DestroyImmediate(hit);UnityEngine.Object.DestroyImmediate(c);UnityEngine.Object.DestroyImmediate(hero);}
        }
        [Test]public void FootprintsDistinguishThinLineConeAndFullCircle()
        {
            Assert.IsTrue(MeleeGeometry.Contains(new Vector3(.2f,0,2.5f),Vector3.forward,3,50,.6f,MeleeFootprint.Line));Assert.IsFalse(MeleeGeometry.Contains(new Vector3(.8f,0,2),Vector3.forward,3,50,.6f,MeleeFootprint.Line));Assert.IsFalse(MeleeGeometry.Contains(Vector3.back,Vector3.forward,3,50,.6f,MeleeFootprint.Line));
            Assert.IsTrue(MeleeGeometry.Contains(new Vector3(1,0,2),Vector3.forward,3,100,.6f,MeleeFootprint.Cone));Assert.IsFalse(MeleeGeometry.Contains(Vector3.back,Vector3.forward,3,100,.6f,MeleeFootprint.Cone));Assert.IsTrue(MeleeGeometry.Contains(Vector3.back,Vector3.forward,3,360,.6f,MeleeFootprint.Circle));
        }
        [Test]public void NineWeaponCategoriesHaveAuthoredMeshesAndRenderedIcons()
        {
            foreach(var category in Enum.GetValues(typeof(WeaponCategory)).Cast<WeaponCategory>().Where(x=>x!=WeaponCategory.None))
            {var items=catalog.equipment.Where(x=>x.category==category).ToArray();Assert.IsNotEmpty(items,category.ToString());foreach(var item in items){Assert.NotNull(item.visualMesh,item.itemId);Assert.Greater(item.visualMesh.vertexCount,100);Assert.IsTrue(item.visualMaterials.Length>0&&item.visualMaterials.All(x=>x!=null));Assert.NotNull(item.icon);Assert.That(item.visualMesh.bounds.size.y,Is.InRange(.4f,2.5f));}}
            var hand=session.Actor(0).GetComponentsInChildren<EquipmentMeshView>(true).First();Assert.Greater(hand.GetComponent<MeshFilter>().sharedMesh.vertexCount,100);
        }
        [Test]public void ClipsExistForThreeWeaponStylesWithoutLosingLocomotion()
        {
            var model=session.Actor(0).Presentation.Visible.GetComponentInChildren<HeroModelView>();Assert.NotNull(model);
            string[] names={"Idle","Walk","Run","SwordSlash","SwordReturn","SwordThrust","AxeCleave","AxeBackhand","AxeSpin","HammerSweep","HammerBack","HammerSlam","HeavySword","HeavyAxe","HeavyHammer"};
            foreach(string name in names)Assert.IsTrue(model.Animator.runtimeAnimatorController.animationClips.Any(x=>x.name==name&&x.length>.1f),name);
            Assert.IsFalse(model.Animator.applyRootMotion);Assert.AreEqual(2,model.Animator.layerCount);
        }
        [Test]public void EmberWarriorHasOneSkinAndProportionalEquipment()
        {
            var view=session.Actor(0).Presentation.Visible.GetComponentInChildren<HeroModelView>();Assert.IsTrue(view.name.StartsWith("EmberHero"));
            var skins=view.GetComponentsInChildren<SkinnedMeshRenderer>();Assert.AreEqual(1,skins.Length);Assert.AreEqual(72,skins[0].bones.Length);
            Assert.That(skins[0].bounds.size.y,Is.InRange(1.5f,2.1f));Assert.NotNull(view.GetComponent<BrasaFootPlant>());
            var sword=catalog.equipment.First(x=>x.category==WeaponCategory.Sword);float factor=view.EquipmentScale(sword);
            Vector3 size=Vector3.Scale(sword.visualMesh.bounds.size,sword.authoredScale)*factor;Assert.LessOrEqual(Mathf.Max(size.x,size.y,size.z),1.101f);
        }
        [UnityTest]public IEnumerator NormalCombatHasNoDebugConeOrLegacyWireGlyphs()
        {
            Assert.IsTrue(session.HostDirect("CLEANFX",7894,false));yield return null;session.StartGame();float deadline=Time.realtimeSinceStartup+10;
            while(session.Phase==CoopPhase.Loading&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.AreEqual(CoopPhase.Playing,session.Phase);var actor=session.Actor(0);Assert.IsTrue(actor.Combat.TryAttack());
            var ribbon=actor.GetComponent<WeaponSweepRibbon>();Assert.NotNull(ribbon);bool seen=false;
            for(int i=0;i<25;i++){yield return null;seen|=ribbon.Visible;}
            Assert.IsFalse(actor.GetComponent<HeroAttackArea>().Visible);
            Assert.IsFalse(actor.GetComponentsInChildren<LineRenderer>(true).Any(x=>x.name.StartsWith("PrismaGlyph_")||x.name.StartsWith("HeroEnergy_")));
            Assert.IsTrue(seen,"Actual weapon motion must generate the ribbon. state="+actor.Presentation.State+" progress="+actor.Presentation.ActionProgress);Assert.AreEqual(24,ribbon.SegmentCapacity);Assert.IsNull(actor.transform.Find("MotionWeaponRibbon").GetComponent<Collider>());
        }
        [Test]public void SwordAxeAndHammerCombosHaveDifferentRealHitAreas()
        {
            var sword=catalog.equipment.First(x=>x.category==WeaponCategory.Sword&&!x.starter);var axe=catalog.equipment.First(x=>x.category==WeaponCategory.Axe);var hammer=catalog.equipment.First(x=>x.category==WeaponCategory.Hammer);
            Assert.AreEqual(MeleeFootprint.Line,sword.attacks[2].footprint);Assert.AreEqual(MeleeFootprint.Circle,axe.attacks[2].footprint);Assert.AreEqual(360,axe.attacks[2].arc);Assert.AreEqual(MeleeFootprint.Line,hammer.attacks[0].footprint);Assert.Greater(hammer.attacks[0].width,sword.attacks[2].width);
        }
        [UnityTest]public IEnumerator RealLineAttackDeduplicatesCollidersAndRespectsWalls()
        {
            var victim=GameObject.CreatePrimitive(PrimitiveType.Capsule);var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);var hit=ScriptableObject.CreateInstance<AttackDefinition>();try{
            victim.layer=10;victim.transform.position=new Vector3(100,1,102);victim.AddComponent<SphereCollider>();var hp=victim.AddComponent<Health>();hp.Configure(100,CombatFaction.Enemy,.1f);wall.layer=9;wall.transform.position=new Vector3(100,1,101);wall.transform.localScale=new Vector3(2,3,.2f);wall.SetActive(false);hit.damage=20;hit.footprint=MeleeFootprint.Line;hit.width=.6f;hit.maximumTargets=3;hit.knockback=0;Physics.SyncTransforms();
            var resolver=new DamageResolver();int count=resolver.Area(new Vector3(100,1,100),3,Vector3.forward,50,hit,session.Actor(0).Health,1<<10,1<<9);Assert.AreEqual(1,count);Assert.AreEqual(82,hp.Current);hp.Restore();wall.SetActive(true);Physics.SyncTransforms();Assert.AreEqual(0,resolver.Area(new Vector3(100,1,100),3,Vector3.forward,50,hit,session.Actor(0).Health,1<<10,1<<9));Assert.AreEqual(100,hp.Current);
            }finally{UnityEngine.Object.Destroy(victim);UnityEngine.Object.Destroy(wall);UnityEngine.Object.Destroy(hit);}yield return null;
        }
        [Test]public void WeaponCategorySelectsItsOwnAnimatorStates()
        {
            var actor=session.Actor(0);actor.Presentation.enabled=false;var model=actor.Presentation.Visible.GetComponentInChildren<HeroModelView>();int version=0;
            var categories=new[]{WeaponCategory.Sword,WeaponCategory.Axe,WeaponCategory.Axe,WeaponCategory.Hammer,WeaponCategory.Hammer};var variants=new[]{0,0,2,2,3};var names=new[]{"SwordSlash","AxeCleave","AxeSpin","HammerSlam","HeavyHammer"};
            for(int i=0;i<names.Length;i++){model.SetWeaponCategory(categories[i]);model.Animate(HeroState.Attack,.5f,0,true,variants[i],++version,.3f);model.Animator.Update(.15f);Assert.IsTrue(model.Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer."+names[i]),names[i]);}
        }
        [UnityTest]public IEnumerator GuaranteedCriticalAppliesConfiguredMultiplierInRealCombat()
        {
            Assert.IsTrue(session.HostDirect("ARS2",7899,false));yield return null;session.StartGame();float deadline=Time.realtimeSinceStartup+5;while(session.Phase!=CoopPhase.Playing&&Time.realtimeSinceStartup<deadline)yield return null;
            var actor=session.Actor(0);var pool=session.Dungeon.Encounter.Enemies;var definition=Enumerable.Range(0,pool.Capacity).Select(pool.DefinitionAt).First(x=>x.kind==EnemyKind.Raider);var enemy=pool.TrySpawn(definition,actor.transform.position+actor.transform.forward*1.5f);Assert.NotNull(enemy);enemy.Brain.enabled=false;enemy.Health.Configure(1000,CombatFaction.Enemy,0);var hit=UnityEngine.Object.Instantiate(actor.Combat.BasicAttack);hit.damage=20;hit.range=3;hit.arc=120;hit.knockback=0;hit.footprint=MeleeFootprint.Cone;
            try{actor.Combat.ApplyEquipment(new[]{hit},new StatSnapshot{damageMultiplier=1.25f,criticalChance=1,criticalMultiplier=1.75f,attackSpeed=1,cooldownMultiplier=1},1,false);Physics.SyncTransforms();Assert.IsTrue(actor.Combat.TryAttack());actor.Combat.Simulate(hit.windup+.01f);Assert.AreEqual(956,enemy.Health.Current,"20 ×1.25 ×1.75 =44 after rounding");}
            finally{actor.Combat.ResetCombat();actor.Combat.ApplyEquipment(null,new StatSnapshot{damageMultiplier=1,attackSpeed=1,cooldownMultiplier=1,criticalMultiplier=1.5f},1,false);UnityEngine.Object.Destroy(hit);}
        }
        [UnityTest]public IEnumerator AttackIndicatorHasNoColliderAndCannotInflictDamage()
        {
            Assert.IsTrue(session.HostDirect("ARS1",7898,false));yield return null;session.StartGame();float deadline=Time.realtimeSinceStartup+5;while(session.Phase!=CoopPhase.Playing&&Time.realtimeSinceStartup<deadline)yield return null;Assert.AreEqual(CoopPhase.Playing,session.Phase);var actor=session.Actor(0);int hp=actor.Health.Current;Assert.IsTrue(actor.Combat.TryAttack(),"Attack did not start");var area=actor.GetComponent<HeroAttackArea>();Assert.NotNull(area);area.ShowDebugFootprint(true);for(int i=0;i<5&&!area.Visible;i++)yield return null;Assert.IsTrue(area.Visible,"state="+actor.Presentation.State+" replica="+actor.Presentation.ReplicaMode+" busy="+actor.Combat.IsBusy);Assert.IsNull(actor.transform.Find("AttackFootprint").GetComponent<Collider>());Assert.AreEqual(hp,actor.Health.Current);yield return new WaitForSeconds(.8f);Assert.IsFalse(area.Visible);
        }
    }
}
