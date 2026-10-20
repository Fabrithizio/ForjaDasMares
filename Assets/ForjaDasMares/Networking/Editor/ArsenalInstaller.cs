using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
namespace ForjaDasMares.Networking.Editor
{
    public static class ArsenalInstaller
    {
        const string Root="Assets/ForjaDasMares/Art/Arsenal/";
        static readonly string[] Moves={"SwordSlash","SwordReturn","SwordThrust","AxeCleave","AxeBackhand","AxeSpin","HammerSweep","HammerBack","HammerSlam","HeavySword","HeavyAxe","HeavyHammer"};
        [Serializable]sealed class Palette{public Entry[] materials;}
        [Serializable]sealed class Entry{public string name;public float[] color;public float roughness,metallic;}
        static Mesh Bake(MeshRenderer renderer,WeaponCategory category)
        {
            string directory=Root+"Meshes/";Directory.CreateDirectory(directory);AssetDatabase.Refresh();string path=directory+category+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing!=null)return existing;
            var mesh=UnityEngine.Object.Instantiate(renderer.GetComponent<MeshFilter>().sharedMesh);mesh.name="Authored_"+category;var transform=renderer.transform.localToWorldMatrix;var vertices=mesh.vertices;for(int i=0;i<vertices.Length;i++)vertices[i]=transform.MultiplyPoint3x4(vertices[i]);mesh.vertices=vertices;mesh.RecalculateBounds();mesh.RecalculateNormals();
            float height=category==WeaponCategory.Sword?1.66f:category==WeaponCategory.Axe?1.49f:category==WeaponCategory.Hammer?1.62f:category==WeaponCategory.Bow?1.56f:category==WeaponCategory.Daggers?1.03f:category==WeaponCategory.Grimoire ? .64f:category==WeaponCategory.Shield ? .87f:category==WeaponCategory.Staff?1.97f:1.45f;
            float scale=height/Mathf.Max(.01f,mesh.bounds.size.y);for(int i=0;i<vertices.Length;i++)vertices[i]*=scale;mesh.vertices=vertices;mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);Debug.Log("ARSENAL_MESH "+category+" bounds="+mesh.bounds.size+" vertices="+mesh.vertexCount);return mesh;
        }
        public static void Upgrade()
        {
            AssetDatabase.Refresh();var palette=JsonUtility.FromJson<Palette>(File.ReadAllText(Root+"palette.json"));Directory.CreateDirectory(Root+"Materials");AssetDatabase.Refresh();
            foreach(var entry in palette.materials)
            {
                string path=Root+"Materials/"+entry.name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
                material.SetColor("_BaseColor",new Color(entry.color[0],entry.color[1],entry.color[2],entry.color[3]));material.SetFloat("_Metallic",entry.metallic);material.SetFloat("_Smoothness",1-entry.roughness);EditorUtility.SetDirty(material);
            }
            foreach(string kind in Enum.GetNames(typeof(WeaponCategory)).Where(x=>x!="None"))
            {
                string path=Root+kind+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.importAnimation=false;importer.isReadable=true;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
                foreach(var entry in palette.materials)importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),entry.name),AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/"+entry.name+".mat"));importer.SaveAndReimport();
            }
            foreach(string kind in Enum.GetNames(typeof(WeaponCategory)).Where(x=>x!="None")){string icon=Root+"Icons/"+kind+".png";var texture=(TextureImporter)AssetImporter.GetAtPath(icon);texture.textureType=TextureImporterType.Sprite;texture.spriteImportMode=SpriteImportMode.Single;texture.alphaIsTransparency=true;texture.mipmapEnabled=false;texture.SaveAndReimport();}
            var catalog=AssetDatabase.LoadAssetAtPath<ProgressionCatalog>("Assets/ForjaDasMares/Data/Campaign/Forge/Catalog.asset");catalog.baseCriticalMultiplier=1.5f;catalog.maximumDefensePoints=75;
            if(!catalog.affixes.Any(x=>x.stat==ItemStat.CriticalDamage))catalog.affixes=catalog.affixes.Concat(new[]{new AffixRule{stat=ItemStat.CriticalDamage,minimum=.06f,maximum=.18f}}).ToArray();
            foreach(var item in catalog.equipment)
            {
                if(item.category!=WeaponCategory.None)
                {
                    item.icon=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Icons/"+item.category+".png");var model=AssetDatabase.LoadAssetAtPath<GameObject>(Root+item.category+".fbx");var renderer=model.GetComponentInChildren<MeshRenderer>();item.visualMesh=Bake(renderer,item.category);item.visualMaterials=renderer.sharedMaterials;item.authoredScale=Vector3.one*(item.starter ? .80f : 1);
                }
                if(item.attacks!=null)for(int i=0;i<item.attacks.Length;i++)
                {
                    var a=item.attacks[i];if(a.shape==AttackShape.Projectile)continue;
                    a.footprint=item.category==WeaponCategory.Hammer?MeleeFootprint.Line:item.category==WeaponCategory.Axe&&i==2?MeleeFootprint.Circle:item.category==WeaponCategory.Sword&&i==2||item.category==WeaponCategory.Daggers?MeleeFootprint.Line:MeleeFootprint.Cone;
                    if(a.footprint==MeleeFootprint.Line){a.width=item.category==WeaponCategory.Hammer?1.0f:.55f;a.range=Mathf.Max(a.range,item.category==WeaponCategory.Hammer?2.8f:2.3f);}
                    if(a.footprint==MeleeFootprint.Circle)a.arc=360;EditorUtility.SetDirty(a);
                }
                if(item.ability!=null&&item.ability.effect==AbilityEffect.WeaponStrike&&item.ability.hit.shape!=AttackShape.Projectile){item.ability.hit.footprint=item.category==WeaponCategory.Sword||item.category==WeaponCategory.Daggers?MeleeFootprint.Line:MeleeFootprint.Cone;item.ability.hit.width=.65f;EditorUtility.SetDirty(item.ability.hit);}
                if(!item.starter&&item.slot==EquipmentSlot.Accessory&&!item.bonuses.Any(x=>x.stat==ItemStat.CriticalDamage))item.bonuses=item.bonuses.Concat(new[]{new StatModifier(ItemStat.CriticalDamage,.15f)}).ToArray();
                EditorUtility.SetDirty(item);
            }
            // Shared starter attacks must stay single-target cones (same instance in three combo entries).
            foreach(var item in catalog.equipment.Where(x=>x.starter))foreach(var attack in item.attacks){attack.footprint=MeleeFootprint.Cone;EditorUtility.SetDirty(attack);}
            string brasa="Assets/ForjaDasMares/Art/Characters/Brasa/Brasa.fbx";var brasaImporter=(ModelImporter)AssetImporter.GetAtPath(brasa);var clips=brasaImporter.defaultClipAnimations;
            string[] all={"Idle","Walk","Run","Attack","Attack2","Attack3","Charged","Skill","Super","Dodge","Hit","Dead","Victory"};
            foreach(var c in clips){string name=all.Concat(Moves).FirstOrDefault(x=>c.name.EndsWith("|"+x,StringComparison.Ordinal)||c.name==x||c.name.EndsWith("_"+x,StringComparison.Ordinal));if(name==null)continue;c.name=name;c.loopTime=name=="Idle"||name=="Walk"||name=="Run"||name=="Victory";c.lockRootRotation=c.lockRootPositionXZ=c.lockRootHeightY=true;}
            brasaImporter.clipAnimations=clips;brasaImporter.animationCompression=ModelImporterAnimationCompression.Off;brasaImporter.SaveAndReimport();
            var imported=AssetDatabase.LoadAllAssetsAtPath(brasa).OfType<AnimationClip>().ToArray();var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/ForjaDasMares/Art/Characters/Brasa/Brasa.controller");
            foreach(string name in Moves)
            {
                var clip=imported.Single(x=>x.name==name);if(AnimationUtility.GetCurveBindings(clip).Length<30)throw new InvalidOperationException("Missing authored bone curves: "+name);
                foreach(var layer in controller.layers){var state=layer.stateMachine.states.Select(x=>x.state).FirstOrDefault(x=>x.name==name)??layer.stateMachine.AddState(name);state.motion=clip;state.writeDefaultValues=true;state.timeParameterActive=true;state.timeParameter="ActionTime";EditorUtility.SetDirty(layer.stateMachine);}
            }
            string reachPath=Root+"AttackArea.mat";var reach=AssetDatabase.LoadAssetAtPath<Material>(reachPath);if(reach==null){reach=new Material(Shader.Find("Universal Render Pipeline/Unlit"));AssetDatabase.CreateAsset(reach,reachPath);}
            reach.SetFloat("_Surface",1);reach.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);reach.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);reach.SetFloat("_ZWrite",0);reach.SetFloat("_Cull",0);reach.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");reach.renderQueue=3000;EditorUtility.SetDirty(reach);
            foreach(string guid in AssetDatabase.FindAssets("t:HeroDefinition",new[]{"Assets/ForjaDasMares/Data"})){var hero=AssetDatabase.LoadAssetAtPath<HeroDefinition>(AssetDatabase.GUIDToAssetPath(guid));hero.attackAreaMaterial=reach;EditorUtility.SetDirty(hero);}
            catalog.Validate();EditorUtility.SetDirty(catalog);EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();Debug.Log("ARSENAL013_UPGRADE_OK models9 newClips12 totalClips="+imported.Count(x=>!x.name.StartsWith("__")));
        }
    }
}
