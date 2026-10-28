using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    public sealed partial class CoopInventoryHud
    {
        readonly List<GameObject> bagNodes=new List<GameObject>();
        RectTransform areaNavigation;GameObject characterArea,statusArea,resourceArea;
        readonly Button[] areaButtons=new Button[5];readonly Image[] areaEquipmentIcons=new Image[5];
        readonly Text[] areaEquipmentNames=new Text[5];readonly HudIcon[] areaEmptySlots=new HudIcon[5];
        Button changeProfileHero,resourceScopeButton;Text characterXp;RectTransform characterXpFill;
        Text areaHeading,characterIdentity,characterRole,characterSummary,resourceGold,resourceSummary,statusDescription;
        readonly Text[] statNames=new Text[16],statValues=new Text[16],statExplanations=new Text[16];
        readonly GameObject[] statRows=new GameObject[16];readonly Button[] statTabs=new Button[3];
        int activeArea=1,statCategory;RawImage areaPortrait;
        public int ProfileArea=>activeArea;
        void LateUpdate(){if(areaNavigation!=null&&IsOpen&&!results&&activeArea==3&&!sheet.IsOpen)NavigateProfile(0);}
        static readonly string[] AreaLabels={"PERSONAGEM","MOCHILA","STATUS","TALENTOS","RECURSOS"};
        void BuildProfileAreas()
        {
            foreach(Transform child in overlay.transform)bagNodes.Add(child.gameObject);
            paperDoll.SetBackdrop(new Color(.035f,.05f,.045f,0));
            areaHeading=InventoryText(overlay.transform,"AreaHeading",325,22,740,43,"MOCHILA · ITENS E EQUIPAMENTOS",23);areaHeading.color=RusticHudTheme.Gold;
            overlay.transform.Find("InventoryNavigation").gameObject.SetActive(false);
            characterArea=InventoryPanel(overlay.transform,"CharacterPage",18,82,1242,566,new Color(.02f,.05f,.045f,.95f)).gameObject;
            var art=MenuArtwork.Fill(characterArea.transform,"CharacterBackdrop","MenuArt/RoutesBackdrop",new Color(.35f,.45f,.4f,1));art.transform.SetAsFirstSibling();
            var identity=InventoryPanel(characterArea.transform,"Identity",20,20,335,526,new Color(.025f,.05f,.045f,.97f));
            characterIdentity=InventoryText(identity,"Name",20,18,295,90,"",26);characterIdentity.color=RusticHudTheme.Gold;
            var xpTrack=RusticHudTheme.Rect(identity,"ExperienceTrack",InventoryTop,new Vector2(20,-105),new Vector2(295,7));xpTrack.gameObject.AddComponent<Image>().color=new Color(.1f,.17f,.15f,1);
            characterXpFill=RusticHudTheme.Rect(xpTrack,"Fill",Vector2.zero,Vector2.zero,Vector2.zero);characterXpFill.anchorMax=Vector2.one;characterXpFill.offsetMin=characterXpFill.offsetMax=Vector2.zero;characterXpFill.gameObject.AddComponent<Image>().color=new Color(.2f,.8f,.95f);
            characterXp=InventoryText(identity,"Experience",20,115,295,19,"",12);
            characterRole=InventoryText(identity,"Role",20,142,295,160,"",17);characterRole.alignment=TextAnchor.UpperLeft;
            var roleViewport=RusticHudTheme.Rect(identity,"RoleScroll",InventoryTop,new Vector2(20,-142),new Vector2(295,160));roleViewport.gameObject.AddComponent<Image>().color=Color.clear;roleViewport.gameObject.AddComponent<RectMask2D>();
            var roleScroll=roleViewport.gameObject.AddComponent<ScrollRect>();roleScroll.viewport=roleViewport;roleScroll.horizontal=false;roleScroll.movementType=ScrollRect.MovementType.Clamped;roleScroll.scrollSensitivity=24;
            characterRole.transform.SetParent(roleViewport,false);PlaceInventory(characterRole.rectTransform,0,0,295,160);roleScroll.content=characterRole.rectTransform;
            characterSummary=InventoryText(identity,"Summary",20,310,295,140,"",18);characterSummary.alignment=TextAnchor.UpperLeft;
            RusticHudTheme.Button(identity,"OpenFullStats",InventoryTop,new Vector2(20,-465),new Vector2(295,42),"VER TODOS OS STATUS",()=>NavigateProfile(2));
            changeProfileHero=RusticHudTheme.Button(identity,"ChangeHero",InventoryTop,new Vector2(20,-414),new Vector2(295,38),"TROCAR HERÓI",()=>{director.SelectHero((director.SelectedHero+1)%director.HeroCount);selected=null;Refresh();});
            var portraitStage=InventoryPanel(characterArea.transform,"PortraitStage",420,20,370,510,new Color(.025f,.04f,.035f,1));
            var portrait=RusticHudTheme.Rect(portraitStage,"EquippedHero",InventoryTop,new Vector2(5,-5),new Vector2(360,500));areaPortrait=portrait.gameObject.AddComponent<RawImage>();areaPortrait.raycastTarget=false;
            for(int i=0;i<5;i++)
            {
                int slot=i;var b=RusticHudTheme.Button(characterArea.transform,"InspectSlot_"+i,InventoryTop,new Vector2(860,-20-i*103),new Vector2(350,92),"",()=>{NavigateProfile(1);SelectEquipped(slot);});
                b.GetComponentInChildren<Text>().gameObject.SetActive(false);
                var icon=RusticHudTheme.Rect(b.transform,"Item",InventoryTop,new Vector2(12,-10),new Vector2(68,68));areaEmptySlots[i]=RusticHudTheme.Icon(b.transform,SlotSymbol((EquipmentSlot)i),InventoryTop,new Vector2(24,-22),44);areaEmptySlots[i].color=new Color(.5f,.6f,.55f,.5f);areaEquipmentIcons[i]=icon.gameObject.AddComponent<Image>();areaEquipmentIcons[i].preserveAspect=true;areaEquipmentIcons[i].raycastTarget=false;
                areaEquipmentNames[i]=InventoryText(b.transform,"Identity",94,12,244,68,"",16);
            }
            statusArea=InventoryPanel(overlay.transform,"StatusPage",18,82,1242,566,new Color(.02f,.05f,.045f,.98f)).gameObject;
            string[] tabs={"COMBATE","SOBREVIVÊNCIA","MOBILIDADE E HABILIDADES"};
            for(int i=0;i<3;i++){int pick=i;statTabs[i]=RusticHudTheme.Button(statusArea.transform,"StatusTab_"+i,InventoryTop,new Vector2(20+i*402,-18),new Vector2(390,42),tabs[i],()=>{statCategory=pick;RefreshProfileAreas();});}
            statusDescription=InventoryText(statusArea.transform,"FormulaNote",24,75,1190,48,"",16);statusDescription.color=RusticHudTheme.Muted;
            var viewport=RusticHudTheme.Rect(statusArea.transform,"StatusScroll",InventoryTop,new Vector2(20,-132),new Vector2(1202,410));viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=30;
            var content=RusticHudTheme.Rect(viewport,"Content",InventoryTop,Vector2.zero,new Vector2(1180,16*70));scroll.content=content;
            var track=RusticHudTheme.Rect(viewport,"Scrollbar",new Vector2(1,1),new Vector2(-2,0),new Vector2(8,410));track.gameObject.AddComponent<Image>().color=new Color(.1f,.17f,.14f,1);
            var handle=RusticHudTheme.Rect(track,"Handle",Vector2.zero,Vector2.zero,Vector2.zero);handle.anchorMax=Vector2.one;handle.offsetMin=handle.offsetMax=Vector2.zero;var handleImage=handle.gameObject.AddComponent<Image>();handleImage.color=RusticHudTheme.Gold;
            var scrollbar=track.gameObject.AddComponent<Scrollbar>();scrollbar.handleRect=handle;scrollbar.targetGraphic=handleImage;scrollbar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=scrollbar;
            for(int i=0;i<16;i++)
            {
                var row=InventoryPanel(content,"StatRow_"+i,0,i*70,1180,62,new Color(.05f,.095f,.078f,1));statRows[i]=row.gameObject;
                statNames[i]=InventoryText(row,"Name",16,8,300,44,"",19);
                statValues[i]=InventoryText(row,"Value",326,8,190,44,"",22);statValues[i].color=new Color(.4f,.9f,.95f);
                statExplanations[i]=InventoryText(row,"Meaning",535,8,625,46,"",15);statExplanations[i].color=RusticHudTheme.Muted;
            }
            resourceArea=InventoryPanel(overlay.transform,"ResourcesPage",18,82,1242,566,new Color(.02f,.05f,.045f,.98f)).gameObject;
            InventoryText(resourceArea.transform,"Title",28,22,1100,42,"RECURSOS · CARTEIRA E ARMAZENAMENTO",26).color=RusticHudTheme.Gold;
            var wallet=InventoryPanel(resourceArea.transform,"GoldCard",28,90,480,180,new Color(.15f,.11f,.045f,1));
            RusticHudTheme.Icon(wallet,HudSymbol.Bag,InventoryTop,new Vector2(20,-22),45);
            resourceGold=InventoryText(wallet,"Balance",85,20,370,65,"",32);resourceGold.color=RusticHudTheme.Gold;
            InventoryText(wallet,"Meaning",24,100,430,60,"OURO · moeda do perfil ativo\nObtido em combate e ao reciclar equipamentos.",18);
            resourceSummary=InventoryText(resourceArea.transform,"Storage",550,100,645,220,"",22);resourceSummary.alignment=TextAnchor.UpperLeft;
            resourceScopeButton=RusticHudTheme.Button(resourceArea.transform,"SwitchProfile",InventoryTop,new Vector2(550,-270),new Vector2(645,40),"ALTERNAR PERFIL LOCAL / ÚLTIMA SALA",()=>{if(channel.HomeMode&&channel.Hub.HasRemote){profileSource.onClick.Invoke();Refresh();}else channel.SetNotice("Nenhum outro perfil de sala disponível neste aparelho.");});
            InventoryText(resourceArea.transform,"Scope",32,330,1160,92,"O perfil deste aparelho e o perfil de uma sala podem ter saldos diferentes.\nAqui aparecem somente os recursos já existentes no perfil selecionado.\nItens equipados não ocupam espaço na mochila.",18);
            RusticHudTheme.Button(resourceArea.transform,"OpenDeliveries",InventoryTop,new Vector2(32,-450),new Vector2(550,52),"ABRIR ENTREGAS",()=>{NavigateProfile(1);SelectSource(2);});
            RusticHudTheme.Button(resourceArea.transform,"OpenBag",InventoryTop,new Vector2(610,-450),new Vector2(550,52),"ORGANIZAR MOCHILA",()=>NavigateProfile(1));
            areaNavigation=InventoryPanel(overlay.transform,"ProfileBottomNavigation",18,646,1242,48,new Color(.025f,.055f,.045f,1));
            for(int i=0;i<5;i++){int pick=i;areaButtons[i]=RusticHudTheme.Button(areaNavigation,"ProfileArea_"+i,InventoryTop,new Vector2(8+i*246,-5),new Vector2(238,38),AreaLabels[i],()=>NavigateProfile(pick));}
            HudSymbol[] symbols={HudSymbol.Hero,HudSymbol.Bag,HudSymbol.Shield,HudSymbol.Rune,HudSymbol.Artifact};
            for(int i=0;i<5;i++){RusticHudTheme.Icon(areaButtons[i].transform,symbols[i],InventoryTop,new Vector2(14,-8),22);var label=areaButtons[i].GetComponentInChildren<Text>();label.rectTransform.anchoredPosition=new Vector2(16,0);label.rectTransform.sizeDelta=new Vector2(190,36);label.fontSize=15;}
            characterArea.SetActive(false);statusArea.SetActive(false);resourceArea.SetActive(false);
        }
        public void NavigateProfile(int area)
        {
            if(areaNavigation==null)return;activeArea=Mathf.Clamp(area,0,4);sheet.Hide();itemPopup.SetActive(false);
            Refresh();if(activeArea==3){sheet.OpenTalents();var talents=overlay.transform.Find("CharacterTalents");talents.localScale=Vector3.one*.84f;((RectTransform)talents).anchoredPosition=new Vector2(0,18);}
        }
        void RefreshProfileAreas()
        {
            if(areaNavigation==null)return;
            bool normal=!results,bag=normal&&activeArea==1;
            foreach(var node in bagNodes)
            {
                if(node!=null&&(node.name=="InventoryBackdrop"||node.name=="RusticSurface")){node.SetActive(true);continue;}
                if(node==null||node==itemPopup||node.name=="CharacterTalents"||node==closeButton.gameObject)continue;
                if(!normal)continue;
                if(!bag)node.SetActive(false);
            }
            areaNavigation.gameObject.SetActive(normal);areaHeading.gameObject.SetActive(normal);
            if(!normal){characterArea.SetActive(false);statusArea.SetActive(false);resourceArea.SetActive(false);return;}
            foreach(string name in new[]{"Brand","CharacterColumn","HeroName","HeroPreview","LevelBadge","GoldWallet","ExperienceTrack","ExperienceValue","InventoryNavigation"}){var n=overlay.transform.Find(name);if(n!=null)n.gameObject.SetActive(name=="Brand");}
            profileSource.gameObject.SetActive(false);
            heroChoice.gameObject.SetActive(false);foreach(var slot in equipmentSlots)slot.gameObject.SetActive(false);sheet.SetSummaryVisible(false);
            areaHeading.text=AreaLabels[activeArea]+(activeArea==1?" · ITENS E EQUIPAMENTOS":activeArea==2?" · FICHA DETALHADA":"");
            characterArea.SetActive(activeArea==0);statusArea.SetActive(activeArea==2);resourceArea.SetActive(activeArea==4);
            for(int i=0;i<5;i++){var frame=areaButtons[i].GetComponent<RoundedHudGraphic>();frame.borderColor=i==activeArea?RusticHudTheme.Gold:RusticHudTheme.Copper;frame.border=i==activeArea?3:1;frame.SetVerticesDirty();}
            if(bag)
            {
                PlaceInventory(desktopDetails,804,82,456,560);
                PlaceInventory(desktopStatus.rectTransform,18,458,420,25);
                if(!itemPopup.activeSelf){PlaceInventory(equip,18,486,204,32,13);PlaceInventory(remove,18,486,420,32,13);PlaceInventory(discard,234,486,204,32,11);PlaceInventory(secondaryEquip,234,526,204,30,11);PlaceInventory(desktopAdvanced,18,526,secondaryEquip.gameObject.activeSelf?204:420,30,12);}
                var panel=(RectTransform)overlay.transform.Find("InventoryColumn");PlaceInventory(panel,18,82,774,560);
                for(int i=0;i<rows.Length;i++){PlaceInventory(rows[i],30+i%4*190,207+i/4*85,180,80,13);PlaceInventory(icons[i].rectTransform,62,2,52,48);PlaceInventory(rows[i].GetComponentInChildren<Text>().rectTransform,6,52,168,25);}
                PlaceInventory(classFilter,30,130,750,26,13);for(int i=0;i<6;i++)PlaceInventory(categoryTabs[i],30+i*126,164,120,30,12);
                PlaceInventory(capacity.rectTransform,600,96,180,26);PlaceInventory(emptyHint.rectTransform,70,260,660,120);PlaceInventory(clearFilters,200,400,390,40);
                PlaceInventory(bagSource,30,553,240,34,13);PlaceInventory(expeditionButton,282,553,240,34,13);PlaceInventory(stockView,534,553,240,34,13);
                PlaceInventory(previous,30,589,120,32,12);PlaceInventory(next,654,589,120,32,12);PlaceInventory(pageLabel.rectTransform,250,589,320,32);
                PlaceInventory(sourceHint.rectTransform,180,622,550,20);sourceHint.fontSize=11;PlaceInventory(claimAll,180,622,550,20,11);notice.gameObject.SetActive(false);
            }
            var profile=channel.LocalProfile;if(profile==null)return;var hero=ActiveActor.Roster.Current;var catalog=channel.Catalog;var s=ProgressionStats.Calculate(hero,profile,catalog);
            if(activeArea==0)
            {
                changeProfileHero.interactable=channel.HomeMode||director.Phase==CoopPhase.Lobby;
                int xp=ProgressionStats.ExperienceInLevel(profile.experience,catalog.maximumLevel),required=ProgressionStats.RequiredExperience(s.level);bool maximum=s.level>=catalog.maximumLevel;
                characterXp.text=maximum?"NÍVEL MÁXIMO":"EXPERIÊNCIA   "+xp+" / "+required;characterXpFill.anchorMax=new Vector2(maximum?1:Mathf.Clamp01((float)xp/required),1);
                areaPortrait.texture=characterPreview.texture;characterIdentity.text=hero.displayName+"\nNÍVEL "+s.level;
                characterRole.text=hero.role+"\n\nPONTOS FORTES\n"+hero.strengths+"\n\nLIMITAÇÕES\n"+hero.weaknesses;characterRole.fontSize=15;characterRole.rectTransform.sizeDelta=new Vector2(295,Mathf.Max(160,characterRole.preferredHeight+8));
                characterSummary.text="Vida máxima   "+s.maximumHealth+"\nDano por acerto   "+s.damage+"\nDefesa   "+s.defensePoints.ToString("0.#")+"%\nMovimento   "+s.speed.ToString("0.#")+" m/s\nPontos de talento   "+HeroTalents.Available(profile,hero.heroId,catalog);
                var set=profile.Loadout(hero.heroId);for(int i=0;i<5;i++){var item=set!=null?profile.Item(set.items[i]):null;var def=item!=null?catalog.Find(item.definitionId):null;areaEquipmentIcons[i].sprite=def?.icon;areaEquipmentIcons[i].enabled=def?.icon!=null;areaEmptySlots[i].gameObject.SetActive(def?.icon==null);areaEquipmentNames[i].text=RusticHudTheme.SlotName((EquipmentSlot)i)+"\n"+(def!=null?def.displayName:"Slot vazio · ver itens compatíveis");}
            }
            if(activeArea==4){resourceScopeButton.interactable=channel.HomeMode&&channel.Hub.HasRemote;resourceScopeButton.GetComponentInChildren<Text>().text=resourceScopeButton.interactable?"TROCAR PERFIL · APARELHO / ÚLTIMA SALA":"PERFIL DESTE APARELHO";resourceGold.text=profile.coins.ToString("N0")+" ouro";resourceSummary.text="MOCHILA   "+InventoryRules.BagCount(profile)+" / "+catalog.inventoryCapacity+"\n\nENTREGAS   "+profile.deliveries.Count+" itens\n\nSAQUE DA EXPEDIÇÃO   "+(profile.expedition?.items.Count??0)+" itens";}
            if(activeArea!=2)return;
            for(int i=0;i<3;i++)statTabs[i].interactable=i!=statCategory;
            statusDescription.text="Valores permanentes do perfil ativo: base, nível, equipamentos e talentos. Bônus temporários de combate não entram nesta ficha.";
            var data=new List<string[]>();
            if(statCategory==0)
            {
                data.Add(new[]{"Dano por acerto",s.damage.ToString(),"Antes da defesa do alvo; varia com o golpe e a arma."});
                data.Add(new[]{"Dano base da arma",s.baseDamage.ToString(),"Base do primeiro ataque, antes dos bônus."});
                data.Add(new[]{"Chance crítica",(s.criticalChance*100).ToString("0.#")+"%","Probabilidade de um acerto crítico."});
                data.Add(new[]{"Dano crítico",s.criticalDamage.ToString(),"Dano do primeiro ataque quando o acerto é crítico."});
                data.Add(new[]{"Multiplicador crítico",s.criticalMultiplier.ToString("0.##")+"×","Multiplica o dano de um acerto normal."});
                data.Add(new[]{"Cadência",s.attacksPerSecond.ToString("0.##")+" /s","Ataques por segundo, considerando preparação e recuperação."});
                data.Add(new[]{"Bônus elemental",(s.elementalDamage*100).ToString("0.#")+"%","Bônus aplicado pelo cálculo atual de dano elemental."});
            }
            else if(statCategory==1)
            {
                data.Add(new[]{"Vida máxima",s.maximumHealth.ToString(),"Quantidade máxima de vida deste herói."});
                data.Add(new[]{"Defesa",s.defensePoints.ToString("0.#"),"Cada ponto corresponde a 1% de redução; sujeito ao limite."});
                data.Add(new[]{"Redução de dano",(s.defense*100).ToString("0.#")+"%","100 de dano bruto → "+ProgressionStats.MitigatedDamage(100,s.defense)+" recebido, sem outras proteções."});
                data.Add(new[]{"Regeneração",s.recovery.ToString("0.##")+" vida/s","Recuperação concedida pelos modificadores do perfil."});
                data.Add(new[]{"Bloqueio frontal",(s.block*100).ToString("0.#")+"%","Proteção frontal configurada pelo escudo equipado."});
            }
            else
            {
                data.Add(new[]{"Movimento",s.speed.ToString("0.##")+" m/s","Velocidade sem efeitos temporários."});
                data.Add(new[]{"Redução de recarga",((1-s.cooldownMultiplier)*100).ToString("0.#")+"%","Reduz o tempo de espera das habilidades compatíveis."});
                data.Add(new[]{"Velocidade de ataque",s.attackSpeed.ToString("0.##")+"×","Bônus de cadência; já considerado em ataques por segundo."});
                var equipped=profile.Item(profile.Loadout(hero.heroId)?.items[0]);var weapon=equipped!=null?catalog.Find(equipped.definitionId):null;var attack=weapon!=null?weapon.attacks[0]:hero.attacks[0];
                data.Add(new[]{"Alcance do primeiro golpe",attack.range.ToString("0.#")+" m","Distância configurada para o primeiro ataque da arma atual."});
                data.Add(new[]{"Duração da esquiva",hero.dodge.duration.ToString("0.##")+" s","Tempo do deslocamento de esquiva."});
                data.Add(new[]{"Recarga base da esquiva",hero.dodge.cooldown.ToString("0.##")+" s","Valor base da classe, antes de ajustes temporários."});
            }
            for(int i=0;i<16;i++){statRows[i].SetActive(i<data.Count);if(i<data.Count){statNames[i].text=data[i][0];statValues[i].text=data[i][1];statExplanations[i].text=data[i][2];}}
            var rect=(RectTransform)statRows[0].transform.parent;rect.sizeDelta=new Vector2(1180,Mathf.Max(410,data.Count*70));
        }
    }
}
