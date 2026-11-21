using UnityEngine;

namespace ForjaDasMares
{
    // The first 7 real abilities (approved by Fabricio): one per role, using the 6 elements.
    // Each has two forms: near with a melee weapon (sword...), far with a ranged weapon (staff...).
    // They help the weapon (which is the main damage): they hold, mark, weaken, protect, heal and move.
    public static class HabilidadesReais
    {
        static AbilityDefinition[] todas;
        public static AbilityDefinition[] Todas => todas ??= Criar();

        static AbilityDefinition[] Criar()
        {
            var list = new[]
            {
                Nova("raizes", "Raízes", Elemento.Seiva, Subtipo.Controle, 25, 10, 4, ControlEffect.Root, 1.5f,
                    "Golpe no chão: raízes prendem quem está em volta.", "Semente lançada que brota raízes onde cai."),
                Nova("marca", "Marca do Raio", Elemento.Raio, Subtipo.Preparo, 20, 8, 6, ControlEffect.None, 0,
                    "Golpe que marca: o próximo ataque da arma nele dá uma descarga extra.", "Faísca que marca de longe."),
                Nova("brasa", "Brasa", Elemento.Fogo, Subtipo.Enfraquecer, 25, 10, 8, ControlEffect.None, 0,
                    "Corte em chamas: quem queima recebe +20% de dano por 5 s.", "Bola de fogo que queima uma área pequena."),
                Nova("rajada", "Rajada", Elemento.Vento, Subtipo.Movimento, 20, 7, 6, ControlEffect.None, 0,
                    "Avanço rápido, empurrando quem está no caminho.", "Salto pra trás que empurra quem está perto."),
                Nova("bolha", "Bolha d'Água", Elemento.Agua, Subtipo.Protecao, 30, 14, 1, ControlEffect.None, 0,
                    "Bloqueia o próximo golpe e devolve metade do dano.", "Bolha em você e nos aliados perto que segura 30 de dano."),
                Nova("brotar", "Brotar", Elemento.Seiva, Subtipo.Cura, 35, 18, 1, ControlEffect.None, 0,
                    "Por 5 s, cada golpe da arma te cura.", "Círculo no chão que cura você e os aliados."),
                Nova("gelo", "Pele de Gelo", Elemento.Gelo, Subtipo.Fortalecer, 30, 16, 1, ControlEffect.None, 0,
                    "Por 6 s você fica mais resistente e seus golpes deixam lento.", "O mesmo, para você e os aliados perto."),
            };
            // What each hit does to whoever it touches.
            EstadosDeCombate.AoAcertar[Hit("marca")] = (target, source) => EstadosDeCombate.Of(target).Marcar(source, .6f, 6);
            EstadosDeCombate.AoAcertar[Shot("marca")] = (target, source) => EstadosDeCombate.Of(target).Marcar(source, .6f, 6);
            System.Action<Health, Health> burn = (target, source) =>
            {
                EstadosDeCombate.Of(target).Enfraquecer(.2f, 5);
                var ailments = target.GetComponent<CombatAilments>() ?? target.gameObject.AddComponent<CombatAilments>();
                ailments.ApplyBurn(source, 3);
            };
            EstadosDeCombate.AoAcertar[Hit("brasa")] = burn;
            EstadosDeCombate.AoAcertar[Shot("brasa")] = burn;
            return list;
        }

        static readonly System.Collections.Generic.Dictionary<string, AttackDefinition> hits = new System.Collections.Generic.Dictionary<string, AttackDefinition>();
        static readonly System.Collections.Generic.Dictionary<string, AttackDefinition> shots = new System.Collections.Generic.Dictionary<string, AttackDefinition>();
        static AttackDefinition Hit(string id) => hits[id];
        static AttackDefinition Shot(string id) => shots[id];

        static AbilityDefinition Nova(string id, string nome, Elemento e, Subtipo s, float custo, float espera, int dano, ControlEffect control, float controlTime, string perto, string longe)
        {
            var a = ScriptableObject.CreateInstance<AbilityDefinition>();
            a.name = nome; a.displayName = nome; a.scriptId = id; a.effect = AbilityEffect.Scripted;
            a.elemento = e; a.subtipo = s; a.energyCost = custo; a.cooldown = espera;
            a.descricaoEspada = perto; a.descricaoCajado = longe;
            var color = EfeitosDeHabilidade.Cor(e);
            var hit = ScriptableObject.CreateInstance<AttackDefinition>();
            hit.shape = AttackShape.Arc; hit.footprint = MeleeFootprint.Circle; hit.damage = Mathf.Max(1, dano);
            hit.range = 3; hit.arc = 360; hit.windup = .2f; hit.recovery = .3f; hit.knockback = 0; hit.color = color;
            hit.control = control; hit.controlDuration = controlTime;
            a.hit = hit; hits[id] = hit;
            var shot = Object.Instantiate(hit);
            shot.shape = AttackShape.Projectile; shot.range = 14; shot.projectileSpeed = 20; shot.projectileRadius = .3f; shot.maximumTargets = 1;
            shots[id] = shot;
            return a;
        }

        // ---------- Each ability, near and far ----------
        public static void Executar(HeroCombat c, AbilityDefinition a)
        {
            bool longe = c.RangedWeapon;
            var me = c.Owner; var feet = c.transform.position; var f = c.Facing; var o = c.StrikePoint != null ? c.StrikePoint.position : feet + Vector3.up;
            var cor = EfeitosDeHabilidade.Cor(a.elemento);
            var hit = Hit(a.scriptId);
            switch (a.scriptId)
            {
                case "raizes":
                    if (!longe)
                    {
                        c.Resolver.Area(feet + Vector3.up * .5f, 3.4f, f, 360, hit, me, c.TargetMask, c.WallMask, true, c.StrikeScale, c.Critical);
                        EfeitosDeHabilidade.Explosao(feet, 3.4f, cor, 50);
                        EfeitosDeHabilidade.Extra("raizes", feet, cor);
                    }
                    else
                    {
                        var point = Ponto(c, o, f, 7);
                        EfeitosDeHabilidade.Rastro(o, point + Vector3.up * .3f, cor);
                        c.Resolver.Area(point + Vector3.up * .5f, 2.8f, f, 360, hit, me, c.TargetMask, c.WallMask, true, c.StrikeScale, c.Critical);
                        EfeitosDeHabilidade.Explosao(point, 2.8f, cor, 45);
                        EfeitosDeHabilidade.Extra("raizes", point, cor);
                    }
                    break;
                case "marca":
                case "brasa":
                    if (!longe)
                    {
                        float arc = a.scriptId == "marca" ? 90 : 120;
                        c.Resolver.Area(o, 3.4f, f, arc, hit, me, c.TargetMask, c.WallMask, true, c.StrikeScale, c.Critical);
                        EfeitosDeHabilidade.Explosao(feet + f * 1.6f, 1.8f, cor, 40);
                        EfeitosDeHabilidade.Extra(a.scriptId + "_perto", o + f * 1.2f, Quaternion.LookRotation(f), 1);
                    }
                    else
                    {
                        var shot = Shot(a.scriptId);
                        if (a.scriptId == "brasa") shot.splashRadius = 2.2f;
                        c.Pool.Shoot(o + f * .3f, f, shot, me, c.TargetMask, c.WallMask, c.StrikeScale, c.Critical);
                        EfeitosDeHabilidade.Explosao(o + f * .6f, .8f, cor, 20);
                        EfeitosDeHabilidade.Extra(a.scriptId + "_longe", o + f * .6f, cor);
                    }
                    break;
                case "rajada":
                    if (!longe)
                    {
                        c.Motor.StartCharge(f, .22f, 18);
                        var push = Object.Instantiate(hit); push.knockback = 4; push.damage = 6;
                        c.Resolver.Area(feet + f * 2.5f + Vector3.up * .5f, 2.6f, f, 360, push, me, c.TargetMask, c.WallMask, true, c.StrikeScale, c.Critical);
                        EfeitosDeHabilidade.Rastro(feet + Vector3.up * .6f, feet + f * 4 + Vector3.up * .6f, cor);
                        EfeitosDeHabilidade.Extra("rajada", feet, Quaternion.LookRotation(f), 1);
                        Object.Destroy(push, 1);
                    }
                    else
                    {
                        var push = Object.Instantiate(hit); push.knockback = 5; push.damage = 4;
                        c.Resolver.Area(feet + Vector3.up * .5f, 3.2f, f, 360, push, me, c.TargetMask, c.WallMask, true, c.StrikeScale, c.Critical);
                        EfeitosDeHabilidade.Explosao(feet, 3.2f, cor, 45);
                        EfeitosDeHabilidade.Extra("rajada", feet, Quaternion.LookRotation(-f), 1);
                        c.Motor.StartCharge(-f, .2f, 16);
                        Object.Destroy(push, 1);
                    }
                    break;
                case "bolha":
                    if (!longe) EstadosDeCombate.Of(me).BloquearProximo(4, .5f);
                    else foreach (var ally in Aliados(c, 6)) EstadosDeCombate.Of(ally).Escudo(30, 5);
                    EfeitosDeHabilidade.Coluna(feet, 1.2f, cor);
                    EfeitosDeHabilidade.Extra("bolha", feet + Vector3.up, cor);
                    break;
                case "brotar":
                    if (!longe) { EstadosDeCombate.Of(me).RouboDeVida(3, 5); EfeitosDeHabilidade.Coluna(feet, 1f, cor); }
                    else
                    {
                        var circle = new GameObject("Brotar").AddComponent<CirculoDeCura>();
                        circle.Configurar(c, feet, 3.2f, 5, 5, cor);
                    }
                    EfeitosDeHabilidade.Extra("brotar", feet, cor);
                    break;
                case "gelo":
                    foreach (var ally in longe ? Aliados(c, 6) : new System.Collections.Generic.List<Health> { me })
                    {
                        ally.Protect(.3f, 6);
                        EstadosDeCombate.Of(ally).GeloNosGolpes(6);
                        EfeitosDeHabilidade.Coluna(ally.transform.position, 1f, cor);
                    }
                    EfeitosDeHabilidade.Extra("gelo", feet + Vector3.up, cor);
                    break;
            }
        }

        // Where a thrown thing lands: in front, or just before a wall.
        static Vector3 Ponto(HeroCombat c, Vector3 from, Vector3 f, float distance)
        {
            var to = from + f * distance;
            if (Physics.Linecast(from, to, out var hit, c.WallMask, QueryTriggerInteraction.Ignore)) to = hit.point - f * .5f;
            if (Physics.Raycast(to + Vector3.up * 2, Vector3.down, out var ground, 6, ~0, QueryTriggerInteraction.Ignore)) to.y = ground.point.y; else to.y = c.transform.position.y;
            return to;
        }

        // You and the allies near you.
        static readonly Collider[] near = new Collider[24];
        public static System.Collections.Generic.List<Health> Aliados(HeroCombat c, float radius)
        {
            var list = new System.Collections.Generic.List<Health> { c.Owner };
            int count = Physics.OverlapSphereNonAlloc(c.transform.position, radius, near, c.AllyMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var h = near[i].GetComponentInParent<Health>();
                if (h != null && h.IsAlive && h.Faction == CombatFaction.Player && !list.Contains(h)) list.Add(h);
            }
            System.Array.Clear(near, 0, count);
            return list;
        }
    }

    // Healing circle on the ground (Brotar with a ranged weapon).
    public sealed class CirculoDeCura : MonoBehaviour
    {
        HeroCombat owner; float radius, perSecond, until, next; Color color;
        public void Configurar(HeroCombat c, Vector3 at, float r, float heal, float seconds, Color cor)
        {
            owner = c; radius = r; perSecond = heal; until = Time.time + seconds; color = cor;
            transform.position = at;
            EfeitosDeHabilidade.Coluna(at, r * .8f, cor);
            EfeitosDeHabilidade.Aura(transform, cor, true, r * .9f, 30);
        }
        void Update()
        {
            if (owner == null || Time.time > until) { Destroy(gameObject); return; }
            if (Time.time < next) return;
            next = Time.time + 1;
            foreach (var h in HabilidadesReais.Aliados(owner, 30))
                if ((h.transform.position - transform.position).sqrMagnitude <= radius * radius) h.Heal(Mathf.RoundToInt(perSecond));
        }
    }
}
