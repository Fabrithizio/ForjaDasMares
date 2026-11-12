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
            new Color(.29f,.065f,.19f), new Color(1f,.44f,.72f),
            new Color(1f,.91f,.78f), new Color(.67f,.36f,.96f)
        };

        [MenuItem("Forja/Arsenal/Gerar Asa do Rosal")]
        public static void Apply()
        {
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh();
            var vertices = new List<Vector3>();
            var faces = new List<int>[Palette.Length];
            for (int i = 0; i < faces.Length; i++) faces[i] = new List<int>();
            BuildWing(vertices, faces, -1);
            BuildWing(vertices, faces, 1);
            Heart(vertices, faces[1], Vector2.zero, .235f, .10f);
            Heart(vertices, faces[3], Vector2.up*.012f, .166f, .15f);
            Heart(vertices, faces[1], new Vector2(.16f,-.58f), .093f, .065f);
            Heart(vertices, faces[3], new Vector2(.16f,-.575f), .064f, .09f);
            for (int i = 0; i < 13; i++)
            {
                Vector2 Ribbon(float t) => new Vector2(.16f*t+.16f*Mathf.Sin(t*Mathf.PI*2),-.08f-.48f*t);
                Vector2 a = Ribbon(i/13f), b = Ribbon((i+1)/13f);
                Vector2 width = Vector2.right * Mathf.Lerp(.055f,.017f,i/13f);
                Quad(vertices,faces[i%3==0?3:0],a-width,a+width,b+width,b-width,.052f,.057f);
            }

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
                if (i == 0 || i >= 3)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor",Palette[i]*(i == 3 || i == 4 ? 1.35f : .35f));
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

            var heavy = UnityEngine.Object.Instantiate(hit);
            heavy.range = 9;
            heavy.splashRadius = 2.7f;
            heavy = Save(heavy,"RosalBurst.asset");
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.displayName = "Coração do Rosal";
            ability.effect = AbilityEffect.WeaponStrike;
            ability.hit = heavy;
            ability.cooldown = 8;
            ability = Save(ability,"RosalAbility.asset");

            var item = ScriptableObject.CreateInstance<EquipmentDefinition>();
            item.itemId = Id;
            item.displayName = "Asa do Rosal";
            item.description = "Duas lâminas de cristal rosa presas a um cabo de bronze. O arremesso abre uma curva, atravessa inimigos e volta ao portador. O modo pesado faz o Coração do Rosal florescer ao voltar, atingindo inimigos próximos ao portador.";
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
            item.ability = ability;
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
            Vector2 Arc(float t) => new Vector2(sign*(.18f+.52f*Mathf.Sin(t*Mathf.PI)), -.24f+.99f*t);
            Vector2 Normal(float t) => new Vector2(sign*.99f,-.52f*Mathf.PI*Mathf.Cos(t*Mathf.PI)).normalized;
            for (int i = 0; i < 28; i++)
            {
                float t = i/28f, u = (i+1)/28f;
                Vector2 a = Arc(t), b = Arc(u);
                Vector2 wa = Normal(t) * (.012f+.155f*Mathf.Sin(t*Mathf.PI));
                Vector2 wb = Normal(u) * (.008f+.155f*Mathf.Sin(u*Mathf.PI));
                Quad(vertices,faces[4],a+wa,a+wa*.56f,b+wb*.56f,b+wb,.045f,.079f);
                Quad(vertices,faces[0],a+wa*.56f,a-wa*.78f,b-wb*.78f,b+wb*.56f,.08f,.084f);
                Quad(vertices,faces[1],a-wa*.77f,a-wa,b-wb,b-wb*.77f,.09f,.06f);
                Quad(vertices,faces[2],a+wa,a-wa,b-wb,b+wb,-.042f,-.042f);
                Quad(vertices,faces[1],a+wa,a+wa,b+wb,b+wb,.045f,-.042f,true);
                Quad(vertices,faces[1],a-wa,a-wa,b-wb,b-wb,-.042f,.06f,true);
                if (i % 5 == 2 && i < 22)
                    Diamond(vertices,faces,a-wa*.17f,.027f,.043f,.112f,1);
            }
            for (int i = 0; i < 7; i++)
            {
                float t = .24f+i*.055f;
                Vector2 root = Arc(t)+new Vector2(sign*.1f,0);
                Vector2 tip = root+new Vector2(sign*(.30f-i*.017f),-.26f+i*.089f);
                Vector2 side = new Vector2(-(tip-root).y,(tip-root).x).normalized * .052f;
                Vector2 mid = Vector2.Lerp(root,tip,.48f);
                Quad(vertices,faces[5],root,mid+side,tip,mid-side,.03f,.058f);
                Quad(vertices,faces[3],root,mid,tip,mid-side*.7f,.05f,.077f);
            }
        }

        static void Heart(List<Vector3> vertices,List<int> faces,Vector2 center,float size,float depth)
        {
            Vector2 Point(float t)
            {
                float sin = Mathf.Sin(t);
                return center + new Vector2(16*sin*sin*sin,13*Mathf.Cos(t)-5*Mathf.Cos(2*t)
                    -2*Mathf.Cos(3*t)-Mathf.Cos(4*t)) * (size/17);
            }
            for (int i = 0; i < 28; i++)
            {
                Vector2 a = Point(i*Mathf.PI/14), b = Point((i+1)*Mathf.PI/14);
                int first = vertices.Count;
                vertices.Add(new Vector3(center.x,center.y,depth+.035f));
                vertices.Add(new Vector3(a.x,a.y,depth));
                vertices.Add(new Vector3(b.x,b.y,depth));
                faces.AddRange(new[] {first,first+1,first+2,first+2,first+1,first});
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
            const int size = 192;
            var texture = new Texture2D(size,size,TextureFormat.RGBA32,false);
            var pixels = new Color32[size*size];
            float Cross(Vector2 a,Vector2 b) => a.x*b.y-a.y*b.x;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2((x+.5f)/size*2.32f-1.16f,(y+.5f)/size*2.32f-1.03f);
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
