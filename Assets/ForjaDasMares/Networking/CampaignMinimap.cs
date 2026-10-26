using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace ForjaDasMares.Networking
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CampaignMinimap:MaskableGraphic,IPointerClickHandler
    {
        CoopDirector session;RectTransform panel;Text guide;float timer;bool expanded;CanvasGroup visibility;
        Vector2 low,high;float scale;
        public void Build(Transform parent,CoopDirector owner)
        {
            session=owner;panel=RusticHudTheme.Rect(parent,"MinimapPanel",new Vector2(0,1),new Vector2(18,-18),new Vector2(160,165));RusticHudTheme.Panel(panel,RusticHudTheme.Ink,18);visibility=panel.gameObject.AddComponent<CanvasGroup>();
            transform.SetParent(panel,false);rectTransform.anchorMin=rectTransform.anchorMax=rectTransform.pivot=new Vector2(.5f,1);rectTransform.anchoredPosition=new Vector2(0,-10);rectTransform.sizeDelta=new Vector2(140,115);raycastTarget=true;
            guide=RusticHudTheme.Label(panel,"MapGuide",new Vector2(0,1),new Vector2(10,-130),new Vector2(140,30),"MINIMAPA · toque / M",10);
        }
        public void OnPointerClick(PointerEventData data)=>Toggle();
        void Toggle(){expanded=!expanded;panel.sizeDelta=expanded?new Vector2(470,480):new Vector2(160,165);rectTransform.sizeDelta=expanded?new Vector2(450,410):new Vector2(140,115);guide.rectTransform.anchoredPosition=new Vector2(10,expanded?-425:-130);guide.rectTransform.sizeDelta=new Vector2(expanded?450:140,45);SetVerticesDirty();}
        void Update()
        {
            if(session==null)return;bool active=session.Phase==CoopPhase.Playing;visibility.alpha=active?1:0;visibility.blocksRaycasts=active;if(!active)return;
            if(Keyboard.current!=null&&Keyboard.current.mKey.wasPressedThisFrame)Toggle();
            timer-=Time.unscaledDeltaTime;if(timer>0)return;timer=.2f;SetVerticesDirty();
            var actor=session.Actor(session.LocalSlot);if(actor==null)return;float nearest=float.MaxValue;int remaining=0;
            foreach(var enemy in session.Dungeon.Encounter.Enemies.Actors)if(enemy.gameObject.activeInHierarchy&&enemy.Health.IsAlive){remaining++;nearest=Mathf.Min(nearest,Vector3.Distance(actor.transform.position,enemy.transform.position));}
            int fight=session.Campaign.FightRoomId;
            guide.text=nearest<float.MaxValue?remaining+" inimigos · próximo "+Mathf.CeilToInt(nearest)+"m":fight>=0?"Combate pendente no setor "+(fight+1):"Azul: grupo · dourado: objetivo";
            guide.text+=" · M: ampliar";
        }
        Vector2 Map(Vector3 world)=>new Vector2((world.x-low.x)*scale,(world.z-low.y)*scale)-((high-low)*scale)*.5f+rectTransform.rect.center;
        void Quad(VertexHelper vh,Vector2 point,Vector2 size,Color tint)
        {int n=vh.currentVertCount;vh.AddVert(point+new Vector2(-size.x,-size.y)*.5f,tint,Vector2.zero);vh.AddVert(point+new Vector2(-size.x,size.y)*.5f,tint,Vector2.zero);vh.AddVert(point+new Vector2(size.x,size.y)*.5f,tint,Vector2.zero);vh.AddVert(point+new Vector2(size.x,-size.y)*.5f,tint,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(session==null||session.Dungeon.Generator.Rooms==null)return;var rooms=session.Dungeon.Generator.Rooms;
            low=Vector2.one*float.MaxValue;high=Vector2.one*float.MinValue;
            foreach(var room in rooms){Vector2 p=new Vector2(room.Center.x,room.Center.z);low=Vector2.Min(low,p-Vector2.one*room.walkableHalf);high=Vector2.Max(high,p+Vector2.one*room.walkableHalf);}
            scale=Mathf.Min((rectTransform.rect.width-8)/Mathf.Max(1,high.x-low.x),(rectTransform.rect.height-8)/Mathf.Max(1,high.y-low.y));
            foreach(var room in rooms)foreach(int next in room.node.connections)if(next>room.node.id){Vector2 a=Map(room.Center),z=Map(rooms[next].Center),delta=z-a;Quad(vh,(a+z)*.5f,new Vector2(Mathf.Abs(delta.x)+4,Mathf.Abs(delta.y)+4),new Color(.25f,.28f,.23f));}
            foreach(var room in rooms)
            {Color c=room.completed?new Color(.18f,.35f,.25f):room.entered?new Color(.38f,.35f,.23f):new Color(.17f,.19f,.18f);if(room.node.id==session.Campaign.FightRoomId)c=new Color(.55f,.17f,.13f);Quad(vh,Map(room.Center),Vector2.one*room.walkableHalf*2*scale,c);if(!room.completed&&room.node.category!=RoomCategory.Combat&&room.node.category!=RoomCategory.HardCombat)Quad(vh,Map(room.Center),Vector2.one*5,RusticHudTheme.Gold);}
            foreach(var enemy in session.Dungeon.Encounter.Enemies.Actors)if(enemy.gameObject.activeInHierarchy&&enemy.Health.IsAlive)Quad(vh,Map(enemy.transform.position),Vector2.one*(enemy.Profile.miniboss?6:4),new Color(1,.2f,.1f));
            if(session.Campaign.Features?.Pings!=null)foreach(var ping in session.Campaign.Features.Pings)if(ping!=null&&ping.remaining>0)Quad(vh,Map(ping.position),Vector2.one*8,ping.kind==2?Color.red:Color.yellow);
            for(int i=0;i<4;i++)if(session.IsConnected(i)){var a=session.Actor(i);Quad(vh,Map(a.transform.position),Vector2.one*(i==session.LocalSlot?7:5),i==session.LocalSlot?Color.cyan:Color.blue);}
        }
    }
}
