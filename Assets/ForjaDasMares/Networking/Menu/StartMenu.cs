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
        public const string TestCreationKey = "ForjaTestarCriacao";

        GameObject loading, creation, menu, options, confirmDelete, accounts;
        RectTransform accountList;
        Text creationError, lookLabel, characterLabel, accountLabel, volumeLabel;
        InputField nameField;
        readonly Button[] genderButtons = new Button[2];
        int chosenLook = 2, chosenGender;
        // 3D preview of the character: its own camera draws into a picture shown on the menu.
        const int PreviewLayer = 31;
        Camera previewCamera; RenderTexture previewTexture; Transform previewStage; RawImage previewImage;
        readonly GameObject[] previewModels = new GameObject[2];
        float previewYaw, previewZoom = 1; float pinchStart = -1, pinchZoom = 1;
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
            Camera.main.cullingMask &= ~(1 << PreviewLayer);
            BuildInterface();
        }

        IEnumerator Start()
        {
            Show(loading);
            yield return null;
            AccountService.SignIn();
            yield return new WaitForSecondsRealtime(.6f);
            // Test from the editor (menu Forja/Personagem/Testar tela de criação): open creation even with a character.
            bool test = PlayerPrefs.GetInt(TestCreationKey, 0) == 1;
            if (test) { PlayerPrefs.DeleteKey(TestCreationKey); PlayerPrefs.Save(); }
            if (AccountService.HasCharacter && !test) OpenMenu(); else OpenCreation();
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

            BuildPreview(root);
            BuildCreation(root);
            BuildMenu(root);
            BuildOptions(root);
            BuildConfirmDelete(root);
            BuildAccounts(root);
        }

        void BuildCreation(RectTransform root)
        {
            creation = Page(root, "CriarPersonagem", true);
            Label(creation, "CRIAR PERSONAGEM", 65, 50, 900, 64, 38).color = RusticHudTheme.Gold;
            Label(creation, "Escolha seu expedicionário e dê um nome a ele.\nRoupas, chapéus e outros estilos você ganha jogando.", 65, 115, 560, 70, 19);
            Label(creation, "Personagem", 65, 205, 300, 30, 18).color = RusticHudTheme.Gold;
            genderButtons[0] = Button(creation, "Masculino", 65, 240, 270, "MASCULINO", () => ChooseGender(0));
            genderButtons[1] = Button(creation, "Feminino", 355, 240, 270, "FEMININO", () => ChooseGender(1));
            Label(creation, "Nome", 65, 330, 300, 30, 18).color = RusticHudTheme.Gold;
            nameField = Field(creation, "Nome", 65, 365, 560, "Nome do personagem", AccountService.MaxNameLength);
            lookLabel = Label(creation, "", 65, 440, 560, 40, 18);
            creationError = Label(creation, "", 65, 480, 560, 60, 18);
            creationError.color = new Color(1f, .62f, .55f);
            Button(creation, "CriarEEntrar", 65, 600, 560, "CRIAR E ENTRAR", ConfirmCreation);
            ChooseGender(chosenGender);
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
            Button(menu, "TrocarConta", 65, 564, 470, "TROCAR DE CONTA", OpenAccounts);
            accountLabel = Label(menu, "", 68, 640, 900, 60, 16);
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

        // ---------- Switch account (saved on this device) ----------
        void BuildAccounts(RectTransform root)
        {
            accounts = Page(root, "Contas", true);
            Label(accounts, "TROCAR DE CONTA", 65, 50, 900, 64, 38).color = RusticHudTheme.Gold;
            Label(accounts, "Contas salvas neste aparelho. Escolha uma ou crie uma nova para começar do zero.", 65, 115, 1100, 40, 19);
            accountList = RusticHudTheme.Rect(accounts.transform, "Lista", new Vector2(0, 1), new Vector2(65, -170), new Vector2(1150, 400));
            Button(accounts, "NovaConta", 65, 600, 540, "NOVA CONTA", () => { AccountService.NewAccount(); OpenCreation(); });
            Button(accounts, "VoltarContas", 675, 600, 540, "VOLTAR", OpenMenu);
        }

        void OpenAccounts()
        {
            foreach (Transform child in accountList) Destroy(child.gameObject);
            var list = AccountService.LocalAccounts();
            string active = AccountService.Peek()?.accountId;
            for (int i = 0; i < list.Length && i < 6; i++)
            {
                var account = list[i];
                string who = account.HasCharacter ? account.character.name + " · " + AccountService.Genders[Mathf.Clamp(account.character.gender, 0, 1)] : "sem personagem";
                string label = (account.accountId == active ? "▶ " : "") + who + "   (" + AccountService.ShortIdOf(account.accountId) + ")";
                string id = account.accountId;
                RusticHudTheme.Button(accountList, "Conta" + i, new Vector2(0, 1), new Vector2(i % 2 * 575, -(i / 2) * 75), new Vector2(560, 60), label, () => { AccountService.UseAccount(id); OpenMenu(); });
            }
            Show(accounts);
        }

        void ChooseGender(int index)
        {
            chosenGender = Mathf.Clamp(index, 0, 1);
            for (int i = 0; i < genderButtons.Length; i++)
            {
                var graphic = genderButtons[i] != null ? genderButtons[i].GetComponent<RoundedHudGraphic>() : null;
                if (graphic == null) continue;
                bool active = i == chosenGender;
                graphic.borderColor = active ? RusticHudTheme.Gold : RusticHudTheme.Copper;
                graphic.border = active ? 3f : 1.5f;
                graphic.SetVerticesDirty();
            }
            lookLabel.text = AccountService.Genders[chosenGender] + " · roupa inicial de expedicionário";
            ShowPreview(chosenGender);
        }

        // ---------- 3D preview (drag to turn, wheel or two fingers to zoom) ----------
        void BuildPreview(RectTransform root)
        {
            previewTexture = new RenderTexture(720, 960, 24, RenderTextureFormat.ARGB32) { name = "PreviewPersonagem", antiAliasing = 4 };
            previewStage = new GameObject("PalcoPersonagem").transform;
            previewStage.position = new Vector3(0, -500, 0);
            var cameraObject = new GameObject("CameraPersonagem");
            cameraObject.transform.SetParent(previewStage, false);
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor; previewCamera.backgroundColor = new Color(0, 0, 0, 0);
            previewCamera.cullingMask = 1 << PreviewLayer; previewCamera.targetTexture = previewTexture;
            previewCamera.fieldOfView = 26; previewCamera.nearClipPlane = .1f; previewCamera.farClipPlane = 30;
            var lightObject = new GameObject("LuzPersonagem");
            lightObject.transform.SetParent(previewStage, false);
            lightObject.transform.rotation = Quaternion.Euler(32, 155, 0);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.25f; light.color = new Color(1f, .95f, .88f); light.shadows = LightShadows.None;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.62f, .62f, .68f);

            var rect = RusticHudTheme.Rect(root, "PreviewPersonagem", new Vector2(0, 1), new Vector2(0, 0), new Vector2(540, 720));
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(300, -10);
            previewImage = rect.gameObject.AddComponent<RawImage>();
            previewImage.texture = previewTexture;
            var drag = rect.gameObject.AddComponent<PreviewDragArea>();
            drag.Dragged = dx => previewYaw -= dx * .45f;
            drag.Scrolled = dy => previewZoom = Mathf.Clamp(previewZoom - dy * .08f, .55f, 1.15f);
            Label(rect.gameObject, "Arraste para girar · role ou use dois dedos para aproximar", 0, 690, 540, 26, 14, TextAnchor.MiddleCenter).color = RusticHudTheme.Muted;
            rect.gameObject.SetActive(false);
        }

        void ShowPreview(int gender)
        {
            if (previewStage == null) return;
            for (int i = 0; i < previewModels.Length; i++)
            {
                if (previewModels[i] == null && i == gender)
                {
                    var prefab = Resources.Load<GameObject>("Personagens/" + (i == 0 ? "Masculino" : "Feminino"));
                    if (prefab == null) continue;
                    previewModels[i] = Instantiate(prefab, previewStage);
                    previewModels[i].transform.localPosition = Vector3.zero;
                    foreach (var t in previewModels[i].GetComponentsInChildren<Transform>(true)) t.gameObject.layer = PreviewLayer;
                    // Until real "standing" animations arrive, the body is kept alive by code (breathing, arms down, looking around).
                    foreach (var animator in previewModels[i].GetComponentsInChildren<Animator>(true)) animator.enabled = false;
                    previewModels[i].AddComponent<NpcIdle>();
                }
                if (previewModels[i] != null) previewModels[i].SetActive(i == gender);
            }
        }

        void Update()
        {
            if (previewCamera == null || previewImage == null || !previewImage.gameObject.activeInHierarchy) return;
            // Two fingers on the phone: pinch to zoom the preview.
            var screen = UnityEngine.InputSystem.Touchscreen.current;
            int pressed = 0; Vector2 a = default, b = default;
            if (screen != null) foreach (var touch in screen.touches) if (touch.press.isPressed) { if (pressed == 0) a = touch.position.ReadValue(); else if (pressed == 1) b = touch.position.ReadValue(); pressed++; }
            if (pressed >= 2) { float d = Vector2.Distance(a, b); if (pinchStart < 0) { pinchStart = d; pinchZoom = previewZoom; } else if (d > 1) previewZoom = Mathf.Clamp(pinchZoom * pinchStart / d, .55f, 1.15f); }
            else pinchStart = -1;
            foreach (var model in previewModels) if (model != null && model.activeSelf) model.transform.localRotation = Quaternion.Euler(0, 180 + previewYaw, 0);
            // Close: looks at the face; far: the whole body.
            float t = Mathf.InverseLerp(.55f, 1.15f, previewZoom);
            var look = new Vector3(0, Mathf.Lerp(1.4f, .92f, t), 0);
            previewCamera.transform.localPosition = look + new Vector3(0, .15f, Mathf.Lerp(-2.4f, -7.4f, t));
            previewCamera.transform.LookAt(previewStage.position + look);
        }

        void ConfirmCreation()
        {
            string error = AccountService.ValidateName(nameField.text);
            if (error != null) { creationError.text = error; return; }
            if (AccountService.CreateCharacter(nameField.text, chosenLook, chosenGender) == null) { creationError.text = "Não foi possível criar o personagem."; return; }
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
            characterLabel.text = character.name + "\n" + AccountService.Genders[Mathf.Clamp(character.gender, 0, AccountService.Genders.Length - 1)];
            accountLabel.text = "Conta de convidado neste aparelho · " + AccountService.ShortId +
                "\nEntrar com Google e salvar na nuvem: em breve.";
            Show(menu);
        }

        void Play()
        {
            if (travelling || !AccountService.HasCharacter) return;
            travelling = true;
            Show(loading);
            SceneManager.LoadSceneAsync(Historia.PrimeiraCena); // the home village until the prologue is done
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
            foreach (var each in new[] { loading, creation, menu, options, confirmDelete, accounts })
                if (each != null) each.SetActive(each == page);
            bool preview = page == creation;
            if (previewImage != null) { previewImage.gameObject.SetActive(preview); previewImage.transform.SetAsLastSibling(); }
            if (previewCamera != null) previewCamera.enabled = preview;
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
