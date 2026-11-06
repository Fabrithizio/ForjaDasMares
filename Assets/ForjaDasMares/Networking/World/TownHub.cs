using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace ForjaDasMares.Networking
{
    public sealed class TownHub:MonoBehaviour
    {
        [SerializeField]RegionalWorld world;
        [SerializeField]Vector3[] arrivalPoints;
        public string Notice {get;private set;}="Bem-vindo ao Refúgio do Último Sino. Clique nos moradores para conversar.";
        public Vector3[] ArrivalPoints=>arrivalPoints;
        WorldClickTarget[] targets;
        bool travelling;
        public bool IsTravelling=>travelling;
        public void Configure(RegionalWorld region,Vector3[] spawn){world=region;arrivalPoints=spawn;}
        public void Speak(string message)=>Notice=message;
        void Start()=>targets=FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None);
        public WorldClickTarget NearestInteraction
        {
            get
            {
                if(targets==null||world==null||world.Hero==null)return null;
                WorldClickTarget nearest=null;float distance=float.MaxValue;
                foreach(var t in targets)
                {
                    if(t==null||!t.isActiveAndEnabled||!t.InRange(world.Hero.transform))continue;
                    float d=(t.transform.position-world.Hero.transform.position).sqrMagnitude;
                    if(d<distance){distance=d;nearest=t;}
                }
                return nearest;
            }
        }
        public bool TryInteractNearest(){var nearest=NearestInteraction;return nearest!=null&&nearest.TryUse(world.Hero.transform);}
        public string Hint {get{var nearest=NearestInteraction;return nearest!=null?"F · "+nearest.Label:world.RefugeDistance<5?"F · Descansar no refúgio":"";}}
        public void TalkStory(){world.Journey.Interact();Notice=world.Journey.Notice;}
        public void Rest(){world.Hero.Health.Restore();Notice="Fonte das Marés: vida restaurada.";}
        public void Mentor(){world.Hero.Select((world.Hero.Selected+1)%world.Hero.Count);Notice="Instrutora: agora você está com "+world.Hero.Current.displayName+". Experimente a área de treino.";}
        public void Explore()=>BeginTravel("CostaAto1");
        public void OpenExpeditions()=>BeginTravel("CampaignCoop");
        void BeginTravel(string scene)
        {
            if(travelling || world==null || !world.Hero.Health.IsAlive) return;
            travelling=true;
            world.Hero.GetComponent<ArenaInput>().Desktop?.Cancel();
            world.Hero.GetComponent<ArenaInput>().BlockCommands(true);
            StartCoroutine(Travel(scene));
        }
        IEnumerator Travel(string scene)
        {
            var curtain=new GameObject("TownTravelFade",typeof(RectTransform),typeof(Canvas),typeof(CanvasGroup));
            curtain.transform.SetParent(transform,false);
            var canvas=curtain.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
            var panel=new GameObject("Curtain",typeof(RectTransform),typeof(Image));panel.transform.SetParent(curtain.transform,false);
            var rect=(RectTransform)panel.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            panel.GetComponent<Image>().color=Color.black;
            var group=curtain.GetComponent<CanvasGroup>();group.alpha=0;
            float elapsed=0;
            while(elapsed<.28f){elapsed+=Time.unscaledDeltaTime;group.alpha=Mathf.Clamp01(elapsed/.28f);yield return null;}
            var load=SceneManager.LoadSceneAsync(scene);
            if(load==null){Destroy(curtain);travelling=false;world.Hero.GetComponent<ArenaInput>().BlockCommands(false);yield break;}
            while(!load.isDone)yield return null;
        }
    }
}
