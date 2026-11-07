using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
namespace ForjaDasMares.Networking.Editor
{
    public static class TownArrivalPolish
    {
        public static void Apply()
        {
            var scene=EditorSceneManager.OpenScene(TownInstaller.ScenePath);
            var world=Object.FindFirstObjectByType<RegionalWorld>();
            var arrival=new Vector3(0,.16f,-9);
            if(!NavMesh.SamplePosition(arrival,out _,1,NavMesh.AllAreas))
                throw new System.InvalidOperationException("Chegada fora da via navegavel.");
            world.Definition.refuge=arrival;
            EditorUtility.SetDirty(world.Definition);
            var env=GameObject.Find("TownEnvironment").transform;
            var group=env.Find("RelevoVisual");
            if(group==null)
            {
                group=new GameObject("RelevoVisual").transform;
                group.SetParent(env,false);
                Place(group,"Act1/CoastalRock/CoastalRock",new Vector3(-25,0,15),.26f,25);
                Place(group,"Act1/CoastalRock/CoastalRock",new Vector3(-27,0,16),.18f,75);
                Place(group,"SettlementKit/Samambaia",new Vector3(-24,0,16),1.2f,20);
                Place(group,"Act1/CoastalRock/CoastalRock",new Vector3(23,0,51),.23f,115);
                Place(group,"Act1/CoastalRock/CoastalRock",new Vector3(24.5f,0,50),.16f,55);
                Place(group,"SettlementKit/Samambaia",new Vector3(23,0,49),1.1f,40);
            }
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
        static void Place(Transform parent,string asset,Vector3 position,float scale,float yaw)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ForjaDasMares/Art/World/"+asset+".prefab");
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);
            instance.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
            instance.transform.localScale=Vector3.one*scale;
            foreach(var renderer in instance.GetComponentsInChildren<Renderer>())
                renderer.shadowCastingMode=ShadowCastingMode.Off;
            foreach(var collider in instance.GetComponentsInChildren<Collider>())
                collider.enabled=false;
        }
    }
}
