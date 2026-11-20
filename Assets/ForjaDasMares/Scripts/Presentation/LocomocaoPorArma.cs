using System.Collections.Generic;
using UnityEngine;

namespace ForjaDasMares
{
    // Standing, walking and running for each kind of weapon (made by "Forja/Personagem/Animações por arma").
    // The hero's body swaps its base Idle/Walk/Run for these while the weapon is equipped.
    public sealed class LocomocaoPorArma : ScriptableObject
    {
        [System.Serializable]
        public sealed class Jeito
        {
            public string nome;
            public AnimationClip parado, andar, correr;
            // Metres per loop for each body (0 = male, 1 = female), so the feet keep pace with the ground.
            public float[] passadaAndar = { 1, 1 }, passadaCorrer = { 2.4f, 2.4f };
        }

        // The base clips of the hero's Animator, the ones that get replaced.
        public AnimationClip baseParado, baseParadoOlhando, baseAndar, baseAndarFeminino, baseCorrer;
        public Jeito espada, cajado;

        public Jeito Para(WeaponCategory category) =>
            category == WeaponCategory.Staff ? Valid(cajado) :
            category == WeaponCategory.Sword || category == WeaponCategory.Axe || category == WeaponCategory.Hammer ? Valid(espada) : null;
        static Jeito Valid(Jeito j) => j != null && j.parado != null && j.andar != null && j.correr != null ? j : null;

        static LocomocaoPorArma loaded; static bool tried;
        public static LocomocaoPorArma Load()
        {
            if (!tried) { tried = true; loaded = Resources.Load<LocomocaoPorArma>("Personagens/LocomocaoPorArma"); }
            return loaded;
        }

        // A copy of the body's Animator with the weapon's own Idle/Walk/Run.
        public RuntimeAnimatorController Montar(RuntimeAnimatorController original, Jeito jeito)
        {
            if (original == null || jeito == null) return original;
            var root = original; var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            if (original is AnimatorOverrideController parent) { root = parent.runtimeAnimatorController; parent.GetOverrides(pairs); }
            var result = new AnimatorOverrideController(root) { name = original.name + " · " + jeito.nome };
            if (pairs.Count == 0) result.GetOverrides(pairs);
            for (int i = 0; i < pairs.Count; i++)
            {
                var key = pairs[i].Key; AnimationClip swap = null;
                if (key == baseParado || key == baseParadoOlhando) swap = jeito.parado;
                else if (key == baseAndar || key == baseAndarFeminino) swap = jeito.andar;
                else if (key == baseCorrer) swap = jeito.correr;
                if (swap != null) pairs[i] = new KeyValuePair<AnimationClip, AnimationClip>(key, swap);
            }
            result.ApplyOverrides(pairs);
            return result;
        }
    }
}
