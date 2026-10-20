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
            foreach(var b in item.modifiers)values[(int)b.stat]+=Mathf.Clamp(b.value,0,StatModifier.Limit(b.stat));return values;
        }
        public static string Preview(HeroDefinition hero,PlayerProfile profile,ProgressionCatalog catalog,ItemInstance item)
        {
            var definition=catalog.Find(item.definitionId);if(!ProgressionStats.CanEquip(profile,catalog,hero.heroId,item,definition.slot,out string reason))return reason;
            if(InventoryRules.Equipped(profile,item.instanceId))return "Item equipado: seus bônus já estão incluídos na ficha.";
            var a=ProgressionStats.Calculate(hero,profile,catalog);var b=ProgressionStats.Calculate(hero,profile,catalog,item.instanceId,definition.slot,true);
            var main=profile.Item(profile.Loadout(hero.heroId)?.items[0]);var weapon=main!=null?catalog.Find(main.definitionId):null;var attack=weapon!=null?weapon.attacks[0]:hero.attacks[0];var next=definition.slot==EquipmentSlot.MainHand?definition.attacks[0]:attack;
            float power=main!=null?catalog.Rarity(main.rarity).power:1,otherPower=definition.slot==EquipmentSlot.MainHand?catalog.Rarity(item.rarity).power:power;
            int damageA=Mathf.RoundToInt(attack.damage*ProgressionStats.StrikeMultiplier(a.damageMultiplier*power,a.elementalDamage,false));int damageB=Mathf.RoundToInt(next.damage*ProgressionStats.StrikeMultiplier(b.damageMultiplier*otherPower,b.elementalDamage,false));
            return "Dano base da arma "+b.baseDamage+"\nDano normal "+a.damage+" → "+b.damage+"\nDano crítico "+a.criticalDamage+" → "+b.criticalDamage+"\nBônus crítico "+((a.criticalMultiplier-1)*100).ToString("0.#")+"% → "+((b.criticalMultiplier-1)*100).ToString("0.#")+"%\nVida "+a.maximumHealth+" → "+b.maximumHealth+"\nPrimeiro golpe "+damageA+" → "+damageB+" (antes da defesa/crítico)\nDefesa "+(a.defense*100).ToString("0.#")+"% → "+(b.defense*100).ToString("0.#")+"%\nAtaque ×"+a.attackSpeed.ToString("0.##")+" → ×"+b.attackSpeed.ToString("0.##")+"\nCrítico "+(a.criticalChance*100).ToString("0.#")+"% → "+(b.criticalChance*100).ToString("0.#")+"%\nMovimento "+a.speed.ToString("0.#")+" → "+b.speed.ToString("0.#")+"m/s\nRedução de recarga "+((1-a.cooldownMultiplier)*100).ToString("0.#")+"% → "+((1-b.cooldownMultiplier)*100).ToString("0.#")+"%\nElemental "+(a.elementalDamage*100).ToString("0.#")+"% → "+(b.elementalDamage*100).ToString("0.#")+"%\nRegeneração "+a.recovery.ToString("0.#")+" → "+b.recovery.ToString("0.#")+" vida/s";
        }
        public static string Describe(ProgressionCatalog catalog,ItemInstance item,ItemInstance compare=null)
        {
            if(item==null)return "Nenhum item neste slot.";var d=catalog.Find(item.definitionId);var text=new StringBuilder();
            text.AppendLine(d.displayName+" · "+RusticHudTheme.RarityName(item.rarity));text.AppendLine(RusticHudTheme.SlotName(d.slot)+" · nível "+d.minimumLevel);text.AppendLine("Classes: "+RusticHudTheme.ClassNames(d.compatibleHeroes));text.AppendLine();
            var a=Bonuses(catalog,item);var b=Bonuses(catalog,compare);bool any=false;
            for(int i=0;i<10;i++)if(a[i]!=0||b[i]!=0){any=true;var stat=(ItemStat)i;text.Append(Name(stat)+": +"+Amount(stat,a[i]));if(compare!=null){float delta=a[i]-b[i];text.Append("  ("+(delta>=0?"+":"−")+Amount(stat,Mathf.Abs(delta))+")");}text.AppendLine();}
            if(!any)text.AppendLine("Sem bônus passivos. Usa o padrão de ataque abaixo.");
            if(d.frontalBlock>0)text.AppendLine("Bloqueio frontal: "+(Mathf.Min(.5f,d.frontalBlock*catalog.Rarity(item.rarity).power)*100).ToString("0")+"%");
            if(d.attacks!=null&&d.attacks.Length>0){text.AppendLine();text.AppendLine("SEQUÊNCIA DE ATAQUES");foreach(var attack in d.attacks)text.AppendLine(attack.damage+" dano base ×"+catalog.Rarity(item.rarity).power.ToString("0.##")+" · alcance "+attack.range.ToString("0.#")+"m · ciclo "+(attack.windup+attack.recovery).ToString("0.##")+"s · "+(attack.shape==AttackShape.Arc?"arco "+attack.arc.ToString("0")+"°":"projétil")+" · alvos "+(attack.maximumTargets==0?"em área":attack.maximumTargets.ToString()));}
            if(d.ability!=null)text.AppendLine("Habilidade: "+d.ability.displayName+" · recarga "+d.ability.cooldown.ToString("0.#")+"s");
            if(d.perk!=WeaponPerk.None)text.AppendLine("EFEITO DA ARMA: "+CombatPerkDescriptions.Perk(d.perk));if(d.synergy!=BuildSynergy.None)text.AppendLine("SINERGIA: "+CombatPerkDescriptions.Synergy(d.synergy));
            text.AppendLine();text.AppendLine(d.description);text.AppendLine("Reciclagem: "+InventoryRules.SellValue(catalog,item)+" ouro");
            text.AppendLine("Bônus sujeitos aos limites globais da ficha.");return text.ToString();
        }
    }
}
