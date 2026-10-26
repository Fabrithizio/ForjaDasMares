using System.Text;
using UnityEngine;
namespace ForjaDasMares
{
    public static class ItemDescriptions
    {
        static readonly string[] names={"Vida máxima","Dano","Defesa (1 ponto = 1%)","Velocidade","Chance crítica","Velocidade de ataque","Redução de recarga","Dano elemental","Regeneração","Dano crítico"};
        public static string Name(ItemStat stat)=>names[(int)stat];
        public static string Amount(ItemStat stat,float value)=>stat==ItemStat.Health||stat==ItemStat.Defense?value.ToString("0.#"):stat==ItemStat.Recovery?value.ToString("0.##")+" vida/s":(value*100).ToString("0.#")+"%";
        public static float[] Bonuses(ProgressionCatalog catalog,ItemInstance item)
        {
            var values=new float[10];if(item==null)return values;var d=catalog.Find(item.definitionId);if(d==null)return values;
            foreach(var b in d.bonuses)values[(int)b.stat]+=Mathf.Clamp(b.value,0,StatModifier.Limit(b.stat))*catalog.Rarity(item.rarity).power;
            if(item.modifiers!=null)foreach(var b in item.modifiers)values[(int)b.stat]+=Mathf.Clamp(b.value,0,StatModifier.Limit(b.stat));return values;
        }
        public static string Preview(HeroDefinition hero,PlayerProfile profile,ProgressionCatalog catalog,ItemInstance item)
        {
            if(item==null)return "Selecione um item para comparar.";
            if(InventoryRules.EquippedBy(profile,hero.heroId,item.instanceId))return "Item equipado: seus bônus já estão incluídos na ficha.";
            var definition=catalog.Find(item.definitionId);if(definition==null)return "Item não encontrado no catálogo.";
            if(!ProgressionStats.CanEquip(profile,catalog,hero.heroId,item,definition.slot,out string reason))return reason;
            var a=ProgressionStats.Calculate(hero,profile,catalog);
            var b=ProgressionStats.Calculate(hero,profile,catalog,item.instanceId,definition.slot,true);
            var text=new StringBuilder();
            text.AppendLine("Dano base da arma "+a.baseDamage+" → "+b.baseDamage);
            text.AppendLine("Dano normal "+a.damage+" → "+b.damage);
            text.AppendLine("Dano crítico "+a.criticalDamage+" → "+b.criticalDamage);
            text.AppendLine("Bônus crítico +"+((a.criticalMultiplier-1)*100).ToString("0.#")+"% → +"+((b.criticalMultiplier-1)*100).ToString("0.#")+"%");
            text.AppendLine("Vida "+a.maximumHealth+" → "+b.maximumHealth);
            text.AppendLine("Defesa "+(a.defense*100).ToString("0.#")+"% → "+(b.defense*100).ToString("0.#")+"%");
            text.AppendLine("Velocidade de ataque +"+((a.attackSpeed-1)*100).ToString("0.#")+"% → +"+((b.attackSpeed-1)*100).ToString("0.#")+"%");
            text.AppendLine("Chance crítica "+(a.criticalChance*100).ToString("0.#")+"% → "+(b.criticalChance*100).ToString("0.#")+"%");
            text.AppendLine("Movimento "+a.speed.ToString("0.#")+" → "+b.speed.ToString("0.#")+" m/s");
            text.AppendLine("Redução de recarga "+((1-a.cooldownMultiplier)*100).ToString("0.#")+"% → "+((1-b.cooldownMultiplier)*100).ToString("0.#")+"%");
            text.AppendLine("Elemental "+(a.elementalDamage*100).ToString("0.#")+"% → "+(b.elementalDamage*100).ToString("0.#")+"%");
            text.AppendLine("Regeneração "+a.recovery.ToString("0.#")+" → "+b.recovery.ToString("0.#")+" vida/s");
            text.AppendLine("Bloqueio frontal "+(a.block*100).ToString("0.#")+"% → "+(b.block*100).ToString("0.#")+"%");
            text.Append("Cadência "+a.attacksPerSecond.ToString("0.##")+" → "+b.attacksPerSecond.ToString("0.##")+" golpes/s");
            return text.ToString();
        }
        public static string Category(EquipmentDefinition item)
        {
            if(item==null)return "Item indisponível";
            switch(item.category)
            {
                case WeaponCategory.Sword:return "Espada"; case WeaponCategory.Axe:return "Machado";
                case WeaponCategory.Hammer:return "Martelo"; case WeaponCategory.Bow:return "Arco";
                case WeaponCategory.Daggers:return "Adaga"; case WeaponCategory.Staff:return "Cajado";
                case WeaponCategory.Grimoire:return "Grimório"; case WeaponCategory.Shield:return "Escudo";
                case WeaponCategory.MagicWeapon:return "Foco mágico"; default:return RusticHudTheme.SlotName(item.slot);
            }
        }
        public static string RoleSummary(EquipmentDefinition item)
        {
            if(item==null)return "Item indisponível.";
            if(item.attacks!=null&&item.attacks.Length>0)
            {
                var a=item.attacks[0];return a.shape==AttackShape.Projectile?"Ataque à distância. Mire no alvo e mantenha espaço para esquivar.":a.footprint==MeleeFootprint.Line?"Golpe estreito e comprido. Alinhe os inimigos à sua frente.":a.footprint==MeleeFootprint.Circle||a.arc>=359?"Golpe ao redor do herói. Aproximação e tempo de ataque são importantes.":"Golpe em cone à frente. Vire para o alvo antes de atacar.";
            }
            return item.frontalBlock>0?"Proteção frontal enquanto equipado. Não protege de todas as direções.":"Bônus aplicados enquanto equipado. Confira quais atributos mudam abaixo.";
        }
        public static string Pattern(AttackDefinition attack)
        {
            if(attack==null)return "Sem ataque próprio";
            if(attack.shape==AttackShape.Projectile)
            {
                string shot=attack.projectileCount>1?"Leque de "+Mathf.Clamp(attack.projectileCount,1,5)+" projéteis":"Projétil único";
                return shot+(attack.splashRadius>0?" · explosão de "+attack.splashRadius.ToString("0.#")+" m":" · para no primeiro impacto");
            }
            string shape=attack.footprint==MeleeFootprint.Circle||attack.arc>=359?"Ao redor do personagem":attack.footprint==MeleeFootprint.Line?"Linha estreita":"Cone de "+attack.arc.ToString("0")+"°";
            return shape+(attack.maximumTargets==1?" · um alvo":attack.maximumTargets>1?" · até "+attack.maximumTargets+" alvos":" · vários alvos");
        }
        public static string EquipmentState(PlayerProfile profile,ProgressionCatalog catalog,HeroDefinition hero,ItemInstance item)
        {
            if(item==null)return "Nenhum item selecionado.";
            foreach(var set in profile.loadouts)foreach(var id in set.items)if(id==item.instanceId)
                return set.heroId==hero.heroId?"Equipado neste herói":"Equipado em outro herói: "+(catalog.Hero(set.heroId)!=null?RusticHudTheme.ClassNames(catalog.Hero(set.heroId).affinity):set.heroId);
            var definition=catalog.Find(item.definitionId);
            if(definition==null)return "Item não encontrado no catálogo.";
            return ProgressionStats.CanEquip(profile,catalog,hero.heroId,item,definition.slot,out string reason)?"Disponível para equipar":reason;
        }
        public static string Summary(ProgressionCatalog catalog,ItemInstance item)
        {
            if(item==null)return "Nenhum item neste slot.";
            var d=catalog.Find(item.definitionId);if(d==null)return "Item não encontrado no catálogo.";
            var text=new StringBuilder();text.AppendLine(d.displayName);text.AppendLine(RusticHudTheme.RarityName(item.rarity)+" · "+Category(d));
            text.AppendLine("ONDE EQUIPA: "+RusticHudTheme.SlotName(d.slot));text.AppendLine("CLASSE: "+RusticHudTheme.ClassNames(d.compatibleHeroes)+" · nível "+d.minimumLevel);
            text.AppendLine(RoleSummary(d));
            if(d.twoHanded)text.AppendLine("Duas mãos: não permite item na mão secundária.");
            else if(d.allowsOffHand)text.AppendLine("Permite equipamento na mão secundária.");
            if(d.attacks!=null&&d.attacks.Length>0)
            {
                var attack=d.attacks[0];text.AppendLine();text.AppendLine("ATAQUE DA ARMA");
                text.AppendLine(Mathf.RoundToInt(attack.damage*catalog.Rarity(item.rarity).power)+" dano da arma (sem bônus do herói) · alcance "+attack.range.ToString("0.#")+" m");
                text.AppendLine(Pattern(attack));
            }
            var bonuses=Bonuses(catalog,item);int count=0;for(int i=0;i<bonuses.Length;i++)if(bonuses[i]!=0)count++;
            text.AppendLine();text.AppendLine("BÔNUS DO ITEM");int shown=0;
            for(int i=0;i<bonuses.Length&&shown<3;i++)if(bonuses[i]!=0){text.AppendLine("+"+Amount((ItemStat)i,bonuses[i])+" "+Name((ItemStat)i));shown++;}
            if(count==0)text.AppendLine("Sem bônus passivos.");if(count>shown)text.AppendLine("+ "+(count-shown)+" bônus nos detalhes avançados");
            if(d.frontalBlock>0)text.AppendLine("Bloqueio frontal: "+(Mathf.Min(.5f,d.frontalBlock*catalog.Rarity(item.rarity).power)*100).ToString("0")+"%");
            if(d.ability!=null)text.AppendLine("Habilidade especial: "+d.ability.displayName);
            if(d.perk!=WeaponPerk.None)text.AppendLine("Efeito: "+CombatPerkDescriptions.Perk(d.perk));
            if(d.synergy!=BuildSynergy.None)text.AppendLine("Tem sinergia: consulte os detalhes.");
            text.AppendLine();text.Append("Reciclar devolve "+InventoryRules.SellValue(catalog,item)+" ouro.");return text.ToString();
        }
        public static string QuickPreview(HeroDefinition hero,PlayerProfile profile,ProgressionCatalog catalog,ItemInstance item)
        {
            if(item==null)return "Selecione um item para comparar.";
            var definition=catalog.Find(item.definitionId);if(definition==null)return "Item não encontrado no catálogo.";
            if(InventoryRules.EquippedBy(profile,hero.heroId,item.instanceId))return "Os bônus deste item já estão na sua ficha.";
            if(!ProgressionStats.CanEquip(profile,catalog,hero.heroId,item,definition.slot,out string reason))return reason;
            var a=ProgressionStats.Calculate(hero,profile,catalog);var b=ProgressionStats.Calculate(hero,profile,catalog,item.instanceId,definition.slot,true);
            var text=new StringBuilder();int changes=0,shown=0;
            AddChange(text,"Dano",a.damage,b.damage,"0",ref changes,ref shown);
            AddChange(text,"Vida máxima",a.maximumHealth,b.maximumHealth,"0",ref changes,ref shown);
            AddChange(text,"Defesa",a.defense*100,b.defense*100,"0.#",ref changes,ref shown,"%");
            AddChange(text,"Dano crítico",a.criticalDamage,b.criticalDamage,"0",ref changes,ref shown);
            AddChange(text,"Chance crítica",a.criticalChance*100,b.criticalChance*100,"0.#",ref changes,ref shown,"%");
            AddChange(text,"Cadência",a.attacksPerSecond,b.attacksPerSecond,"0.##",ref changes,ref shown," golpes/s");
            AddChange(text,"Movimento",a.speed,b.speed,"0.#",ref changes,ref shown," m/s");
            AddChange(text,"Redução de recarga",(1-a.cooldownMultiplier)*100,(1-b.cooldownMultiplier)*100,"0.#",ref changes,ref shown,"%");
            AddChange(text,"Dano elemental",a.elementalDamage*100,b.elementalDamage*100,"0.#",ref changes,ref shown,"%");
            AddChange(text,"Regeneração",a.recovery,b.recovery,"0.#",ref changes,ref shown," vida/s");
            AddChange(text,"Bloqueio frontal",a.block*100,b.block*100,"0.#",ref changes,ref shown,"%");
            if(changes==0)text.AppendLine("Não altera os números da ficha. Confira o padrão de ataque e os efeitos.");
            if(changes>shown)text.AppendLine("+ "+(changes-shown)+" mudanças nos detalhes avançados");
            return text.ToString();
        }
        static void AddChange(StringBuilder text,string label,float before,float after,string format,ref int changes,ref int shown,string unit="")
        {
            if(Mathf.Abs(before-after)<.0001f)return;changes++;if(shown>=4)return;shown++;
            text.AppendLine("<color="+(after>before?"#72D7A0":"#FF998B")+">"+(after>before?"▲ ":"▼ ")+label+": "+before.ToString(format)+unit+" → "+after.ToString(format)+unit+" ("+(after>=before?"+":"−")+Mathf.Abs(after-before).ToString(format)+unit+")</color>");
        }
        public static string CompareBonuses(ProgressionCatalog catalog,ItemInstance selected,ItemInstance reference)
        {
            if(reference==null)return "Nenhum item de referência. Você pode escolher outro item do mesmo slot.";
            var d=catalog.Find(reference.definitionId);if(d==null)return "Referência indisponível.";
            var text=new StringBuilder();text.AppendLine("REFERÊNCIA: "+d.displayName+" · "+RusticHudTheme.RarityName(reference.rarity));
            var a=Bonuses(catalog,selected);var b=Bonuses(catalog,reference);int shown=0;
            for(int i=0;i<a.Length&&shown<3;i++)if(Mathf.Abs(a[i]-b[i])>.0001f){float delta=a[i]-b[i];text.AppendLine(Name((ItemStat)i)+": "+(delta>=0?"+":"−")+Amount((ItemStat)i,Mathf.Abs(delta))+" em relação à referência");shown++;}
            if(shown==0)text.AppendLine("Sem diferença nos bônus passivos. Ataques e efeitos podem ser diferentes.");
            return text.ToString();
        }
        public static string Describe(ProgressionCatalog catalog,ItemInstance item,ItemInstance compare=null)
        {
            if(item==null)return "Nenhum item neste slot.";var d=catalog.Find(item.definitionId);if(d==null)return "Item não encontrado no catálogo.";var text=new StringBuilder();
            text.AppendLine(d.displayName+" · "+RusticHudTheme.RarityName(item.rarity));text.AppendLine(RusticHudTheme.SlotName(d.slot)+" · nível "+d.minimumLevel);text.AppendLine("Classes: "+RusticHudTheme.ClassNames(d.compatibleHeroes));text.AppendLine(RoleSummary(d));text.AppendLine();
            var a=Bonuses(catalog,item);var b=Bonuses(catalog,compare);bool any=false;
            for(int i=0;i<10;i++)if(a[i]!=0||b[i]!=0){any=true;var stat=(ItemStat)i;text.Append(Name(stat)+": +"+Amount(stat,a[i]));if(compare!=null){float delta=a[i]-b[i];text.Append("  ("+(delta>=0?"+":"−")+Amount(stat,Mathf.Abs(delta))+")");}text.AppendLine();}
            if(!any)text.AppendLine("Sem bônus passivos. Usa o padrão de ataque abaixo.");
            if(d.frontalBlock>0)text.AppendLine("Bloqueio frontal: "+(Mathf.Min(.5f,d.frontalBlock*catalog.Rarity(item.rarity).power)*100).ToString("0")+"%");
            if(d.attacks!=null&&d.attacks.Length>0){text.AppendLine();text.AppendLine("SEQUÊNCIA DE ATAQUES");foreach(var attack in d.attacks)text.AppendLine(Mathf.RoundToInt(attack.damage*catalog.Rarity(item.rarity).power)+" dano da arma (base "+attack.damage+") · alcance "+attack.range.ToString("0.#")+" m · ciclo "+(attack.windup+attack.recovery).ToString("0.##")+" s · "+Pattern(attack));}
            if(d.ability!=null)text.AppendLine("Habilidade: "+d.ability.displayName+" · recarga "+d.ability.cooldown.ToString("0.#")+"s");
            if(d.perk!=WeaponPerk.None)text.AppendLine("EFEITO DA ARMA: "+CombatPerkDescriptions.Perk(d.perk));if(d.synergy!=BuildSynergy.None)text.AppendLine("SINERGIA: "+CombatPerkDescriptions.Synergy(d.synergy));
            text.AppendLine();text.AppendLine(d.description);text.AppendLine("Reciclagem: "+InventoryRules.SellValue(catalog,item)+" ouro");
            text.AppendLine("Bônus sujeitos aos limites globais da ficha.");return text.ToString();
        }
    }
}
