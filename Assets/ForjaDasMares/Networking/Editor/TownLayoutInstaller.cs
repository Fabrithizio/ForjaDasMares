using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Unity.AI.Navigation;

namespace ForjaDasMares.Networking.Editor
{
    // Town-only migration. Does not rebuild the coast, quests, saves, heroes or campaign.
    public static class TownLayoutInstaller
    {
        const string Data="Assets/ForjaDasMares/Data/World/Town/Surroundings/";
        const string Kit="Assets/ForjaDasMares/Art/World/SettlementKit/";
        static Transform root;
        static Material stone,grass;
        [MenuItem("Forja/World/Cidade e entorno 0.28")]
        public static void Apply()
        {
            Directory.CreateDirectory(Data);AssetDatabase.Refresh();
            var scene=EditorSceneManager.OpenScene(TownInstaller.ScenePath);
            var environment=GameObject.Find("TownEnvironment");
            var prior=environment.transform.Find("TownLayout028");if(prior!=null)UnityEngine.Object.DestroyImmediate(prior.gameObject);
            root=new GameObject("TownLayout028").transform;root.SetParent(environment.transform,false);
            stone=Mat("WeatheredStone",new Color(.44f,.48f,.43f),"Rock");
            grass=Mat("Meadow",new Color(.64f,.78f,.54f),"Grass");
            var earth=Mat("DistantRock",new Color(.37f,.46f,.43f),"Rock");
            var road=Mat("Road",Color.white,"Soil");
            road.mainTextureScale=new Vector2(11f/7.6f,12f/7.6f);
            road.mainTextureOffset=new Vector2(32.5f/7.6f,-8f/7.6f);EditorUtility.SetDirty(road);
            var world=UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();var hub=world.GetComponent<TownHub>();
            // Wide low-detail terrain closes the entire camera footprint, not just the playable floor.
            Landscape(grass);
            GardenBeds(grass);
            var ground=GameObject.Find("TownFloor");ground.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Data/World/Town/Paving.mat");
            // Preserve solid perimeter but replace invisible straight edges with visible architecture and landscape.
            foreach(var wall in environment.GetComponentsInChildren<Transform>().Where(t=>t.name=="BoundaryWall").ToArray())
                UnityEngine.Object.DestroyImmediate(wall.gameObject);
            Wall(new Vector3(-36,1.5f,0),new Vector3(.9f,3,73));
            Wall(new Vector3(36,1.5f,0),new Vector3(.9f,3,73));
            Wall(new Vector3(0,1.5f,36),new Vector3(73,3,.9f));
            Wall(new Vector3(-21,1.5f,-36),new Vector3(30,3,.9f));
            Wall(new Vector3(21,1.5f,-36),new Vector3(30,3,.9f));
            // Road continues through a real opening. No device or teleport pad at the exit.
            Box("RoadThresholdFloor",new Vector3(0,-.19f,-40),new Vector3(11,.38f,12),road,true,8);
            Wall(new Vector3(-5.5f,.8f,-41),new Vector3(.6f,1.6f,10));Wall(new Vector3(5.5f,.8f,-41),new Vector3(.6f,1.6f,10));
            foreach(float x in new[]{-5.4f,5.4f})
            {
                Place("Marco_antigo",new Vector3(x,0,-35),1.6f);
                Place("Lanterna_de_rua",new Vector3(x,0,-31),1.5f);
            }
            var gate=GameObject.Find("SouthExpeditionGate");
            foreach(var component in gate.GetComponents<Renderer>())UnityEngine.Object.DestroyImmediate(component);
            foreach(var component in gate.GetComponents<MeshFilter>())UnityEngine.Object.DestroyImmediate(component);
            foreach(var component in gate.GetComponents<Collider>())UnityEngine.Object.DestroyImmediate(component);
            gate.transform.position=new Vector3(0,0,-40);gate.transform.localScale=Vector3.one;
            var click=gate.GetComponent<WorldClickTarget>();click.Configure("Seguir pela estrada · Costa",2.2f);
            var clickable=gate.AddComponent<BoxCollider>();clickable.isTrigger=true;clickable.center=Vector3.up;clickable.size=new Vector3(8,3,2);
            var passage=gate.GetComponent<TownRoadPassage>();if(passage==null)passage=gate.AddComponent<TownRoadPassage>();passage.Configure(hub,world.Hero.transform);
            foreach(var label in environment.GetComponentsInChildren<TextMesh>())
                if(label.text.StartsWith("EXPEDIÇÃO")) {label.text="ESTRADA PARA A COSTA";label.transform.position=new Vector3(0,3.5f,-34);}
            // Social square: open arrival disk remains walkable; protector sits behind it.
            Place("Altar_antigo",new Vector3(0,0,12),1.4f,true);
            Place("Console_antigo",new Vector3(0,0,9.5f),1.2f,true);
            foreach(float x in new[]{-2.1f,2.1f})Place("Marco_antigo",new Vector3(x,0,12),1.5f,true);
            // The existing two tents no longer compete with marketplace and expedition circulation.
            var tents=environment.GetComponentsInChildren<LODGroup>().Where(g=>g.name.StartsWith("RefugeTent")).ToArray();
            if(tents.Length>1){tents[1].transform.position=new Vector3(25,0,-8);}
            // Functional expedition court inside the town: uses the real existing campaign lobby.
            Place("Escada_de_pedra",new Vector3(23,0,-13),1.5f);
            Place("Marco_de_transporte",new Vector3(25,0,-9),1.6f);
            Place("Estandarte_de_tecido",new Vector3(20,0,-10),1.8f);
            var board=Place("Placa_de_caminho",new Vector3(21,0,-14),1.3f,true);
            var expedition=board.AddComponent<WorldClickTarget>();expedition.Configure("Expedições · abrir desafios em grupo",3);
            UnityEventTools.AddPersistentListener(expedition.Used,hub.OpenExpeditions);
            // Reserve the southward approach, arrival points and all established service destinations.
            for(int side=-1;side<=1;side+=2)
            {
                for(int i=0;i<9;i++)
                {
                    var p=new Vector3(side*(43+i%3*7),Height(side*(43+i%3*7),-42+i*12),-42+i*12);
                    Place(i%3==0?"Arvore_luminosa":"Arvore_copa_aberta",p,1.7f+i%2*.4f);
                    Place("Arbusto_folhoso",p+new Vector3(2,0,3),2);
                }
                for(int i=0;i<5;i++)
                    Place("Arvore_copa_aberta",new Vector3(-42+i*20,Height(-42+i*20,side*56),side*56),2.1f);
            }
            // Distant landforms are a single combined mesh; no colliders, scripts or dynamic lights.
            DistantRidges(earth);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.48f,.61f,.61f);RenderSettings.fogStartDistance=55;RenderSettings.fogEndDistance=175;
            var camera=UnityEngine.Object.FindFirstObjectByType<ArenaCamera>().GetComponent<Camera>();
            camera.farClipPlane=210;camera.backgroundColor=RenderSettings.fogColor;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.63f,.71f,.76f);
            RenderSettings.ambientEquatorColor=new Color(.44f,.49f,.4f);RenderSettings.ambientGroundColor=new Color(.28f,.25f,.2f);
            var sun=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l=>l.type==LightType.Directional);
            if(sun!=null){sun.color=new Color(1,.91f,.76f);sun.intensity=1.15f;}
            RebuildNavigation(world);
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("TOWN_LAYOUT_READY townOnly=true roadPassage=true landscape=440m servicesValidated=true");
        }
        static Material Mat(string name,Color tint,string texture)
        {
            string path=Data+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.color=tint;m.SetFloat("_Smoothness",.05f);m.enableInstancing=true;
            m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ForjaDasMares/Art/World/Coast/"+texture+".png");
            EditorUtility.SetDirty(m);return m;
        }
        static GameObject Place(string name,Vector3 p,float scale,bool collide=false)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Kit+name+".prefab");if(prefab==null)throw new InvalidOperationException(name);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(root,false);go.transform.position=p;go.transform.localScale=Vector3.one*scale;
            if(Mathf.Max(Mathf.Abs(p.x),Mathf.Abs(p.z))>38) foreach(var r in go.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.Off;
            if(collide){var box=go.AddComponent<BoxCollider>();var render=go.GetComponent<LODGroup>().GetLODs()[0].renderers[0];var b=ActOneInstaller.RendererBounds(go.transform,render);box.center=b.center;box.size=b.size;}
            return go;
        }
        static GameObject Box(string name,Vector3 p,Vector3 size,Material material,bool solid,int layer=9)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.position=p;go.transform.localScale=size;go.layer=layer;
            go.GetComponent<Renderer>().sharedMaterial=material;
            if(!solid)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        static void Wall(Vector3 p,Vector3 size)
        {
            var segment=new GameObject("TownStoneBoundary");segment.transform.SetParent(root,false);segment.transform.position=p;segment.layer=9;
            var pieces=new List<GameObject>();
            pieces.Add(Box("WallBody",p,size,stone,false));
            pieces.Add(Box("WallCoping",p+Vector3.up*(size.y*.5f),new Vector3(size.x+.16f,.24f,size.z+.16f),stone,false));
            bool horizontal=size.x>size.z;float length=Mathf.Max(size.x,size.z);
            for(float a=-length*.5f+1;a<length*.5f;a+=3)
                pieces.Add(Box("WallMerlon",p+new Vector3(horizontal?a:0,size.y*.5f+.4f,horizontal?0:a),new Vector3(1,.65f,1),stone,false));
            var combine=pieces.Select(piece=>new CombineInstance{mesh=piece.GetComponent<MeshFilter>().sharedMesh,transform=segment.transform.worldToLocalMatrix*piece.transform.localToWorldMatrix}).ToArray();
            var mesh=new Mesh{name="WallSegment"};mesh.CombineMeshes(combine);mesh.RecalculateBounds();
            string path=Data+"Wall_"+Mathf.RoundToInt(p.x)+"_"+Mathf.RoundToInt(p.z)+".asset";
            var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved==null){AssetDatabase.CreateAsset(mesh,path);saved=mesh;}else{EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);}
            foreach(var piece in pieces)UnityEngine.Object.DestroyImmediate(piece);
            segment.AddComponent<MeshFilter>().sharedMesh=saved;segment.AddComponent<MeshRenderer>().sharedMaterial=stone;
            segment.AddComponent<BoxCollider>().size=size;
        }
        static float Height(float x,float z)
        {
            float edge=Mathf.Max(Mathf.Abs(x),Mathf.Abs(z));
            float rise=Mathf.SmoothStep(0,1,Mathf.InverseLerp(39,90,edge));
            float hills=(Mathf.PerlinNoise(x*.019f+9,z*.019f+4)*8+Mathf.PerlinNoise(x*.045f+2,z*.045f+7)*2)*rise;
            // Exterior road valley remains visible beyond the south gate.
            float valley=1-Mathf.Exp(-x*x/140f)*Mathf.Clamp01((-z-32)/18);
            return -.28f+hills*valley;
        }
        static void Landscape(Material material)
        {
            const int n=64;const float half=220;
            var vertices=new Vector3[(n+1)*(n+1)];var uv=new Vector2[vertices.Length];var triangles=new int[n*n*6];
            for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){int i=z*(n+1)+x;float px=-half+x*half*2/n,pz=-half+z*half*2/n;vertices[i]=new Vector3(px,Height(px,pz),pz);uv[i]=new Vector2(px/10,pz/10);}
            int t=0;for(int z=0;z<n;z++)for(int x=0;x<n;x++){int i=z*(n+1)+x;triangles[t++]=i;triangles[t++]=i+n+1;triangles[t++]=i+1;triangles[t++]=i+1;triangles[t++]=i+n+1;triangles[t++]=i+n+2;}
            var mesh=new Mesh{name="TownLandscape"};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
            MeshObject("TownLandscape",mesh,material);
        }
        static void GardenBeds(Material material)
        {
            var vertices=new List<Vector3>();var indices=new List<int>();var uv=new List<Vector2>();
            foreach(var centre in new[]{new Vector3(-25,.015f,26),new Vector3(25,.015f,27),new Vector3(-27,.015f,-24)})
            {
                int start=vertices.Count;vertices.Add(centre);uv.Add(new Vector2(centre.x/10,centre.z/10));
                for(int i=0;i<12;i++){float a=i*Mathf.PI*2/12;var p=centre+new Vector3(Mathf.Cos(a)*7,0,Mathf.Sin(a)*5);vertices.Add(p);uv.Add(new Vector2(p.x/10,p.z/10));}
                for(int i=0;i<12;i++){indices.Add(start);indices.Add(start+1+(i+1)%12);indices.Add(start+1+i);}
            }
            var mesh=new Mesh{name="TownGardenBeds"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();MeshObject("TownGardenBeds",mesh,material);
        }
        static void DistantRidges(Material material)
        {
            var vertices=new List<Vector3>();var indices=new List<int>();var uv=new List<Vector2>();
            for(int i=0;i<24;i++)
            {
                float a=i*Mathf.PI*2/24;var centre=new Vector3(Mathf.Cos(a)*140,0,Mathf.Sin(a)*140);
                int start=vertices.Count;float width=18+i%3*5,height=18+i%5*6;
                vertices.Add(centre+new Vector3(-width,-2,-width));vertices.Add(centre+new Vector3(width,-2,-width));vertices.Add(centre+new Vector3(width,-2,width));vertices.Add(centre+new Vector3(-width,-2,width));vertices.Add(centre+new Vector3(i%2*7,height,0));
                uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up,new Vector2(.5f,.5f)});
                for(int j=0;j<4;j++){indices.Add(start+j);indices.Add(start+4);indices.Add(start+(j+1)%4);}
            }
            var mesh=new Mesh{name="DistantRidges"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();MeshObject("DistantRidges",mesh,material);
        }
        static void MeshObject(string name,Mesh mesh,Material material)
        {
            string path=Data+name+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved==null){AssetDatabase.CreateAsset(mesh,path);saved=mesh;}else{EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);}
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.layer=2;go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=saved;var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=true;
        }
        static void RebuildNavigation(RegionalWorld world)
        {
            Physics.SyncTransforms();var surface=UnityEngine.Object.FindFirstObjectByType<NavMeshSurface>();surface.BuildNavMesh();
            if(!NavMesh.SamplePosition(world.Definition.refuge,out var start,1,NavMesh.AllAreas))throw new InvalidOperationException("Arrival blocked");
            foreach(var target in UnityEngine.Object.FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None))
            {
                if(!NavMesh.SamplePosition(target.Approach,out var end,2.5f,NavMesh.AllAreas))throw new InvalidOperationException("Missing service path "+target.name);
                var path=new NavMeshPath();if(!NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)||path.status!=NavMeshPathStatus.PathComplete)throw new InvalidOperationException("Disconnected service "+target.name);
            }
            var stable=AssetDatabase.LoadAssetAtPath<NavMeshData>("Assets/ForjaDasMares/Data/World/Town/Navigation.asset");
            EditorUtility.CopySerialized(surface.navMeshData,stable);surface.RemoveData();surface.navMeshData=stable;surface.AddData();
        }
    }
}



