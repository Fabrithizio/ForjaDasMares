using UnityEngine;

namespace ForjaDasMares
{
    // How a special weapon plays (decided with Fabricio, 26/11): every weapon has its family's combo; a special
    // weapon can also have its own passive and its own Super. There is no general rule: each one is written here,
    // one by one. A weapon without a mechanic plays its family's way.
    public abstract class MecanicaDeArma
    {
        public EquipmentDefinition Arma { get; private set; }
        protected MecanicaDeArma(EquipmentDefinition arma) { Arma = arma; }
        // After a basic blow landed (hits = enemies struck), with its place in the combo (0, 1, 2).
        public virtual void GolpeBasico(HeroCombat c, int acertos, int combo) { }
        // The Super at the moment it lands. True = this weapon resolved it (the plain one is skipped).
        public virtual bool Super(HeroCombat c, AttackDefinition golpe) => false;
        public virtual void Passo(HeroCombat c, float dt) { }
        public virtual void Zerar() { }
        // True while the weapon's own effects are hitting (they do not fill the Super bar).
        public virtual bool SemCarga => false;
    }

    public static class MecanicasDeArma
    {
        public static MecanicaDeArma Criar(EquipmentDefinition arma)
        {
            if (arma == null || string.IsNullOrEmpty(arma.itemId)) return null;
            if (arma.itemId.StartsWith("netuno")) return new MareCrescente(arma);
            return null;
        }
        // How far something running along the ground from `inicio` can go before a wall (0 walls = no check).
        public static float Alcance(Vector3 inicio, Vector3 direcao, float maximo, int paredes)
        {
            if (paredes == 0) return maximo;
            return Physics.SphereCast(inicio + Vector3.up * .9f, .4f, direcao, out var hit, maximo, paredes, QueryTriggerInteraction.Ignore) ? Mathf.Max(0, hit.distance - .2f) : maximo;
        }
    }

    // Perdição de Netuno.
    //   Passive "Maré Crescente": each blow that lands fills one drop on the blade (5 drops). With the five full,
    //   the next blow also releases a wave that runs 7.5 m forward along the ground.
    //   Super "Fúria do Touro Abissal": a bull of water charges 10 m forward; every enemy it hits is carried along in
    //   front of it. Where it stops, a whirlpool: the carried ones spin in it, lifted, taking damage (and it pulls in
    //   whoever is near); at the end it bursts — extra damage to everyone around — and they fall.
    // The look of all this is in PerdicaoDeNetuno (same numbers, read from here).
    public sealed class MareCrescente : MecanicaDeArma
    {
        public const int Max = 5;
        public const float OndaDistancia = 7.5f, OndaTempo = .62f, OndaRaio = 1.7f;
        public const float TouroDistancia = 10f, TouroTempo = .55f, TouroRaio = 2.2f;
        public const float RedemoinhoRaio = 5f, RedemoinhoTempo = 3f, RedemoinhoPulso = .4f;

        public int Gotas { get; private set; }
        public int Ondas { get; private set; } // counts the waves released (the look watches it)
        public Vector3 OndaInicio { get; private set; }
        public Vector3 OndaDirecao { get; private set; }
        public float OndaAlcance { get; private set; }

        readonly DamageResolver ondaAcertos = new DamageResolver(), touroAcertos = new DamageResolver(), redemoinhoAcertos = new DamageResolver();
        AttackDefinition redemoinho, explosao;
        // The enemies the bull took: their side offset while charging, then their angle in the whirlpool.
        readonly System.Collections.Generic.List<Health> presos = new System.Collections.Generic.List<Health>();
        readonly System.Collections.Generic.List<float> lado = new System.Collections.Generic.List<float>();
        float onda = -1, touro = -1, giro = -1, pulso, escala = 1, touroAlcance; bool critico;
        Vector3 touroInicio, touroDirecao, centro;

        public MareCrescente(EquipmentDefinition arma) : base(arma) { }
        public override bool SemCarga => touro >= 0 || giro >= 0;
        public void Encher() => Gotas = Max; // Editor test

        public override void GolpeBasico(HeroCombat c, int acertos, int combo)
        {
            if (Gotas >= Max && Arma.passiveAttack != null)
            {
                Gotas = 0; escala = c.StrikeScale; critico = c.Critical;
                OndaDirecao = Plano(c.Facing); OndaInicio = c.transform.position + OndaDirecao * .6f;
                OndaAlcance = MecanicasDeArma.Alcance(OndaInicio, OndaDirecao, OndaDistancia, c.WallMask);
                onda = 0; ondaAcertos.Clear(); Ondas++;
            }
            else if (acertos > 0) Gotas = Mathf.Min(Max, Gotas + 1);
        }

        public override bool Super(HeroCombat c, AttackDefinition golpe)
        {
            if (golpe == null) return false;
            escala = c.StrikeScale; critico = c.Critical;
            touroDirecao = Plano(c.Facing); touroInicio = c.transform.position + touroDirecao;
            touroAlcance = MecanicasDeArma.Alcance(touroInicio, touroDirecao, TouroDistancia, c.WallMask);
            touro = 0; giro = -1; touroAcertos.Clear(); presos.Clear(); lado.Clear();
            if (redemoinho == null)
            {
                // The whirlpool's pulses: small hits that pull inward and slow.
                redemoinho = Object.Instantiate(golpe); redemoinho.name = golpe.name + "_Redemoinho";
                redemoinho.damage = Mathf.Max(1, Mathf.RoundToInt(golpe.damage * .12f)); redemoinho.knockback = 1.4f;
                redemoinho.control = ControlEffect.Slow; redemoinho.slowMultiplier = .45f; redemoinho.controlDuration = .6f;
                // The burst at the end, as they fall.
                explosao = Object.Instantiate(golpe); explosao.name = golpe.name + "_Explosao";
                explosao.damage = Mathf.Max(1, Mathf.RoundToInt(golpe.damage * .9f)); explosao.knockback = 5f; explosao.control = ControlEffect.None;
            }
            return true;
        }

        public override void Passo(HeroCombat c, float dt)
        {
            if (c.Owner == null || !c.Owner.IsAlive) { Zerar(); return; }
            if (onda >= 0)
            {
                onda += dt; float k = Mathf.Clamp01(onda / OndaTempo), d = OndaAlcance * (1 - (1 - k) * (1 - k));
                ondaAcertos.Area(OndaInicio + OndaDirecao * d + Vector3.up * .9f, OndaRaio, OndaDirecao, 360, Arma.passiveAttack, c.Owner, c.TargetMask, c.WallMask, false, escala, critico);
                if (k >= 1) onda = -1;
            }
            if (touro >= 0 && Arma.superAttack != null)
            {
                touro += dt; float k = Mathf.Clamp01(touro / TouroTempo), d = touroAlcance * (1 - (1 - k) * (1 - k));
                Vector3 p = touroInicio + touroDirecao * d, right = Vector3.Cross(Vector3.up, touroDirecao);
                touroAcertos.Area(p + Vector3.up * .9f, TouroRaio, touroDirecao, 360, Arma.superAttack, c.Owner, c.TargetMask, c.WallMask, false, escala, critico, 1);
                // Whoever the bull hit goes with it, held in front of its head, each keeping its side.
                for (int i = 0; i < touroAcertos.Struck; i++)
                {
                    var h = touroAcertos.StruckAt(i);
                    if (h == null || presos.Contains(h)) continue;
                    presos.Add(h); lado.Add(Mathf.Clamp(Vector3.Dot(h.transform.position - p, right), -1.4f, 1.4f));
                }
                for (int i = 0; i < presos.Count; i++)
                    if (presos[i] != null) presos[i].Carry(p + touroDirecao * 1.3f + right * lado[i] + Vector3.up * .3f, .25f);
                if (k >= 1)
                {
                    touro = -1; giro = 0; pulso = 0; centro = p;
                    for (int i = 0; i < presos.Count; i++)
                        if (presos[i] != null) { var off = presos[i].transform.position - centro; lado[i] = Mathf.Atan2(off.z, off.x); }
                }
            }
            if (giro >= 0)
            {
                giro += dt; pulso -= dt;
                if (pulso <= 0) { pulso = RedemoinhoPulso; redemoinhoAcertos.Area(centro + Vector3.up * .9f, RedemoinhoRaio, touroDirecao, 360, redemoinho, c.Owner, c.TargetMask, c.WallMask, true, escala, critico, -1); }
                // The carried ones spin in the whirlpool, closer and higher as it goes on.
                float k = Mathf.Clamp01(giro / RedemoinhoTempo), raio = Mathf.Lerp(2.6f, 1.5f, k), alto = Mathf.Lerp(.3f, 1.5f, Mathf.Clamp01(giro / .6f));
                for (int i = 0; i < presos.Count; i++)
                {
                    if (presos[i] == null || !presos[i].IsAlive) continue;
                    lado[i] += dt * Mathf.Lerp(4f, 9f, k);
                    presos[i].Carry(centro + new Vector3(Mathf.Cos(lado[i]), 0, Mathf.Sin(lado[i])) * raio + Vector3.up * alto, .25f);
                }
                if (giro >= RedemoinhoTempo)
                {
                    // The burst: extra damage to everyone around; the carried ones are let go and fall.
                    giro = -1; presos.Clear(); lado.Clear();
                    redemoinhoAcertos.Area(centro + Vector3.up * .9f, RedemoinhoRaio, touroDirecao, 360, explosao, c.Owner, c.TargetMask, c.WallMask, true, escala, critico);
                }
            }
        }

        public override void Zerar() { onda = touro = giro = -1; presos.Clear(); lado.Clear(); }
        static Vector3 Plano(Vector3 v) { v.y = 0; return v.sqrMagnitude > .0001f ? v.normalized : Vector3.forward; }
    }
}
