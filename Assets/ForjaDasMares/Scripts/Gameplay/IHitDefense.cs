using UnityEngine;
namespace ForjaDasMares
{
    public interface IHitDefense
    {
        float DamageMultiplier(Vector3 incomingDirection, Health source, AttackDefinition attack);
    }
}
