using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    public static class RoseBoomerangInstaller
    {
        const string Folder = "Assets/ForjaDasMares/Art/Arsenal/RoseBoomerang/";
        const string Id = "asa-rosal";
        static readonly Color[] Palette =
        {
            new Color(.88f,.16f,.46f), new Color(.99f,.69f,.34f),
            new Color(.29f,.065f,.19f), new Color(1f,.44f,.72f)
        };

        [MenuItem("Forja/Arsenal/Gerar Asa do Rosal")]
        public static void Apply()
        {
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh();
            var vertices = new List<Vector3>();
            var faces = new[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
            BuildWing(vertices, faces, -1);
            BuildWing(vertices, faces, 1);
            Diamond(vertices, faces, Vector2.zero, .13f, .19f, .085f, 2);
            Diamond(vertices, faces, Vector2.up*.026f, .082f, .12f, .112f, 3);

            var mesh = new Mesh { name = "Asa do Rosal" };
            mesh.SetVertices(vertices);
            mesh.subMeshCount = faces.Length;
            for (int i = 0; i < faces.Length; i++) mesh.SetTriangles(faces[i],i);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var savedMesh = Save(mesh,"Model.asset");
            var materials = new Material[Palette.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetColor("_BaseColor",Palette[i]);
                material.SetFloat("_Metallic",i == 1 ? .65f : .28f);
                material.SetFloat("_Smoothness",.67f);
                if (i == 0 || i == 3)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor",Palette[i]*(i == 3 ? .65f : .22f));
                }
                materials[i] = Save(material,"Finish"+i+".mat");
            }

            var hit = ScriptableObject.CreateInstance<AttackDefinition>();
            hit.shape = AttackShape.Projectile;
            hit.damage = 17;
            hit.range = 8;
            hit.projectileSpeed = 13;
            hit.projectileRadius = .32f;
            hit.windup = .18f;
            hit.recovery = .65f;
            hit.knockback = 0;
            hit.returningProjectile = true;
            hit.projectileVisual = savedMesh;
            hit.projectileMaterials = materials;
            hit.color = Palette[3];
            hit = Save(hit,"ReturningThrow.asset");

            var item = ScriptableObject.CreateInstance<EquipmentDefinition>();
            item.itemId = Id;
            item.displayName = "Asa do Rosal";
            item.description = "Duas lâminas de cristal rosa presas a um cabo de bronze. O arremesso abre uma curva, atravessa inimigos e volta ao portador. Cada alvo sofre um acerto por arremesso.";
            item.slot = EquipmentSlot.MainHand;
            item.category = WeaponCategory.MagicWeapon;
            item.compatibleHeroes = HeroAffinity.Ranger | HeroAffinity.Arcanist;
            item.minimumLevel = 1;
            item.minimumDropRarity = ItemRarity.Legendary;
            item.visualMesh = savedMesh;
            item.visualMaterials = materials;
            item.authoredScale = Vector3.one * .65f;
            item.color = Palette[3];
            item.icon = WriteIcon(vertices,faces);
            item.attacks = new[] { hit };
            item = Save(item,"AsaDoRosal.asset");
            var catalog = AssetDatabase.LoadAssetAtPath<ProgressionCatalog>("Assets/ForjaDasMares/Data/Campaign/Forge/Catalog.asset");
            if (catalog.Find(Id) == null)
            {
                var equipment = catalog.equipment;
                Array.Resize(ref equipment,equipment.Length+1);
                equipment[equipment.Length-1] = item;
                catalog.equipment = equipment;
                EditorUtility.SetDirty(catalog);
            }
            catalog.Validate();
            AssetDatabase.SaveAssets();
            Debug.Log("ROSE_BOOMERANG_READY "+savedMesh.vertexCount+" vertices");
        }

        static void BuildWing(List<Vector3> vertices,List<int>[] faces,int sign)
        {
            Vector2 Point(float t) => new Vector2(sign*(.04f+.63f*t),-.09f+.83f*t*t-.18f*t);
            Vector2 Normal(float t) => new Vector2(.18f-1.66f*t,sign*.63f).normalized;
            for (int i = 0; i < 14; i++)
            {
                float t = i / 14f, end = (i+1)/14f;
                Vector2 a = Point(t), b = Point(end);
                Vector2 normal = Normal(t), endNormal = Normal(end);
                float w = (.035f+.16f*Mathf.Sin(t*Mathf.PI))*Mathf.Lerp(1,.22f,t);
                float v = (.035f+.16f*Mathf.Sin(end*Mathf.PI))*Mathf.Lerp(1,.22f,end);
                Vector2 al = a+normal*w, ar = a-normal*w, bl = b+endNormal*v, br = b-endNormal*v;
                Quad(vertices,faces[0],al,a,b,bl,.035f,.095f);
                Quad(vertices,faces[3],a,ar,br,b,.095f,.035f);
                Quad(vertices,faces[1],al,Vector2.Lerp(al,a,.22f),Vector2.Lerp(bl,b,.22f),bl,.108f,.108f);
                Quad(vertices,faces[2],ar,al,bl,br,-.04f,-.04f);
                Quad(vertices,faces[1],ar,ar,br,br,-.04f,.035f,true);
                Quad(vertices,faces[1],al,al,bl,bl,.035f,-.04f,true);
            }
        }

        static void Diamond(List<Vector3> vertices,List<int>[] faces,Vector2 p,float w,float h,float depth,int layer)
        {
            Quad(vertices,faces[layer],p+Vector2.up*h,p+Vector2.right*w,p-Vector2.up*h,p-Vector2.right*w,depth,depth);
        }

        static void Quad(List<Vector3> vertices,List<int> indices,Vector2 a,Vector2 b,Vector2 c,Vector2 d,
            float outer,float inner,bool side = false)
        {
            int start = vertices.Count;
            vertices.Add(new Vector3(a.x,a.y,outer));
            vertices.Add(new Vector3(b.x,b.y,inner));
            vertices.Add(new Vector3(c.x,c.y,side ? inner : outer));
            vertices.Add(new Vector3(d.x,d.y,side ? outer : inner));
            indices.AddRange(new[] {start,start+1,start+2,start,start+2,start+3,start+2,start+1,start,start+3,start+2,start});
        }

        static Sprite WriteIcon(List<Vector3> vertices,List<int>[] faces)
        {
            const int size = 144;
            var texture = new Texture2D(size,size,TextureFormat.RGBA32,false);
            var pixels = new Color32[size*size];
            float Cross(Vector2 a,Vector2 b) => a.x*b.y-a.y*b.x;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2((x+.5f)/size*1.62f-.81f,(y+.5f)/size*1.62f-.54f);
                float nearest = float.NegativeInfinity;
                for (int layer = 0; layer < faces.Length; layer++)
                for (int j = 0; j < faces[layer].Count; j += 6)
                {
                    var f = faces[layer];
                    for (int t = 0; t < 2; t++)
                    {
                        Vector3 a = vertices[f[j+t*3]], b = vertices[f[j+t*3+1]], c = vertices[f[j+t*3+2]];
                        float area = Cross(b-a,c-a);
                        if (Mathf.Abs(area) < .00001f) continue;
                        float u = Cross(point-(Vector2)a,c-a)/area, v = Cross(b-a,point-(Vector2)a)/area;
                        if (u < 0 || v < 0 || u+v > 1) continue;
                        float depth = a.z+(b.z-a.z)*u+(c.z-a.z)*v;
                        if (depth < nearest) continue;
                        nearest = depth;
                        float light = Mathf.Clamp01(.74f+point.y*.27f+u*.11f);
                        Color color = Palette[layer] * light;
                        color.a = 1;
                        pixels[y*size+x] = color;
                    }
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            string path = Folder+"Icon.png";
            File.WriteAllBytes(path,texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static T Save<T>(T asset,string file) where T : UnityEngine.Object
        {
            string path = Folder+file;
            var saved = AssetDatabase.LoadAssetAtPath<T>(path);
            if (saved == null)
            {
                AssetDatabase.CreateAsset(asset,path);
                return asset;
            }
            EditorUtility.CopySerialized(asset,saved);
            UnityEngine.Object.DestroyImmediate(asset);
            EditorUtility.SetDirty(saved);
            return saved;
        }
    }
}
