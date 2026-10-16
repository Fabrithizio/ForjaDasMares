using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace ForjaDasMares
{
    // Presentation only: reuses existing actions, commands and progression windows.
    public sealed class RusticDungeonHud : MonoBehaviour
    {
        DungeonSession session;
        HeroCombat combat;
        PlayerMotor motor;
        Canvas canvas;
        ProgressionHud inventory;
        ProgressionSceneBridge bridge;
        RectTransform safe, menu, tools, healthFill, xpFill, bossFill;
        Text heroName, healthText, levelText, location, objective, money, loot, prompt, bossName, specialTime, dodgeTime;
        GameObject bossPanel;
        GameObject mobileControls;
        HudIcon portrait;
        Button interact;
        readonly Dictionary<string, Button> legacy = new Dictionary<string, Button>();
        float refresh, lootUntil;
        int lastRunCoins, lastRunItems;
        Text[] labels;
        int[] baseSizes;
        bool largeText;
        RectTransform superFill;
        Text superLabel, chargeLabel;
        Button superButton;
        ArenaInput input;
        public RectTransform SafeRoot => safe;
        public bool MenuOpen => menu != null && menu.gameObject.activeSelf;
        public void Configure(DungeonSession game, Canvas target, ProgressionHud progression)
        {
            session = game; canvas = target; inventory = progression; bridge = progression != null ? progression.Bridge : null;
            combat = session.Heroes.GetComponent<HeroCombat>(); motor = session.Heroes.GetComponent<PlayerMotor>();
            input = session.Heroes.GetComponent<ArenaInput>();
            foreach (var button in canvas.GetComponentsInChildren<Button>(true)) legacy[button.name] = button;
            Build(); labels = safe.GetComponentsInChildren<Text>(true); baseSizes = new int[labels.Length];
            for (int i = 0; i < labels.Length; i++) baseSizes[i] = labels[i].fontSize;
            UpdateSafeArea(); Refresh();
        }
        void Hide(string name) { var item = canvas.transform.Find(name); if (item != null) item.gameObject.SetActive(false); }
        void Register(RectTransform rect) { if (inventory != null) inventory.RegisterHitArea(rect); }
        Button Action(Transform parent,string name,Vector2 anchor,Vector2 position,Vector2 size,string label,UnityAction action)
        { var button=RusticHudTheme.Button(parent,name,anchor,position,size,label,action);Register((RectTransform)button.transform);return button; }
        void Existing(string name)
        {
            menu.gameObject.SetActive(false); tools.gameObject.SetActive(false);
            if(name=="Inventory"&&bridge!=null&&!bridge.CanManage)
            {loot.text="Termine o combate para organizar a mochila";lootUntil=Time.unscaledTime+4;return;}
            if(legacy.TryGetValue(name,out var button))button.onClick.Invoke();
        }
        Button Shortcut(string name,string label,HudSymbol icon,float x,UnityAction action)
        {
            var button=Action(safe,name,Vector2.one,new Vector2(x,-18),new Vector2(62,54),"",action);
            RusticHudTheme.Icon(button.transform,icon,Vector2.one*.5f,new Vector2(0,5),25);
            RusticHudTheme.Label(button.transform,"Caption",new Vector2(.5f,0),new Vector2(0,3),new Vector2(58,17),label,11,TextAnchor.MiddleCenter);return button;
        }
        Text Label(Transform parent,string name,Vector2 anchor,Vector2 position,Vector2 size,string text,int font=16,TextAnchor align=TextAnchor.MiddleLeft)
            => RusticHudTheme.Label(parent,name,anchor,position,size,text,font,align);
        RectTransform Bar(Transform parent,string name,Vector2 position,Vector2 size,Color color)
        {
            var background=RusticHudTheme.Rect(parent,name+"Track",new Vector2(0,1),position,size);
            var track=RusticHudTheme.Panel(background,new Color(.025f,.035f,.028f),size.y*.5f);track.border=.5f;
            var fill=RusticHudTheme.Rect(background,name,new Vector2(0,.5f),new Vector2(2,0),new Vector2(size.x-4,Mathf.Max(2,size.y-4)));
            var graphic=RusticHudTheme.Panel(fill,color,size.y*.5f);graphic.border=0;return fill;
        }
        void Build()
        {
            safe=RusticHudTheme.Rect(canvas.transform,"RusticSafeArea",Vector2.zero,Vector2.zero,Vector2.zero);
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            foreach(string name in new[]{"Title","HeroStatus","EncounterStatus","ControlsHelp","ProgressionStatus","InteractionPrompt","BossBar","Biome","Seed","Fixed","Arena","Inventory","RunResult","ProfileOptions","ResetArenaButton","NextHero","Interact"})Hide(name);
            var card=RusticHudTheme.Rect(safe,"HeroCard",new Vector2(0,1),new Vector2(18,-18),new Vector2(308,106));RusticHudTheme.Panel(card,RusticHudTheme.Ink,18);
            var emblem=RusticHudTheme.Rect(card,"HeroEmblem",new Vector2(0,1),new Vector2(14,-15),new Vector2(54,64));RusticHudTheme.Panel(emblem,RusticHudTheme.Surface,24);
            portrait=RusticHudTheme.Icon(emblem,HudSymbol.Hero,Vector2.one*.5f,Vector2.zero,38);
            heroName=Label(card,"HeroName",new Vector2(0,1),new Vector2(80,-10),new Vector2(215,25),"",17);
            heroName.resizeTextForBestFit=true;heroName.resizeTextMinSize=12;heroName.resizeTextMaxSize=17;
            levelText=Label(card,"Level",new Vector2(0,1),new Vector2(80,-35),new Vector2(210,18),"",12);levelText.color=RusticHudTheme.Muted;
            healthFill=Bar(card,"HealthFill",new Vector2(80,-57),new Vector2(212,15),new Color(.61f,.20f,.16f));
            healthText=Label(card,"Health",new Vector2(0,1),new Vector2(80,-77),new Vector2(212,20),"",14);
            xpFill=Bar(card,"ExperienceFill",new Vector2(15,-98),new Vector2(278,4),RusticHudTheme.Gold);
            if(combat.EnhancedControls)
            {
                var power=RusticHudTheme.Rect(safe,"PowerCard",new Vector2(0,1),new Vector2(18,-132),new Vector2(308,54));RusticHudTheme.Panel(power,RusticHudTheme.Ink,14);
                superLabel=Label(power,"SuperLabel",new Vector2(0,1),new Vector2(14,-7),new Vector2(280,23),"",13);
                superFill=Bar(power,"SuperFill",new Vector2(14,-37),new Vector2(280,8),new Color(1,.57f,.14f));
                chargeLabel=Label(safe,"ChargeLabel",new Vector2(0,1),new Vector2(24,-190),new Vector2(310,25),"Segure ataque: combo + golpe pesado",12);
            }
            var quest=RusticHudTheme.Rect(safe,"ExpeditionCard",new Vector2(.5f,1),new Vector2(0,-18),new Vector2(340,74));RusticHudTheme.Panel(quest,RusticHudTheme.Ink);
            RusticHudTheme.Icon(quest,HudSymbol.Compass,new Vector2(0,.5f),new Vector2(12,0),28);
            location=Label(quest,"Location",new Vector2(0,1),new Vector2(52,-10),new Vector2(275,22),"",15);location.color=RusticHudTheme.Gold;
            objective=Label(quest,"Objective",new Vector2(0,1),new Vector2(52,-36),new Vector2(275,28),"",13);
            Shortcut("HudInventory","Mochila",HudSymbol.Bag,-158,()=>Existing("Inventory")).gameObject.SetActive(legacy.ContainsKey("Inventory"));
            Shortcut("HudHero","Herói",HudSymbol.Hero,-88,()=>Existing("NextHero"));
            Shortcut("HudMenu","Menu",HudSymbol.Menu,-18,ToggleMenu);
            money=Label(safe,"Gold",Vector2.one,new Vector2(-18,-82),new Vector2(210,24),"",15,TextAnchor.MiddleRight);money.color=RusticHudTheme.Gold;
            loot=Label(safe,"LootToast",new Vector2(.5f,0),new Vector2(0,148),new Vector2(470,28),"",16,TextAnchor.MiddleCenter);loot.color=RusticHudTheme.Gold;
            prompt=Label(safe,"ContextPrompt",new Vector2(.5f,0),new Vector2(0,185),new Vector2(470,32),"",16,TextAnchor.MiddleCenter);
            menu=RusticHudTheme.Rect(safe,"AdventureMenu",Vector2.one,new Vector2(-18,-120),new Vector2(260,304));RusticHudTheme.Panel(menu,RusticHudTheme.Ink,18);Register(menu);
            Label(menu,"Heading",new Vector2(0,1),new Vector2(18,-12),new Vector2(220,28),"EXPEDIÇÃO",16).color=RusticHudTheme.Gold;
            string[] names={"Inventory","RunResult","ProfileOptions","Arena"};string[] labels={"Inventário e equipamentos","Resultado da expedição","Configurações","Voltar à arena de teste"};
            for(int i=0;i<names.Length;i++){string id=names[i];Action(menu,"Menu_"+id,new Vector2(0,1),new Vector2(14,-50-i*48),new Vector2(232,42),labels[i],()=>Existing(id)).gameObject.SetActive(legacy.ContainsKey(id));}
            Action(menu,"TestTools",new Vector2(0,1),new Vector2(14,-248),new Vector2(232,40),"Ferramentas de teste",()=>{menu.gameObject.SetActive(false);tools.gameObject.SetActive(true);});
            tools=RusticHudTheme.Rect(safe,"TestToolsPanel",Vector2.one,new Vector2(-18,-120),new Vector2(260,294));RusticHudTheme.Panel(tools,RusticHudTheme.Ink,18);Register(tools);
            Label(tools,"Heading",new Vector2(0,1),new Vector2(18,-12),new Vector2(220,28),"TESTE DO MAPA",16).color=RusticHudTheme.Gold;
            names=new[]{"Biome","Seed","Fixed","ResetArenaButton"};labels=new[]{"Trocar bioma","Gerar outra seed","Alternar fixo / procedural","Reiniciar teste"};
            for(int i=0;i<names.Length;i++){string id=names[i];Action(tools,"Tool_"+id,new Vector2(0,1),new Vector2(14,-50-i*48),new Vector2(232,42),labels[i],()=>Existing(id));}
            Action(tools,"CloseTools",new Vector2(0,1),new Vector2(14,-244),new Vector2(232,36),"Voltar",()=>{tools.gameObject.SetActive(false);menu.gameObject.SetActive(true);});menu.gameObject.SetActive(false);tools.gameObject.SetActive(false);
            BuildControls();
            bossPanel=RusticHudTheme.Rect(safe,"BossHealthCard",new Vector2(.5f,1),new Vector2(0,-107),new Vector2(400,58)).gameObject;RusticHudTheme.Panel((RectTransform)bossPanel.transform,RusticHudTheme.Ink);
            bossName=Label(bossPanel.transform,"BossName",new Vector2(0,1),new Vector2(15,-6),new Vector2(370,24),"",15,TextAnchor.MiddleCenter);
            bossFill=Bar(bossPanel.transform,"BossHealth",new Vector2(15,-35),new Vector2(370,12),new Color(.67f,.25f,.12f));bossPanel.SetActive(false);
            safe.SetAsLastSibling();
            // Existing inventory overlays must stay above all HUD panels.
            var overlay=canvas.transform.Find("InventoryOverlay");if(overlay!=null)overlay.SetAsLastSibling();
        }
        void BuildControls()
        {
            bool mobile=Application.isMobilePlatform;
            var touch=canvas.transform.Find("TouchControls_Minimal");
            if(touch!=null)
            {
                mobileControls=touch.gameObject;
                touch.SetParent(safe,false);var rect=(RectTransform)touch;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;touch.gameObject.SetActive(mobile);
                if(mobile)
                {
                    var stick=(RectTransform)touch.Find("TouchMoveStick");Position(stick,Vector2.zero,new Vector2(114,114),new Vector2(164,164),Vector2.one*.5f);
                    stick.GetComponent<Image>().enabled=false;var ring=RusticHudTheme.Panel(stick,new Color(.08f,.1f,.08f,.48f),82);ring.border=2;ring.raycastTarget=true;
                    var text=stick.Find("MoveLabel");if(text!=null)text.gameObject.SetActive(false);
                    var thumb=(RectTransform)stick.Find("StickThumb");thumb.GetComponent<Image>().enabled=false;var knob=RusticHudTheme.Panel(thumb,new Color(.47f,.41f,.28f,.84f),28);knob.raycastTarget=false;
                    StyleTouch((RectTransform)touch.Find("TouchAttack"),new Vector2(-100,100),98,HudSymbol.Sword,"Ataque");
                    ConfigureHold(touch.Find("TouchAttack").gameObject);
                    StyleTouch((RectTransform)touch.Find("TouchDodge"),new Vector2(-212,72),78,HudSymbol.Dodge,"Esquiva");
                    StyleTouch((RectTransform)touch.Find("TouchSpecial"),new Vector2(-210,174),78,HudSymbol.Rune,"Habilidade");
                }
            }
            if(mobile)
            {
                if(combat.EnhancedControls)
                {
                    superButton=Action(safe,"SuperAttack",new Vector2(1,0),new Vector2(-310,110),new Vector2(78,78),"SUPER",()=>input.RequestSuper());
                    StyleTouch((RectTransform)superButton.transform,new Vector2(-310,110),78,HudSymbol.Rune,"SUPER");
                }
                if(legacy.TryGetValue("Interact",out interact))
                { interact.transform.SetParent(safe,false);interact.gameObject.SetActive(true);StyleTouch((RectTransform)interact.transform,new Vector2(-96,219),66,HudSymbol.Interact,"Interagir");Register((RectTransform)interact.transform); }
            }
            else
            {
                var dock=RusticHudTheme.Rect(safe,"ActionDock",new Vector2(.5f,0),new Vector2(0,18),new Vector2(combat.EnhancedControls?426:324,88));RusticHudTheme.Panel(dock,RusticHudTheme.Ink,20);
                var attackButton=PcAction(dock,"BasicAttack",HudSymbol.Sword,"Segure","Mouse",18,()=>combat.TryAttack());
                attackButton.onClick.RemoveAllListeners();ConfigureHold(attackButton.gameObject);
                PcAction(dock,"Special",HudSymbol.Rune,"Habilidade","E",120,()=>combat.TrySpecial());
                PcAction(dock,"Dodge",HudSymbol.Dodge,"Esquiva","Espaço",222,()=>motor.TryDodge(motor.Commands.Move));
                if(combat.EnhancedControls)superButton=PcAction(dock,"SuperAttack",HudSymbol.Rune,"Super","Q",324,()=>input.RequestSuper());
                Label(safe,"KeyboardHint",new Vector2(0,0),new Vector2(18,18),new Vector2(320,22),"WASD mover  ·  I mochila  ·  Tab herói",12).color=RusticHudTheme.Muted;
            }
        }
        Button PcAction(Transform parent,string name,HudSymbol icon,string caption,string key,float x,UnityAction action)
        {var button=Action(parent,name,new Vector2(0,0),new Vector2(x,10),new Vector2(84,66),"",action);RusticHudTheme.Icon(button.transform,icon,Vector2.one*.5f,new Vector2(-18,9),27);Label(button.transform,"Key",new Vector2(1,1),new Vector2(-8,-5),new Vector2(42,22),key,11,TextAnchor.MiddleRight);Label(button.transform,"Caption",new Vector2(.5f,0),new Vector2(0,5),new Vector2(82,20),caption,12,TextAnchor.MiddleCenter);var cd=Label(button.transform,"Cooldown",Vector2.one*.5f,new Vector2(18,5),new Vector2(35,25),"",14,TextAnchor.MiddleCenter);if(name=="Special")specialTime=cd;else if(name=="Dodge")dodgeTime=cd;return button;}
        void ConfigureHold(GameObject button)
        {
            var old=button.GetComponent<UnityEngine.InputSystem.OnScreen.OnScreenButton>();if(old!=null)old.enabled=false;
            var held=button.GetComponent<HoldAttackButton>()??button.AddComponent<HoldAttackButton>();held.Configure(input);
        }
        void StyleTouch(RectTransform rect,Vector2 position,float size,HudSymbol symbol,string caption)
        {
            Position(rect,new Vector2(1,0),position,Vector2.one*size,Vector2.one*.5f);
            foreach(var t in rect.GetComponentsInChildren<Text>(true))t.gameObject.SetActive(false);
            var image=rect.GetComponent<Image>();if(image!=null)image.enabled=false;
            var graphic=RusticHudTheme.Panel(rect,new Color(.12f,.15f,.11f,.82f),size*.5f);graphic.raycastTarget=true;graphic.border=2;
            var button=rect.GetComponent<Button>();if(button!=null)button.targetGraphic=graphic;
            RusticHudTheme.Icon(rect,symbol,Vector2.one*.5f,new Vector2(0,8),size*.36f);
            Label(rect,"Caption",new Vector2(.5f,0),new Vector2(0,9),new Vector2(size,19),caption,12,TextAnchor.MiddleCenter);
            var cd=Label(rect,"Cooldown",new Vector2(1,1),new Vector2(-4,-2),new Vector2(32,25),"",15,TextAnchor.MiddleCenter);cd.color=RusticHudTheme.Gold;
            if(symbol==HudSymbol.Rune)specialTime=cd;else if(symbol==HudSymbol.Dodge)dodgeTime=cd;
        }
        static void Position(RectTransform rect,Vector2 anchor,Vector2 position,Vector2 size,Vector2 pivot)
        {rect.anchorMin=rect.anchorMax=anchor;rect.pivot=pivot;rect.anchoredPosition=position;rect.sizeDelta=size;}
        public void ToggleMenu(){bool open=!MenuOpen;tools.gameObject.SetActive(false);menu.gameObject.SetActive(open);}
        void UpdateSafeArea()
        {if(safe==null)return;Rect area=Screen.safeArea;safe.anchorMin=new Vector2(area.xMin/Mathf.Max(1,Screen.width),area.yMin/Mathf.Max(1,Screen.height));safe.anchorMax=new Vector2(area.xMax/Mathf.Max(1,Screen.width),area.yMax/Mathf.Max(1,Screen.height));safe.offsetMin=safe.offsetMax=Vector2.zero;}
        void Update()
        {
            if(session==null)return;
            bool menuVisible=MenuOpen||tools.gameObject.activeSelf;
            if(mobileControls!=null)mobileControls.SetActive(Application.isMobilePlatform&&!menuVisible);
            if(interact!=null)interact.gameObject.SetActive(!menuVisible);
            if(superButton!=null)superButton.gameObject.SetActive(!menuVisible);
            UpdateSafeArea();refresh-=Time.unscaledDeltaTime;if(refresh<=0){refresh=.1f;Refresh();}
        }
        void Refresh()
        {
            var heroes=session.Heroes;var health=heroes.Health;heroName.text=heroes.Current.displayName;portrait.color=heroes.Current.color;
            if(superFill!=null)
            {
                superFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,276*combat.SuperProgress);
                superLabel.text=combat.SuperReady?"SUPER PRONTO  ·  "+(Application.isMobilePlatform?"toque SUPER":"Q / SUPER"):"BRASA INTERIOR  ·  "+Mathf.RoundToInt(combat.SuperProgress*100)+"%";
                chargeLabel.text=combat.IsSuper?"RUPTURA DE BRASA!":combat.SuperQueued?"Super preparado · terminando o golpe":combat.IsHeavy?"GOLPE PESADO!":combat.ChargeProgress>.05f?"Carregando golpe  ·  "+Mathf.RoundToInt(combat.ChargeProgress*100)+"%":"Segure ataque: combo + golpe pesado";
                if(superButton!=null)superButton.interactable=combat.SuperReady&&health.IsAlive;
            }
            healthText.text=health.Current+" / "+health.Maximum+"  VIDA";healthFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,208f*health.Current/Mathf.Max(1,health.Maximum));
            var room=session.CurrentRoom;location.text=session.Biome.displayName.Replace(" — mapa de teste","");
            objective.text=session.Finished?"Expedição concluída":!health.IsAlive?"Você caiu · reinicie pelo menu":room==null?"Preparando o caminho…":room.completed?"Caminho livre · continue explorando":room.encounter!=null&&!room.encounterComplete?"Derrote os inimigos para avançar":room.node.category==RoomCategory.Trap?"Desative a armadilha":room.node.category==RoomCategory.Event?"Ative o cristal da passagem":"Explore e interaja";
            prompt.text=session.Nearby!=null?session.Nearby.Label+(Application.isMobilePlatform?"":"  ·  F") : "";
            if(interact!=null)interact.interactable=session.Nearby!=null&&health.IsAlive;
            if(specialTime!=null)specialTime.text=combat.SpecialCooldownRemaining>.05f?Mathf.CeilToInt(combat.SpecialCooldownRemaining)+"s":"";
            if(dodgeTime!=null)dodgeTime.text=motor.DodgeCooldownRemaining>.05f?Mathf.CeilToInt(motor.DodgeCooldownRemaining)+"s":"";
            if(bridge!=null&&bridge.Service!=null)
            {
                var profile=bridge.Service.Profile;int level=ProgressionStats.Level(profile.experience,bridge.Catalog.maximumLevel);int xp=ProgressionStats.ExperienceInLevel(profile.experience,bridge.Catalog.maximumLevel);int required=ProgressionStats.RequiredExperience(level);
                if (profile.options.largeText != largeText)
                { largeText = profile.options.largeText; for (int i = 0; i < labels.Length; i++) labels[i].fontSize = baseSizes[i] + (largeText ? 2 : 0); }
                levelText.text="NÍVEL "+level+"   ·   "+xp+" / "+required+" XP";xpFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,274f*Mathf.Clamp01((float)xp/Mathf.Max(1,required)));
                var run=profile.expedition;money.text="OURO  "+profile.coins+(run!=null&&run.coins>0?"   +"+run.coins:"");
                if(run!=null&&(run.coins!=lastRunCoins||run.items.Count!=lastRunItems)){if(run.coins>lastRunCoins||run.items.Count>lastRunItems){loot.text="Saque da expedição  ·  "+run.coins+" ouro  ·  "+run.items.Count+" itens";lootUntil=Time.unscaledTime+4;}lastRunCoins=run.coins;lastRunItems=run.items.Count;}
            }
            else{levelText.text="EXPEDIÇÃO DE TESTE";money.text="OURO  "+session.VisibleCoins;}
            if(Time.unscaledTime>lootUntil)loot.text="";
            var boss=session.Encounter.Boss;bool alive=boss!=null&&boss.IsAlive;bossPanel.SetActive(alive);
            if(alive){bossName.text=boss.Profile.displayName+"  ·  Fase "+boss.Brain.BossPhase;bossFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,366f*boss.Health.Current/Mathf.Max(1,boss.Health.Maximum));}
        }
    }
}
