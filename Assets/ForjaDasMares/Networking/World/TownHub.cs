using UnityEngine;
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
        public void Configure(RegionalWorld region,Vector3[] spawn){world=region;arrivalPoints=spawn;}
        public void Speak(string message)=>Notice=message;
        void Start()=>targets=FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None);
        public bool TryInteractNearest(){if(targets==null)return false;WorldClickTarget nearest=null;float distance=float.MaxValue;foreach(var t in targets)if(t!=null&&t.InRange(world.Hero.transform)){float d=(t.transform.position-world.Hero.transform.position).sqrMagnitude;if(d<distance){distance=d;nearest=t;}}return nearest!=null&&nearest.TryUse(world.Hero.transform);}
        public string Hint {get{if(targets!=null)foreach(var t in targets)if(t!=null&&t.InRange(world.Hero.transform))return "F · "+t.Label;return world.InteractionHint;}}
        public void TalkStory(){world.Journey.Interact();Notice=world.Journey.Notice;}
        public void Rest(){world.Hero.Health.Restore();Notice="Fonte das Marés: vida restaurada.";}
        public void Mentor(){world.Hero.Select((world.Hero.Selected+1)%world.Hero.Count);Notice="Instrutora: agora você está com "+world.Hero.Current.displayName+". Experimente a área de treino.";}
        public void Explore(){if(travelling)return;travelling=true;world.Hero.GetComponent<ArenaInput>().BlockCommands(true);SceneManager.LoadSceneAsync("CostaAto1");}
    }
}
