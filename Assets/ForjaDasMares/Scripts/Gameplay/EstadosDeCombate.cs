using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForjaDasMares
{
    // Statuses left by the abilities on anyone with Health (hero or enemy), with their visible effect:
    // marked (next hit stronger), vulnerable (takes more damage), shield (absorbs), block (cancels the next hit
    // and returns part of it), life steal and frost on the hero's hits. Health asks this component on every hit.
    public sealed class EstadosDeCombate : MonoBehaviour
    {
        Health health;
        float markUntil, markBonus; Health markSource;
        float vulnUntil, vulnBonus;
        float shieldUntil; int shieldLeft;
        float blockUntil, blockReflect;
        float stealUntil; int stealPerHit;
        float frostUntil;
        // Hooks of each ability's hit (marking, burning...), by the AttackDefinition that hit.
        public static readonly Dictionary<AttackDefinition, Action<Health, Health>> AoAcertar = new Dictionary<AttackDefinition, Action<Health, Health>>();

        public static EstadosDeCombate Of(Component c)
        {
            if (c == null) return null;
            var e = c.GetComponent<EstadosDeCombate>();
            return e != null ? e : c.gameObject.AddComponent<EstadosDeCombate>();
        }
        void Awake() => health = GetComponent<Health>();
        static float Now => Time.time;

        public bool Marcado => Now < markUntil;
        public bool Vulneravel => Now < vulnUntil;
        public bool ComEscudo => Now < shieldUntil && shieldLeft > 0;
        public bool Bloqueando => Now < blockUntil;

        public void Marcar(Health by, float bonus, float seconds) { markSource = by; markBonus = bonus; markUntil = Now + seconds; }
        public void Enfraquecer(float bonus, float seconds) { vulnBonus = Mathf.Max(vulnBonus * (Vulneravel ? 1 : 0), bonus); vulnUntil = Now + seconds; }
        public void Escudo(int amount, float seconds) { shieldLeft = Mathf.Max(ComEscudo ? shieldLeft : 0, amount); shieldUntil = Now + seconds; }
        public void BloquearProximo(float seconds, float reflect) { blockUntil = Now + seconds; blockReflect = reflect; }
        public void RouboDeVida(int perHit, float seconds) { stealPerHit = perHit; stealUntil = Now + seconds; }
        public void GeloNosGolpes(float seconds) { frostUntil = Now + seconds; }

        // ---------- Called by Health ----------
        // Extra damage taken (vulnerable, marked by the attacker). The mark is spent by the hit.
        public float Multiplicador(Health source)
        {
            float m = 1;
            if (Vulneravel) m *= 1 + vulnBonus;
            if (Marcado && source != null && source == markSource)
            {
                m *= 1 + markBonus; markUntil = 0;
                EfeitosDeHabilidade.Explosao(transform.position, 1.6f, EfeitosDeHabilidade.Cor(Elemento.Raio), 30);
                EfeitosDeHabilidade.Extra("marca_descarga", transform.position + Vector3.up, Color.yellow);
            }
            return m;
        }
        // A block cancels the hit and returns part of it to whoever attacked.
        public bool Bloqueia(Health source, int damage)
        {
            if (!Bloqueando) return false;
            blockUntil = 0;
            EfeitosDeHabilidade.Explosao(transform.position + Vector3.up * .2f, 1.4f, EfeitosDeHabilidade.Cor(Elemento.Agua), 30);
            if (source != null && source != health && source.IsAlive && blockReflect > 0) source.ApplyDamage(Mathf.Max(1, Mathf.RoundToInt(damage * blockReflect)));
            return true;
        }
        // A shield eats damage before the life.
        public int Absorve(int damage)
        {
            if (!ComEscudo) return damage;
            int eaten = Mathf.Min(shieldLeft, damage);
            shieldLeft -= eaten;
            if (shieldLeft <= 0) { shieldUntil = 0; EfeitosDeHabilidade.Explosao(transform.position + Vector3.up, 1.2f, EfeitosDeHabilidade.Cor(Elemento.Agua), 25); }
            return damage - eaten;
        }
        // The hero hit someone: life steal and frost.
        public void Acertou(Health target)
        {
            if (Now < stealUntil && health != null) health.Heal(stealPerHit);
            if (Now < frostUntil && target != null) target.ApplyControl(ControlEffect.Slow, 1.5f, .5f);
        }

        // ---------- Visible statuses ----------
        readonly Dictionary<string, GameObject> auras = new Dictionary<string, GameObject>();
        void Update()
        {
            bool alive = health == null || health.IsAlive;
            Aura("marca", alive && Marcado, Elemento.Raio, false, "marca_aura");
            Aura("vuln", alive && Vulneravel, Elemento.Fogo, true, "brasa_aura");
            Aura("escudo", alive && ComEscudo, Elemento.Agua, false, "bolha_aura");
            Aura("bloqueio", alive && Bloqueando, Elemento.Agua, false, "bolha_aura");
            Aura("roubo", alive && Now < stealUntil, Elemento.Seiva, true, "brotar_aura");
            Aura("gelo", alive && Now < frostUntil, Elemento.Gelo, true, null);
            bool held = alive && health != null && health.ControlMultiplier < 1 && !health.IsStunned;
            Aura("preso", held, Elemento.Seiva, true, null);
        }
        void Aura(string key, bool on, Elemento e, bool rising, string pack)
        {
            auras.TryGetValue(key, out var current);
            if (on && current == null)
            {
                var root = new GameObject("Estado_" + key); root.transform.SetParent(transform, false);
                EfeitosDeHabilidade.Aura(root.transform, EfeitosDeHabilidade.Cor(e), rising, rising ? .6f : .45f);
                if (pack != null) EfeitosDeHabilidade.ExtraLoop(pack, root.transform, Vector3.up * (rising ? .1f : 1f), .7f);
                auras[key] = root;
            }
            else if (!on && current != null) { Destroy(current); auras[key] = null; }
        }
    }
}
