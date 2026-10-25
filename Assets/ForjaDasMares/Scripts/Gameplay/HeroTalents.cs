using System;
using UnityEngine;
namespace ForjaDasMares
{
    public enum TalentId { Vitality, Might, Guard, Haste, Precision, Focus }
    [Serializable] public sealed class TalentRule
    {public TalentId id;public string displayName;public ItemStat stat;public float perRank;public int maximumRank=5;public Sprite icon;[TextArea]public string description;}
    [Serializable] public sealed class HeroTalentAllocation
    {public string heroId;public int[] ranks=new int[6];}
    public static class HeroTalents
    {
        public static TalentRule[] Defaults()=>new[]{
            Rule(TalentId.Vitality,"Vigor",ItemStat.Health,8),Rule(TalentId.Might,"Força",ItemStat.Damage,.025f),
            Rule(TalentId.Guard,"Tenacidade",ItemStat.Defense,4),Rule(TalentId.Haste,"Agilidade",ItemStat.AttackSpeed,.025f),
            Rule(TalentId.Precision,"Precisão",ItemStat.CriticalChance,.015f),Rule(TalentId.Focus,"Disciplina",ItemStat.CooldownReduction,.02f)};
        static TalentRule Rule(TalentId id,string name,ItemStat stat,float value)=>new TalentRule{id=id,displayName=name,stat=stat,perRank=value};
        public static HeroTalentAllocation Allocation(PlayerProfile profile,string hero)
        {if(profile.talents!=null)foreach(var set in profile.talents)if(set.heroId==hero)return set;return null;}
        public static int Rank(PlayerProfile profile,string hero,TalentId id)=>Allocation(profile,hero)?.ranks[(int)id]??0;
        public static int Spent(PlayerProfile profile,string hero){int count=0;var set=Allocation(profile,hero);if(set!=null)foreach(int rank in set.ranks)count+=rank;return count;}
        public static int Available(PlayerProfile profile,string hero,ProgressionCatalog catalog)=>Mathf.Max(0,ProgressionStats.Level(profile.experience,catalog.maximumLevel)-1-Spent(profile,hero));
        public static string Signature(PlayerProfile profile,string hero){var set=Allocation(profile,hero);return set==null?"":string.Join(",",set.ranks);}
        public static int ResetCost(PlayerProfile profile,string hero,ProgressionCatalog catalog)=>Spent(profile,hero)*catalog.talentResetGold;
    }
}
