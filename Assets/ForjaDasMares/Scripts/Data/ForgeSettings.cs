using System;
using UnityEngine;

namespace ForjaDasMares
{
    // Forge numbers, editable in the Inspector (Assets/ForjaDasMares/Resources/Forge/ForgeSettings).
    [CreateAssetMenu(menuName = "Forja das Marés/Configuração da Forja", fileName = "ForgeSettings")]
    public sealed class ForgeSettings : ScriptableObject
    {
        [Serializable]
        public sealed class Band
        {
            public string label;
            [Tooltip("Maior nível de destino desta faixa (ex.: 3 = do +1 ao +3).")]
            public int upToLevel;
            [Tooltip("% que UMA pedra soma, para Nv1, Nv2, Nv3, Nv4 e Nv5. 0 = a pedra não serve nesta faixa.")]
            public float[] percentPerStone = new float[5];
        }

        public Band[] bands;
        [Tooltip("% extra do Amuleto da Sorte.")] public float luckBonus = 5f;
        [Tooltip("A partir deste nível a falha faz a arma cair 1 nível.")] public int dropFromLevel = 7;
        [Tooltip("Dano extra por nível de Forja (0,05 = 5%).")] public float damagePerLevel = .05f;
        [Tooltip("Pedras iguais para fazer 1 do nível seguinte.")] public int fusionCost = 4;
    }
}
