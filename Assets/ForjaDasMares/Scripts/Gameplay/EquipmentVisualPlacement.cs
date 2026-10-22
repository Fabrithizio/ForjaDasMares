using UnityEngine;
namespace ForjaDasMares
{
    // Imported skeletons may carry a 100x scale. Equipment dimensions are world metres.
    public static class EquipmentVisualPlacement
    {
        static Vector3 ParentScale(Transform visual)
        {
            var scale=visual.parent!=null?visual.parent.lossyScale:Vector3.one;
            return new Vector3(Mathf.Max(.0001f,Mathf.Abs(scale.x)),Mathf.Max(.0001f,Mathf.Abs(scale.y)),Mathf.Max(.0001f,Mathf.Abs(scale.z)));
        }
        static Vector3 Divide(Vector3 value,Vector3 scale)=>new Vector3(value.x/scale.x,value.y/scale.y,value.z/scale.z);
        public static void Mount(Transform visual,Transform parent,bool socket,Vector3 restPosition,Quaternion restRotation,EquipmentDefinition equipment=null)
        {
            if(visual==null||parent==null)return;
            visual.SetParent(parent,false);
            bool custom=socket&&equipment!=null&&equipment.customGrip;
            visual.localPosition=Divide(socket?(custom?equipment.socketPosition:new Vector3(0,-.3f,0)):restPosition,ParentScale(visual));
            visual.localRotation=socket?Quaternion.Euler(custom?equipment.socketEuler:new Vector3(90,0,0)):restRotation;
        }
        public static void SetWorldScale(Transform visual,Vector3 size)
        {if(visual!=null)visual.localScale=Divide(size,ParentScale(visual));}
    }
}
