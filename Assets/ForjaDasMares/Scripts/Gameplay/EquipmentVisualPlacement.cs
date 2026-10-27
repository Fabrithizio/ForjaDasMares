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
            float fit=EquipmentFactor(visual,equipment);
            var model=parent.GetComponentInParent<HeroModelView>(true);
            Vector3 fallback=model!=null&&model.FitsEquipmentToBody?Vector3.zero:new Vector3(0,-.3f,0);
            visual.localPosition=Divide(fit*(socket?(custom?equipment.socketPosition:fallback):restPosition),ParentScale(visual));
            visual.localRotation=socket?Quaternion.Euler(custom?equipment.socketEuler:new Vector3(90,0,0)):restRotation;
        }
        static float EquipmentFactor(Transform visual,EquipmentDefinition item)
        {
            var model=visual.GetComponentInParent<HeroModelView>(true);
            return model!=null?model.EquipmentScale(item):1;
        }
        public static void SetWorldScale(Transform visual,Vector3 size,EquipmentDefinition item=null)
        {if(visual!=null)visual.localScale=Divide(size*EquipmentFactor(visual,item),ParentScale(visual));}
    }
}
