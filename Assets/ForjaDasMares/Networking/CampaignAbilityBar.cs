using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace ForjaDasMares.Networking
{
    public sealed class CampaignAbilityBar:MonoBehaviour
    {
        CoopDirector director;Button[] buttons=new Button[6];Image[] cooldown=new Image[6];Text[] labels=new Text[6];SkillHint[] hints=new SkillHint[6];HudIcon ultimateIcon,attackSymbol,specialSymbol;Image weaponIcon;int profileRevision=-1,profileHero=-1;float cooldownMultiplier=1;Text tooltip;Sprite fill;float clock;
        readonly float[] remaining=new float[6],totals=new float[6];
        readonly string[] keys={"MOUSE","E","ESPAÇO","F","Q","I"};
        public void Build(Transform parent,CoopDirector owner,ArenaInput input)
        {
            director=owner;var dock=RusticHudTheme.Rect(parent,"RpgAbilityBar",new Vector2(.5f,0),new Vector2(0,18),new Vector2(520,94));dock.pivot=new Vector2(.5f,0);RusticHudTheme.Panel(dock,RusticHudTheme.Ink,20);
            tooltip=RusticHudTheme.Label(dock,"SkillTooltip",new Vector2(.5f,1),new Vector2(0,46),new Vector2(650,44),"",15);tooltip.alignment=TextAnchor.MiddleCenter;
            fill=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f);
            HudSymbol[] symbols={HudSymbol.Sword,HudSymbol.Rune,HudSymbol.Dodge,HudSymbol.Interact,HudSymbol.Hero,HudSymbol.Bag};
            for(int i=0;i<6;i++)
            {
                int index=i;buttons[i]=RusticHudTheme.Button(dock,"Action_"+i,new Vector2(0,1),new Vector2(10+i*84,-8),new Vector2(78,78),"",()=>Activate(index));
                var icon=RusticHudTheme.Rect(buttons[i].transform,"SkillIcon",new Vector2(.5f,1),new Vector2(0,-7),new Vector2(34,34));var graphic=icon.gameObject.AddComponent<HudIcon>();graphic.symbol=symbols[i];graphic.color=RusticHudTheme.Gold;graphic.raycastTarget=false;if(i==4)ultimateIcon=graphic;if(i==1)specialSymbol=graphic;
                if(i==0){attackSymbol=graphic;var imageRect=RusticHudTheme.Rect(buttons[i].transform,"WeaponIcon",new Vector2(.5f,1),new Vector2(0,-7),new Vector2(38,38));weaponIcon=imageRect.gameObject.AddComponent<Image>();weaponIcon.preserveAspect=true;weaponIcon.raycastTarget=false;}
                var cover=RusticHudTheme.Rect(buttons[i].transform,"Cooldown",new Vector2(.5f,1),new Vector2(0,-4),new Vector2(68,51));cooldown[i]=cover.gameObject.AddComponent<Image>();cooldown[i].sprite=fill;cooldown[i].type=Image.Type.Filled;cooldown[i].fillMethod=Image.FillMethod.Radial360;cooldown[i].color=new Color(0,0,0,.72f);cooldown[i].raycastTarget=false;
                labels[i]=RusticHudTheme.Label(buttons[i].transform,"KeyAndCooldown",new Vector2(.5f,0),new Vector2(0,4),new Vector2(76,34),keys[i],10);labels[i].alignment=TextAnchor.MiddleCenter;
                hints[i]=buttons[i].gameObject.AddComponent<SkillHint>();hints[i].output=tooltip;
            }
            buttons[0].gameObject.AddComponent<HoldAttackButton>().Configure(input);
            for(int i=0;i<4;i++){var actor=owner.Actor(i);var aura=actor.GetComponent<CampaignFuryVisual>();if(aura==null)aura=actor.gameObject.AddComponent<CampaignFuryVisual>();aura.Configure(actor);}
        }
        void Activate(int index)
        {if(director.LocalSlot<0)return;if(index==5){GetComponent<CoopInventoryHud>().OpenInventory();return;}director.Request(index==0?HeroButtons.Attack:index==1?HeroButtons.Special:index==2?HeroButtons.Dodge:index==3?HeroButtons.Interact:HeroButtons.Super);}
        void Update()
        {
            if(director==null||director.Phase!=CoopPhase.Playing||director.LocalSlot<0)return;
            if(Keyboard.current!=null&&Keyboard.current.iKey.wasPressedThisFrame){var inventory=GetComponent<CoopInventoryHud>();if(inventory.IsOpen)inventory.CloseInventory();else inventory.OpenInventory();}
            clock-=Time.unscaledDeltaTime;if(clock>0)return;clock=.08f;
            var actor=director.Actor(director.LocalSlot);var channel=director.GetComponent<CoopProfileChannel>();var profile=channel.LocalProfile;var set=profile?.Loadout(actor.Roster.Current.heroId);var item=profile?.Item(set?.items[0]);var weapon=item!=null?channel.Catalog.Find(item.definitionId):null;var ability=weapon!=null&&weapon.ability!=null?weapon.ability:actor.Roster.Current.special;
            if(profile!=null&&(profileRevision!=channel.Revision||profileHero!=actor.Roster.Selected)){cooldownMultiplier=ProgressionStats.Calculate(actor.Roster.Current,profile,channel.Catalog).cooldownMultiplier;profileRevision=channel.Revision;profileHero=actor.Roster.Selected;}
            weaponIcon.sprite=weapon!=null?weapon.icon:null;weaponIcon.enabled=weaponIcon.sprite!=null;attackSymbol.enabled=!weaponIcon.enabled;
            var skillSymbol=ability==null?HudSymbol.Rune:ability.effect==AbilityEffect.WeaponStrike?HudSymbol.Sword:ability.effect==AbilityEffect.Trap?HudSymbol.Compass:ability.effect==AbilityEffect.ProtectionCharge?HudSymbol.Hero:HudSymbol.Rune;
            if(specialSymbol.symbol!=skillSymbol){specialSymbol.symbol=skillSymbol;specialSymbol.SetVerticesDirty();}
            remaining[1]=actor.SpecialCooldown;remaining[2]=actor.DodgeCooldown;remaining[4]=actor.FuryRemaining;totals[0]=totals[3]=totals[5]=1;totals[1]=ability!=null?ability.cooldown*cooldownMultiplier:1;totals[2]=actor.Roster.Current.dodge.cooldown*cooldownMultiplier;totals[4]=actor.Roster.Current.onlineFlow!=null?Mathf.Max(.1f,actor.Roster.Current.onlineFlow.furyDuration):1;
            for(int i=0;i<6;i++){bool ready=actor.Health.IsAlive&&(i!=4||actor.SuperProgress>=1&&actor.FuryRemaining<=0)&&(i!=1&&i!=2||remaining[i]<=0);buttons[i].interactable=ready||i==5;cooldown[i].fillAmount=!actor.Health.IsAlive&&i<5?1:i==4&&actor.FuryRemaining<=0?1-actor.SuperProgress:Mathf.Clamp01(remaining[i]/Mathf.Max(.1f,totals[i]));labels[i].text=keys[i]+"\n"+(remaining[i]>0?remaining[i].ToString("F1")+"s":i==4?Mathf.FloorToInt(actor.SuperProgress*100)+"%":"");}
            var desired=actor.Roster.Current.heroId=="warrior"?HudSymbol.Fury:HudSymbol.Rune;if(ultimateIcon.symbol!=desired){ultimateIcon.symbol=desired;ultimateIcon.SetVerticesDirty();}
            hints[0].description=(weapon!=null?weapon.displayName:"Ataque básico")+" · segure para atacar e carregar o pesado";
            hints[1].description=(ability!=null?ability.displayName:"Habilidade")+" · habilidade da arma / classe";
            hints[2].description="Esquiva · janela curta de invulnerabilidade e recuperação";hints[3].description="Interagir · baús, alavancas e saída";
            hints[4].description=actor.Roster.Current.heroId=="warrior"?"FÚRIA · +"+Mathf.RoundToInt((actor.Roster.Current.onlineFlow.furyAttackSpeed-1)*100)+"% de cadência por "+actor.Roster.Current.onlineFlow.furyDuration.ToString("F1")+"s; preserva a arma equipada":"SUPER · requer a barra cheia";hints[5].description="Personagem · equipamentos, mochila e comparação";
        }
        void OnDestroy(){if(fill!=null)Destroy(fill);}
    }
    public sealed class SkillHint:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        public Text output;public string description;
        public void OnPointerEnter(PointerEventData e){if(output!=null)output.text=description;}
        public void OnPointerExit(PointerEventData e){if(output!=null)output.text="";}
    }
    public sealed class CampaignFuryVisual:MonoBehaviour
    {
        CoopActor actor;LineRenderer ring;
        public void Configure(CoopActor owner)
        {
            actor=owner;if(ring!=null)return;var go=new GameObject("FuryAura");go.transform.SetParent(transform,false);ring=go.AddComponent<LineRenderer>();ring.useWorldSpace=false;ring.loop=true;ring.positionCount=24;ring.startWidth=ring.endWidth=.07f;
            ring.sharedMaterial=actor.Presentation.Visible.GetComponentInChildren<Renderer>().sharedMaterial;var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",new Color(1,.25f,.025f));ring.SetPropertyBlock(block);
            for(int i=0;i<24;i++){float a=i*Mathf.PI*2/24;ring.SetPosition(i,new Vector3(Mathf.Cos(a)*.65f,.1f,Mathf.Sin(a)*.65f));}ring.enabled=false;
        }
        void LateUpdate(){if(ring==null)return;ring.enabled=actor.Health.IsAlive&&actor.FuryRemaining>0;if(ring.enabled)ring.transform.localScale=Vector3.one*(1+.1f*Mathf.Sin(Time.time*14));}
    }
}
