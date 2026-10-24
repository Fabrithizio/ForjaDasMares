using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    // Presentation only: previews use the same stat calculation as real equipment.
    // No inventory mutations, reward requests or persistent data are owned here.
    public sealed class InventoryComparisonView
    {
        sealed class Card
        {
            public RectTransform root;
            public Image icon;
            public Text title,meta,attack,effect;
        }
        readonly Card[] cards=new Card[2];
        readonly Text[,] cells=new Text[8,4];
        readonly string[] labels={"Dano","Vida máxima","Defesa","Chance crítica","Dano crítico","Cadência","Movimento","Redução de recarga"};
        GameObject root;Text context;Button equippedTab,selectedTab;bool mobile;int shown=1;
        public void Build(Transform parent,bool touch)
        {
            mobile=touch;
            var rect=RusticHudTheme.Rect(parent,"VisualComparison",new Vector2(0,1),Vector2.zero,new Vector2(1160,480));root=rect.gameObject;
            for(int i=0;i<2;i++)cards[i]=CreateCard(rect,i);
            if(mobile)
            {
                equippedTab=RusticHudTheme.Button(rect,"InspectEquipped",new Vector2(0,1),new Vector2(25,-12),new Vector2(235,44),"VER EQUIPADO",()=>ShowCard(0));
                selectedTab=RusticHudTheme.Button(rect,"InspectSelected",new Vector2(0,1),new Vector2(275,-12),new Vector2(235,44),"VER SELECIONADO",()=>ShowCard(1));
                ShowCard(1);
            }
            var table=RusticHudTheme.Rect(rect,"AttributeComparison",new Vector2(0,1),new Vector2(25,-274),new Vector2(1105,205));RusticHudTheme.Panel(table,new Color(.065f,.115f,.095f,1),14);
            string[] titles={"ATRIBUTOS DO HERÓI","ATUAL","AO EQUIPAR","DIFERENÇA"};float[] x={14,440,655,870};float[] widths={405,205,205,215};
            for(int c=0;c<4;c++)RusticHudTheme.Label(table,"Heading_"+c,new Vector2(0,1),new Vector2(x[c],-5),new Vector2(widths[c],24),titles[c],15).color=RusticHudTheme.Gold;
            for(int r=0;r<8;r++)
            {
                if(r%2==0){var stripe=RusticHudTheme.Rect(table,"Stripe_"+r,new Vector2(0,1),new Vector2(8,-30-r*21),new Vector2(1089,21));var image=stripe.gameObject.AddComponent<Image>();image.color=new Color(1,1,1,.035f);image.raycastTarget=false;}
                for(int c=0;c<4;c++)cells[r,c]=RusticHudTheme.Label(table,"Value_"+r+"_"+c,new Vector2(0,1),new Vector2(x[c],-30-r*21),new Vector2(widths[c],21),c==0?labels[r]:"—",17);
            }
            context=RusticHudTheme.Label(rect,"PreviewContext",new Vector2(0,1),new Vector2(25,-249),new Vector2(1105,23),"",15);
        }
        static Text Text(Transform parent,string name,Vector2 pos,Vector2 size,int font)
        {var t=RusticHudTheme.Label(parent,name,new Vector2(0,1),pos,size,"",font);t.alignment=TextAnchor.UpperLeft;t.resizeTextForBestFit=true;t.resizeTextMinSize=14;t.resizeTextMaxSize=font;return t;}
        Card CreateCard(Transform parent,int index)
        {
            float width=mobile?1105:535,x=mobile?25:index==0?25:595,top=mobile?60:12;
            var card=new Card();card.root=RusticHudTheme.Rect(parent,index==0?"EquippedCard":"SelectedCard",new Vector2(0,1),new Vector2(x,-top),new Vector2(width,mobile?183:230));
            RusticHudTheme.Panel(card.root,new Color(.11f,.145f,.12f,1),16);
            if(!mobile)RusticHudTheme.Label(card.root,"Role",new Vector2(0,1),new Vector2(14,-6),new Vector2(490,23),index==0?"EQUIPADO ATUALMENTE":"ITEM SELECIONADO",15).color=RusticHudTheme.Gold;
            var icon=RusticHudTheme.Rect(card.root,"Portrait",new Vector2(0,1),new Vector2(12,mobile?-8:-32),new Vector2(92,92));card.icon=icon.gameObject.AddComponent<Image>();card.icon.preserveAspect=true;card.icon.raycastTarget=false;
            card.title=Text(card.root,"Name",new Vector2(116,mobile?-8:-32),new Vector2(width-132,51),23);
            card.meta=Text(card.root,"Identity",new Vector2(116,mobile?-62:-85),new Vector2(width-132,43),16);
            float boxTop=mobile?105:133,boxWidth=(width-36)/2;
            var a=RusticHudTheme.Rect(card.root,"AttackBlock",new Vector2(0,1),new Vector2(12,-boxTop),new Vector2(boxWidth,mobile?69:86));RusticHudTheme.Panel(a,new Color(.055f,.085f,.075f,1),9);
            RusticHudTheme.Label(a,"Heading",new Vector2(0,1),new Vector2(8,-3),new Vector2(boxWidth-16,20),"ATAQUE",14).color=RusticHudTheme.Gold;
            card.attack=Text(a,"Behavior",new Vector2(8,-24),new Vector2(boxWidth-16,mobile?42:56),16);
            var e=RusticHudTheme.Rect(card.root,"EffectBlock",new Vector2(0,1),new Vector2(24+boxWidth,-boxTop),new Vector2(boxWidth,mobile?69:86));RusticHudTheme.Panel(e,new Color(.09f,.075f,.13f,1),9);
            RusticHudTheme.Label(e,"Heading",new Vector2(0,1),new Vector2(8,-3),new Vector2(boxWidth-16,20),"BÔNUS E EFEITO",14).color=new Color(.8f,.67f,1);
            card.effect=Text(e,"Effect",new Vector2(8,-24),new Vector2(boxWidth-16,mobile?42:56),16);
            return card;
        }
        void ShowCard(int index)
        {shown=index;for(int i=0;i<2;i++)cards[i].root.gameObject.SetActive(i==shown);if(equippedTab!=null){equippedTab.interactable=shown!=0;selectedTab.interactable=shown!=1;}}
        public void Reset(){if(mobile)ShowCard(1);}
        public void SetVisible(bool value)=>root.SetActive(value);
        void Fill(Card card,ProgressionCatalog catalog,ItemInstance item,EquipmentSlot slot)
        {
            var d=item!=null?catalog.Find(item.definitionId):null;
            card.icon.sprite=d!=null?d.icon:null;card.icon.enabled=card.icon.sprite!=null;
            if(d==null){card.title.text="Slot vazio";card.title.color=RusticHudTheme.Muted;card.meta.text=RusticHudTheme.SlotName(slot)+" · nenhum item equipado";card.attack.text="Sem padrão de ataque neste slot.";card.effect.text="Sem bônus de equipamento.";return;}
            card.title.text=d.displayName;card.title.color=catalog.Rarity(item.rarity).color;
            card.meta.text=RusticHudTheme.RarityName(item.rarity)+" · "+ItemDescriptions.Category(d)+" · Nv "+d.minimumLevel+"\n"+RusticHudTheme.SlotName(slot)+" · "+RusticHudTheme.ClassNames(d.compatibleHeroes);
            var attack=d.attacks!=null&&d.attacks.Length>0?d.attacks[0]:null;
            card.attack.text=attack==null?"Usa o padrão da arma equipada.":Mathf.RoundToInt(attack.damage*catalog.Rarity(item.rarity).power)+" dano da arma · "+attack.range.ToString("0.#")+" m\n"+ItemDescriptions.Pattern(attack);
            var bonuses=ItemDescriptions.Bonuses(catalog,item);string bonus="";int count=0;
            for(int i=0;i<bonuses.Length;i++)if(bonuses[i]!=0){if(count++<2)bonus+=(bonus.Length>0?" · ":"")+"+"+ItemDescriptions.Amount((ItemStat)i,bonuses[i])+" "+ItemDescriptions.Name((ItemStat)i).Replace(" (1 ponto = 1%)","");}
            string effect=d.perk!=WeaponPerk.None?(d.perk==WeaponPerk.Bleed?"Sangramento":d.perk==WeaponPerk.Scorch?"Queimadura":d.perk==WeaponPerk.Fracture?"Quebra de armadura":d.perk==WeaponPerk.Pull?"Atração":d.perk.ToString()):d.ability!=null?d.ability.displayName:d.twoHanded?"Ocupa as duas mãos":d.allowsOffHand?"Permite mão secundária":"Sem efeito especial";
            card.effect.text=effect+(bonus.Length>0?"\n"+bonus:"");
            if(count>2)card.effect.text+=" · mais em Avançado";
        }
        public void Refresh(ProgressionCatalog catalog,PlayerProfile profile,HeroDefinition hero,ItemInstance selected,ItemInstance equipped,EquipmentSlot slot,bool delivery)
        {
            Fill(cards[0],catalog,equipped,slot);Fill(cards[1],catalog,selected,slot);
            bool already=selected.instanceId==equipped?.instanceId;
            bool valid=already||ProgressionStats.CanEquip(profile,catalog,hero.heroId,selected,slot,out _);
            var before=ProgressionStats.Calculate(hero,profile,catalog);var after=valid&&!already?ProgressionStats.Calculate(hero,profile,catalog,selected.instanceId,slot,true):before;
            float[] a={before.damage,before.maximumHealth,before.defense*100,before.criticalChance*100,before.criticalDamage,before.attacksPerSecond,before.speed,(1-before.cooldownMultiplier)*100};
            float[] b={after.damage,after.maximumHealth,after.defense*100,after.criticalChance*100,after.criticalDamage,after.attacksPerSecond,after.speed,(1-after.cooldownMultiplier)*100};
            for(int i=0;i<8;i++)
            {
                string format=i==5?"0.##":i<2||i==4?"0":"0.#";string unit=i==2||i==3||i==7?"%":i==5?" golpes/s":i==6?" m/s":"";
                cells[i,1].text=a[i].ToString(format)+unit;cells[i,2].text=valid?b[i].ToString(format)+unit:"—";
                float delta=b[i]-a[i];bool equal=Mathf.Abs(delta)<.0001f;
                cells[i,3].text=!valid?"Indisponível":equal?"—":(delta>0?"▲ +":"▼ −")+Mathf.Abs(delta).ToString(format)+(i==2||i==3||i==7?" p.p.":unit);
                cells[i,3].color=equal||!valid?RusticHudTheme.Muted:delta>0?new Color(.45f,.86f,.63f):new Color(1,.6f,.55f);
            }
            context.text=!valid?"Este item não pode ser equipado neste herói. Veja o motivo abaixo.":already?"Já equipado · os bônus já estão na ficha.":delivery?"Prévia · receber leva à mochila; não equipa automaticamente.":"Prévia sem efeitos temporários · referência: slot "+RusticHudTheme.SlotName(slot)+(equipped==null?" sem item":" do herói");
        }
    }
}
