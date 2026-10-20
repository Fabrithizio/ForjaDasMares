using UnityEngine;
namespace ForjaDasMares
{
    public enum WeaponPerk { None, Bleed, Scorch, Fracture, Pull }
    [System.Flags]public enum BuildSynergy { None=0, ScorchedEdge=1, ControlHunter=2, KindredGuard=4 }
    // Authority-only combat modifiers; never grants persistent currency or inventory.
    public sealed class CombatBuildEffects:MonoBehaviour
    {
        Health health;WeaponPerk perk;BuildSynergy synergy;readonly Collider[] nearby=new Collider[24];
        readonly Health[] healed=new Health[24];readonly Health[] recent=new Health[32];readonly float[] times=new float[32];int cursor,supportCharges;
        float counterUntil,lastSupport;readonly int[] boons=new int[3];
        public float Tempo=>1+.1f*boons[1];public float Shelter=>.06f*boons[2];public float Power=>1+.12f*boons[0];public bool CounterReady=>Time.time<counterUntil;
        public int Boon(int index)=>index>=0&&index<3?boons[index]:0;
        void Awake(){health=GetComponent<Health>();}
        void OnEnable(){if(health==null)health=GetComponent<Health>();health.DealtHit+=Hit;}
        void OnDisable(){if(health!=null)health.DealtHit-=Hit;counterUntil=0;}
        public void Configure(WeaponPerk weapon,BuildSynergy set){perk=weapon;synergy=set;}
        public void ResetRun(){System.Array.Clear(boons,0,3);System.Array.Clear(recent,0,32);System.Array.Clear(times,0,32);supportCharges=0;counterUntil=lastSupport=0;}
        public bool ChooseBoon(int choice){if(choice<0||choice>=3||boons[choice]>=3)return false;boons[choice]++;return true;}
        public void SetReplicaBoons(int packed){for(int i=0;i<3;i++)boons[i]=Mathf.Clamp(packed>>(i*2)&3,0,3);}
        public int PackedBoons=>boons[0]|boons[1]<<2|boons[2]<<4;
        public void GrantCounter(){counterUntil=Time.time+3;}
        public float ConsumeCounter(){bool ready=CounterReady;counterUntil=0;return ready?1.25f:1;}
        public float Against(Health victim)
        {
            float bonus=0;var status=victim.GetComponent<CombatAilments>();if((synergy&BuildSynergy.ScorchedEdge)!=0&&status!=null&&status.Burning)bonus+=.2f;
            if((synergy&BuildSynergy.ControlHunter)!=0&&(victim.IsStunned||victim.ControlMultiplier<.99f))bonus+=.15f;
            var enemy=victim.GetComponent<EnemyActor>();if(enemy!=null&&enemy.Profile.miniboss&&enemy.Combat.IsRecovering)bonus+=.15f;return Power*(1+Mathf.Min(.35f,bonus));
        }
        void Hit(Health victim)
        {
            if(health==null||!health.IsAlive||victim==null||victim.Faction!=CombatFaction.Enemy)return;
            bool ready=true;for(int i=0;i<recent.Length;i++)if(recent[i]==victim&&Time.time<times[i]){ready=false;break;}
            if(ready){recent[cursor]=victim;times[cursor]=Time.time+.8f;cursor=(cursor+1)%recent.Length;}
            var combat=GetComponent<HeroCombat>();int baseDamage=combat!=null&&combat.BasicAttack!=null?combat.BasicAttack.damage:10;
            var status=victim.GetComponent<CombatAilments>()??victim.gameObject.AddComponent<CombatAilments>();
            if(ready&&perk==WeaponPerk.Bleed)status.ApplyBleed(health,Mathf.Max(1,Mathf.RoundToInt(baseDamage*.1f)));
            if(ready&&perk==WeaponPerk.Scorch)status.ApplyBurn(health,Mathf.Max(1,Mathf.RoundToInt(baseDamage*.1f)));
            if(ready&&perk==WeaponPerk.Fracture){status.Fracture();var enemy=victim.GetComponent<EnemyActor>();victim.Stun(enemy!=null&&enemy.Profile.kind==EnemyKind.Boss?.12f:.35f);}
            if(ready&&perk==WeaponPerk.Pull&&victim.IsAlive){Vector3 delta=transform.position-victim.transform.position;if(delta.sqrMagnitude>1)victim.Push(delta,2.5f);}
            if(!victim.IsAlive&&(synergy&BuildSynergy.ScorchedEdge)!=0&&status.Burning)
            {
                int count=Physics.OverlapSphereNonAlloc(victim.transform.position,2.5f,nearby,1<<10,QueryTriggerInteraction.Ignore);
                for(int i=0;i<count;i++){var other=nearby[i].GetComponentInParent<Health>();if(other==null||other==victim||!other.IsAlive||Physics.Linecast(victim.transform.position+Vector3.up,other.transform.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore))continue;(other.GetComponent<CombatAilments>()??other.gameObject.AddComponent<CombatAilments>()).ApplyBurn(health,Mathf.Max(1,baseDamage/12));}System.Array.Clear(nearby,0,count);
            }
            if(!victim.IsAlive&&(synergy&BuildSynergy.KindredGuard)!=0&&supportCharges<4&&Time.time>=lastSupport)
            {
                supportCharges++;lastSupport=Time.time+3;int used=0;int count=Physics.OverlapSphereNonAlloc(transform.position,5,nearby,1<<11,QueryTriggerInteraction.Ignore);
                for(int i=0;i<count;i++){var ally=nearby[i].GetComponentInParent<Health>();if(ally==null||!ally.IsAlive||ally.Faction!=health.Faction)continue;bool duplicate=false;for(int j=0;j<used;j++)if(healed[j]==ally)duplicate=true;if(duplicate)continue;healed[used++]=ally;if(!Physics.Linecast(transform.position+Vector3.up,ally.transform.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore))ally.Heal(4);}System.Array.Clear(nearby,0,count);System.Array.Clear(healed,0,used);
            }
        }
    }
}
