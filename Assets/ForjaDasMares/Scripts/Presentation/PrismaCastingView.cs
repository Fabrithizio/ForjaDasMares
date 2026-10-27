using UnityEngine;
namespace ForjaDasMares
{
    // Caster identity/socket reference. Legacy wire circles intentionally removed.
    [DisallowMultipleComponent]
    public sealed class PrismaCastingView:MonoBehaviour
    {
        [SerializeField] HeroModelView model;
        public HeroModelView Model=>model;
        public int EffectCapacity=>0;
        public void Configure(HeroModelView view,Material legacyMaterial){model=view;}
    }
}
