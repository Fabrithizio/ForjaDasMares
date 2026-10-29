using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace ForjaDasMares.Networking
{
    [DefaultExecutionOrder(260)]public sealed class RegionalWorldHud:MonoBehaviour
    {
        [SerializeField]RegionalWorld world;
        Text life,objective,hint,controlHint;Button movementMode;GameObject death;float poll;InputAction interact;
        public void Configure(RegionalWorld value){world=value;}
        static readonly Vector2 Top=new Vector2(0,1);
        Text Label(Transform parent,string name,Vector2 position,Vector2 size,string text,int font=18)=>RusticHudTheme.Label(parent,name,Top,position,size,text,font);
        void Start()
        {
            var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;gameObject.AddComponent<GraphicRaycaster>();
            var scale=gameObject.AddComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1280,720);scale.matchWidthOrHeight=.5f;
            var root=RusticHudTheme.Rect(transform,"RegionSafeArea",Vector2.zero,Vector2.zero,Vector2.zero);root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            life=Label(root,"Health",new Vector2(22,-18),new Vector2(450,65),"",20);
            movementMode=RusticHudTheme.Button(root,"MovementMode",Top,new Vector2(22,-88),new Vector2(300,36),"",DesktopRpgControls.ToggleMode);
            controlHint=Label(root,"ControlHint",new Vector2(22,-131),new Vector2(430,56),"",14);
            var quest=RusticHudTheme.Rect(root,"Quest",new Vector2(1,1),new Vector2(-20,-82),new Vector2(330,125));RusticHudTheme.Panel(quest,new Color(.025f,.05f,.045f,.92f),16);objective=Label(quest,"Objective",new Vector2(14,-12),new Vector2(302,105),"",16);
            var map=RusticHudTheme.Rect(root,"LocalMap",new Vector2(1,1),new Vector2(-20,-220),new Vector2(175,175));RusticHudTheme.Panel(map,new Color(.025f,.05f,.045f,.9f),16);var mapContent=RusticHudTheme.Rect(map,"Markers",Vector2.zero,new Vector2(8,8),new Vector2(159,159));mapContent.gameObject.AddComponent<RegionalMap>().Configure(world);
            Label(root,"MapLegend",new Vector2(1060,-403),new Vector2(200,42),"Branco: você · verde: refúgio\nLaranja: patrulhas · azul: portal",11);
            RusticHudTheme.Button(root,"ReturnToMenu",new Vector2(1,1),new Vector2(-20,-20),new Vector2(210,42),"VOLTAR AO MENU",world.ReturnToCampaign);
            hint=Label(root,"Interact",new Vector2(350,-625),new Vector2(580,35),"",18);hint.alignment=TextAnchor.MiddleCenter;hint.color=RusticHudTheme.Gold;
            Label(root,"Controls",new Vector2(22,-674),new Vector2(1200,35),"F8: modo de controle · Shift: atacar parado · botão direito / E: habilidade · Espaço: esquiva · roda: zoom · F: interagir · R: refúgio",13);
            Label(root,"Scope",new Vector2(22,-640),new Vector2(320,24),world.Definition.displayName.ToUpperInvariant()+" · PRÉVIA LOCAL",12).color=RusticHudTheme.Muted;
            death=RusticHudTheme.Rect(root,"Death",Vector2.one*.5f,Vector2.zero,new Vector2(450,180)).gameObject;RusticHudTheme.Panel((RectTransform)death.transform,RusticHudTheme.Ink,20);
            Label(death.transform,"Title",new Vector2(25,-20),new Vector2(400,60),"Você caiu. Retorne ao refúgio.",23);RusticHudTheme.Button(death.transform,"Respawn",Top,new Vector2(25,-100),new Vector2(400,50),"REAPARECER",world.Respawn);death.SetActive(false);
            interact=new InputAction("RegionalInteract",InputActionType.Button);interact.AddBinding("<Keyboard>/f");interact.AddBinding("<Gamepad>/buttonWest");interact.performed+=OnInteract;interact.Enable();
            world.Hero.GetComponent<ArenaInput>().PointerBlocked=p=>EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject();
        }
        void OnInteract(InputAction.CallbackContext _)=>world.Interact();
        void Update()
        {
            poll-=Time.unscaledDeltaTime;if(poll>0)return;poll=.1f;
            movementMode.GetComponentInChildren<Text>().text=DesktopRpgControls.ModeName+" · F8 PARA TROCAR";
            controlHint.text=DesktopRpgControls.MouseMode?"Chão: andar · segure no inimigo: atacar\nShift: atacar parado · WASD também pode mover":"WASD/setas: mover · segure o mouse: atacar\nMova o cursor para direcionar os próximos golpes";
            var hero=world.Hero;life.text=hero.Current.displayName+"\nVIDA  "+hero.Health.Current+" / "+hero.Health.Maximum;
            objective.text=world.Definition.displayName.ToUpperInvariant()+"\nPatrulhas derrotadas: "+world.ClearedCount+" / "+world.Definition.camps.Length+"\n"+(world.PortalReady?"O portal dos desafios está aberto.":"Derrote a primeira patrulha de cada local.");
            var navigationNotice=world.Hero.GetComponent<ArenaInput>().Desktop?.Notice;hint.text=string.IsNullOrEmpty(navigationNotice)?world.InteractionHint:navigationNotice;death.SetActive(!hero.Health.IsAlive);
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
        protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(world==null)return;for(int i=1;i<world.Definition.route.Length;i++)Path(vh,world.Definition.route[i-1],world.Definition.route[i]);Dot(vh,world.Definition.refuge,Color.green,4);for(int i=0;i<world.Definition.camps.Length;i++)Dot(vh,world.Definition.camps[i].center,world.IsCampCleared(i)?Color.gray:new Color(1,.4f,.2f),4);Dot(vh,world.Definition.portal,Color.cyan,4);Dot(vh,world.Hero.transform.position,Color.white,3);}
    }
}
