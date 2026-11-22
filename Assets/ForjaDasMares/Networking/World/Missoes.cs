using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // The missions of the game, read from the story steps (Historia), so there is only one saved state.
    // Several can be in progress at once. The player picks up to 3 to follow in the "Acompanhamento" panel.
    public static class Missoes
    {
        public enum Tipo { Principal, Secundaria }
        public sealed class Objetivo { public string texto; public int tem, precisa; public bool feito; }
        public sealed class Missao
        {
            public string id, npc, titulo, quem, descricao, recompensa, recurso; public Tipo tipo;
            public Func<bool> aceita, feita; public Func<List<Objetivo>> objetivos; public Func<string> linha;
        }

        static Objetivo Junta(string nome, string recurso, int precisa, bool entregue)
        {
            int n = entregue ? precisa : Mathf.Min(precisa, Recursos.Quantos(recurso));
            return new Objetivo { texto = nome, tem = n, precisa = precisa, feito = n >= precisa };
        }
        static Objetivo Passo(string texto, bool feito) => new Objetivo { texto = texto, feito = feito };

        public static readonly List<Missao> Todas = new List<Missao>
        {
            new Missao
            {
                id = "nascente", npc = "Nara", tipo = Tipo.Principal, titulo = "A Nascente do Rio", quem = "Nara · tenda, no Refúgio",
                descricao = "O rio está escurecendo e os bichos perto dele foram tomados pelo Breu. A Nara acha que vem de cima, da Nascente. Vá até lá e só olhe. Volte inteiro.",
                recompensa = "Descobrir o que está escurecendo o rio.",
                aceita = () => Historia.Feito("m4_aceita"), feita = () => Habilidades.Aprendeu("rajada"),
                objetivos = () => new List<Objetivo> { Passo("Chegue à Nascente do Rio (saída sul, rio acima)", Historia.Feito("m4_chegou")), Passo("Descubra o que está escurecendo o rio", Historia.Feito("m4_fonte")), Passo("Conte à Nara", Habilidades.Aprendeu("rajada")) },
                linha = () => Historia.Feito("m4_fonte") ? "Volte ao Refúgio e conte à Nara." : Historia.Feito("m4_chegou") ? "Descubra o que escurece o rio (Poço da Nascente)." : "Vá até a Nascente do Rio (saída sul)."
            },
            new Missao
            {
                id = "ervas", npc = "Iria", recurso = Recursos.ErvaLuz, tipo = Tipo.Secundaria, titulo = "Erva para a Iria", quem = "Iria · fonte do Refúgio",
                descricao = "Acabou a erva-luz da Iria, e sem ela não tem poção pra quem sai ferido. São plantinhas que brilham no campo, logo fora da muralha.",
                recompensa = "2 Poções de cura e a receita de poção.",
                aceita = () => Historia.Feito("m1_aceita"), feita = () => Historia.Feito("m1_feita"),
                objetivos = () => { bool f = Historia.Feito("m1_feita"); var o = Junta("Colha Erva-luz", Recursos.ErvaLuz, Historia.ErvasPedidas, f); return new List<Objetivo> { o, Passo("Entregue as ervas à Iria", f) }; },
                linha = () => { int n = Recursos.Quantos(Recursos.ErvaLuz); return n < Historia.ErvasPedidas ? "Erva-luz (fora da muralha): " + n + "/" + Historia.ErvasPedidas : "Leve as ervas à Iria (fonte)."; }
            },
            new Missao
            {
                id = "carne", npc = "Brás", recurso = Recursos.Carne, tipo = Tipo.Secundaria, titulo = "Carne para o Brás", quem = "Brás · feira do Refúgio",
                descricao = "Ninguém mais caça perto do rio: os bichos de lá ficaram de olho roxo e atacam quem passa. A cidade está sem carne.",
                recompensa = "40 de ouro.",
                aceita = () => Historia.Feito("m2_aceita"), feita = () => Historia.Feito("m2_feita"),
                objetivos = () => { bool f = Historia.Feito("m2_feita"); return new List<Objetivo> { Junta("Cace bichos tomados perto do rio (sul)", Recursos.Carne, Historia.CarnesPedidas, f), Passo("Entregue a carne ao Brás", f) }; },
                linha = () => { int n = Recursos.Quantos(Recursos.Carne); return n < Historia.CarnesPedidas ? "Carne (bichos do rio): " + n + "/" + Historia.CarnesPedidas : "Leve a carne ao Brás (feira)."; }
            },
            new Missao
            {
                id = "couro", npc = "Téo", recurso = Recursos.Couro, tipo = Tipo.Secundaria, titulo = "Couro para o Téo", quem = "Téo · forja do Refúgio",
                descricao = "O couro dos bichos tomados pelo Breu fica duro feito casca. O Téo quer três para fazer uma peça para você.",
                recompensa = "Um manto de couro (já vestido).",
                aceita = () => Historia.Feito("m3_aceita"), feita = () => Historia.Feito("m3_feita"),
                objetivos = () => { bool f = Historia.Feito("m3_feita"); return new List<Objetivo> { Junta("Junte couro dos bichos tomados", Recursos.Couro, Historia.CourosPedidos, f), Passo("Leve o couro ao Téo", f) }; },
                linha = () => { int n = Recursos.Quantos(Recursos.Couro); return n < Historia.CourosPedidos ? "Couro (bichos do rio): " + n + "/" + Historia.CourosPedidos : "Leve o couro ao Téo (forja)."; }
            },
            new Missao
            {
                // Found in the world (a note at the abandoned Liga camp in the Nascente), not given by anyone.
                id = "liga", npc = "", tipo = Tipo.Secundaria, titulo = "Estandartes da Liga", quem = "Bilhete · acampamento da Liga, Nascente do Rio",
                descricao = "Um bilhete velho no acampamento abandonado da Liga: uma patrulha marcou o caminho com três estandartes antes de sumir. Achar os três pode contar o que aconteceu com ela.",
                recompensa = "60 de ouro e o que a patrulha deixou.",
                aceita = () => Historia.Feito("m5_aceita"), feita = () => Historia.Feito("m5_feita"),
                objetivos = () => { int n = NascenteDoRio.EstandartesAchados; return new List<Objetivo> { new Objetivo { texto = "Ache os estandartes da patrulha", tem = n, precisa = 3, feito = n >= 3 } }; },
                linha = () => "Estandartes da Liga (Nascente): " + NascenteDoRio.EstandartesAchados + "/3"
            },
        };

        // The sign over a person's head (and on the minimap), like in Drakensang:
        // "!" = has a mission for you, "?" = you have what was asked (bright) or still missing something (grey).
        // Gold = main story, copper = side mission. Empty = nothing.
        public static readonly Color Cinza = new Color(.62f, .66f, .7f);
        public static (string simbolo, Color cor) Marcador(string quem)
        {
            if (string.IsNullOrEmpty(quem) || !Historia.Feito("chegada")) return ("", default);
            if (quem == "Nara" && !Historia.Feito("nara")) return ("!", Saque.Principal);
            if (quem == "Téo" && Historia.Feito("nara") && !Historia.Feito("teo")) return ("!", Saque.Principal);
            if (!Historia.Feito("teo")) return ("", default);
            if (quem == "Nara" && Historia.Feito("m2_feita") && !Historia.Feito("m4_aceita")) return ("!", Saque.Principal);
            foreach (var m in Todas)
            {
                if (m.npc != quem || m.tipo != Tipo.Secundaria || m.feita()) continue;
                if (!m.aceita()) return ("!", Saque.Secundaria);
                return ("?", m.objetivos()[0].feito ? Saque.Secundaria : Cinza);
            }
            return ("", default);
        }

        // The mission in progress that still asks for this resource (its drops glow in the mission colour).
        public static Missao QuemPrecisa(string recurso) => EmAndamento.FirstOrDefault(m => m.recurso == recurso);
        public static IEnumerable<Missao> EmAndamento => Ordem(Todas.Where(m => m.aceita() && !m.feita()));
        public static IEnumerable<Missao> Concluidas => Ordem(Todas.Where(m => m.feita()));
        static IEnumerable<Missao> Ordem(IEnumerable<Missao> list) => list.OrderBy(m => m.tipo == Tipo.Principal ? 0 : 1);

        // ---- Following (up to 3) ----
        public const int MaxSeguindo = 3;
        static string Key(string what) => "ForjaMissoes_" + Historia.Who + "_" + what;
        static List<string> Lista(string what) { var s = PlayerPrefs.GetString(Key(what), ""); return s.Length == 0 ? new List<string>() : s.Split(',').ToList(); }
        static void Guardar(string what, List<string> list) { PlayerPrefs.SetString(Key(what), string.Join(",", list)); PlayerPrefs.Save(); }
        public static bool Segue(Missao m) => Lista("seguir").Contains(m.id);
        // Returns false when 3 are already followed.
        public static bool Alternar(Missao m)
        {
            var list = Lista("seguir");
            if (list.Remove(m.id)) { Guardar("seguir", list); return true; }
            list.RemoveAll(id => !EmAndamento.Any(x => x.id == id));
            if (list.Count >= MaxSeguindo) return false;
            list.Add(m.id); Guardar("seguir", list); return true;
        }
        // A newly accepted mission is followed by itself while there is room.
        static void NovasSeguidas()
        {
            var vistas = Lista("vistas"); bool changed = false;
            foreach (var m in EmAndamento)
            {
                if (vistas.Contains(m.id)) continue;
                vistas.Add(m.id); changed = true;
                var seguir = Lista("seguir"); seguir.RemoveAll(id => !EmAndamento.Any(x => x.id == id));
                if (seguir.Count < MaxSeguindo) { seguir.Add(m.id); Guardar("seguir", seguir); }
            }
            if (changed) Guardar("vistas", vistas);
        }

        // What the "Acompanhamento" panel shows: one short line per followed mission, plus who has a request for you.
        public static string Acompanhamento
        {
            get
            {
                NovasSeguidas();
                var lines = new List<string>();
                foreach (var m in EmAndamento) if (Segue(m)) lines.Add((m.tipo == Tipo.Principal ? "<color=#E8BD6E>" : "") + "» " + m.linha() + (m.tipo == Tipo.Principal ? "</color>" : ""));
                if (lines.Count < MaxSeguindo) foreach (var p in Pedidos) { if (lines.Count >= MaxSeguindo) break; lines.Add("<color=#A9C3CC>! " + p + "</color>"); }
                return lines.Count > 0 ? string.Join("\n", lines) : null;
            }
        }
        // Requests waiting to be accepted.
        static IEnumerable<string> Pedidos
        {
            get
            {
                if (!Historia.Feito("teo")) yield break;
                if (Historia.Feito("m2_feita") && !Historia.Feito("m4_aceita")) yield return "A Nara quer falar com você.";
                if (!Historia.Feito("m1_aceita")) yield return "A Iria, na fonte, tem um pedido.";
                if (!Historia.Feito("m2_aceita")) yield return "O Brás, na feira, está chamando você.";
                if (!Historia.Feito("m3_aceita")) yield return "O Téo, na forja, quer falar com você.";
            }
        }

        public static void Limpar()
        {
            foreach (var w in new[] { "seguir", "vistas" }) PlayerPrefs.DeleteKey(Key(w));
            Descobertas.Limpar();
        }
    }

    // Things the player saw for the first time. The townsfolk who would care comment on them in the next talk
    // (1 or 2 lines before the usual talk), each person once per thing.
    public static class Descobertas
    {
        public const string BreuVisto = "breu_visto", TomadoAbatido = "tomado_abatido", Nascente = "nascente", PocaoBebida = "pocao_bebida";
        static readonly string[] Todas = { BreuVisto, TomadoAbatido, Nascente, PocaoBebida };
        static readonly string[] Pessoas = { "Iria", "Brás", "Téo", "Nara" };
        static string Key(string what) => "ForjaDescoberta_" + Historia.Who + "_" + what;
        public static bool Viu(string what) => PlayerPrefs.GetInt(Key(what), 0) == 1;
        public static void Marcar(string what) { if (Viu(what)) return; PlayerPrefs.SetInt(Key(what), 1); PlayerPrefs.Save(); }

        // Who says what about each thing. Order = which comment comes first.
        static readonly (string quem, string coisa, string[] falas)[] Comentarios =
        {
            ("Brás", BreuVisto, new[] { "Viu os bichos de olho roxo, né? Eu te falei! Arrepia só de lembrar." }),
            ("Iria", BreuVisto, new[] { "Você tá pálido. Viu os bichos do rio, foi?", "Bebe uma água da fonte, bebe. Ajuda. Acho." }),
            ("Téo", BreuVisto, new[] { "Viu os bichos tomados. Não chega perto sem uma arma boa." }),
            ("Nara", BreuVisto, new[] { "Olhos roxos, perto do rio. Anotei. Está descendo mais rápido do que eu pensava." }),
            ("Brás", TomadoAbatido, new[] { "Derrubou um daqueles? Inteiro? Respeito, amigo. Respeito." }),
            ("Téo", TomadoAbatido, new[] { "Derrubou um tomado. Hm. Bom. O couro deles serve pra peça boa." }),
            ("Iria", PocaoBebida, new[] { "Usou a poção! Funcionou? Claro que funcionou. Eu que fiz!" }),
            ("Nara", Nascente, new[] { "Você chegou à Nascente. Eu vi na sua cara.", "Depois me conta tudo. Com calma. Cada detalhe." }),
            ("Iria", Nascente, new[] { "Você foi até a Nascente?! Sozinho? Ai, ai, ai..." }),
        };
        static string Visto(string quem, string coisa) => "ForjaComentario_" + Historia.Who + "_" + quem + "_" + coisa;

        // The next comment this person has for you, or null. Marks it as said.
        public static string[] Proximo(string quem)
        {
            foreach (var c in Comentarios)
            {
                if (c.quem != quem || !Viu(c.coisa) || PlayerPrefs.GetInt(Visto(quem, c.coisa), 0) == 1) continue;
                PlayerPrefs.SetInt(Visto(quem, c.coisa), 1); PlayerPrefs.Save();
                return c.falas;
            }
            return null;
        }
        public static void Limpar()
        {
            foreach (var t in Todas) { PlayerPrefs.DeleteKey(Key(t)); foreach (var p in Pessoas) PlayerPrefs.DeleteKey(Visto(p, t)); }
        }
    }

    // The mission journal (icon next to the backpack): In progress / Completed, details and "follow".
    public sealed class DiarioDeMissoes : MonoBehaviour
    {
        RectTransform list, detail; Button tabOpen, tabDone; bool showDone; string selected; Text notice;

        public void Montar(RectTransform panel, UnityEngine.Events.UnityAction fechar)
        {
            var top = new Vector2(0, 1);
            RusticHudTheme.Label(panel, "Titulo", top, new Vector2(30, -18), new Vector2(520, 48), "DIÁRIO DE MISSÕES", 28).color = RusticHudTheme.Gold;
            tabOpen = RusticHudTheme.Button(panel, "EmAndamento", top, new Vector2(30, -76), new Vector2(220, 44), "EM ANDAMENTO", () => { showDone = false; selected = null; Abrir(); });
            tabDone = RusticHudTheme.Button(panel, "Concluidas", top, new Vector2(262, -76), new Vector2(200, 44), "CONCLUÍDAS", () => { showDone = true; selected = null; Abrir(); });
            var listBox = RusticHudTheme.Rect(panel, "Lista", top, new Vector2(30, -134), new Vector2(330, 420));
            RusticHudTheme.Panel(listBox, new Color(0, 0, 0, .25f), 12);
            list = RusticHudTheme.Rect(listBox, "Itens", top, new Vector2(10, -10), new Vector2(310, 400));
            var detailBox = RusticHudTheme.Rect(panel, "Detalhe", top, new Vector2(376, -134), new Vector2(574, 420));
            RusticHudTheme.Panel(detailBox, new Color(0, 0, 0, .25f), 12);
            detail = RusticHudTheme.Rect(detailBox, "Conteudo", top, new Vector2(18, -14), new Vector2(538, 392));
            notice = RusticHudTheme.Label(panel, "Aviso", new Vector2(0, 0), new Vector2(30, 22), new Vector2(640, 40), "", 17);
            notice.color = RusticHudTheme.Muted;
            RusticHudTheme.Button(panel, "Voltar", new Vector2(1, 0), new Vector2(-30, 20), new Vector2(220, 48), "VOLTAR", fechar);
        }

        static void Limpar(Transform t) { for (int i = t.childCount - 1; i >= 0; i--) Destroy(t.GetChild(i).gameObject); }

        public void Abrir()
        {
            if (list == null) return;
            Limpar(list); Limpar(detail); notice.text = "";
            var open = Missoes.EmAndamento.ToList(); var done = Missoes.Concluidas.ToList();
            tabOpen.GetComponentInChildren<Text>().text = "EM ANDAMENTO (" + open.Count + ")";
            tabDone.GetComponentInChildren<Text>().text = "CONCLUÍDAS (" + done.Count + ")";
            tabOpen.GetComponentInChildren<Text>().color = showDone ? RusticHudTheme.Muted : RusticHudTheme.Gold;
            tabDone.GetComponentInChildren<Text>().color = showDone ? RusticHudTheme.Gold : RusticHudTheme.Muted;
            var shown = showDone ? done : open;
            var top = new Vector2(0, 1);
            if (shown.Count == 0)
            {
                RusticHudTheme.Label(list, "Vazio", top, Vector2.zero, new Vector2(310, 80), showDone ? "Nenhuma missão concluída ainda." : "Nenhuma missão aceita.\nConverse com o pessoal do Refúgio.", 17).color = RusticHudTheme.Muted;
                return;
            }
            if (selected == null || !shown.Any(m => m.id == selected)) selected = shown[0].id;
            float y = 0; string grupo = null;
            foreach (var m in shown)
            {
                string g = m.tipo == Missoes.Tipo.Principal ? "PRINCIPAL" : "SECUNDÁRIAS";
                if (g != grupo) { grupo = g; RusticHudTheme.Label(list, "Grupo", top, new Vector2(4, -y), new Vector2(300, 26), g, 15).color = RusticHudTheme.Muted; y += 28; }
                var mission = m;
                string mark = !showDone && Missoes.Segue(m) ? "» " : "   ";
                var b = RusticHudTheme.Button(list, "Missao_" + m.id, top, new Vector2(0, -y), new Vector2(310, 46), mark + m.titulo, () => { selected = mission.id; Abrir(); });
                var t = b.GetComponentInChildren<Text>(); t.alignment = TextAnchor.MiddleLeft; t.fontSize = 18;
                t.color = m.id == selected ? Color.white : m.tipo == Missoes.Tipo.Principal ? RusticHudTheme.Gold : RusticHudTheme.Parchment;
                y += 52;
            }
            Detalhe(shown.First(m => m.id == selected));
        }

        void Detalhe(Missoes.Missao m)
        {
            var top = new Vector2(0, 1); float y = 0;
            Text Linha(string text, int size, Color color, float height)
            {
                var l = RusticHudTheme.Label(detail, "Linha", top, new Vector2(0, -y), new Vector2(538, height), text, size, TextAnchor.UpperLeft);
                l.color = color; l.supportRichText = true; l.horizontalOverflow = HorizontalWrapMode.Wrap; y += height; return l;
            }
            bool done = m.feita();
            Linha(m.titulo.ToUpperInvariant(), 24, m.tipo == Missoes.Tipo.Principal ? RusticHudTheme.Gold : RusticHudTheme.Parchment, 34);
            Linha((m.tipo == Missoes.Tipo.Principal ? "Principal" : "Secundária") + " · " + m.quem + (done ? " · <color=#8FD18F>concluída</color>" : ""), 16, RusticHudTheme.Muted, 30);
            Linha(m.descricao, 17, RusticHudTheme.Parchment, 70);
            Linha("OBJETIVOS", 16, RusticHudTheme.Gold, 26);
            foreach (var o in m.objetivos())
            {
                string count = o.precisa > 0 ? "  " + o.tem + "/" + o.precisa : "";
                Linha(o.feito || done ? "<color=#8FD18F>•  " + o.texto + count + "  (feito)</color>" : "•  " + o.texto + count, 17, RusticHudTheme.Parchment, 28);
            }
            y += 10;
            Linha("RECOMPENSA", 16, RusticHudTheme.Gold, 26);
            Linha(m.recompensa, 17, RusticHudTheme.Parchment, 30);
            if (done) return;
            bool following = Missoes.Segue(m);
            var mission = m;
            RusticHudTheme.Button(detail, "Seguir", new Vector2(0, 0), new Vector2(0, 0), new Vector2(300, 46), following ? "PARAR DE ACOMPANHAR" : "ACOMPANHAR", () =>
            {
                if (!Missoes.Alternar(mission)) { notice.text = "Você já acompanha " + Missoes.MaxSeguindo + " missões. Pare de acompanhar uma antes."; return; }
                Abrir();
            });
        }
    }
}

namespace ForjaDasMares.Networking
{
    // "!" / "?" floating over a person's head (see Missoes.Marcador). Faces the camera and bobs gently.
    public sealed class MarcadorDeMissao : MonoBehaviour
    {
        string quem; TextMesh sign, shadow; float next, baseY;
        public static void Colocar(Transform person, string who)
        {
            if (person == null || person.GetComponentInChildren<MarcadorDeMissao>() != null) return;
            float y = 2.7f;
            foreach (var t in person.GetComponentsInChildren<TextMesh>(true)) y = Mathf.Max(y, t.transform.position.y - person.position.y + .6f);
            var go = new GameObject("MarcadorDeMissao"); go.transform.SetParent(person, false); go.transform.position = person.position + Vector3.up * y;
            var m = go.AddComponent<MarcadorDeMissao>(); m.quem = who; m.baseY = go.transform.localPosition.y;
            m.shadow = m.Texto("Sombra", new Color(0, 0, 0, .7f), new Vector3(.03f, -.03f, .02f));
            m.sign = m.Texto("Sinal", Color.white, Vector3.zero);
        }
        TextMesh Texto(string name, Color c, Vector3 offset)
        {
            var t = new GameObject(name).AddComponent<TextMesh>(); t.transform.SetParent(transform, false); t.transform.localPosition = offset;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.font = font; t.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            t.fontSize = 96; t.characterSize = .06f; t.fontStyle = FontStyle.Bold; t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center; t.color = c;
            return t;
        }
        void LateUpdate()
        {
            if (Time.unscaledTime >= next)
            {
                next = Time.unscaledTime + .3f;
                var (s, c) = Missoes.Marcador(quem);
                sign.text = shadow.text = s; sign.color = c;
            }
            if (string.IsNullOrEmpty(sign.text)) return;
            var cam = Camera.main; if (cam != null) transform.rotation = cam.transform.rotation;
            var p = transform.localPosition; p.y = baseY + Mathf.Sin(Time.time * 2.4f) * .08f; transform.localPosition = p;
        }
    }
}
