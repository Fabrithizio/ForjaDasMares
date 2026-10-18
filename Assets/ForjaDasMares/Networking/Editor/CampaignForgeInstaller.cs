using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ForjaDasMares.Networking.Editor
{
    public static class CampaignForgeInstaller
    {
        const string Root="Assets/ForjaDasMares/Data/Campaign/Forge/";
        static T Copy<T>(T source,string file)where T:ScriptableObject
        {var result=AssetDatabase.LoadAssetAtPath<T>(Root+file+".asset");if(result!=null)return result;result=UnityEngine.Object.Instantiate(source);result.name=file;AssetDatabase.CreateAsset(result,Root+file+".asset");return result;}
        static AttackDefinition Hit(AttackDefinition source,string file,int damage,float range,int maximum,float arc)
        {var hit=Copy(source,file);hit.damage=damage;hit.range=range;hit.maximumTargets=maximum;hit.arc=arc;EditorUtility.SetDirty(hit);return hit;}
        [MenuItem("Forja das Mares/Campaign/Upgrade profiles and builds 0.11")]
        public static void Upgrade()
        {
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(CampaignInstaller.ScenePath);
            var progression=UnityEngine.Object.FindFirstObjectByType<CoopProgression>();var catalog=Copy(progression.Catalog,"Catalog");
            var roster=UnityEngine.Object.FindFirstObjectByType<DungeonSession>().Heroes;var serialized=new SerializedObject(roster);var heroes=serialized.FindProperty("heroes");
            var profiles=new List<HeroDefinition>();var starters=new List<EquipmentDefinition>();
            for(int h=0;h<heroes.arraySize;h++)
            {
                var original=(HeroDefinition)heroes.GetArrayElementAtIndex(h).objectReferenceValue;var hero=Copy(original,"Hero_"+original.heroId);profiles.Add(hero);heroes.GetArrayElementAtIndex(h).objectReferenceValue=hero;
                hero.maximumHealth=h==0?100:h==1?86:h==2?72:130;
                var attack=Hit(original.attacks[0],"StarterHit_"+hero.heroId,h==0?10:h==1?8:h==2?9:8,h==1?9:h==2?8:1.8f,1,55);attack.splashRadius=0;attack.projectileCount=1;attack.windup=.16f;attack.recovery=.44f;attack.knockback=.5f;
                hero.attacks=new[]{attack};hero.special=Copy(original.special,"StarterSkill_"+hero.heroId);
                if(h==0){hero.special.effect=AbilityEffect.WeaponStrike;hero.special.displayName="Estocada";hero.special.hit=Hit(attack,"StarterThrust",17,2.3f,1,45);hero.special.cooldown=7;}
                var flow=Copy(original.onlineFlow,"Flow_"+hero.heroId);hero.onlineFlow=flow;
                if(h==0){flow.furyDuration=6;flow.furyAttackSpeed=1.55f;flow.hitsForSuper=24;flow.superProtection=0;flow.heavyMultiplier=1.6f;flow.heavyRangeMultiplier=1;}
                var starter=Copy(catalog.equipment.First(x=>x.slot==EquipmentSlot.MainHand&&((int)x.compatibleHeroes&(1<<h))!=0),"Starter_"+hero.heroId);
                starter.itemId="initiation-"+hero.heroId;starter.displayName=h==0?"Espada de Aprendiz":h==1?"Arco de Aprendiz":h==2?"Cajado de Aprendiz":"Lâmina de Vigília";starter.description="Arma comum inicial. Ataque concentrado em um alvo, sem efeitos extras. Encontre armas para definir sua build.";
                starter.slot=EquipmentSlot.MainHand;starter.category=h==1?WeaponCategory.Bow:h==2?WeaponCategory.Staff:WeaponCategory.Sword;starter.compatibleHeroes=(HeroAffinity)(1<<h);starter.starter=true;starter.minimumLevel=1;starter.bonuses=Array.Empty<StatModifier>();starter.attacks=new[]{attack,attack,attack};starter.ability=null;starter.twoHanded=h==1||h==2;starter.allowsOffHand=!starter.twoHanded;starter.twinStrike=false;starter.frontalBlock=0;starter.visualScale=h==1?new Vector3(.12f,.8f,.15f):new Vector3(.1f,.7f,.12f);starter.icon=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ForjaDasMares/Art/ExpeditionSet/Icon_"+starter.category+".png");
                starters.Add(starter);EditorUtility.SetDirty(starter);EditorUtility.SetDirty(hero.special);EditorUtility.SetDirty(flow);EditorUtility.SetDirty(hero);EditorUtility.SetDirty(attack);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(roster);
            var equipment=new List<EquipmentDefinition>();
            foreach(var original in catalog.equipment.Where(x=>!x.starter).ToArray())
            {
                var item=Copy(original,"Item_"+original.itemId);
                if(item.slot==EquipmentSlot.MainHand)
                {
                    item.compatibleHeroes=item.category==WeaponCategory.Axe||item.category==WeaponCategory.Hammer?HeroAffinity.Warrior:item.category==WeaponCategory.Bow||item.category==WeaponCategory.Daggers?HeroAffinity.Ranger:item.category==WeaponCategory.Sword?HeroAffinity.Warrior|HeroAffinity.RuneKnight:HeroAffinity.Arcanist;
                    int power=item.category==WeaponCategory.Hammer?24:item.category==WeaponCategory.Axe?20:item.category==WeaponCategory.Sword?16:item.category==WeaponCategory.Daggers?9:13;
                    var combo=new AttackDefinition[3];for(int i=0;i<3;i++){var source=original.attacks[i%original.attacks.Length];combo[i]=Hit(source,"Combo_"+item.itemId+"_"+i,power+(i==2?4:0),source.range,source.shape==AttackShape.Projectile?0:item.category==WeaponCategory.Hammer?4:item.category==WeaponCategory.Axe?3:2,source.arc);}
                    item.attacks=combo;var ability=Copy(profiles[item.category==WeaponCategory.Bow||item.category==WeaponCategory.Daggers?1:item.category==WeaponCategory.Staff||item.category==WeaponCategory.Grimoire||item.category==WeaponCategory.MagicWeapon?2:0].special,"Skill_"+item.itemId);
                    ability.hit=Hit(combo[2],"SkillHit_"+item.itemId,power+6,item.category==WeaponCategory.Bow?10:3,0,150);ability.cooldown=8;ability.radius=3;ability.hit.projectileCount=item.category==WeaponCategory.Bow?3:1;ability.hit.projectileSpread=16;
                    ability.effect=item.category==WeaponCategory.Bow||item.category==WeaponCategory.Sword||item.category==WeaponCategory.Daggers?AbilityEffect.WeaponStrike:item.category==WeaponCategory.Grimoire?AbilityEffect.Trap:item.category==WeaponCategory.Axe||item.category==WeaponCategory.Hammer?AbilityEffect.AreaImpact:AbilityEffect.ElementalBurst;
                    ability.displayName=item.category==WeaponCategory.Bow?"Salva de Caça":item.category==WeaponCategory.Sword?"Corte Avançado":item.category==WeaponCategory.Daggers?"Perfuração":item.category==WeaponCategory.Axe?"Ceifa":item.category==WeaponCategory.Hammer?"Quebra-Guarda":item.category==WeaponCategory.Grimoire?"Selo Imobilizante":"Ruptura Elemental";
                    if(item.category==WeaponCategory.Hammer){ability.hit.control=ControlEffect.Root;ability.hit.controlDuration=.7f;ability.hit.knockback=5;}
                    if(item.category==WeaponCategory.Grimoire){ability.hit.control=ControlEffect.Root;ability.hit.controlDuration=1.5f;}
                    item.ability=ability;EditorUtility.SetDirty(ability.hit);EditorUtility.SetDirty(ability);
                }
                equipment.Add(item);EditorUtility.SetDirty(item);
            }
            foreach(var starter in starters)if(equipment.All(x=>x.itemId!=starter.itemId))equipment.Add(starter);
            var armor=equipment.First(x=>x.slot==EquipmentSlot.Armor);for(int h=0;h<4;h++)
            {
                var item=Copy(armor,"ClassArmor_"+h);item.itemId="class-armor-"+h;item.displayName=h==0?"Couraça do Duelista":h==1?"Traje do Batedor":h==2?"Manto do Círculo":"Placas do Bastião";item.compatibleHeroes=(HeroAffinity)(1<<h);item.description="Armadura da classe. Proteção e atributos limitados para complementar sua arma.";item.bonuses=new[]{new StatModifier(ItemStat.Health,h==3?24:h==0?16:10),new StatModifier(h==1?ItemStat.AttackSpeed:h==2?ItemStat.CooldownReduction:ItemStat.Defense,h<2&&h!=0||h==2?.06f:7)};if(equipment.All(x=>x.itemId!=item.itemId))equipment.Add(item);EditorUtility.SetDirty(item);
            }
            catalog.equipment=equipment.ToArray();catalog.classStarters=profiles.Select(x=>new ClassStarterRule{heroId=x.heroId,itemId="initiation-"+x.heroId}).ToArray();catalog.starterItems=Array.Empty<string>();catalog.Validate();progression.Configure(catalog);EditorUtility.SetDirty(progression);EditorUtility.SetDirty(catalog);
            var pool=UnityEngine.Object.FindFirstObjectByType<EnemyPool>();var poolData=new SerializedObject(pool).FindProperty("definitions");for(int i=0;i<poolData.arraySize;i++)
            {var enemy=(EnemyDefinition)poolData.GetArrayElementAtIndex(i).objectReferenceValue;enemy.spawnWarning=.9f;if(enemy.kind==EnemyKind.Raider){enemy.maximumHealth=34;enemy.attack=Hit(enemy.attack,"RaiderHit",9,enemy.attack.range,0,enemy.attack.arc);}EditorUtility.SetDirty(enemy);}
            var run=UnityEngine.Object.FindFirstObjectByType<CampaignRun>();for(int i=0;i<run.Count;i++)foreach(var encounter in run.At(i).combatVariants.Concat(new[]{run.At(i).normal,run.At(i).hard}))
            {foreach(var wave in encounter.waves)wave.reinforceThreshold=2;EditorUtility.SetDirty(encounter);}
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("FORGE_UPGRADE_OK heroes="+profiles.Count+" equipment="+catalog.equipment.Length+" soloBonus=false");
        }
    }
}
