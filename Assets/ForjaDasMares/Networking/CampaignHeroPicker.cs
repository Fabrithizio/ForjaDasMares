using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    public sealed class CampaignHeroPicker : MonoBehaviour
    {
        GameObject overlay;CoopDirector director;Button[] choices;
        public bool IsOpen=>overlay!=null&&overlay.activeSelf;
        public void Build(RectTransform parent,CoopDirector owner)
        {
            director=owner;var full=RusticHudTheme.Rect(parent,"HeroSelection",Vector2.zero,Vector2.zero,Vector2.zero);full.anchorMax=Vector2.one;full.offsetMin=full.offsetMax=Vector2.zero;full.gameObject.AddComponent<Image>().color=new Color(.01f,.02f,.025f,.94f);overlay=full.gameObject;
            var panel=RusticHudTheme.Rect(full,"HeroCards",Vector2.one*.5f,Vector2.zero,new Vector2(1140,650));RusticHudTheme.Panel(panel,RusticHudTheme.Ink,24);
            RusticHudTheme.Label(panel,"Title",new Vector2(0,1),new Vector2(30,-15),new Vector2(850,60),"ESCOLHA SEU HERÓI",30);
            RusticHudTheme.Label(panel,"Hint",new Vector2(0,1),new Vector2(30,-75),new Vector2(1050,55),"Armas e equipamentos mantêm a identidade de cada classe. Troque antes da partida.",18);
            RusticHudTheme.Button(panel,"CloseHeroes",new Vector2(1,1),new Vector2(-25,-25),new Vector2(180,44),"VOLTAR",Close);
            choices=new Button[owner.HeroCount];int columns=choices.Length<=4?2:3;float spacing=columns==2?540:365,cardWidth=columns==2?525:350;
            for(int i=0;i<choices.Length;i++)
            {
                int index=i;var hero=owner.HeroAt(i);var pos=new Vector2(30+(i%columns)*spacing,-145-(i/columns)*230);
                var button=RusticHudTheme.Button(panel,"HeroChoice_"+i,new Vector2(0,1),pos,new Vector2(cardWidth,210),"",()=>{director.SelectHero(index);Close();});choices[i]=button;
                var icon=RusticHudTheme.Icon(button.transform,hero.heroId=="arcanist"?HudSymbol.Rune:hero.heroId=="ranger"?HudSymbol.Compass:hero.heroId=="runeknight"?HudSymbol.Shield:HudSymbol.Sword,new Vector2(0,1),new Vector2(18,-18),38);icon.color=hero.color;
                var name=RusticHudTheme.Label(button.transform,"HeroName",new Vector2(0,1),new Vector2(65,-10),new Vector2(cardWidth-80,60),hero.displayName,21);name.resizeTextForBestFit=true;name.resizeTextMinSize=17;name.resizeTextMaxSize=21;
                RusticHudTheme.Label(button.transform,"Role",new Vector2(0,1),new Vector2(18,-75),new Vector2(cardWidth-35,58),string.IsNullOrWhiteSpace(hero.strengths)?hero.role:hero.strengths,17);
                RusticHudTheme.Label(button.transform,"BaseStats",new Vector2(0,1),new Vector2(18,-145),new Vector2(cardWidth-35,50),hero.maximumHealth+" vida base · "+hero.speed.ToString("0.#")+" m/s\n"+(hero.special!=null?hero.special.displayName:""),16);
            }
            Close();
        }
        public void Open()
        {
            if(director.Phase!=CoopPhase.Offline&&director.Phase!=CoopPhase.Closed&&director.Phase!=CoopPhase.Lobby)return;
            overlay.SetActive(true);overlay.transform.SetAsLastSibling();for(int i=0;i<choices.Length;i++){var g=choices[i].GetComponent<RoundedHudGraphic>();g.borderColor=i==director.SelectedHero?RusticHudTheme.Gold:RusticHudTheme.Copper;g.SetVerticesDirty();}
        }
        public void Close(){if(overlay!=null)overlay.SetActive(false);}
        void Update(){if(IsOpen&&director.Phase!=CoopPhase.Offline&&director.Phase!=CoopPhase.Closed&&director.Phase!=CoopPhase.Lobby)Close();}
    }
}
