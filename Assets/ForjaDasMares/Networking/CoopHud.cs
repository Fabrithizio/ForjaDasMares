using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    [DefaultExecutionOrder(250)] public sealed class CoopHud : MonoBehaviour
    {
        [SerializeField] CoopDirector session;
        [SerializeField] ArenaInput controls;
        Text status, party; InputField address, pin, port;
        Button host, join, start, hero; RectTransform root; GameObject menu, touch;
        float refresh; readonly StringBuilder text = new StringBuilder(512);
        Font font;int previousSleep;
        CoopCloud cloud; InputField cloudCode; Text cloudStatus; Button cloudHost, cloudJoin;
        public RectTransform SafeRoot => root;
        readonly System.Collections.Generic.List<RaycastResult> pointerHits = new System.Collections.Generic.List<RaycastResult>(16);
        PointerEventData pointerData;
        EventSystem pointerEvents;
        public void Configure(CoopDirector director, ArenaInput input) { session = director; controls = input; }
        void Start()
        {
            (gameObject.GetComponent<PresentationQuality>()??gameObject.AddComponent<PresentationQuality>()).Configure(Camera.main);
            previousSleep=Screen.sleepTimeout;if(Application.isMobilePlatform)Screen.sleepTimeout=SleepTimeout.NeverSleep;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            root = new GameObject("SafeArea", typeof(RectTransform)).GetComponent<RectTransform>(); root.SetParent(transform, false);
            var layout = GetComponent<CanvasScaler>(); layout.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; layout.referenceResolution = new Vector2(1280, 720); layout.matchWidthOrHeight = .5f;
            if(session.Campaign!=null)
            {
                layout.matchWidthOrHeight=1;
                gameObject.AddComponent<CampaignHud>().Build(root,session,controls);controls.PointerBlocked=PointerOverUi;return;
            }
            menu = Panel(root, "SessionMenu", new Vector2(340, 390), new Vector2(15, -15));
            Label(menu.transform, "FORJA DAS MARÉS — COOP", new Vector2(310, 40), new Vector2(15, -8), 21);
            address = Field(menu.transform, "127.0.0.1", new Vector2(15, -58), 195); port = Field(menu.transform, "7777", new Vector2(220, -58), 100);
            pin = Field(menu.transform, "TEST42", new Vector2(15, -108), 305);
            hero = Button(menu.transform, "HERÓI", new Vector2(15, -158), 305, () => session.SelectHero((session.SelectedHero + 1) % session.HeroCount));
            host = Button(menu.transform, "CRIAR SALA LOCAL", new Vector2(15, -210), 305, () => { if (ushort.TryParse(port.text, out ushort value)) session.HostDirect(pin.text, value); });
            join = Button(menu.transform, "ENTRAR POR IP E CÓDIGO", new Vector2(15, -262), 305, () => { if (ushort.TryParse(port.text, out ushort value)) session.JoinDirect(address.text, pin.text, value); });
            start = Button(menu.transform, "INICIAR PARTIDA", new Vector2(15, -314), 305, session.StartGame);
            status = Label(root, "", new Vector2(870, 110), new Vector2(385, -15), 20);
            party = Label(root, "", new Vector2(870, 130), new Vector2(385, -132), 20);
            TopRight("ENCERRAR / SAIR", -15, session.Close);
            TopRight("JOGAR LOCAL", -67, () => { session.Close(); UnityEngine.SceneManagement.SceneManager.LoadScene("ProgressionDungeon"); });
            touch = new GameObject("TouchControls", typeof(RectTransform)); touch.transform.SetParent(root, false); var tr = (RectTransform)touch.transform; tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
            var stick = new GameObject("MoveStick", typeof(RectTransform), typeof(Image), typeof(TouchMoveStick)); stick.transform.SetParent(touch.transform, false);
            var sr = (RectTransform)stick.transform; sr.anchorMin = sr.anchorMax = new Vector2(0, 0); sr.pivot = Vector2.one * .5f; sr.anchoredPosition = new Vector2(112, 115); sr.sizeDelta = new Vector2(160, 160); stick.GetComponent<Image>().color = new Color(.2f, .4f, .5f, .5f);
            var thumb = new GameObject("Thumb", typeof(RectTransform), typeof(Image)); thumb.transform.SetParent(stick.transform, false); var th = (RectTransform)thumb.transform; th.sizeDelta = new Vector2(52, 52); thumb.GetComponent<Image>().raycastTarget = false; thumb.GetComponent<Image>().color = new Color(.7f, .9f, 1, .7f); stick.GetComponent<TouchMoveStick>().Configure(controls, th);
            TouchButton("ATAQUE", new Vector2(-150, 55), HeroButtons.Attack); TouchButton("ESQUIVA", new Vector2(-300, 55), HeroButtons.Dodge); TouchButton("HABILIDADE", new Vector2(-150, 130), HeroButtons.Special); TouchButton("INTERAGIR", new Vector2(-300, 130), HeroButtons.Interact);
            touch.SetActive(Application.isMobilePlatform); controls.PointerBlocked = PointerOverUi;
            cloud = session.GetComponent<CoopCloud>();
            cloudHost = Button(root, "CRIAR SALA INTERNET", new Vector2(385, -280), 280, () => cloud.Create());
            cloudJoin = Button(root, "ENTRAR POR CÓDIGO", new Vector2(690, -280), 280, () => cloud.Join(cloudCode.text));
            cloudCode = Field(root, "", new Vector2(385, -332), 280);
            cloudStatus = Label(root, "", new Vector2(870, 95), new Vector2(385, -385), 18);
        }
        bool PointerOverUi(Vector2 position)
        {
            var events = EventSystem.current; if (events == null) return false;
            if (pointerData == null || pointerEvents != events) { pointerEvents = events; pointerData = new PointerEventData(events); }
            pointerData.Reset(); pointerData.position = position; pointerHits.Clear(); events.RaycastAll(pointerData, pointerHits);
            return pointerHits.Count > 0;
        }
        void TopRight(string label, float y, UnityEngine.Events.UnityAction action)
        { var rect = (RectTransform)Button(root, label, Vector2.zero, 190, action).transform; rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one; rect.anchoredPosition = new Vector2(-15, y); }
        void TouchButton(string name, Vector2 offset, HeroButtons action)
        { var button = Button(touch.transform, name, Vector2.zero, 135, () => session.Request(action)); var rect = (RectTransform)button.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 0); rect.anchoredPosition = offset; rect.sizeDelta = new Vector2(135, 65); }
        void Update()
        {
            if (root == null) return;
            Rect safe = Screen.safeArea; root.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height); root.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height); root.offsetMin = root.offsetMax = Vector2.zero;
            if(session.Campaign!=null)return;
            refresh -= Time.unscaledDeltaTime; if (refresh > 0) return; refresh = .15f;
            bool disconnected = session.Phase == CoopPhase.Offline || session.Phase == CoopPhase.Closed;
            host.interactable = join.interactable = disconnected && !session.Manager.IsListening;
            start.interactable = session.IsHost && session.Phase == CoopPhase.Lobby;
            hero.interactable = disconnected || session.Phase == CoopPhase.Lobby;
            hero.GetComponentInChildren<Text>().text = "HERÓI: " + session.HeroName;
            menu.SetActive(session.Phase != CoopPhase.Playing);
            status.text = session.Message + "\n" + session.Phase + " | jogadores " + session.ConnectedCount + "/4" + (session.IsHost ? " | código " + session.Pin : "");
            text.Clear();
            for (int i = 0; i < 4; i++) { var actor = session.Actor(i); if (actor == null || !actor.gameObject.activeSelf) continue; text.Append(i == session.LocalSlot ? "VOCÊ " : "ALIADO ").Append(i + 1).Append(": ").Append(actor.Roster.Current.displayName).Append("  ").Append(actor.Health.Current).Append('/').Append(actor.Health.Maximum).Append('\n'); }
            if (session.Dungeon.CurrentRoom != null) text.Append("Sala ").Append(session.Dungeon.CurrentRoom.node.id + 1).Append(" — ").Append(session.Dungeon.CurrentRoom.node.category);
            party.text = text.ToString();
            cloudHost.gameObject.SetActive(disconnected); cloudJoin.gameObject.SetActive(disconnected); cloudCode.gameObject.SetActive(disconnected); cloudStatus.gameObject.SetActive(disconnected);
            cloudHost.interactable = cloudJoin.interactable = disconnected && !cloud.Busy && !session.Manager.IsListening;
            cloudStatus.text = cloud.Message;
        }
        GameObject Panel(Transform parent, string name, Vector2 size, Vector2 offset)
        { var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = offset; rect.sizeDelta = size; go.GetComponent<Image>().color = new Color(.03f, .08f, .12f, .94f); return go; }
        Text Label(Transform parent, string value, Vector2 size, Vector2 offset, int fontSize = 18)
        { var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = offset; rect.sizeDelta = size; var label = go.GetComponent<Text>(); label.font = font; label.fontSize = fontSize; label.text = value; label.color = Color.white; label.raycastTarget = false; return label; }
        Button Button(Transform parent, string value, Vector2 offset, float width, UnityEngine.Events.UnityAction action)
        { var go = Panel(parent, value, new Vector2(width, 44), offset); go.GetComponent<Image>().color = new Color(.12f, .3f, .4f); var button = go.AddComponent<Button>(); button.onClick.AddListener(action); var label = Label(go.transform, value, new Vector2(width, 44), Vector2.zero); label.alignment = TextAnchor.MiddleCenter; return button; }
        InputField Field(Transform parent, string value, Vector2 offset, float width)
        { var go = Panel(parent, "Input", new Vector2(width, 40), offset); go.GetComponent<Image>().color = new Color(.12f, .17f, .2f); var label = Label(go.transform, value, new Vector2(width - 12, 40), new Vector2(6, 0)); label.alignment = TextAnchor.MiddleLeft; var field = go.AddComponent<InputField>(); field.textComponent = label; field.characterLimit = 48; field.text = value; return field; }
        void OnDestroy() { if(Application.isMobilePlatform)Screen.sleepTimeout=previousSleep; if (controls != null) controls.PointerBlocked = null; }
    }
}
