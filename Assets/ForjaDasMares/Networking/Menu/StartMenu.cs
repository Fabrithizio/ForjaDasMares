using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // First scene of the game: enters the account automatically, creates the character when there is none,
    // then shows the main menu. Everything is built in code, so the scene only holds this component.
    public sealed class StartMenu : MonoBehaviour
    {
        public const string FirstGameScene = "RefugioUltimoSino";
        const string VolumeKey = "CampaignMasterVolume";

        GameObject loading, creation, menu, options, confirmDelete;
        Text creationError, lookLabel, characterLabel, accountLabel, volumeLabel;
        InputField nameField;
        readonly Image[] lookFrames = new Image[4];
        int chosenLook = 2;
        bool travelling;

        void Awake()
        {
            AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, .8f));
            if (Camera.main == null)
            {
                var cameraObject = new GameObject("MenuCamera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                var view = cameraObject.GetComponent<Camera>();
                view.clearFlags = CameraClearFlags.SolidColor;
                view.backgroundColor = new Color(.02f, .04f, .06f);
            }
            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            BuildInterface();
        }

        IEnumerator Start()
        {
            Show(loading);
            yield return null;
            AccountService.SignIn();
            yield return new WaitForSecondsRealtime(.6f);
            if (AccountService.HasCharacter) OpenMenu(); else OpenCreation();
        }

        void BuildInterface()
        {
            var canvasObject = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = .5f;
            var root = (RectTransform)canvasObject.transform;
            MenuArtwork.Fill(root, "Fundo", "MenuArt/RoutesBackdrop", Color.white);
            var shade = RusticHudTheme.Rect(root, "Sombra", Vector2.zero, Vector2.zero, Vector2.zero);
            shade.anchorMax = Vector2.one; shade.offsetMin = shade.offsetMax = Vector2.zero;
            var shadeImage = shade.gameObject.AddComponent<Image>(); shadeImage.color = new Color(0, 0, 0, .35f); shadeImage.raycastTarget = false;

            loading = Page(root, "Carregando", false);
            Label(loading, "FORJA\nDAS MARÉS", 0, 250, 1280, 160, 60, TextAnchor.MiddleCenter);
            Label(loading, "Carregando...", 0, 430, 1280, 50, 22, TextAnchor.MiddleCenter).color = RusticHudTheme.Gold;

            BuildCreation(root);
            BuildMenu(root);
            BuildOptions(root);
            BuildConfirmDelete(root);
        }

        void BuildCreation(RectTransform root)
        {
            creation = Page(root, "CriarPersonagem", true);
            Label(creation, "CRIAR PERSONAGEM", 65, 50, 900, 64, 38).color = RusticHudTheme.Gold;
            Label(creation, "Escolha seu nome e sua aparência. A arma que você usa define como você luta.", 65, 115, 1150, 45, 20);
            nameField = Field(creation, "Nome", 65, 175, 560, "Nome do personagem", AccountService.MaxNameLength);
            var cards = Resources.Load<Texture2D>("MenuArt/ClassCards");
            float[] left = { .045f, .107f, .171f, .234f };
            for (int i = 0; i < AccountService.Looks.Length; i++)
            {
                int index = i;
                float x = 65 + i * 205;
                var frame = RusticHudTheme.Rect(creation.transform, "Aparencia" + i, new Vector2(0, 1), new Vector2(x, -260), new Vector2(185, 200));
                lookFrames[i] = frame.gameObject.AddComponent<Image>();
                lookFrames[i].raycastTarget = false;
                var portraitRect = RusticHudTheme.Rect(frame, "Retrato", new Vector2(.5f, .5f), Vector2.zero, new Vector2(170, 185));
                var portrait = portraitRect.gameObject.AddComponent<RawImage>();
                portrait.texture = cards;
                portrait.uvRect = new Rect(left[i], .686f, .057f, .147f);
                portrait.raycastTarget = false;
                Button(creation, "EscolherAparencia" + i, x, 470, 185, AccountService.Looks[i], () => ChooseLook(index));
            }
            lookLabel = Label(creation, "", 65, 540, 820, 40, 19);
            creationError = Label(creation, "", 65, 580, 820, 40, 18);
            creationError.color = new Color(1f, .62f, .55f);
            Button(creation, "CriarEEntrar", 900, 610, 315, "CRIAR E ENTRAR", ConfirmCreation);
            ChooseLook(chosenLook);
        }

        void BuildMenu(RectTransform root)
        {
            menu = Page(root, "Menu", false);
            Label(menu, "FORJA\nDAS MARÉS", 64, 62, 640, 155, 62);
            Label(menu, "O MUNDO ALÉM DO REFÚGIO", 68, 220, 550, 38, 21).color = RusticHudTheme.Gold;
            characterLabel = Label(menu, "", 68, 262, 700, 70, 20);
            Button(menu, "Jogar", 65, 345, 470, "JOGAR", Play);
            Button(menu, "Configuracoes", 65, 418, 225, "CONFIGURAÇÕES", () => Show(options));
            Button(menu, "Sair", 310, 418, 225, "SAIR", Application.Quit);
            Button(menu, "ApagarPersonagem", 65, 491, 470, "APAGAR PERSONAGEM", () => Show(confirmDelete));
            accountLabel = Label(menu, "", 68, 600, 900, 70, 16);
            accountLabel.color = RusticHudTheme.Muted;
            var badge = RusticHudTheme.Rect(menu.transform, "Versao", new Vector2(1, 0), new Vector2(-40, 40), new Vector2(310, 65));
            RusticHudTheme.Panel(badge, new Color(.025f, .05f, .055f, .85f), 16);
            RusticHudTheme.Label(badge, "Texto", Vector2.one * .5f, Vector2.zero, new Vector2(280, 60), "Versão " + Application.version, 17, TextAnchor.MiddleCenter);
        }

        void BuildOptions(RectTransform root)
        {
            options = Page(root, "Configuracoes", true);
            Label(options, "CONFIGURAÇÕES", 50, 40, 1100, 70, 34);
            Label(options, "Volume geral", 50, 165, 500, 60, 24);
            Button(options, "Menos", 50, 235, 240, "DIMINUIR", () => Volume(-.1f));
            Button(options, "Mais", 320, 235, 240, "AUMENTAR", () => Volume(.1f));
            volumeLabel = Label(options, "", 600, 235, 400, 60, 24);
            Label(options, "Mais opções (gráficos, música, controles) continuam no menu dentro do jogo.", 50, 330, 1150, 45, 18);
            Button(options, "Voltar", 50, 620, 500, "VOLTAR", OpenMenu);
            Volume(0);
        }

        void BuildConfirmDelete(RectTransform root)
        {
            confirmDelete = Page(root, "ConfirmarApagar", true);
            Label(confirmDelete, "APAGAR PERSONAGEM?", 150, 170, 1000, 80, 36).color = RusticHudTheme.Gold;
            Label(confirmDelete, "Todo o progresso dele será perdido: itens, ouro, nível e história.\nIsso não pode ser desfeito.", 150, 260, 1000, 90, 22);
            Button(confirmDelete, "Cancelar", 150, 420, 430, "CANCELAR", OpenMenu);
            Button(confirmDelete, "Confirmar", 690, 420, 430, "APAGAR", () => { AccountService.DeleteCharacter(); OpenCreation(); });
        }

        void ChooseLook(int index)
        {
            chosenLook = Mathf.Clamp(index, 0, AccountService.Looks.Length - 1);
            for (int i = 0; i < lookFrames.Length; i++)
                if (lookFrames[i] != null) lookFrames[i].color = i == chosenLook ? RusticHudTheme.Gold : new Color(.08f, .12f, .14f, .9f);
            lookLabel.text = "Aparência: " + AccountService.Looks[chosenLook] + " · arma inicial: " + AccountService.LookWeapons[chosenLook];
        }

        void ConfirmCreation()
        {
            string error = AccountService.ValidateName(nameField.text);
            if (error != null) { creationError.text = error; return; }
            if (AccountService.CreateCharacter(nameField.text, chosenLook) == null) { creationError.text = "Não foi possível criar o personagem."; return; }
            Play();
        }

        void OpenCreation()
        {
            nameField.text = "";
            creationError.text = "";
            Show(creation);
        }

        void OpenMenu()
        {
            var character = AccountService.Character;
            if (character == null) { OpenCreation(); return; }
            characterLabel.text = character.name + "\n" + AccountService.Looks[Mathf.Clamp(character.look, 0, AccountService.Looks.Length - 1)];
            accountLabel.text = "Conta de convidado neste aparelho · " + AccountService.ShortId +
                "\nEntrar com Google e salvar na nuvem: em breve.";
            Show(menu);
        }

        void Play()
        {
            if (travelling || !AccountService.HasCharacter) return;
            travelling = true;
            Show(loading);
            SceneManager.LoadSceneAsync(FirstGameScene);
        }

        void Volume(float step)
        {
            AudioListener.volume = Mathf.Clamp01(AudioListener.volume + step);
            PlayerPrefs.SetFloat(VolumeKey, AudioListener.volume);
            PlayerPrefs.Save();
            if (volumeLabel != null) volumeLabel.text = Mathf.RoundToInt(AudioListener.volume * 100) + "%";
        }

        void Show(GameObject page)
        {
            foreach (var each in new[] { loading, creation, menu, options, confirmDelete })
                if (each != null) each.SetActive(each == page);
        }

        static GameObject Page(Transform parent, string name, bool panel)
        {
            var rect = RusticHudTheme.Rect(parent, name, Vector2.one * .5f, Vector2.zero, new Vector2(1280, 720));
            if (panel) RusticHudTheme.Panel(rect, new Color(.025f, .05f, .045f, .96f), 25);
            return rect.gameObject;
        }

        static Text Label(GameObject page, string value, float x, float y, float width, float height, int font, TextAnchor align = TextAnchor.MiddleLeft) =>
            RusticHudTheme.Label(page.transform, "Texto", new Vector2(0, 1), new Vector2(x, -y), new Vector2(width, height), value, font, align);

        static Button Button(GameObject page, string name, float x, float y, float width, string text, UnityEngine.Events.UnityAction action) =>
            RusticHudTheme.Button(page.transform, name, new Vector2(0, 1), new Vector2(x, -y), new Vector2(width, 60), text, action);

        static InputField Field(GameObject page, string name, float x, float y, float width, string placeholder, int limit)
        {
            var rect = RusticHudTheme.Rect(page.transform, name, new Vector2(0, 1), new Vector2(x, -y), new Vector2(width, 60));
            rect.gameObject.AddComponent<Image>().color = RusticHudTheme.Surface;
            var field = rect.gameObject.AddComponent<InputField>();
            field.textComponent = Label(rect.gameObject, "", 12, 0, width - 24, 60, 22);
            var hint = Label(rect.gameObject, placeholder, 12, 0, width - 24, 60, 18);
            hint.color = RusticHudTheme.Muted;
            field.placeholder = hint;
            field.characterLimit = limit;
            return field;
        }
    }
}
