using UnityEngine;

namespace ForjaDasMares
{
    public enum AbilityEffect { AreaImpact, Trap, ElementalBurst, ProtectionCharge, WeaponStrike, Scripted }
    // Elements and roles of the abilities (decided with Fabricio). Breu comes later, at high levels.
    public enum Elemento { Nenhum, Fogo, Gelo, Raio, Vento, Agua, Seiva }
    public enum Subtipo { Nenhum, Controle, Protecao, Cura, Movimento, Preparo, Enfraquecer, Fortalecer }
    [CreateAssetMenu(menuName = "Forja/Ability")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        public string displayName;
        public AbilityEffect effect;
        public AttackDefinition hit;
        // Scripted abilities (effect = Scripted) run their own code in HabilidadesReais, by this id.
        public string scriptId;
        public Elemento elemento;
        public Subtipo subtipo;
        [TextArea] public string descricaoEspada, descricaoCajado;
        [Min(0.1f)] public float cooldown = 5f;
        // Inner energy spent to use it (EnergiaInterior). There is no mana.
        [Min(0)] public float energyCost = 25f;
        [Min(0.1f)] public float radius = 3.5f;
        [Min(0.1f)] public float trapLifetime = 6f;
        [Min(0.1f)] public float placementDistance = 2.2f;
        [Min(0.05f)] public float chargeDuration = 0.3f;
        [Min(0.1f)] public float chargeSpeed = 13f;
        [Min(0.1f)] public float protectionDuration = 3f;
        [Range(0, 0.8f)] public float protectionReduction = 0.6f;
    }
}
