using UnityEngine;
namespace ForjaDasMares
{
    public sealed class EquipmentMeshView:MonoBehaviour
    {
        Mesh fallback;Material[] originals;MeshFilter filter;MeshRenderer view;
        public void Apply(EquipmentDefinition item)
        {
            filter??=GetComponent<MeshFilter>();view??=GetComponent<MeshRenderer>();if(fallback==null){fallback=filter.sharedMesh;originals=view.sharedMaterials;}
            filter.sharedMesh=item.visualMesh!=null?item.visualMesh:fallback;view.sharedMaterials=item.visualMesh!=null&&item.visualMaterials!=null&&item.visualMaterials.Length>0?item.visualMaterials:originals;
            EquipmentVisualPlacement.SetWorldScale(transform,item.visualMesh!=null?item.authoredScale:item.visualScale);
            if(item.visualMesh!=null){view.SetPropertyBlock(null);if(transform.parent!=null&&transform.parent.name.StartsWith("Hand."))transform.localPosition=Vector3.zero;}
            else{var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",item.color);view.SetPropertyBlock(block);}
        }
    }
}
