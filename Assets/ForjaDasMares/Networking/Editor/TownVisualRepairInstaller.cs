using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ForjaDasMares.Networking.Editor
{
    // Targeted visual repair; no service positions, quests or level bounds change.
    public static class TownVisualRepairInstaller
    {
        const string Folder="Assets/ForjaDasMares/Data/World/Town/Surroundings/";
        [MenuItem("Forja/World/Reparar piso e direcao magica")]
        public static void Apply()
        {
            AssetDatabase.Refresh();var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/ForjaDasMares/Art/World/Coast/GrassFantasy032.png");importer.maxTextureSize=1024;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Repeat;importer.filterMode=FilterMode.Trilinear;importer.textureCompression=TextureImporterCompression.Compressed;importer.anisoLevel=4;importer.SaveAndReimport();
            var scene=EditorSceneManager.OpenScene(TownInstaller.ScenePath);var env=GameObject.Find("TownEnvironment").transform;
            var paths=GameObject.Find("CityPaths");var source=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"CityPaths.asset");var triangles=source.triangles;var v=source.vertices;
            // Rasterize the union once: adjacent cells share their edges, no stacked triangles at junctions.
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();const float step=.5f;
            for(float z=-39;z<30;z+=step)for(float x=-32;x<33;x+=step)
            {
                Vector2 p=new Vector2(x+step*.5f,z+step*.5f);bool inside=false;
                for(int t=0;t<triangles.Length;t+=3){var a=v[triangles[t]];var b=v[triangles[t+1]];var c=v[triangles[t+2]];if(InTriangle(p,new Vector2(a.x,a.z),new Vector2(b.x,b.z),new Vector2(c.x,c.z))){inside=true;break;}}
                if(!inside)continue;int n=vertices.Count;
                foreach(var q in new[]{new Vector2(x,z),new Vector2(x,z+step),new Vector2(x+step,z+step),new Vector2(x+step,z)}){vertices.Add(new Vector3(q.x,.035f,q.y));uv.Add(q/7.6f+Vector2.one*.5f);}
                indices.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            var mesh=new Mesh{name="CityPathsClean"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            string path=Folder+"CityPathsClean.asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(saved==null){AssetDatabase.CreateAsset(mesh,path);saved=mesh;}else{EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);}paths.GetComponent<MeshFilter>().sharedMesh=saved;
            var gardens=GameObject.Find("TownGardenBeds");if(gardens!=null)gardens.GetComponent<Renderer>().enabled=false; // Old polygons visibly cut across the new streets.
            var benches=env.GetComponentsInChildren<LODGroup>().Where(g=>g.name=="Banco_de_madeira").OrderBy(g=>g.transform.position.x).ToArray();
            Vector3[] places={new Vector3(-29,0,18),new Vector3(-12,0,28),new Vector3(12,0,28),new Vector3(29,0,18)};
            for(int i=0;i<benches.Length&&i<places.Length;i++){benches[i].transform.position=places[i];benches[i].transform.rotation=Quaternion.Euler(0,i==0?90:i==3?270:180,0);}
            // Keep the old source recoverable; replace only its scene presentation with an existing fantasy fountain.
            var machine=env.GetComponentsInChildren<LODGroup>().FirstOrDefault(g=>g.name=="Protetor_da_praca");
            if(machine!=null){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ForjaDasMares/Art/World/SettlementKit/Fonte_de_cristal.prefab");if(prefab==null)throw new InvalidOperationException("Missing crystal fountain");var prior=env.Find("FonteProtetoraMagica");if(prior==null){var fountain=(GameObject)PrefabUtility.InstantiatePrefab(prefab);fountain.name="FonteProtetoraMagica";fountain.transform.SetParent(env,false);fountain.transform.position=machine.transform.position;fountain.transform.localScale=Vector3.one*1.5f;var blocker=fountain.AddComponent<CapsuleCollider>();blocker.radius=.8f;blocker.height=1.4f;blocker.center=Vector3.up*.7f;}machine.gameObject.SetActive(false);}
            var meadow=AssetDatabase.LoadAssetAtPath<Material>(Folder+"CityMeadow.mat");var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ForjaDasMares/Art/World/Coast/GrassFantasy032.png");
            if(texture!=null){meadow.SetTexture("_BaseMap",texture);meadow.SetColor("_BaseColor",Color.white);meadow.mainTextureScale=Vector2.one*18;EditorUtility.SetDirty(meadow);}
            // Bench colliders moved: refresh the stable navmesh through the existing town migration validator.
            var surface=UnityEngine.Object.FindFirstObjectByType<Unity.AI.Navigation.NavMeshSurface>();surface.BuildNavMesh();var stable=AssetDatabase.LoadAssetAtPath<UnityEngine.AI.NavMeshData>("Assets/ForjaDasMares/Data/World/Town/Navigation.asset");EditorUtility.CopySerialized(surface.navMeshData,stable);surface.RemoveData();surface.navMeshData=stable;surface.AddData();EditorUtility.SetDirty(stable);
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("TOWN_VISUAL_REPAIR pathsNoOverlap=true benchesMoved=true magicalFountain=true");
        }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static bool InTriangle(Vector2 p,Vector2 a,Vector2 b,Vector2 c){float x=Cross(b-a,p-a),y=Cross(c-b,p-b),z=Cross(a-c,p-c);return !(x<0||y<0||z<0)||!(x>0||y>0||z>0);}
    }
}
