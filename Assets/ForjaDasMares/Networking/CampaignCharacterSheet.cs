using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    // Talent presentation follows catalog rules; the host/profile service owns purchases.
    public sealed class CampaignCharacterSheet:MonoBehaviour
    {
        CoopDirector session;CoopProfileChannel channel;GameObject popup;
        readonly Text[] stats=new Text[6];readonly GameObject[] overview=new GameObject[7];
        Image[] customIcons;HudIcon[] fallbackIcons;Image customDetailIcon;
        Button[] choices;Text[] choiceNames,choiceRanks,choiceDescriptions;RectTransform[] choiceFill;
        Text heading,resources,points,heroSummary,detailTitle,detailRank,description,currentBonus,nextBonus,perPoint,status;
        Button improve,reset;RectTransform xpFill;RawImage portrait,portraitSource;HudIcon detailIcon;RoundedHudGraphic detailFrame;
        bool confirm,pending;int selected,requestRevision,detailSelection=-1;string requestMessage,operationNotice,lastHero;float refresh;
        CoopActor Actor=>session.Actor(session.LocalSlot>=0?session.LocalSlot:0);
        static readonly Vector2 Top=new Vector2(0,1);
        static Text Label(Transform parent,string name,Vector2 pos,Vector2 size,string value,int font=18)
        {var t=RusticHudTheme.Label(parent,name,Top,pos,size,value,font);t.alignment=TextAnchor.UpperLeft;return t;}
        static RectTransform Panel(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
        {var r=RusticHudTheme.Rect(parent,name,Top,pos,size);RusticHudTheme.Panel(r,color,16);return r;}
        static RectTransform Meter(Transform parent,string name,Vector2 pos,Vector2 size,Color tint)
        {
            var r=RusticHudTheme.Rect(parent,name,Top,pos,size);r.gameObject.AddComponent<Image>().color=new Color(.025f,.055f,.05f,1);
            var fill=RusticHudTheme.Rect(r,"Fill",Vector2.zero,Vector2.zero,Vector2.zero);fill.anchorMax=Vector2.one;fill.offsetMin=fill.offsetMax=Vector2.zero;var image=fill.gameObject.AddComponent<Image>();image.color=tint;image.raycastTarget=false;return fill;
        }
        static HudSymbol Symbol(ItemStat stat)=>stat==ItemStat.Health?HudSymbol.Heart:stat==ItemStat.Defense?HudSymbol.Shield:stat==ItemStat.AttackSpeed?HudSymbol.Dodge:stat==ItemStat.CriticalChance?HudSymbol.Target:stat==ItemStat.CooldownReduction?HudSymbol.Hourglass:HudSymbol.Sword;
        static Color Tint(ItemStat stat)=>stat==ItemStat.Health?new Color(1,.43f,.4f):stat==ItemStat.Defense?new Color(.35f,.7f,1):stat==ItemStat.AttackSpeed?new Color(.35f,.95f,.65f):stat==ItemStat.CriticalChance?new Color(.8f,.5f,1):stat==ItemStat.CooldownReduction?new Color(.35f,.9f,.95f):new Color(1,.7f,.3f);
        static string Purpose(ItemStat stat)=>stat==ItemStat.Health?"Aumenta a vida máxima do herói.":stat==ItemStat.Damage?"Aumenta o dano dos seus ataques.":stat==ItemStat.Defense?"Aumenta a redução do dano recebido.":stat==ItemStat.AttackSpeed?"Permite atacar com maior frequência.":stat==ItemStat.CriticalChance?"Aumenta a chance de causar um acerto crítico.":stat==ItemStat.CooldownReduction?"Reduz a espera para usar habilidades novamente.":"Melhora "+ItemDescriptions.Name(stat)+".";
        public void Build(Transform parent,CoopDirector owner,CoopProfileChannel profiles,RawImage preview=null)
        {
            session=owner;channel=profiles;portraitSource=preview;
            for(int i=0;i<6;i++)
            {
                var r=Panel(parent,"AttributeCard_"+i,new Vector2(30+i%3*158,-382-i/3*58),new Vector2(150,52),RusticHudTheme.Surface);
                overview[i]=r.gameObject;stats[i]=Label(r,"Value",new Vector2(8,-4),new Vector2(138,46),"",14);
            }
            overview[6]=RusticHudTheme.Button(parent,"Talents",Top,new Vector2(30,-505),new Vector2(465,38),"ATRIBUTOS E TALENTOS",Open).gameObject;
            var modal=RusticHudTheme.Rect(parent,"CharacterTalents",Vector2.one*.5f,Vector2.zero,new Vector2(1160,650));popup=modal.gameObject;
            var surface=RusticHudTheme.Panel(modal,new Color(.035f,.065f,.055f,1),22);surface.raycastTarget=true;
            var art=MenuArtwork.Fill(modal,"TalentBackdrop","MenuArt/RoutesBackdrop",new Color(.24f,.32f,.29f,1));art.rectTransform.offsetMin=new Vector2(8,8);art.rectTransform.offsetMax=new Vector2(-8,-8);art.transform.SetAsFirstSibling();
            heading=Label(modal,"HeroName",new Vector2(25,-12),new Vector2(710,35),"",24);heading.color=RusticHudTheme.Gold;
            points=Label(modal,"FreePoints",new Vector2(760,-17),new Vector2(225,30),"",20);points.color=new Color(.4f,1,.8f);
            resources=Label(modal,"Resources",new Vector2(25,-48),new Vector2(940,20),"",15);
            RusticHudTheme.Button(modal,"CloseTalents",new Vector2(1,1),new Vector2(-20,-15),new Vector2(140,42),"VOLTAR",Hide);
            var hero=Panel(modal,"HeroSummary",new Vector2(20,-75),new Vector2(285,480),new Color(.035f,.065f,.06f,.96f));
            Label(hero,"Title",new Vector2(16,-10),new Vector2(253,26),"SEU HERÓI",18).color=RusticHudTheme.Gold;
            var portraitRect=RusticHudTheme.Rect(hero,"Portrait",Top,new Vector2(42,-40),new Vector2(200,260));portrait=portraitRect.gameObject.AddComponent<RawImage>();portrait.raycastTarget=false;
            xpFill=Meter(hero,"Experience",new Vector2(16,-307),new Vector2(253,9),new Color(.3f,.85f,.95f));
            heroSummary=Label(hero,"Stats",new Vector2(16,-335),new Vector2(253,135),"",16);
            var list=Panel(modal,"TalentList",new Vector2(320,-75),new Vector2(435,480),new Color(.035f,.065f,.055f,.96f));
            Label(list,"Title",new Vector2(12,-8),new Vector2(410,24),"TALENTOS · selecione para ver",17).color=RusticHudTheme.Gold;
            var viewport=RusticHudTheme.Rect(list,"Viewport",Top,new Vector2(8,-40),new Vector2(419,430));viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=35;
            int count=channel.Catalog.talents.Length;
            var content=RusticHudTheme.Rect(viewport,"Content",Top,Vector2.zero,new Vector2(405,Mathf.Max(430,count*88+8)));scroll.content=content;
            var track=RusticHudTheme.Rect(viewport,"Scrollbar",new Vector2(1,1),new Vector2(-2,-2),new Vector2(7,426));track.gameObject.AddComponent<Image>().color=new Color(.15f,.2f,.17f,1);
            var handle=RusticHudTheme.Rect(track,"Handle",Vector2.zero,Vector2.zero,Vector2.zero);handle.anchorMax=Vector2.one;handle.offsetMin=handle.offsetMax=Vector2.zero;var handleImage=handle.gameObject.AddComponent<Image>();handleImage.color=RusticHudTheme.Gold;
            var bar=track.gameObject.AddComponent<Scrollbar>();bar.handleRect=handle;bar.targetGraphic=handleImage;bar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=bar;
            choices=new Button[count];choiceNames=new Text[count];choiceRanks=new Text[count];choiceDescriptions=new Text[count];choiceFill=new RectTransform[count];customIcons=new Image[count];fallbackIcons=new HudIcon[count];
            for(int i=0;i<count;i++)
            {
                int pick=i;var rule=channel.Catalog.talents[i];var tint=Tint(rule.stat);
                choices[i]=RusticHudTheme.Button(content,"Talent_"+rule.id,Top,new Vector2(0,-4-i*88),new Vector2(403,80),"",()=>{selected=pick;confirm=false;Refresh();});
                choices[i].GetComponentInChildren<Text>().gameObject.SetActive(false);
                var icon=Panel(choices[i].transform,"IconFrame",new Vector2(10,-10),new Vector2(60,60),Color.Lerp(new Color(.035f,.05f,.04f,1),tint,.17f));icon.GetComponent<RoundedHudGraphic>().borderColor=tint;
                fallbackIcons[i]=RusticHudTheme.Icon(icon,Symbol(rule.stat),Vector2.one*.5f,Vector2.zero,40);fallbackIcons[i].color=tint;
                var spriteRect=RusticHudTheme.Rect(icon,"CustomIcon",Vector2.one*.5f,Vector2.zero,Vector2.one*46);customIcons[i]=spriteRect.gameObject.AddComponent<Image>();customIcons[i].preserveAspect=true;customIcons[i].raycastTarget=false;
                choiceNames[i]=Label(choices[i].transform,"Name",new Vector2(82,-8),new Vector2(220,26),rule.displayName,20);
                choiceRanks[i]=Label(choices[i].transform,"Rank",new Vector2(322,-11),new Vector2(65,22),"",17);
                choiceDescriptions[i]=Label(choices[i].transform,"Effect",new Vector2(82,-37),new Vector2(310,26),"",14);
                choiceFill[i]=Meter(choices[i].transform,"Progress",new Vector2(82,-69),new Vector2(307,4),tint);
            }
            var detail=Panel(modal,"TalentDetail",new Vector2(770,-75),new Vector2(370,480),new Color(.035f,.065f,.055f,.98f));
            var glyph=Panel(detail,"SelectedIcon",new Vector2(18,-16),new Vector2(76,76),RusticHudTheme.Surface);detailFrame=glyph.GetComponent<RoundedHudGraphic>();detailIcon=RusticHudTheme.Icon(glyph,HudSymbol.Heart,Vector2.one*.5f,Vector2.zero,52);
            var detailSprite=RusticHudTheme.Rect(glyph,"CustomIcon",Vector2.one*.5f,Vector2.zero,Vector2.one*60);customDetailIcon=detailSprite.gameObject.AddComponent<Image>();customDetailIcon.preserveAspect=true;customDetailIcon.raycastTarget=false;
            detailTitle=Label(detail,"Name",new Vector2(110,-24),new Vector2(242,35),"",25);
            detailRank=Label(detail,"Rank",new Vector2(110,-63),new Vector2(240,25),"",18);
            var purpose=RusticHudTheme.Rect(detail,"DescriptionScroll",Top,new Vector2(18,-109),new Vector2(334,58));purpose.gameObject.AddComponent<Image>().color=Color.clear;purpose.gameObject.AddComponent<RectMask2D>();
            var purposeScroll=purpose.gameObject.AddComponent<ScrollRect>();purposeScroll.viewport=purpose;purposeScroll.horizontal=false;purposeScroll.movementType=ScrollRect.MovementType.Clamped;purposeScroll.scrollSensitivity=24;
            description=Label(purpose,"Purpose",Vector2.zero,new Vector2(334,58),"",18);purposeScroll.content=description.rectTransform;
            Label(detail,"Contribution",new Vector2(18,-178),new Vector2(334,22),"BÔNUS DESTE TALENTO",15).color=RusticHudTheme.Gold;
            var current=Panel(detail,"Current",new Vector2(18,-205),new Vector2(156,102),RusticHudTheme.Surface);Label(current,"Title",new Vector2(10,-9),new Vector2(136,25),"ATUAL",16);currentBonus=Label(current,"Value",new Vector2(10,-47),new Vector2(136,45),"",23);
            var next=Panel(detail,"Next",new Vector2(192,-205),new Vector2(160,102),new Color(.065f,.17f,.12f,1));Label(next,"Title",new Vector2(10,-9),new Vector2(140,25),"PRÓXIMO NÍVEL",14);nextBonus=Label(next,"Value",new Vector2(10,-47),new Vector2(140,45),"",23);nextBonus.color=new Color(.4f,1,.7f);
            perPoint=Label(detail,"PerPoint",new Vector2(18,-327),new Vector2(334,73),"",16);
            improve=RusticHudTheme.Button(detail,"LearnSelected",Top,new Vector2(18,-411),new Vector2(334,50),"MELHORAR · 1 PONTO",Learn);
            var gold=improve.GetComponent<RoundedHudGraphic>();gold.color=new Color(.3f,.21f,.08f,1);gold.borderColor=RusticHudTheme.Gold;gold.border=2;
            reset=RusticHudTheme.Button(modal,"ResetTalents",Top,new Vector2(20,-575),new Vector2(550,48),"",ConfirmReset);
            Label(modal,"Rules",new Vector2(590,-575),new Vector2(550,49),"1 ponto por nível após o primeiro, por herói.\nBônus compartilham os limites da ficha.\nMelhore fora de combate; redistribuir pede confirmação.",14);
            status=Label(modal,"Status",new Vector2(20,-631),new Vector2(1120,18),"",14);
            popup.SetActive(false);Refresh();
        }
        public void ArrangeInventoryOverview()
        {
            for(int i=0;i<6;i++)
            {
                var r=(RectTransform)overview[i].transform;r.anchoredPosition=new Vector2(438+i%3*116,-524-i/3*49);r.sizeDelta=new Vector2(110,44);
                stats[i].rectTransform.anchoredPosition=new Vector2(7,-3);stats[i].rectTransform.sizeDelta=new Vector2(98,38);stats[i].fontSize=12;stats[i].color=RusticHudTheme.Parchment;
            }
            var talent=(RectTransform)overview[6].transform;talent.anchoredPosition=new Vector2(438,-632);talent.sizeDelta=new Vector2(342,36);
            overview[6].GetComponentInChildren<Text>().rectTransform.sizeDelta=new Vector2(322,36);overview[6].GetComponentInChildren<Text>().fontSize=14;
        }
        void Open(){popup.SetActive(true);popup.transform.SetAsLastSibling();confirm=false;pending=false;operationNotice=null;Refresh();}
        public void SetOverviewVisible(bool visible){foreach(var go in overview)if(go!=null)go.SetActive(visible);if(!visible)Hide();}
        public bool IsOpen=>popup!=null&&popup.activeSelf;
        public void Hide(){if(popup!=null)popup.SetActive(false);confirm=false;}
        void Begin(CoopInventoryAction action,int talent=0)
        {
            if(pending)return;pending=true;requestRevision=channel.Revision;requestMessage=channel.Message;confirm=false;
            channel.Request(action,null,EquipmentSlot.MainHand,action==CoopInventoryAction.ResetTalents,talent);Refresh();
        }
        void Learn(){if(!pending)Begin(CoopInventoryAction.SpendTalent,(int)channel.Catalog.talents[selected].id);}
        void ConfirmReset(){if(pending)return;if(!confirm){confirm=true;Refresh();return;}Begin(CoopInventoryAction.ResetTalents);}
        void Update()
        {
            refresh-=Time.unscaledDeltaTime;if(refresh>0)return;refresh=.2f;
            if(pending&&(channel.Revision!=requestRevision||channel.Message!=requestMessage)){pending=false;operationNotice=channel.Message;}
            Refresh();
        }
        void Refresh()
        {
            var p=channel.LocalProfile;if(p==null)return;var hero=Actor.Roster.Current;var c=channel.Catalog;var s=ProgressionStats.Calculate(hero,p,c);if(session.Phase==CoopPhase.Playing){var build=Actor.GetComponent<CombatBuildEffects>();var item=p.Item(p.Loadout(hero.heroId)?.items[0]);float power=item!=null?c.Rarity(item.rarity).power:1;float bonus=build!=null?build.Power:1;s.damage=Mathf.RoundToInt(s.baseDamage*ProgressionStats.StrikeMultiplier(s.damageMultiplier*power*bonus,s.elementalDamage,false));s.criticalDamage=Mathf.RoundToInt(s.baseDamage*ProgressionStats.StrikeMultiplier(s.damageMultiplier*power*bonus,s.elementalDamage,true,s.criticalMultiplier));float fury=Actor.FuryRemaining>0&&hero.onlineFlow!=null?hero.onlineFlow.furyAttackSpeed:1;s.attacksPerSecond*=Mathf.Min(2.5f,s.attackSpeed*(build!=null?build.Tempo:1)*fury)/s.attackSpeed;}
            int points=HeroTalents.Available(p,hero.heroId,c);
            string[] labels={"VIDA MÁXIMA\n"+s.maximumHealth,"DANO\n"+s.damage+" (base "+s.baseDamage+")","DEFESA\n"+s.defensePoints.ToString("0.#")+" (−"+(s.defense*100).ToString("0.#")+"%)","CADÊNCIA\n"+s.attacksPerSecond.ToString("0.##")+" golpes/s","CRÍTICO\n"+s.criticalDamage+" · chance "+(s.criticalChance*100).ToString("0.#")+"%","MOVIMENTO\n"+s.speed.ToString("0.#")+" m/s"};for(int i=0;i<6;i++)stats[i].text=labels[i];
            if(!popup.activeSelf)return;
            if(lastHero!=hero.heroId){lastHero=hero.heroId;selected=0;confirm=false;operationNotice=null;}
            selected=Mathf.Clamp(selected,0,c.talents.Length-1);
            heading.text=hero.displayName+" · NÍVEL "+s.level;
            this.points.text=points+" PONTOS LIVRES";
            resources.text=p.coins+" OURO · XP "+ProgressionStats.ExperienceInLevel(p.experience,c.maximumLevel)+" / "+(s.level==c.maximumLevel?"MÁXIMO":ProgressionStats.RequiredExperience(s.level).ToString())+" · Talentos por herói";
            xpFill.anchorMax=new Vector2(s.level==c.maximumLevel?1:Mathf.Clamp01((float)ProgressionStats.ExperienceInLevel(p.experience,c.maximumLevel)/ProgressionStats.RequiredExperience(s.level)),1);
            if(portraitSource!=null)portrait.texture=portraitSource.texture;
            heroSummary.text="Vida máxima   "+s.maximumHealth+"\nDano   "+s.damage+"\nDefesa   "+(s.defense*100).ToString("0.#")+"%\nCrítico   "+s.criticalDamage+" · "+(s.criticalChance*100).ToString("0.#")+"%\nCadência   "+s.attacksPerSecond.ToString("0.##")+" golpes/s\nMovimento   "+s.speed.ToString("0.#")+" m/s";
            bool allowed=!(channel.HomeMode&&channel.Hub.UsingRemote)&&(channel.HomeMode||session.GetComponent<CoopProgression>().CanManage(session.LocalSlot));
            for(int i=0;i<choices.Length;i++)
            {
                var rule=c.talents[i];int rank=HeroTalents.Rank(p,hero.heroId,rule.id);
                customIcons[i].sprite=rule.icon;customIcons[i].enabled=rule.icon!=null;fallbackIcons[i].gameObject.SetActive(rule.icon==null);choiceNames[i].text=rule.displayName;choiceRanks[i].text=rank+" / "+rule.maximumRank;choiceDescriptions[i].text="+"+ItemDescriptions.Amount(rule.stat,rule.perRank)+" "+ItemDescriptions.Name(rule.stat).Replace(" (1 ponto = 1%)","")+" / ponto";
                choiceFill[i].anchorMax=new Vector2((float)rank/rule.maximumRank,1);
                var surface=choices[i].GetComponent<RoundedHudGraphic>();surface.borderColor=i==selected?RusticHudTheme.Gold:Tint(rule.stat)*.65f;surface.border=i==selected?3:1;surface.SetVerticesDirty();
            }
            var t=c.talents[selected];int n=HeroTalents.Rank(p,hero.heroId,t.id);bool maximum=n>=t.maximumRank;
            detailTitle.text=t.displayName;detailTitle.color=Tint(t.stat);detailRank.text="RANK "+n+" / "+t.maximumRank;description.text=string.IsNullOrWhiteSpace(t.description)?Purpose(t.stat):t.description;description.rectTransform.sizeDelta=new Vector2(334,Mathf.Max(58,description.preferredHeight+6));if(detailSelection!=selected){detailSelection=selected;description.rectTransform.anchoredPosition=Vector2.zero;}
            customDetailIcon.sprite=t.icon;customDetailIcon.enabled=t.icon!=null;detailIcon.gameObject.SetActive(t.icon==null);detailIcon.symbol=Symbol(t.stat);detailIcon.color=Tint(t.stat);detailIcon.SetVerticesDirty();detailFrame.borderColor=Tint(t.stat);detailFrame.SetVerticesDirty();
            currentBonus.text="+"+ItemDescriptions.Amount(t.stat,t.perRank*n);nextBonus.text=maximum?"MÁXIMO":"+"+ItemDescriptions.Amount(t.stat,t.perRank*(n+1));
            perPoint.text="Cada ponto: +"+ItemDescriptions.Amount(t.stat,t.perRank)+"\n"+ItemDescriptions.Name(t.stat)+"\nSujeito aos limites gerais da ficha.";
            improve.interactable=allowed&&!pending&&points>0&&!maximum;improve.GetComponentInChildren<Text>().text=pending?"AGUARDE...":maximum?"TALENTO NO MÁXIMO":points==0?"SEM PONTOS LIVRES":"MELHORAR · 1 PONTO";
            int cost=HeroTalents.ResetCost(p,hero.heroId,c);reset.GetComponentInChildren<Text>().text=(confirm?"CONFIRMAR REDISTRIBUIÇÃO · ":"REDISTRIBUIR PONTOS · ")+cost+" OURO";reset.interactable=allowed&&!pending&&cost>0&&p.coins>=cost;
            status.text=pending?"Aguardando confirmação do perfil...":confirm?"Confirme para devolver os pontos deste herói. Custo: "+cost+" ouro.":!allowed?"Talentos indisponíveis neste estado. Saia do perigo ou volte à sala de origem.":operationNotice??"Selecione um talento; só o botão MELHORAR gasta pontos.";
            status.color=confirm?RusticHudTheme.Gold:RusticHudTheme.Parchment;
        }
    }
}
