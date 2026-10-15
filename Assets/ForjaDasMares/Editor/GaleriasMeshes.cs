using System.Collections.Generic;
using UnityEngine;
namespace ForjaDasMares.Editor
{
    // Creates original geometry in Editor; the player uses saved shared mesh assets.
    internal sealed class GaleriasMeshes
    {
        readonly List<Vector3> vertices = new List<Vector3>(), normals = new List<Vector3>();
        readonly List<Vector2> uv = new List<Vector2>();
        readonly List<int>[] triangles = { new List<int>(), new List<int>(), new List<int>() };
        void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, int slot, bool triangle = false)
        {
            int start = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c);
            normals.Add(normal); normals.Add(normal); normals.Add(normal);
            uv.Add(Vector2.zero); uv.Add(Vector2.right); uv.Add(Vector2.one);
            if (!triangle) { vertices.Add(d); normals.Add(normal); uv.Add(Vector2.up); }
            bool reverse = Vector3.Dot(Vector3.Cross(b - a, c - a), normal) < 0;
            triangles[slot].Add(start); triangles[slot].Add(start + (reverse ? 2 : 1)); triangles[slot].Add(start + (reverse ? 1 : 2));
            if (!triangle) { triangles[slot].Add(start); triangles[slot].Add(start + (reverse ? 3 : 2)); triangles[slot].Add(start + (reverse ? 2 : 3)); }
        }
        void Box(Vector3 center, Vector3 size, int slot, float bevel = .025f)
        {
            Vector3 h = size * .5f;
            float b = Mathf.Min(bevel, Mathf.Min(h.x, Mathf.Min(h.y, h.z)) * .7f);
            Vector3 v(float x, float y, float z) => center + new Vector3(x, y, z);
            for (int sign = -1; sign <= 1; sign += 2)
            {
                Face(v(sign*h.x,-h.y+b,-h.z+b), v(sign*h.x,h.y-b,-h.z+b), v(sign*h.x,h.y-b,h.z-b), v(sign*h.x,-h.y+b,h.z-b), Vector3.right*sign,slot);
                Face(v(-h.x+b,sign*h.y,-h.z+b), v(h.x-b,sign*h.y,-h.z+b), v(h.x-b,sign*h.y,h.z-b), v(-h.x+b,sign*h.y,h.z-b), Vector3.up*sign,slot);
                Face(v(-h.x+b,-h.y+b,sign*h.z), v(h.x-b,-h.y+b,sign*h.z), v(h.x-b,h.y-b,sign*h.z), v(-h.x+b,h.y-b,sign*h.z), Vector3.forward*sign,slot);
            }
            for (int s = -1; s <= 1; s += 2) for (int t = -1; t <= 1; t += 2)
            {
                Face(v(-h.x+b,s*h.y,t*(h.z-b)),v(h.x-b,s*h.y,t*(h.z-b)),v(h.x-b,s*(h.y-b),t*h.z),v(-h.x+b,s*(h.y-b),t*h.z),new Vector3(0,s,t).normalized,slot);
                Face(v(s*h.x,-h.y+b,t*(h.z-b)),v(s*h.x,h.y-b,t*(h.z-b)),v(s*(h.x-b),h.y-b,t*h.z),v(s*(h.x-b),-h.y+b,t*h.z),new Vector3(s,0,t).normalized,slot);
                Face(v(s*h.x,t*(h.y-b),-h.z+b),v(s*h.x,t*(h.y-b),h.z-b),v(s*(h.x-b),t*h.y,h.z-b),v(s*(h.x-b),t*h.y,-h.z+b),new Vector3(s,t,0).normalized,slot);
                for (int u = -1; u <= 1; u += 2)
                    Face(v(s*h.x,t*(h.y-b),u*(h.z-b)),v(s*(h.x-b),t*h.y,u*(h.z-b)),v(s*(h.x-b),t*(h.y-b),u*h.z),Vector3.zero,new Vector3(s,t,u).normalized,slot,true);
            }
        }
        Mesh Finish(string name, Vector3 size, int slots, bool swapXZ = false)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 p = vertices[i], n = normals[i];
                p = new Vector3(p.x/size.x,p.y/size.y,p.z/size.z);
                n = Vector3.Scale(n,size).normalized;
                if (swapXZ) { p = new Vector3(-p.z,p.y,p.x); n = new Vector3(-n.z,n.y,n.x); }
                vertices[i]=p; normals[i]=n;
            }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0,uv); mesh.subMeshCount = slots;
            for (int i=0;i<slots;i++) mesh.SetTriangles(triangles[i],i);
            mesh.RecalculateBounds(); return mesh;
        }
        public static Mesh Floor(string name, float width, float depth, bool boss = false)
        {
            var m = new GaleriasMeshes();
            m.Box(new Vector3(0,-.08f,0),new Vector3(width,.24f,depth),0,.02f);
            int nx=Mathf.RoundToInt(width/1.5f), nz=Mathf.RoundToInt(depth/1.5f);
            float sx=width/nx,sz=depth/nz;
            for(int x=0;x<nx;x++) for(int z=0;z<nz;z++)
                m.Box(new Vector3(-width*.5f+(x+.5f)*sx,.08f,-depth*.5f+(z+.5f)*sz),new Vector3(sx-.035f,.2f,sz-.035f),(x+z)%3==0?1:0,.035f);
            if(boss)
            {
                for(int s=-1;s<=1;s+=2)
                { m.Box(new Vector3(s*2.8f,.185f,0),new Vector3(.12f,.02f,5.7f),2,.005f); m.Box(new Vector3(0,.185f,s*2.8f),new Vector3(5.7f,.02f,.12f),2,.005f); }
                m.Box(new Vector3(0,.185f,0),new Vector3(.8f,.02f,.8f),2,.01f);
            }
            return m.Finish(name,new Vector3(width,.4f,depth),3);
        }
        public static Mesh Wall(string name, float length, bool alongZ)
        {
            var m = new GaleriasMeshes(); int count=Mathf.CeilToInt(length/1.05f); float step=length/count;
            for(int row=0;row<3;row++) for(int j=-1;j<=count;j++)
            {
                float start=-length*.5f+(j+(row%2==0?0:.5f))*step;
                float lo=Mathf.Max(-length*.5f,start),hi=Mathf.Min(length*.5f,start+step);
                if(hi-lo<.12f)continue;
                m.Box(new Vector3((lo+hi)*.5f,-.6f+row*.55f,0),new Vector3(hi-lo-.03f,.52f,.35f),0,.035f);
            }
            m.Box(new Vector3(0,.82f,0),new Vector3(length,.16f,.43f),1,.03f);
            m.Box(new Vector3(0,-.85f,0),new Vector3(length,.1f,.39f),1,.015f);
            return m.Finish(name,new Vector3(length,1.8f,.35f),2,alongZ);
        }
        public static Mesh Crate(string name, bool lid = false)
        {
            var m=new GaleriasMeshes(); m.Box(Vector3.zero,Vector3.one*.94f,0,.035f);
            for(int s=-1;s<=1;s+=2)
            {
                m.Box(new Vector3(s*.34f,0,0),new Vector3(.08f,1.02f,1.02f),1,.012f);
                if(!lid) m.Box(new Vector3(0,s*.4f,0),new Vector3(1.01f,.08f,1.01f),0,.01f);
            }
            for(int i=-1;i<=1;i++) m.Box(new Vector3(0,0,i*.23f),new Vector3(.94f,.96f,.018f),0,.003f);
            return m.Finish(name,Vector3.one,2);
        }
        public static Mesh Lantern(string name)
        {
            var m=new GaleriasMeshes();
            m.Box(new Vector3(0,-.42f,0),new Vector3(.8f,.16f,.8f),0,.04f);
            m.Box(new Vector3(0,-.1f,0),new Vector3(.16f,.55f,.16f),0,.015f);
            m.Box(new Vector3(0,.2f,0),new Vector3(.6f,.13f,.6f),0,.035f);
            m.Box(new Vector3(0,.34f,0),new Vector3(.34f,.26f,.34f),1,.08f);
            m.Box(new Vector3(0,.48f,0),new Vector3(.16f,.04f,.16f),1,.012f);
            return m.Finish(name,Vector3.one,2);
        }
        public static Mesh Gate(string name)
        {
            var m=new GaleriasMeshes();
            for(int i=-3;i<=3;i++) m.Box(new Vector3(i*.135f,0,0),new Vector3(.035f,1,.35f),0,.008f);
            for(int s=-1;s<=1;s+=2) m.Box(new Vector3(0,s*.33f,0),new Vector3(1,.08f,.5f),1,.012f);
            return m.Finish(name,Vector3.one,2);
        }
    }
}
