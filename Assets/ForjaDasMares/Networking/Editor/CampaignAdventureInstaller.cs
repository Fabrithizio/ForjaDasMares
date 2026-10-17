using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace ForjaDasMares.Networking.Editor
{
    public static class CampaignAdventureInstaller
    {
        const string Root=CampaignInstaller.DataRoot;
        [MenuItem("Forja das Mares/Campaign/Upgrade adventure 0.9")]
        public static void Upgrade()
        {
            var path=Root+"Adventure.asset";
            var adventure=AssetDatabase.LoadAssetAtPath<CampaignAdventureDefinition>(path);
            if(adventure==null){adventure=ScriptableObject.CreateInstance<CampaignAdventureDefinition>();AssetDatabase.CreateAsset(adventure,path);}
            var definitions=AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/ForjaDasMares/Data/Enemies"}).Select(g=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
            EnemyDefinition Enemy(EnemyKind kind)=>definitions.First(x=>x.kind==kind);
            for(int tier=0;tier<3;tier++)
            {
                var level=AssetDatabase.LoadAssetAtPath<CampaignLevel>(Root+"Level"+(tier+1)+".asset");
                if(level==null)throw new InvalidOperationException("Existing campaign is required. Do not create a new project.");
                level.adventure=adventure;level.combatVariants=new EncounterDefinition[3];
                for(int variant=0;variant<3;variant++)
                {
                    string file=Root+"AdventureEncounter_"+tier+"_"+variant+".asset";var encounter=AssetDatabase.LoadAssetAtPath<EncounterDefinition>(file);
                    if(encounter==null)
                    {
                        encounter=ScriptableObject.CreateInstance<EncounterDefinition>();encounter.encounterId="adventure-"+tier+"-"+variant;
                        encounter.displayName=variant==0?"Patrulha da estrada":variant==1?"Linha de tiro":"Cerco dos guardiões";
                        encounter.maximumMobileEnemies=encounter.maximumPcEnemies=10;
                        var opening=variant==1?new[]{Entry(Enemy(EnemyKind.Archer),2),Entry(Enemy(EnemyKind.Raider),3)}:new[]{Entry(Enemy(EnemyKind.Raider),4),Entry(Enemy(EnemyKind.Guardian),1)};
                        var closing=new System.Collections.Generic.List<SpawnEntry>{Entry(Enemy(EnemyKind.Raider),3),Entry(Enemy(variant==2?EnemyKind.Guardian:EnemyKind.Archer),2)};
                        if(tier>0)closing.Add(Entry(Enemy(EnemyKind.Unstable),tier));
                        if(tier==2&&variant==2)closing.Add(Entry(Enemy(EnemyKind.Summoner),1));
                        encounter.waves=new[]{new EncounterWave{displayName="Contato",entries=opening,delayBefore=1.5f},new EncounterWave{displayName="Reforços",entries=closing.ToArray(),delayBefore=2.5f}};
                        AssetDatabase.CreateAsset(encounter,file);
                    }
                    level.combatVariants[variant]=encounter;
                }
                EditorUtility.SetDirty(level);level.Validate();
            }
            var catalog=AssetDatabase.LoadAssetAtPath<ProgressionCatalog>(Root+"Catalog.asset");
            catalog.supplies=new RewardRule{rewardId="supplies",experience=1,coins=2,itemChance=.03f};
            catalog.exploration=new RewardRule{rewardId="exploration",experience=30,coins=15,guaranteedItems=1,minimumRarity=ItemRarity.Rare};
            catalog.completion.itemChance=0;catalog.boss.itemChance=0;catalog.completion.guaranteedItems=1;catalog.completion.minimumRarity=ItemRarity.Uncommon;catalog.boss.minimumRarity=ItemRarity.Rare;
            AddWeapon(catalog,WeaponCategory.Sword,"wavecleaver","Corta-Maré","Golpes largos para abrir espaço entre hordas.",160,1,0);
            AddWeapon(catalog,WeaponCategory.Bow,"reedvolley","Salva de Junco","Três flechas em leque: cobertura ampla, cada flecha causa menos dano.",0,3,12);
            AddWeapon(catalog,WeaponCategory.Staff,"cinderfocus","Foco de Brasa","Orbe lento com explosão pequena; exige manter distância.",0,1,0);
            catalog.Validate();EditorUtility.SetDirty(catalog);
            string flowPath=Root+"WarriorOnlineFlow.asset";var flow=AssetDatabase.LoadAssetAtPath<CombatFlowDefinition>(flowPath);
            if(flow==null){flow=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<CombatFlowDefinition>("Assets/ForjaDasMares/Data/MapStyles/Galerias/CombatFlow.asset"));flow.holdDuration=1.2f;flow.heavyCooldown=4;flow.heavyMultiplier=2.1f;flow.hitsForSuper=18;AssetDatabase.CreateAsset(flow,flowPath);}
            foreach(var guid in AssetDatabase.FindAssets("t:HeroDefinition",new[]{"Assets/ForjaDasMares/Data"}))
            {var hero=AssetDatabase.LoadAssetAtPath<HeroDefinition>(AssetDatabase.GUIDToAssetPath(guid));if(hero.heroId=="warrior"){hero.onlineFlow=flow;EditorUtility.SetDirty(hero);}}
            var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layers=tags.FindProperty("layers");
            if(!string.IsNullOrEmpty(layers.GetArrayElementAtIndex(CampaignAdventure.PropLayer).stringValue)&&layers.GetArrayElementAtIndex(CampaignAdventure.PropLayer).stringValue!="CampaignProps")throw new InvalidOperationException("Layer 14 is already used.");
            layers.GetArrayElementAtIndex(CampaignAdventure.PropLayer).stringValue="CampaignProps";tags.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();Debug.Log("CAMPAIGN_ADVENTURE_UPGRADED variants=9 seals=3 supplies=true fountains=true weapons=3");
        }
        static SpawnEntry Entry(EnemyDefinition enemy,int count)=>new SpawnEntry{enemy=enemy,count=count,delay=1.15f};
        static void AddWeapon(ProgressionCatalog catalog,WeaponCategory category,string id,string name,string description,float arc,int projectiles,float spread)
        {
            if(catalog.Find(id)!=null)return;
            var source=catalog.equipment.First(x=>x.category==category&&x.slot==EquipmentSlot.MainHand);
            var item=UnityEngine.Object.Instantiate(source);item.name=item.displayName=name;item.itemId=id;item.description=description;item.minimumLevel=1;item.attacks=new AttackDefinition[source.attacks.Length];
            for(int i=0;i<item.attacks.Length;i++)
            {
                var attack=UnityEngine.Object.Instantiate(source.attacks[i]);
                if(category==WeaponCategory.Sword){attack.arc=arc;attack.range*=1.12f;attack.recovery*=1.15f;}
                if(category==WeaponCategory.Bow){attack.projectileCount=projectiles;attack.projectileSpread=spread;attack.damage=Mathf.Max(1,Mathf.RoundToInt(attack.damage*.55f));attack.recovery*=1.2f;}
                if(category==WeaponCategory.Staff){attack.splashRadius=1.15f;attack.projectileSpeed*=.7f;attack.recovery*=1.25f;attack.damage=Mathf.Max(1,Mathf.RoundToInt(attack.damage*.8f));}
                AssetDatabase.CreateAsset(attack,Root+id+"Attack"+i+".asset");item.attacks[i]=attack;
            }
            AssetDatabase.CreateAsset(item,Root+id+".asset");catalog.equipment=catalog.equipment.Concat(new[]{item}).ToArray();
        }
    }
}
