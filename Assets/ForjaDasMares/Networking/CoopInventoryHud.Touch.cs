using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    public sealed partial class CoopInventoryHud
    {
        RectTransform touchIdentity,touchXpFill;Text touchName,touchResources,touchXp;
        void BuildTouchInventory()
        {
            var parent=overlay.transform;
            PlaceInventory(header.rectTransform,30,20,500,40);header.color=RusticHudTheme.Gold;
            PlaceInventory(closeButton,1080,20,150,44,18);
            PlaceInventory(classFilter,30,76,240,32,14);PlaceInventory(capacity.rectTransform,282,76,248,32);
            string[] names={"TODOS","ARMAS","2ª MÃO","ARMAD.","ACESS.","ARTEF."};
            for(int i=0;i<categoryTabs.Length;i++){PlaceInventory(categoryTabs[i],30+i*83,120,79,38,12);categoryTabs[i].GetComponentInChildren<Text>().text=names[i];}
            for(int i=0;i<rows.Length;i++)PlaceInventory((RectTransform)rows[i].transform,30+i%2*256,174+i/2*125,248,120);
            PlaceInventory(heroChoice,560,20,500,44,18);PlaceInventory(profileSource,560,72,640,28,13);
            touchIdentity=InventoryPanel(parent,"TouchHeroIdentity",560,110,640,110,new Color(.035f,.075f,.06f,.98f));
            touchName=InventoryText(touchIdentity,"Hero",18,8,320,32,"",22);touchName.color=RusticHudTheme.Gold;
            touchResources=InventoryText(touchIdentity,"Wallet",350,8,272,32,"",22);touchResources.alignment=TextAnchor.MiddleRight;
            var track=InventoryPanel(touchIdentity,"Experience",18,58,604,9,new Color(.02f,.035f,.03f,1));
            touchXpFill=RusticHudTheme.Rect(track,"Fill",Vector2.zero,Vector2.zero,Vector2.zero);touchXpFill.anchorMax=Vector2.one;touchXpFill.offsetMin=touchXpFill.offsetMax=Vector2.zero;var image=touchXpFill.gameObject.AddComponent<Image>();image.color=new Color(.22f,.78f,.91f);image.raycastTarget=false;
            touchXp=InventoryText(touchIdentity,"XP",18,76,604,23,"",15);touchXp.alignment=TextAnchor.MiddleCenter;
            PlaceInventory(characterPreview.rectTransform,785,228,185,280);
            Vector2[] positions={new Vector2(580,235),new Vector2(1005,235),new Vector2(580,350),new Vector2(1005,350),new Vector2(800,475)};
            for(int i=0;i<5;i++)PlaceInventory((RectTransform)equipmentSlots[i].transform,positions[i].x,positions[i].y,175,100);
            sheet.ArrangeTouchOverview();
            PlaceInventory(notice.rectTransform,30,682,1180,16);notice.fontSize=11;
            PlaceInventory(bagSource,30,591,160,36,13);PlaceInventory(expeditionButton,200,591,160,36,13);PlaceInventory(stockView,370,591,160,36,13);
            PlaceInventory(previous,30,635,120,36,13);PlaceInventory(next,410,635,120,36,13);PlaceInventory(pageLabel.rectTransform,155,635,250,36);
            PlaceInventory(sourceHint.rectTransform,580,586,600,65);sourceHint.fontSize=15;
            PlaceInventory(claimAll,580,640,600,34,14);
            PlaceInventory(emptyHint.rectTransform,30,180,500,135);PlaceInventory(clearFilters,30,335,500,44,16);
        }
        void RefreshTouchInventory(PlayerProfile p)
        {
            touchIdentity.gameObject.SetActive(!results);if(results)return;
            header.text="INVENTÁRIO";var hero=ActiveActor.Roster.Current;
            int level=ProgressionStats.Level(p.experience,channel.Catalog.maximumLevel),required=ProgressionStats.RequiredExperience(level),current=ProgressionStats.ExperienceInLevel(p.experience,channel.Catalog.maximumLevel);
            touchName.text=hero.displayName+" · Nv "+level;touchResources.text=p.coins.ToString("N0")+" ouro";
            bool max=level>=channel.Catalog.maximumLevel;touchXp.text=max?"NÍVEL MÁXIMO":"XP "+current+" / "+required;touchXpFill.anchorMax=new Vector2(max?1:Mathf.Clamp01((float)current/required),1);
            sourceHint.rectTransform.sizeDelta=new Vector2(600,deliveries?48:80);
        }
    }
}
