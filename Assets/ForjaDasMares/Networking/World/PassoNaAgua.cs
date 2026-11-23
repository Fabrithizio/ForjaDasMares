using System;
using UnityEngine;

namespace ForjaDasMares.Networking
{
    // Walking in water (rivers): 30% slower, on foot or mounted, until you leave it. A splash when you step in
    // and small splashes while you wade. Any place can use it: PassoNaAgua.Criar(p => is p in the water).
    public sealed class PassoNaAgua : MonoBehaviour
    {
        public const float Lentidao = .7f;
        static readonly Color Cor = new Color(.7f, .92f, 1f);
        Func<Vector2, bool> naAgua; RegionalWorld world; PlayerMotor motor; bool dentro; float respingo;

        public static PassoNaAgua Criar(Func<Vector2, bool> naAgua)
        {
            var p = new GameObject("PassoNaAgua").AddComponent<PassoNaAgua>();
            p.naAgua = naAgua; return p;
        }

        void Update()
        {
            if (world == null) { world = FindFirstObjectByType<RegionalWorld>(); return; }
            if (world.Hero == null) return;
            if (motor == null) { motor = world.Hero.GetComponent<PlayerMotor>(); if (motor == null) return; }
            var pos = world.Hero.transform.position;
            bool agora = naAgua != null && naAgua(new Vector2(pos.x, pos.z));
            // Eases in and out (no sudden stop at the bank).
            motor.WaterMultiplier = Mathf.MoveTowards(motor.WaterMultiplier, agora ? Lentidao : 1f, Time.deltaTime * 3f);
            if (agora && !dentro) EfeitosDeHabilidade.Explosao(pos + Vector3.up * .15f, .9f, Cor, 18);
            dentro = agora;
            if (!agora) return;
            respingo -= Time.deltaTime;
            if (respingo <= 0 && motor.PlanarVelocity.sqrMagnitude > .5f)
            {
                respingo = .3f;
                EfeitosDeHabilidade.Explosao(pos + Vector3.up * .1f, .35f, Cor, 5);
            }
        }

        void OnDestroy() { if (motor != null) motor.WaterMultiplier = 1f; }
    }
}
