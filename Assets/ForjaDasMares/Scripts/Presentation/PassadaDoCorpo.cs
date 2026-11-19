using UnityEngine;

namespace ForjaDasMares
{
    // How many metres the body travels in one full loop of its walk and run animations.
    // Measured by the installer on the real model, so the feet keep pace with the ground (no "Papa-Léguas").
    public sealed class PassadaDoCorpo : MonoBehaviour
    {
        [Min(.1f)] public float andar = .95f, correr = 2.45f;
    }
}
