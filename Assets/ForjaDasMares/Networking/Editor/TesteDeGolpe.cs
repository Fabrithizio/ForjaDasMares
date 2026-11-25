using UnityEditor;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    // Editor test only (not in the game): in Play, presses the attack button like the phone's ATAQUE and
    // freezes the Editor right when the slash is on screen, so the effect can be looked at calmly.
    public static class TesteDeGolpe
    {
        static ArenaInput input; static float started, pauseAfter; static bool waiting;

        [MenuItem("Forja/Teste/Golpe e pausa")] static void Golpe() => Start(.07f);
        [MenuItem("Forja/Teste/Golpe e pausa (fim do corte)")] static void GolpeFim() => Start(.16f);
        [MenuItem("Forja/Teste/Golpe e pausa", true)] static bool Can() => Application.isPlaying;
        [MenuItem("Forja/Teste/Golpe e pausa (fim do corte)", true)] static bool CanFim() => Application.isPlaying;

        static void Start(float after)
        {
            input = Object.FindFirstObjectByType<ArenaInput>();
            if (input == null) { Debug.LogWarning("TESTE_GOLPE: herói não encontrado"); return; }
            EditorApplication.isPaused = false;
            started = Time.time; pauseAfter = after; waiting = true;
            input.SetUiAttackHeld(true);
            EditorApplication.update -= Watch; EditorApplication.update += Watch;
        }

        static void Watch()
        {
            if (!waiting || !Application.isPlaying) { EditorApplication.update -= Watch; return; }
            if (input != null && Time.time - started > .1f) input.SetUiAttackHeld(false);
            bool slash = CortesDaEspada.UltimoCorte >= started;
            if (slash && Time.time - CortesDaEspada.UltimoCorte >= pauseAfter)
            {
                waiting = false; EditorApplication.isPaused = true; EditorApplication.update -= Watch;
                Debug.Log("TESTE_GOLPE: pausado no corte");
            }
            else if (Time.time - started > 3) { waiting = false; EditorApplication.update -= Watch; if (input != null) input.SetUiAttackHeld(false); Debug.LogWarning("TESTE_GOLPE: nenhum corte apareceu"); }
        }
    }
}
