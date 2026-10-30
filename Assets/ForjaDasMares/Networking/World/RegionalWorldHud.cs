using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace ForjaDasMares.Networking
{
    [DefaultExecutionOrder(260)]public sealed class RegionalWorldHud:MonoBehaviour
    {
        [SerializeField]RegionalWorld world;
        Text life,objective,hint,controlHint,storyNotice,healthCaption,superCaption;RectTransform healthFill,superFill;EquipmentPaperDoll portrait;PlayerProfile previewProfile=new PlayerProfile();TownHub town;Button movementMode;GameObject death;float poll;InputAction interact;
        public void Configure(RegionalWorld value){world=value;}
        static readonly Vector2 Top=new Vector2(0,1);
        Text Label(Transform parent,string name,Vector2 position,Vector2 size,string text,int font=18)=>RusticHudTheme.Label(parent,name,Top,position,size,text,font);
        void Start()
        {
            var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;gameObject.AddComponent<GraphicRaycaster>();
            var scale=gameObject.AddComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1280,720);scale.matchWidthOrHeight=.5f;
            var root=RusticHudTheme.Rect(transform,"RegionSafeArea",Vector2.zero,Vector2.zero,Vector2.zero);root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            town=world.GetComponent<TownHub>();BuildVitals(root);
            movementMode=RusticHudTheme.Button(root,"MovementHelp",Top,new Vector2(22,-118),new Vector2(300,36),"CONTROLES · AJUDA",()=>{controlHint.gameObject.SetActive(!controlHint.gameObject.activeSelf);});
            controlHint=Label(root,"ControlHint",new Vector2(22,-158),new Vector2(430,56),"",14);
            var quest=RusticHudTheme.Rect(root,"Quest",new Vector2(1,1),new Vector2(-20,-82),new Vector2(350,135));RusticHudTheme.Panel(quest,new Color(.025f,.05f,.045f,.92f),16);objective=Label(quest,"Objective",new Vector2(14,-12),new Vector2(322,115),"",16);
            var map=RusticHudTheme.Rect(root,"LocalMap",new Vector2(1,1),new Vector2(-20,-220),new Vector2(175,175));RusticHudTheme.Panel(map,new Color(.025f,.05f,.045f,.9f),16);var mapContent=RusticHudTheme.Rect(map,"Markers",Vector2.zero,new Vector2(8,8),new Vector2(159,159));mapContent.gameObject.AddComponent<RegionalMap>().Configure(world);
            Label(root,"MapLegend",new Vector2(1060,-403),new Vector2(200,42),"Branco: você · verde: refúgio\nLaranja: patrulhas · azul: portal",11);
            RusticHudTheme.Button(root,"ReturnToMenu",new Vector2(1,1),new Vector2(-20,-20),new Vector2(210,42),"VOLTAR AO MENU",world.ReturnToCampaign);
            hint=Label(root,"Interact",new Vector2(350,-625),new Vector2(580,35),"",18);hint.alignment=TextAnchor.MiddleCenter;hint.color=RusticHudTheme.Gold;var promptRect=(RectTransform)hint.transform;promptRect.anchorMin=promptRect.anchorMax=promptRect.pivot=new Vector2(.5f,0);promptRect.anchoredPosition=new Vector2(0,174);
            Label(root,"Controls",new Vector2(22,-674),new Vector2(1200,35),"Mouse: mover e atacar · Shift: atacar parado · botão direito / E: habilidade · Espaço: esquiva · roda: zoom · F: interagir · R: refúgio",13);
            Label(root,"Scope",new Vector2(22,-640),new Vector2(320,24),world.Definition.displayName.ToUpperInvariant()+" · PRÉVIA LOCAL",12).color=RusticHudTheme.Muted;
            if(world.Journey!=null)
            {
                var dialogue=RusticHudTheme.Rect(root,"StoryNotice",new Vector2(.5f,0),new Vector2(0,82),new Vector2(750,82));dialogue.pivot=new Vector2(.5f,0);
                RusticHudTheme.Panel(dialogue,new Color(.025f,.05f,.045f,.92f),14);storyNotice=Label(dialogue,"Message",new Vector2(16,-8),new Vector2(718,66),"",16);if(town!=null){dialogue.anchorMin=dialogue.anchorMax=dialogue.pivot=Top;dialogue.anchoredPosition=new Vector2(18,-118);dialogue.sizeDelta=new Vector2(430,116);storyNotice.rectTransform.sizeDelta=new Vector2(398,100);movementMode.gameObject.SetActive(false);controlHint.gameObject.SetActive(false);}
            }
            death=RusticHudTheme.Rect(root,"Death",Vector2.one*.5f,Vector2.zero,new Vector2(450,180)).gameObject;RusticHudTheme.Panel((RectTransform)death.transform,RusticHudTheme.Ink,20);
            Label(death.transform,"Title",new Vector2(25,-20),new Vector2(400,60),"Você caiu. Retorne ao refúgio.",23);RusticHudTheme.Button(death.transform,"Respawn",Top,new Vector2(25,-100),new Vector2(400,50),"REAPARECER",world.Respawn);death.SetActive(false);
            interact=new InputAction("RegionalInteract",InputActionType.Button);interact.AddBinding("<Keyboard>/f");interact.AddBinding("<Gamepad>/buttonWest");interact.performed+=OnInteract;interact.Enable();
            world.Hero.GetComponent<ArenaInput>().PointerBlocked=p=>EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject();
            if(Application.isMobilePlatform)BuildTouchControls(root);else BuildPcActions(root);
        }
        void BuildVitals(RectTransform root)
        {
            var panel=RusticHudTheme.Rect(root,"HeroVitals",Top,new Vector2(18,-16),new Vector2(360,92));RusticHudTheme.Panel(panel,new Color(.025f,.045f,.055f,.94f),20);
            var frame=RusticHudTheme.Rect(panel,"Portrait",Top,new Vector2(8,-8),new Vector2(74,74));RusticHudTheme.Panel(frame,RusticHudTheme.Ink,37);frame.gameObject.AddComponent<Mask>().showMaskGraphic=true;
            var image=RusticHudTheme.Rect(frame,"Hero",Vector2.one*.5f,Vector2.zero,new Vector2(68,68)).gameObject.AddComponent<RawImage>();image.raycastTarget=false;portrait=frame.gameObject.AddComponent<EquipmentPaperDoll>();portrait.Configure(image,true);
            life=Label(panel,"HeroName",new Vector2(94,-7),new Vector2(252,26),"",18);life.color=RusticHudTheme.Gold;
            var track=RusticHudTheme.Rect(panel,"HealthTrack",Top,new Vector2(94,-36),new Vector2(238,16));track.gameObject.AddComponent<Image>().color=new Color(.18f,.05f,.055f);
            healthFill=RusticHudTheme.Rect(track,"Fill",Top,Vector2.zero,new Vector2(238,14));healthFill.gameObject.AddComponent<Image>().color=new Color(.76f,.12f,.18f);
            healthCaption=Label(panel,"HealthValue",new Vector2(96,-32),new Vector2(230,23),"",13);
            var focus=RusticHudTheme.Rect(panel,"FocusTrack",Top,new Vector2(94,-64),new Vector2(238,5));focus.gameObject.AddComponent<Image>().color=RusticHudTheme.Surface;
            superFill=RusticHudTheme.Rect(focus,"Fill",Top,Vector2.zero,new Vector2(0,5));superFill.gameObject.AddComponent<Image>().color=new Color(.18f,.6f,.85f);
            superCaption=Label(panel,"SuperValue",new Vector2(94,-70),new Vector2(238,16),"",11);
        }
        Button RoundAction(RectTransform root,string name,Vector2 position,float size,string text,HudSymbol symbol,UnityEngine.Events.UnityAction action)
        {
            var button=RusticHudTheme.Button(root,name,new Vector2(1,0),position,Vector2.one*size,"",action);var surface=button.GetComponent<RoundedHudGraphic>();surface.radius=size/2;surface.borderColor=RusticHudTheme.Gold;surface.color=new Color(.035f,.065f,.09f,.94f);
            RusticHudTheme.Icon(button.transform,symbol,Vector2.one*.5f,new Vector2(0,10),size*.43f);var caption=button.GetComponentInChildren<Text>();caption.text=text;caption.fontSize=12;caption.rectTransform.anchoredPosition=new Vector2(0,-size*.28f);caption.rectTransform.sizeDelta=new Vector2(size-6,22);return button;
        }
        void BuildPcActions(RectTransform root)
        {
            var input=world.Hero.GetComponent<ArenaInput>();
            RoundAction(root,"SpecialAction",new Vector2(-390,20),64,"E",HudSymbol.Rune,input.RequestSpecial);
            RoundAction(root,"DodgeAction",new Vector2(-310,20),64,"ESPAÇO",HudSymbol.Dodge,input.RequestDodge);
            RoundAction(root,"SuperAction",new Vector2(-230,20),64,"Q",HudSymbol.Fury,input.RequestSuper);
            RoundAction(root,"InteractAction",new Vector2(-150,20),64,"F",HudSymbol.Interact,world.Interact);
        }
        void BuildTouchControls(RectTransform root)
        {
            var input=world.Hero.GetComponent<ArenaInput>();var stick=RusticHudTheme.Rect(root,"RegionalStick",Vector2.zero,new Vector2(120,170),new Vector2(160,160));stick.pivot=Vector2.one*.5f;
            var ring=stick.gameObject.AddComponent<RoundedHudGraphic>();ring.color=new Color(.12f,.19f,.16f,.65f);ring.radius=80;
            var thumb=RusticHudTheme.Rect(stick,"Thumb",Vector2.one*.5f,Vector2.zero,new Vector2(52,52));var cap=thumb.gameObject.AddComponent<RoundedHudGraphic>();cap.color=RusticHudTheme.Gold;cap.radius=26;cap.raycastTarget=false;
            stick.gameObject.AddComponent<TouchMoveStick>().Configure(input,thumb);
            RoundAction(root,"RegionalAttack",new Vector2(-22,70),112,"ATAQUE",HudSymbol.Sword,input.RequestAttack).gameObject.AddComponent<HoldAttackButton>().Configure(input);
            RoundAction(root,"RegionalDodge",new Vector2(-148,50),78,"ESQUIVA",HudSymbol.Dodge,input.RequestDodge);
            RoundAction(root,"RegionalSpecial",new Vector2(-26,198),82,"ESPECIAL",HudSymbol.Rune,input.RequestSpecial);
            RoundAction(root,"RegionalInteract",new Vector2(-242,64),74,"USAR",HudSymbol.Interact,world.Interact);
            RoundAction(root,"RegionalSuper",new Vector2(-136,158),78,"SUPER",HudSymbol.Fury,input.RequestSuper);
            movementMode.gameObject.SetActive(false);
            controlHint.gameObject.SetActive(false);
            foreach(string name in new[]{"Controls","Scope","MapLegend"}){var label=root.Find(name);if(label!=null)label.gameObject.SetActive(false);}
            var prompt=(RectTransform)hint.transform;prompt.anchorMin=prompt.anchorMax=prompt.pivot=new Vector2(.5f,0);prompt.anchoredPosition=new Vector2(0,24);
            if(storyNotice!=null){var panel=(RectTransform)storyNotice.transform.parent;panel.anchorMin=panel.anchorMax=panel.pivot=Top;panel.sizeDelta=new Vector2(450,124);panel.anchoredPosition=new Vector2(22,-118);((RectTransform)storyNotice.transform).sizeDelta=new Vector2(418,108);}
        }
        void OnInteract(InputAction.CallbackContext _)=>world.Interact();
        void Update()
        {
            poll-=Time.unscaledDeltaTime;if(poll>0)return;poll=.1f;
            movementMode.GetComponentInChildren<Text>().text="CONTROLES · AJUDA";
            controlHint.text=Application.isMobilePlatform?"Joystick: mover · ataque: mira automática\nInteragir: conversar e investigar":"Chão: andar · alvo: atacar sem perseguir\nShift + ataque: disparar parado sem alvo";
            var hero=world.Hero;life.text=hero.Current.displayName;
            healthCaption.text="VIDA  "+hero.Health.Current+" / "+hero.Health.Maximum;healthFill.sizeDelta=new Vector2(238f*hero.Health.Current/Mathf.Max(1,hero.Health.Maximum),14);
            float focus=hero.GetComponent<HeroCombat>().SuperProgress;superFill.sizeDelta=new Vector2(238*focus,5);superCaption.text="SUPER "+Mathf.RoundToInt(focus*100)+"%";
            portrait.Refresh(hero.GetComponent<HeroPresentation>(),previewProfile,hero.Current.heroId);
            objective.text=world.Definition.displayName.ToUpperInvariant()+"\nPatrulhas derrotadas: "+world.ClearedCount+" / "+world.Definition.camps.Length+"\n"+(world.PortalReady?"O portal dos desafios está aberto.":"Derrote a primeira patrulha de cada local.");
            var navigationNotice=world.Hero.GetComponent<ArenaInput>().Desktop?.Notice;hint.text=string.IsNullOrEmpty(navigationNotice)?world.InteractionHint:navigationNotice;death.SetActive(!hero.Health.IsAlive);
            if(world.Journey!=null){objective.text=world.Journey.Definition.title.ToUpperInvariant()+"\n"+world.Journey.Objective;hint.text=string.IsNullOrEmpty(world.Journey.Hint)?world.InteractionHint:world.Journey.Hint;if(storyNotice!=null)storyNotice.text=world.Journey.Notice;}
            if(town!=null){if(storyNotice!=null)storyNotice.text=town.Notice;hint.text=town.Hint;}
            if(Application.isMobilePlatform)hint.text=hint.text.Replace("F · ","INTERAGIR · ");
        }
        void OnDestroy(){if(interact!=null){interact.performed-=OnInteract;interact.Dispose();}if(world!=null&&world.Hero!=null)world.Hero.GetComponent<ArenaInput>().PointerBlocked=null;}
    }
    public sealed class RegionalMap:MaskableGraphic
    {
        RegionalWorld world;float poll;public void Configure(RegionalWorld value){world=value;raycastTarget=false;}
        void Update(){poll-=Time.unscaledDeltaTime;if(poll<=0){poll=.15f;SetVerticesDirty();}}
        Vector2 Point(Vector3 p){var b=world.Definition.bounds;return new Vector2((p.x-b.min.x)/b.size.x,(p.z-b.min.z)/b.size.z)*rectTransform.rect.size+rectTransform.rect.min;}
        void Dot(VertexHelper vh,Vector3 point,Color tint,float size){Vector2 p=Point(point);int n=vh.currentVertCount;vh.AddVert(p+new Vector2(-size,-size),tint,Vector2.zero);vh.AddVert(p+new Vector2(-size,size),tint,Vector2.zero);vh.AddVert(p+new Vector2(size,size),tint,Vector2.zero);vh.AddVert(p+new Vector2(size,-size),tint,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
        void Path(VertexHelper vh,Vector3 a,Vector3 b)
        {Vector2 start=Point(a),end=Point(b),delta=end-start;var side=new Vector2(-delta.y,delta.x).normalized*.8f;int n=vh.currentVertCount;var tint=new Color(.65f,.53f,.3f,.8f);vh.AddVert(start-side,tint,Vector2.zero);vh.AddVert(start+side,tint,Vector2.zero);vh.AddVert(end+side,tint,Vector2.zero);vh.AddVert(end-side,tint,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
        protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(world==null)return;if(world.Definition.connections!=null&&world.Definition.connections.Length>0){foreach(var edge in world.Definition.connections)Path(vh,edge.from,edge.to);}else for(int i=1;i<world.Definition.route.Length;i++)Path(vh,world.Definition.route[i-1],world.Definition.route[i]);Dot(vh,world.Definition.refuge,Color.green,4);for(int i=0;i<world.Definition.camps.Length;i++)Dot(vh,world.Definition.camps[i].center,world.IsCampCleared(i)?Color.gray:new Color(1,.4f,.2f),4);if(world.Definition.landmarks!=null)foreach(var mark in world.Definition.landmarks)Dot(vh,mark.position,mark.color,3);Dot(vh,world.Definition.portal,Color.cyan,4);Dot(vh,world.Hero.transform.position,Color.white,3);}
    }
}
