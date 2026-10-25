using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    // PC presentation only. Transactions, comparisons and saved items remain in the shared HUD/channel.
    public sealed partial class CoopInventoryHud
    {
        RectTransform desktopInventory,desktopDetails,desktopContent,desktopXpFill;
        Text desktopName,desktopLevel,desktopGold,desktopXp,desktopItemName,desktopItemType,desktopState,desktopBody,desktopStatus,desktopComparison;
        Image desktopItemIcon;Button desktopAdvanced;string desktopSelection;
        static readonly Vector2 InventoryTop=new Vector2(0,1);
        static void PlaceInventory(RectTransform rect,float x,float y,float width,float height)
        {rect.anchorMin=rect.anchorMax=rect.pivot=InventoryTop;rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(width,height);}
        static void PlaceInventory(Button button,float x,float y,float width,float height,int fontSize=14)
        {
            PlaceInventory((RectTransform)button.transform,x,y,width,height);
            var label=button.GetComponentInChildren<Text>(true);if(label==null)return;
            label.rectTransform.anchorMin=label.rectTransform.anchorMax=label.rectTransform.pivot=Vector2.one*.5f;
            label.rectTransform.anchoredPosition=Vector2.zero;label.rectTransform.sizeDelta=new Vector2(width-10,height-2);label.fontSize=fontSize;
        }
        static RectTransform InventoryPanel(Transform parent,string name,float x,float y,float w,float h,Color color)
        {
            var rect=RusticHudTheme.Rect(parent,name,InventoryTop,new Vector2(x,-y),new Vector2(w,h));
            var frame=RusticHudTheme.Panel(rect,color,18);frame.border=1;frame.borderColor=new Color(.5f,.39f,.22f,.8f);return rect;
        }
        static Text InventoryText(Transform parent,string name,float x,float y,float w,float h,string value,int size=16)
        {return RusticHudTheme.Label(parent,name,InventoryTop,new Vector2(x,-y),new Vector2(w,h),value,size);}
        void BuildDesktopInventory()
        {
            var parent=overlay.transform;desktopInventory=(RectTransform)parent;
            // Replace the two-column frames while retaining the real preview and action objects.
            parent.Find("BagFrame").gameObject.SetActive(false);parent.Find("HeroFrame").gameObject.SetActive(false);
            var left=InventoryPanel(parent,"InventoryColumn",18,82,400,592,new Color(.025f,.055f,.046f,.97f));left.SetSiblingIndex(3);
            var middle=InventoryPanel(parent,"CharacterColumn",430,82,362,592,new Color(.025f,.055f,.046f,.78f));middle.SetSiblingIndex(4);
            desktopDetails=InventoryPanel(parent,"SelectedItemColumn",804,82,456,592,new Color(.025f,.055f,.046f,.98f));desktopDetails.SetSiblingIndex(5);
            var brand=InventoryText(parent,"Brand",30,14,255,56,"FORJA\nDAS MARÉS",24);brand.fontStyle=FontStyle.Bold;brand.color=RusticHudTheme.Gold;brand.lineSpacing=.85f;
            var nav=InventoryPanel(parent,"InventoryNavigation",330,23,690,42,new Color(.025f,.055f,.046f,.96f));
            InventoryText(nav,"Breadcrumb",20,0,290,42,"PERSONAGEM   /   EQUIPAMENTOS",16).color=RusticHudTheme.Gold;
            PlaceInventory(profileSource,660,28,340,32,12);
            PlaceInventory(closeButton,1090,23,160,42,16);
            header.gameObject.SetActive(false);
            RusticHudTheme.Icon(parent,HudSymbol.Bag,InventoryTop,new Vector2(34,-94),27);
            var title=InventoryText(parent,"InventoryTitle",73,93,220,33,"INVENTÁRIO",23);title.fontStyle=FontStyle.Bold;title.color=RusticHudTheme.Gold;
            PlaceInventory(capacity.rectTransform,282,96,124,26);capacity.fontSize=13;capacity.alignment=TextAnchor.MiddleRight;
            PlaceInventory(classFilter,30,130,376,26,12);
            for(int i=0;i<categoryTabs.Length;i++)PlaceInventory(categoryTabs[i],30+i*63,164,60,30,9);
            // Sixteen distinct item instances per page; all categories still share one capacity.
            for(int i=0;i<rows.Length;i++)
            {
                var row=rows[i];PlaceInventory(row,30+i%4*95,207+i/4*87,90,82,11);
                var label=row.GetComponentInChildren<Text>();PlaceInventory(label.rectTransform,3,53,84,26);label.fontSize=11;label.resizeTextMinSize=10;label.resizeTextMaxSize=11;label.alignment=TextAnchor.MiddleCenter;
                PlaceInventory(icons[i].rectTransform,23,3,44,48);
                rowBadges[i].gameObject.SetActive(false);
            }
            PlaceInventory(emptyHint.rectTransform,44,245,348,145);emptyHint.fontSize=18;
            PlaceInventory(clearFilters,44,402,348,38,13);
            PlaceInventory(bagSource,30,564,122,32,11);PlaceInventory(expeditionButton,157,564,122,32,11);PlaceInventory(stockView,284,564,122,32,11);
            PlaceInventory(previous,30,607,76,30,11);previous.GetComponentInChildren<Text>().text="‹ VOLTAR";
            PlaceInventory(next,330,607,76,30,11);next.GetComponentInChildren<Text>().text="MAIS ›";
            PlaceInventory(pageLabel.rectTransform,108,607,220,30);pageLabel.fontSize=12;
            PlaceInventory(sourceHint.rectTransform,32,642,370,25);sourceHint.fontSize=11;
            PlaceInventory(claimAll,32,642,372,26,12);
            // Identity has a hierarchy of its own, rather than one long status sentence.
            PlaceInventory(heroChoice,448,92,326,32,18);
            desktopName=InventoryText(parent,"HeroName",448,128,326,24,"",17);desktopName.alignment=TextAnchor.MiddleCenter;desktopName.color=RusticHudTheme.Muted;
            var level=InventoryPanel(parent,"LevelBadge",450,163,102,43,new Color(.09f,.15f,.12f,.98f));
            desktopLevel=InventoryText(level,"Value",6,0,90,43,"",16);desktopLevel.alignment=TextAnchor.MiddleCenter;desktopLevel.color=RusticHudTheme.Gold;
            var wallet=InventoryPanel(parent,"GoldWallet",564,163,210,43,new Color(.16f,.12f,.055f,.98f));
            var coin=InventoryPanel(wallet,"Coin",12,10,23,23,new Color(.72f,.5f,.15f,1));coin.GetComponent<RoundedHudGraphic>().radius=12;
            InventoryText(coin,"Mark",0,0,23,23,"G",13).alignment=TextAnchor.MiddleCenter;
            desktopGold=InventoryText(wallet,"Amount",44,0,154,43,"",18);desktopGold.color=RusticHudTheme.Gold;
            var xp=InventoryPanel(parent,"ExperienceTrack",450,217,324,7,new Color(.02f,.035f,.03f,1));
            desktopXpFill=RusticHudTheme.Rect(xp,"Fill",Vector2.zero,Vector2.zero,Vector2.zero);desktopXpFill.anchorMax=Vector2.one;desktopXpFill.offsetMin=desktopXpFill.offsetMax=Vector2.zero;var xpImage=desktopXpFill.gameObject.AddComponent<Image>();xpImage.color=new Color(.22f,.78f,.91f);xpImage.raycastTarget=false;
            desktopXp=InventoryText(parent,"ExperienceValue",450,228,324,19,"",12);desktopXp.alignment=TextAnchor.MiddleCenter;desktopXp.color=new Color(.63f,.87f,.9f);
            PlaceInventory(characterPreview.rectTransform,500,248,225,273);
            Vector2[] slots={new Vector2(446,266),new Vector2(714,266),new Vector2(446,350),new Vector2(714,350),new Vector2(714,434)};
            for(int i=0;i<5;i++)
            {
                PlaceInventory(equipmentSlots[i],slots[i].x,slots[i].y,64,72,8);
                PlaceInventory(equipmentSlots[i].GetComponentInChildren<Text>().rectTransform,1,52,62,18);
                PlaceInventory(equippedIcons[i].rectTransform,9,4,46,46);PlaceInventory(emptySlots[i].rectTransform,16,9,32,32);
            }
            sheet.ArrangeInventoryOverview();
            PlaceInventory(notice.rectTransform,30,678,1220,18);notice.fontSize=11;notice.color=RusticHudTheme.Muted;
            // Scrollable real item data and automatic same-slot comparison, always next to the hero.
            var icon=InventoryPanel(desktopDetails,"ItemPortrait",18,18,82,82,RusticHudTheme.Surface);
            var sprite=RusticHudTheme.Rect(icon,"Icon",Vector2.one*.5f,Vector2.zero,new Vector2(68,68));desktopItemIcon=sprite.gameObject.AddComponent<Image>();desktopItemIcon.preserveAspect=true;desktopItemIcon.raycastTarget=false;
            desktopItemName=InventoryText(desktopDetails,"ItemName",114,18,324,50,"Selecione um item",22);desktopItemName.fontStyle=FontStyle.Bold;desktopItemName.resizeTextForBestFit=true;desktopItemName.resizeTextMinSize=17;desktopItemName.resizeTextMaxSize=22;
            desktopItemType=InventoryText(desktopDetails,"ItemType",114,70,324,29,"Veja detalhes e compare antes de equipar.",14);
            desktopState=InventoryText(desktopDetails,"EquipmentState",18,110,420,32,"",13);desktopState.color=new Color(.45f,.94f,.72f);
            var viewport=RusticHudTheme.Rect(desktopDetails,"ItemInformationScroll",InventoryTop,new Vector2(18,-150),new Vector2(420,168));viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=30;
            desktopContent=RusticHudTheme.Rect(viewport,"Content",InventoryTop,Vector2.zero,new Vector2(404,168));scroll.content=desktopContent;
            desktopBody=InventoryText(desktopContent,"Information",5,0,394,168,"Clique na mochila ou em um slot do personagem.\n\nA comparação usa o item equipado no mesmo slot deste herói.",16);desktopBody.alignment=TextAnchor.UpperLeft;desktopBody.supportRichText=true;desktopBody.lineSpacing=1.1f;
            var comparison=InventoryPanel(desktopDetails,"AutomaticComparison",18,328,420,130,new Color(.045f,.105f,.077f,1));
            desktopComparison=InventoryText(comparison,"Changes",12,8,396,115,"Selecione um item para comparar.",13);desktopComparison.alignment=TextAnchor.UpperLeft;
            var track=RusticHudTheme.Rect(viewport,"Scrollbar",new Vector2(1,1),new Vector2(0,0),new Vector2(5,168));track.gameObject.AddComponent<Image>().color=new Color(.12f,.2f,.16f,1);
            var handle=RusticHudTheme.Rect(track,"Handle",Vector2.zero,Vector2.zero,Vector2.zero);handle.anchorMax=Vector2.one;handle.offsetMin=handle.offsetMax=Vector2.zero;var handleImage=handle.gameObject.AddComponent<Image>();handleImage.color=RusticHudTheme.Gold;
            var bar=track.gameObject.AddComponent<Scrollbar>();bar.handleRect=handle;bar.targetGraphic=handleImage;bar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=bar;
            desktopStatus=InventoryText(desktopDetails,"ActionStatus",18,466,420,43,"",12);
            desktopAdvanced=RusticHudTheme.Button(desktopDetails,"FullComparison",InventoryTop,new Vector2(18,-548),new Vector2(420,30),"DETALHES E COMPARAÇÃO COMPLETA",()=>{if(selected==null)return;advancedDetails=false;itemPopup.SetActive(true);itemPopup.transform.SetAsLastSibling();Refresh();ResetInspectionScroll();});desktopAdvanced.GetComponentInChildren<Text>().fontSize=12;
        }
        void RefreshDesktopInventory(PlayerProfile profile,ItemInstance item)
        {
            bool visible=!results;desktopDetails.gameObject.SetActive(visible);
            foreach(string name in new[]{"Brand","InventoryNavigation","InventoryTitle","Icon_Bag"}){var node=overlay.transform.Find(name);if(node!=null)node.gameObject.SetActive(visible);}
            overlay.transform.Find("InventoryColumn").gameObject.SetActive(visible);overlay.transform.Find("CharacterColumn").gameObject.SetActive(visible);
            var hero=ActiveActor.Roster.Current;int level=ProgressionStats.Level(profile.experience,channel.Catalog.maximumLevel);
            header.gameObject.SetActive(results);heroChoice.GetComponentInChildren<Text>().text=RusticHudTheme.ClassNames(channel.Catalog.Hero(hero.heroId).affinity).ToUpperInvariant()+"  ›";
            desktopName.text=hero.displayName.Contains(" - ")?hero.displayName.Substring(hero.displayName.IndexOf(" - ")+3):hero.displayName;
            desktopLevel.text="NÍVEL "+level;desktopGold.text=profile.coins.ToString("N0")+" ouro";
            int current=ProgressionStats.ExperienceInLevel(profile.experience,channel.Catalog.maximumLevel),required=ProgressionStats.RequiredExperience(level);
            bool max=level>=channel.Catalog.maximumLevel;desktopXp.text=max?"NÍVEL MÁXIMO":"XP  "+current+" / "+required;desktopXpFill.anchorMax=new Vector2(max?1:Mathf.Clamp01((float)current/required),1);
            desktopName.gameObject.SetActive(visible);desktopXp.gameObject.SetActive(visible);desktopLevel.transform.parent.gameObject.SetActive(visible);desktopGold.transform.parent.gameObject.SetActive(visible);
            overlay.transform.Find("ExperienceTrack").gameObject.SetActive(visible);
            capacity.text=InventoryRules.BagCount(profile)+" / "+channel.Catalog.inventoryCapacity+" itens";
            sourceHint.rectTransform.sizeDelta=new Vector2(370,25);sourceHint.gameObject.SetActive(visible&&!deliveries);
            sourceHint.text=expeditionView?"Saque desta tentativa · guardado ao terminar.":"Capacidade compartilhada · equipados não ocupam espaço.";
            foreach(var b in new[]{bagSource,stockView,expeditionButton})b.GetComponentInChildren<Text>().fontSize=11;
            desktopAdvanced.interactable=item!=null;
            if(!visible)return;
            bool modal=itemPopup.activeSelf;
            if(desktopSelection!=selected){desktopSelection=selected;desktopContent.anchoredPosition=Vector2.zero;}
            if(item==null)
            {
                desktopItemIcon.enabled=false;desktopItemName.text="Seu próximo equipamento";desktopItemName.color=RusticHudTheme.Gold;desktopItemType.text="Selecione uma peça para inspecionar.";desktopState.text="";
                desktopBody.text="<color=#E8BD6E><b>MONTE SUA COMBINAÇÃO</b></color>\n\nSelecione um item na mochila para ver seus atributos, ataque e efeitos.\n\nA comparação será feita automaticamente com o equipamento do mesmo slot.\n\nClique nos ícones ao redor do herói para inspecionar ou desequipar uma peça.";desktopStatus.text="";desktopComparison.text="A comparação usa apenas o item equipado no mesmo slot do herói. Outros itens da mochila não são usados como referência.";
            }
            else
            {
                var d=channel.Catalog.Find(item.definitionId);var rarity=channel.Catalog.Rarity(item.rarity).color;desktopItemIcon.sprite=d.icon;desktopItemIcon.enabled=d.icon!=null;
                desktopItemName.text=d.displayName;desktopItemName.color=rarity;desktopItemType.text=RusticHudTheme.RarityName(item.rarity)+" · "+ItemDescriptions.Category(d);
                desktopState.text=deliveries?"ENTREGA · receba antes de equipar":ItemDescriptions.EquipmentState(profile,channel.Catalog,hero,item);
                var text=new StringBuilder();
                Section(text,"IDENTIDADE",RusticHudTheme.SlotName(d.slot)+" · "+RusticHudTheme.ClassNames(d.compatibleHeroes)+" · nível "+d.minimumLevel+(d.twoHanded?"\nDuas mãos":d.allowsOffHand?"\nPermite mão secundária":""));
                if(d.attacks!=null&&d.attacks.Length>0){var attack=d.attacks[0];Section(text,"ATAQUE DA ARMA",Mathf.RoundToInt(attack.damage*channel.Catalog.Rarity(item.rarity).power)+" dano sem bônus do herói\nAlcance  "+attack.range.ToString("0.#")+" m · ciclo "+(attack.windup+attack.recovery).ToString("0.##")+" s\n"+ItemDescriptions.Pattern(attack));}
                var bonuses=ItemDescriptions.Bonuses(channel.Catalog,item);var bonus=new StringBuilder();for(int i=0;i<bonuses.Length;i++)if(bonuses[i]!=0)bonus.AppendLine("+"+ItemDescriptions.Amount((ItemStat)i,bonuses[i])+" "+ItemDescriptions.Name((ItemStat)i));
                Section(text,"BÔNUS DO ITEM",bonus.Length>0?bonus.ToString().TrimEnd():"Sem bônus passivos.");
                string effect="";if(d.ability!=null)effect+="Habilidade: "+d.ability.displayName+"\n";if(d.perk!=WeaponPerk.None)effect+=CombatPerkDescriptions.Perk(d.perk)+"\n";if(d.synergy!=BuildSynergy.None)effect+=CombatPerkDescriptions.Synergy(d.synergy);
                if(!string.IsNullOrWhiteSpace(effect))Section(text,"EFEITO ESPECIAL",effect.TrimEnd());
                var set=profile.Loadout(hero.heroId);int slot=(int)d.slot;if(set!=null)for(int i=0;i<set.items.Length;i++)if(set.items[i]==item.instanceId){slot=i;break;}
                var other=set!=null?profile.Item(set.items[slot]):null;
                desktopComparison.text="<color=#E8BD6E><b>COMPARAÇÃO AO EQUIPAR</b></color>\n"+(other==null?"Slot vazio":other.instanceId==item.instanceId?"Já equipado": "Equipado: "+channel.Catalog.Find(other.definitionId).displayName)+"\n\n"+ItemDescriptions.QuickPreview(hero,profile,channel.Catalog,item);
                if(!string.IsNullOrWhiteSpace(d.description))Section(text,"SOBRE A PEÇA",d.description);
                desktopBody.text=text.ToString();desktopStatus.text=itemContextNotice;
            }
            float height=Mathf.Max(168,desktopBody.preferredHeight+12);desktopBody.rectTransform.sizeDelta=new Vector2(394,height);desktopContent.sizeDelta=new Vector2(404,height);
            if(!modal)
            {
                foreach(var b in new[]{equip,remove,discard,secondaryEquip})b.transform.SetParent(desktopDetails,false);
                bool equipped=item!=null&&!deliveries&&EquippedHere(profile,item.instanceId);
                equip.gameObject.SetActive(item!=null&&!equipped);remove.gameObject.SetActive(item!=null&&equipped);discard.gameObject.SetActive(item!=null&&!equipped&&!deliveries);
                PlaceInventory(equip,18,510,204,32,13);PlaceInventory(remove,18,510,420,32,13);PlaceInventory(discard,234,510,204,32,11);
                bool dagger=item!=null&&!deliveries&&channel.Catalog.Find(item.definitionId).category==WeaponCategory.Daggers;
                secondaryEquip.gameObject.SetActive(dagger);if(dagger){PlaceInventory(secondaryEquip,234,548,204,30,11);PlaceInventory(desktopAdvanced,18,548,204,30,10);}else PlaceInventory(desktopAdvanced,18,548,420,30,12);
            }
        }
        static void Section(StringBuilder text,string title,string content)
        {text.Append("<color=#E8BD6E><b>").Append(title).AppendLine("</b></color>");text.AppendLine(content);text.AppendLine();}
    }
}
