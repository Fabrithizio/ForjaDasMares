using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace ForjaDasMares
{
    // Every keyboard key the game uses, in one place, and the player's choices (Configurações › Teclas).
    // Each key is saved on this device (PlayerPrefs "ForjaTecla_<id>"); a change applies right away, no restart.
    public static class Teclas
    {
        public sealed class Acao
        {
            public readonly string id, nome, padrao;
            public Acao(string i, string n, string p) { id = i; nome = n; padrao = p; }
        }

        public static readonly Acao[] Todas =
        {
            new Acao("hab1", "Habilidade 1", "<Keyboard>/q"),
            new Acao("hab2", "Habilidade 2", "<Keyboard>/w"),
            new Acao("hab3", "Habilidade 3", "<Keyboard>/e"),
            new Acao("trocar", "Trocar barra de habilidades", "<Keyboard>/tab"),
            new Acao("super", "Super", "<Keyboard>/r"),
            new Acao("esquiva", "Esquiva (dash)", "<Keyboard>/space"),
            new Acao("pocao", "Poção", "<Keyboard>/4"),
            new Acao("interagir", "Interagir / falar", "<Keyboard>/f"),
            new Acao("parado", "Atacar parado (segurar)", "<Keyboard>/leftShift"),
            new Acao("montar", "Montar / descer", "<Keyboard>/m"),
            new Acao("mochila", "Mochila", "<Keyboard>/i"),
            new Acao("mapa", "Mapa do mundo", "<Keyboard>/k"),
            new Acao("minimapa", "Minimapa grande", "<Keyboard>/n"),
            new Acao("voltar", "Voltar para a cidade", "<Keyboard>/h"),
            new Acao("tela", "Tela cheia / janela", "<Keyboard>/f11"),
            new Acao("fps", "Mostrar FPS", "<Keyboard>/f8"),
        };

        public static event Action Mudou;
        public static bool Escutando { get; private set; }
        public static int ParouNoQuadro { get; private set; } = -1; // Esc that ended a wait must not also close a menu

        static Acao Achar(string id) { foreach (var a in Todas) if (a.id == id) return a; return null; }
        public static string Caminho(string id) { var a = Achar(id); return a == null ? "" : PlayerPrefs.GetString("ForjaTecla_" + id, a.padrao); }

        // The key's name on screen: "Q", "ESPAÇO", "SHIFT"...
        public static string Nome(string id)
        {
            var path = Caminho(id);
            if (string.IsNullOrEmpty(path)) return "—";
            var s = InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice).ToUpperInvariant();
            switch (s)
            {
                case "SPACE": return "ESPAÇO";
                case "LEFT SHIFT": case "RIGHT SHIFT": case "SHIFT": return "SHIFT";
                case "LEFT CTRL": case "RIGHT CTRL": case "CTRL": return "CTRL";
                case "LEFT ALT": case "RIGHT ALT": case "ALT": return "ALT";
                case "ENTER": return "ENTER";
                case "BACKSPACE": return "APAGAR";
                case "UP ARROW": return "↑";
                case "DOWN ARROW": return "↓";
                case "LEFT ARROW": return "←";
                case "RIGHT ARROW": return "→";
            }
            return s;
        }

        // Actions that use each key, so a change reaches them while the game runs.
        static readonly Dictionary<string, List<KeyValuePair<InputAction, int>>> ligadas = new Dictionary<string, List<KeyValuePair<InputAction, int>>>();
        public static void Registrar(string id, InputAction action, int binding)
        {
            if (!ligadas.TryGetValue(id, out var list)) ligadas[id] = list = new List<KeyValuePair<InputAction, int>>();
            list.Add(new KeyValuePair<InputAction, int>(action, binding));
        }
        // A new button action for this key (the caller enables it and adds callbacks).
        public static InputAction Criar(string id)
        {
            var action = new InputAction("Tecla_" + id, InputActionType.Button);
            action.AddBinding(Caminho(id));
            Registrar(id, action, 0);
            return action;
        }
        // An action from an asset: its keyboard keys are replaced by this one (mouse and gamepad stay).
        public static void Assumir(string id, InputAction action)
        {
            if (action == null) return;
            Soltar(action);
            action.AddBinding(Caminho(id));
            Registrar(id, action, action.bindings.Count - 1);
        }
        // Takes every keyboard key off an action (keeps mouse and gamepad).
        public static void Soltar(InputAction action)
        {
            if (action == null) return;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var b = action.bindings[i];
                if (!b.isComposite && !b.isPartOfComposite && b.effectivePath.StartsWith("<Keyboard>")) action.ApplyBindingOverride(i, "");
            }
        }

        // For code that checks a key each frame ("was it pressed now?").
        static readonly Dictionary<string, InputAction> sondas = new Dictionary<string, InputAction>();
        public static bool Apertou(string id)
        {
            if (Escutando) return false;
            if (!sondas.TryGetValue(id, out var action) || action == null) { action = Criar(id); action.Enable(); sondas[id] = action; }
            return action.WasPressedThisFrame();
        }

        // Which other action already uses this key (null if none).
        public static string QuemUsa(string path, string exceto)
        {
            foreach (var a in Todas) if (a.id != exceto && string.Equals(Caminho(a.id), path, StringComparison.OrdinalIgnoreCase)) return a.id;
            return null;
        }
        public static string NomeDaAcao(string id) => Achar(id)?.nome ?? id;

        // Gives this action the key; if another action had it, that one gets this action's old key (they swap).
        public static void Mudar(string id, string path)
        {
            var antiga = Caminho(id);
            var outra = QuemUsa(path, id);
            Gravar(id, path);
            if (outra != null) Gravar(outra, antiga);
            PlayerPrefs.Save();
            Mudou?.Invoke();
        }
        public static void Padrao()
        {
            foreach (var a in Todas) Gravar(a.id, a.padrao);
            PlayerPrefs.Save();
            Mudou?.Invoke();
        }
        static void Gravar(string id, string path)
        {
            var a = Achar(id); if (a == null) return;
            if (path == a.padrao) PlayerPrefs.DeleteKey("ForjaTecla_" + id); else PlayerPrefs.SetString("ForjaTecla_" + id, path);
            if (!ligadas.TryGetValue(id, out var list)) return;
            list.RemoveAll(p => p.Key == null);
            foreach (var p in list)
            {
                try { if (p.Value < p.Key.bindings.Count) p.Key.ApplyBindingOverride(p.Value, path); }
                catch (Exception) { } // an action already thrown away (scene changed)
            }
        }

        // Waits for the next key on the keyboard (Esc cancels; mouse and gamepad are ignored).
        public static void Escutar(Action<string> pronto)
        {
            Escutando = true;
            InputSystem.onAnyButtonPress.CallOnce(control =>
            {
                if (!(control.device is Keyboard)) { Escutar(pronto); return; }
                Escutando = false; ParouNoQuadro = Time.frameCount;
                if (control.name == "escape") { pronto(null); return; }
                pronto("<Keyboard>/" + control.name);
            });
        }
    }
}
