using UnityEditor;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    // Editor test only (not in the game): in Play, presses the attack button like the phone's ATAQUE and
    // freezes the Editor right when the slash is on screen, so the effect can be looked at calmly.
    public static class TesteDeGolpe
    {
        static ArenaInput input; static float started, pauseAfter, holdFor; static bool waiting;

        [MenuItem("Forja/Teste/Golpe e pausa")] static void Golpe() => Start(.07f, .1f);
        [MenuItem("Forja/Teste/Golpe e pausa (fim do corte)")] static void GolpeFim() => Start(.16f, .1f);
        // Fills the five drops of the Maré Crescente: the next blow releases the wave.
        [MenuItem("Forja/Teste/Encher a Maré Crescente")] static void Mare()
        {
            var combat = Object.FindFirstObjectByType<HeroCombat>();
            if (combat != null && combat.Mecanica is MareCrescente mare) { mare.Encher(); Debug.Log("TESTE_MARE: 5 gotas"); }
            else Debug.LogWarning("TESTE_MARE: equipe a Perdição de Netuno primeiro");
        }
        [MenuItem("Forja/Teste/Super e pausa")] static void Super()
        {
            var combat = Object.FindFirstObjectByType<HeroCombat>(); if (combat != null) combat.AddSuper(100);
            input = Object.FindFirstObjectByType<ArenaInput>(); if (input == null) return;
            EditorApplication.isPaused = false; started = Time.time; pauseAfter = .8f; holdFor = 0; waiting = true;
            input.RequestSuper();
            EditorApplication.update -= Watch; EditorApplication.update += Watch;
        }
        [MenuItem("Forja/Teste/Golpe e pausa", true)] static bool Can() => Application.isPlaying;
        [MenuItem("Forja/Teste/Golpe e pausa (fim do corte)", true)] static bool CanFim() => Application.isPlaying;
        [MenuItem("Forja/Teste/Encher a Maré Crescente", true)] static bool CanMare() => Application.isPlaying;
        [MenuItem("Forja/Teste/Super e pausa", true)] static bool CanSuper() => Application.isPlaying;

        // Gives the Perdição de Netuno (legendary) and puts it in the hand.
        [MenuItem("Forja/Teste/Ganhar e equipar Perdição de Netuno")]
        static void Netuno()
        {
            var inventory = Object.FindFirstObjectByType<RegionalInventory>();
            var roster = Object.FindFirstObjectByType<HeroRoster>();
            if (inventory == null || inventory.Service == null || roster == null) { Debug.LogWarning("TESTE_NETUNO: entre no jogo primeiro"); return; }
            var service = inventory.Service;
            service.GrantTestEquipment(NetunoInstaller.Id, ItemRarity.Legendary);
            var owned = service.Profile.inventory.Find(i => i.definitionId == NetunoInstaller.Id);
            if (owned == null) { Debug.LogWarning("TESTE_NETUNO: " + service.Message); return; }
            bool ok = service.Equip(roster.Current.heroId, owned.instanceId, EquipmentSlot.MainHand);
            Debug.Log("TESTE_NETUNO: " + (ok ? "equipado" : "não equipou: " + service.Message));
        }
        [MenuItem("Forja/Teste/Ganhar e equipar Perdição de Netuno", true)] static bool CanNetuno() => Application.isPlaying;

        static void Start(float after, float hold)
        {
            input = Object.FindFirstObjectByType<ArenaInput>();
            if (input == null) { Debug.LogWarning("TESTE_GOLPE: herói não encontrado"); return; }
            EditorApplication.isPaused = false;
            started = Time.time; pauseAfter = after; holdFor = hold; waiting = true;
            input.SetUiAttackHeld(true);
            EditorApplication.update -= Watch; EditorApplication.update += Watch;
        }

        static void Watch()
        {
            if (!waiting || !Application.isPlaying) { EditorApplication.update -= Watch; return; }
            if (input != null && holdFor > 0 && Time.time - started > holdFor) input.SetUiAttackHeld(false);
            bool slash = CortesDaEspada.UltimoCorte >= started;
            if (slash && Time.time - CortesDaEspada.UltimoCorte >= pauseAfter)
            {
                waiting = false; EditorApplication.isPaused = true; EditorApplication.update -= Watch;
                if (input != null) input.SetUiAttackHeld(false);
                Debug.Log("TESTE_GOLPE: pausado no corte");
            }
            else if (Time.time - started > 4) { waiting = false; EditorApplication.update -= Watch; if (input != null) input.SetUiAttackHeld(false); Debug.LogWarning("TESTE_GOLPE: nenhum corte apareceu"); }
        }
    }
}
