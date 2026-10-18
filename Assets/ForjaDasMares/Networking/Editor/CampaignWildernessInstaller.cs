using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ForjaDasMares.Editor;
namespace ForjaDasMares.Networking.Editor
{
    // Upgrade only the existing campaign. Does not rebuild the original scenes or touch saves.
    public static class CampaignWildernessInstaller
    {
        const string Root="Assets/ForjaDasMares/Data/Campaign/Wilderness/";
        const string Art="Assets/ForjaDasMares/Art/ExpeditionSet/";
        static T Save<T>(T data,string name) where T:UnityEngine.Object
        {string path=Root+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<T>(path);if(existing!=null){UnityEngine.Object.DestroyImmediate(data);return existing;}AssetDatabase.CreateAsset(data,path);return data;}
        static GameObject Model(string name)=>AssetDatabase.LoadAssetAtPath<GameObject>(Art+name+".fbx");
        [MenuItem("Forja das Mares/Campaign/Upgrade open landscapes 0.10")]
        public static void Upgrade()
        {
            Directory.CreateDirectory(Root);Directory.CreateDirectory(Art+"Materials");AssetDatabase.Refresh();
            var colors=new Dictionary<string,Color>{["wood"]=new Color(.25f,.10f,.035f),["edge"]=new Color(.55f,.29f,.09f),["steel"]=new Color(.25f,.32f,.34f),["gold"]=new Color(.85f,.49f,.09f),["stone"]=new Color(.20f,.25f,.23f),["leaf"]=new Color(.08f,.28f,.10f),["skin"]=new Color(.32f,.51f,.20f),["cloth"]=new Color(.27f,.06f,.09f),["dark"]=new Color(.045f,.055f,.07f),["cyan"]=new Color(.08f,.7f,.85f),["healing"]=new Color(.15f,.85f,.32f),["fire"]=new Color(.95f,.25f,.035f),["bone"]=new Color(.68f,.60f,.40f)};
            var mats=new Dictionary<string,Material>();foreach(var pair in colors)
            {string path=Art+"Materials/"+pair.Key+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.SetColor("_BaseColor",pair.Value);material.SetFloat("_Smoothness",.25f);AssetDatabase.CreateAsset(material,path);}mats.Add(pair.Key,material);}
            foreach(string file in Directory.GetFiles(Art,"*.fbx"))
            {
                var importer=(ModelImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));importer.importAnimation=false;importer.preserveHierarchy=true;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
                foreach(var mat in mats)importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),mat.Key),mat.Value);
                importer.SaveAndReimport();
            }
            foreach(string file in Directory.GetFiles(Art,"Icon_*.png"))
            {var importer=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=128;importer.SaveAndReimport();}
            var scene=EditorSceneManager.OpenScene(CampaignInstaller.ScenePath);
            var run=UnityEngine.Object.FindFirstObjectByType<CampaignRun>();if(run==null)throw new InvalidOperationException("Existing campaign required.");
            var pool=UnityEngine.Object.FindFirstObjectByType<EnemyPool>();var sourceDefs=AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/ForjaDasMares/Data/Enemies"}).Select(g=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(x=>(int)x.kind).ToArray();
            var defs=new List<EnemyDefinition>();
            string[] models={"raider","guardian","archer","unstable","summoner","raider","elite_warden"};
            foreach(var source in sourceDefs)
            {
                var data=Save(UnityEngine.Object.Instantiate(source),"Enemy_"+source.enemyId);data.enemyId="wild-"+source.enemyId;
                data.poolCapacity=source.kind==EnemyKind.Raider?22:source.kind==EnemyKind.Minion?8:source.kind==EnemyKind.Boss?1:6;
                data.detectionRange=24;data.forgetRange=36;
                if(source.kind==EnemyKind.Raider){data.maximumHealth=28;var hit=Save(UnityEngine.Object.Instantiate(source.attack),"RaiderHit");hit.damage=6;data.attack=hit;EditorUtility.SetDirty(hit);}
                data.prefab=MakePrefab(source.prefab,Model(models[(int)data.kind]),data);defs.Add(data);EditorUtility.SetDirty(data);
            }
            var finalBoss=defs.First(x=>x.kind==EnemyKind.Boss);
            string[] eliteNames={"Malho do Breu","Oráculo das Raízes","Rasga-Cinza"};string[] eliteModels={"elite_warden","elite_shaman","elite_reaver"};
            var elites=new List<EnemyDefinition>();
            for(int i=0;i<3;i++)
            {
                var data=Save(UnityEngine.Object.Instantiate(finalBoss),"Elite"+i);data.enemyId="elite-"+i;data.displayName=eliteNames[i];data.miniboss=true;data.poolCapacity=1;data.maximumHealth=i==0?320:i==1?220:260;data.armor=.08f;data.speed=i==2?3.7f:2.8f;data.height=2.1f;data.radius=.5f;
                var hit=Save(UnityEngine.Object.Instantiate(finalBoss.attack),"EliteHit"+i);hit.damage=14;data.attack=hit;
                var pattern=Save(UnityEngine.Object.Instantiate(finalBoss.boss),"ElitePattern"+i);pattern.rewardSeals=1;pattern.phaseTwoDamage=1.08f;pattern.phaseTwoTiming=.9f;
                pattern.attacks=new[]{new BossAttack{displayName=i==0?"Martelo em arco":i==1?"Raízes marcadas":"Arrancada",kind=i==0?BossAttackKind.Sweep:i==1?BossAttackKind.GroundMarks:BossAttackKind.Charge,hit=hit,warning=1.15f,recovery=1.1f,cooldown=2.2f,range=9,radius=i==1?1.65f:3.5f,length=7,chargeSpeed=7,warningColor=i==1?Color.cyan:new Color(1,.4f,.08f)},new BossAttack{displayName="Retaliação",kind=BossAttackKind.Sweep,hit=hit,warning=1,recovery=1.2f,cooldown=2.8f,range=4,radius=3,arc=150}};
                data.boss=pattern;data.prefab=MakePrefab(finalBoss.prefab,Model(eliteModels[i]),data);elites.Add(data);defs.Add(data);EditorUtility.SetDirty(data);EditorUtility.SetDirty(hit);EditorUtility.SetDirty(pattern);
            }
            var serialized=new SerializedObject(pool);var array=serialized.FindProperty("definitions");array.arraySize=defs.Count;for(int i=0;i<defs.Count;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=defs[i];serialized.ApplyModifiedPropertiesWithoutUndo();
            EnemyDefinition Enemy(EnemyKind kind)=>defs.First(x=>x.kind==kind&&!x.miniboss);
            for(int tier=0;tier<3;tier++)
            {
                var level=run.At(tier);level.openLandscape=true;level.sectorSize=28+tier*2;level.passageWidth=18;
                level.hitSound=AssetDatabase.LoadAssetAtPath<AudioClip>(Art+"Sfx_Hit.wav");level.crateSound=AssetDatabase.LoadAssetAtPath<AudioClip>(Art+"Sfx_Crate.wav");level.healSound=AssetDatabase.LoadAssetAtPath<AudioClip>(Art+"Sfx_Heal.wav");level.lootSound=AssetDatabase.LoadAssetAtPath<AudioClip>(Art+"Sfx_Loot.wav");
                level.coverModels=tier==0?new[]{Model("rock"),Model("crate"),Model("banner")}:tier==1?new[]{Model("bush"),Model("mushrooms"),Model("rock")}:new[]{Model("barrel"),Model("rock"),Model("banner")};
                for(int variant=0;variant<3;variant++)level.combatVariants[variant]=Encounter(Enemy,elites[tier],tier,variant,false);
                level.normal=level.combatVariants[0];level.hard=Encounter(Enemy,elites[tier],tier,0,true);
                var bossEncounter=Save(UnityEngine.Object.Instantiate(level.boss),"FinalBossEncounter"+tier);
                // Replace only the boss reference, preserving existing phase encounter configuration.
                foreach(var wave in bossEncounter.waves)foreach(var entry in wave.entries){var replacement=defs.FirstOrDefault(x=>x.kind==entry.enemy.kind&&!x.miniboss);if(replacement!=null)entry.enemy=replacement;}
                level.boss=bossEncounter;EditorUtility.SetDirty(bossEncounter);EditorUtility.SetDirty(level);level.Validate();
            }
            var adventure=run.At(0).adventure;adventure.suppliesPerSector=3;adventure.supplyHealing=22;adventure.supplyModel=Model("supply");adventure.sealModel=Model("seal");adventure.fountainModel=Model("well");EditorUtility.SetDirty(adventure);
            var catalog=UnityEngine.Object.FindFirstObjectByType<CoopProgression>().Catalog;
            catalog.enemy.experience=1;catalog.enemy.coins=1;catalog.enemy.itemChance=.025f;catalog.miniboss=new RewardRule{rewardId="miniboss",experience=12,coins=8,guaranteedItems=1,minimumRarity=ItemRarity.Uncommon};
            foreach(var item in catalog.equipment)
            {
                string icon=item.slot==EquipmentSlot.Armor?"Armor":item.slot==EquipmentSlot.Accessory?"Accessory":item.slot==EquipmentSlot.Artifact?"Artifact":item.category.ToString();
                item.icon=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"Icon_"+icon+".png");if(item.icon==null)throw new InvalidOperationException("Missing icon "+icon);
                if(item.slot==EquipmentSlot.MainHand&&item.attacks!=null&&item.attacks.Length>0&&AssetDatabase.GetAssetPath(item.attacks[0])!=Root+"Combo_"+item.itemId+"_0.asset")
                {
                    var basic=item.attacks[0];item.attacks=new AttackDefinition[3];
                    for(int i=0;i<3;i++)
                    {
                        var attack=Save(UnityEngine.Object.Instantiate(basic),"Combo_"+item.itemId+"_"+i);attack.damage=Mathf.Max(1,Mathf.RoundToInt(basic.damage*(i==2?1.4f:i==1?.85f:1)));attack.windup=basic.windup+(i==2?.06f:0);attack.recovery=basic.recovery*(i==2?1.2f:.85f);
                        if(attack.shape==AttackShape.Arc){attack.arc=i==2?Mathf.Min(260,basic.arc+65):i==1?Mathf.Max(35,basic.arc-35):basic.arc;attack.range=basic.range*(i==1?1.2f:1);attack.knockback=i==2?basic.knockback*1.5f:basic.knockback;}
                        else if(i==2&&item.category==WeaponCategory.Bow){attack.projectileCount=Mathf.Max(3,basic.projectileCount);attack.projectileSpread=10;attack.damage=Mathf.RoundToInt(basic.damage*.65f);}
                        else if(i==2){attack.splashRadius=Mathf.Max(1,basic.splashRadius);}
                        EditorUtility.SetDirty(attack);item.attacks[i]=attack;
                    }
                }
                EditorUtility.SetDirty(item);
            }
            foreach(string heroId in new[]{"ranger","arcanist","rune-knight"})
            {
                var hero=AssetDatabase.FindAssets("t:HeroDefinition",new[]{"Assets/ForjaDasMares/Data"}).Select(g=>AssetDatabase.LoadAssetAtPath<HeroDefinition>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault(h=>h.heroId==heroId);if(hero==null)throw new InvalidOperationException("Hero missing "+heroId);
                var flow=Save(ScriptableObject.CreateInstance<CombatFlowDefinition>(),"Flow_"+heroId);flow.holdDuration=1.15f;flow.heavyCooldown=4;flow.heavyMultiplier=1.8f;flow.heavyRangeMultiplier=1.2f;flow.hitsForSuper=24;
                var hit=Save(UnityEngine.Object.Instantiate(hero.attacks[0]),"Super_"+heroId);hit.damage=heroId=="ranger"?30:50;hit.range=heroId=="ranger"?12:6;hit.shape=heroId=="ranger"?AttackShape.Projectile:AttackShape.Arc;hit.arc=360;hit.projectileCount=5;hit.projectileSpread=12;hit.knockback=4;hit.windup=.3f;hit.recovery=.7f;
                if(heroId=="arcanist"){hit.control=ControlEffect.Root;hit.controlDuration=1.5f;hit.color=Color.cyan;}
                if(heroId=="rune-knight"){flow.superProtection=.4f;flow.protectionDuration=4;}
                flow.superAttack=hit;hero.onlineFlow=flow;EditorUtility.SetDirty(hero);EditorUtility.SetDirty(flow);EditorUtility.SetDirty(hit);
            }
            EditorUtility.SetDirty(catalog);catalog.Validate();EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("WILDERNESS_UPGRADE_OK open=true size=28,30,32 elite=3 icons=12 hardEnemies>100");
        }
        static GameObject MakePrefab(GameObject source,GameObject model,EnemyDefinition data)
        {
            if(model==null)throw new InvalidOperationException("Generate Blender models first.");var root=PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(source));
            var view=root.GetComponent<EnemyPresentation>();var serialized=new SerializedObject(view);var previous=serialized.FindProperty("model").objectReferenceValue as Transform;if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
            var visual=(GameObject)PrefabUtility.InstantiatePrefab(model);visual.transform.SetParent(root.transform,false);visual.transform.localScale=Vector3.one*(data.height/2);visual.AddComponent<EnemyModelMotion>();
            var parts=visual.GetComponentsInChildren<Renderer>();var tint=parts.Where(x=>x.name.StartsWith("Torso")).ToArray();var shield=parts.FirstOrDefault(x=>x.name.StartsWith("Shield")&&!x.name.StartsWith("ShieldRune"));
            view.Configure(root.GetComponent<EnemyActor>(),visual.transform,tint,shield!=null?shield.gameObject:null);
            var result=PrefabUtility.SaveAsPrefabAsset(root,Root+data.enemyId+".prefab");PrefabUtility.UnloadPrefabContents(root);return result;
        }
        static EncounterDefinition Encounter(Func<EnemyKind,EnemyDefinition> enemy,EnemyDefinition elite,int tier,int variant,bool hard)
        {
            var data=Save(ScriptableObject.CreateInstance<EncounterDefinition>(),"Horde_"+tier+"_"+(hard?"Hard":variant.ToString()));data.encounterId="wild-"+tier+"-"+(hard?"hard":variant.ToString());data.displayName=hard?"Horda e comandante":"Escaramuça "+(variant+1);data.maximumMobileEnemies=20;data.maximumPcEnemies=24;
            int waves=hard?4+tier:2+tier;data.waves=new EncounterWave[waves];
            for(int i=0;i<waves;i++)
            {
                var entries=new List<SpawnEntry>{Entry(enemy(EnemyKind.Raider),hard?20:8),Entry(enemy(EnemyKind.Archer),variant==1?4:3),Entry(enemy(EnemyKind.Guardian),hard?3:1)};
                if(hard||tier>0)entries.Add(Entry(enemy(EnemyKind.Unstable),1));if(tier==2&&i==waves-1)entries.Add(Entry(enemy(EnemyKind.Summoner),1));if(hard&&i==waves-1)entries.Add(Entry(elite,1));
                data.waves[i]=new EncounterWave{displayName="Reforços "+(i+1),entries=entries.ToArray(),delayBefore=2};
            }
            EditorUtility.SetDirty(data);return data;
        }
        static SpawnEntry Entry(EnemyDefinition enemy,int count)=>new SpawnEntry{enemy=enemy,count=count,delay=.28f};
    }
}
