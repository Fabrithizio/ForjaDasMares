using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    // Opt-in local measurements. Frame time is not a separate CPU/GPU measurement.
    public sealed class CampaignPerformanceHud:MonoBehaviour
    {
        CoopDirector session;GameObject panel;Text readout;float[] frames=new float[240],sorted=new float[240];int count,index;float refresh;bool visible;
        public void Build(Transform parent,CoopDirector owner)
        {
            session=owner;var r=RusticHudTheme.Rect(parent,"PerformanceReadout",new Vector2(1,1),new Vector2(-20,-140),new Vector2(330,122));panel=r.gameObject;RusticHudTheme.Panel(r,RusticHudTheme.Ink,12);
            readout=RusticHudTheme.Label(r,"Measurements",new Vector2(0,1),new Vector2(10,-6),new Vector2(310,110),"",14);panel.SetActive(false);
        }
        public void Toggle(){visible=!visible;if(panel!=null)panel.SetActive(visible);}
        public int SampleCount=>count;
        void Update()
        {
            if(panel==null)return;if(Keyboard.current!=null&&Keyboard.current.f8Key.wasPressedThisFrame)Toggle();
            bool gameplay=session.Phase==CoopPhase.Playing&&GetComponent<CoopInventoryHud>()?.IsOpen!=true&&GetComponent<CampaignSessionMenu>()?.IsOpen!=true;
            // Menu/lobby frames must not make combat measurements look faster.
            if(!gameplay){count=index=0;refresh=0;panel.SetActive(false);return;}
            float dt=Time.unscaledDeltaTime;if(dt>0&&dt<1){frames[index]=dt*1000;index=(index+1)%frames.Length;count=Mathf.Min(count+1,frames.Length);}
            bool show=visible;panel.SetActive(show);refresh-=dt;if(!show||refresh>0||count==0)return;refresh=.5f;
            float sum=0;for(int i=0;i<count;i++){sum+=frames[i];sorted[i]=frames[i];}Array.Sort(sorted,0,count);float average=sum/count,p95=sorted[Mathf.Clamp(Mathf.CeilToInt(count*.95f)-1,0,count-1)];
            var pool=session.Dungeon.Encounter.Enemies;var effects=session.Actor(session.LocalSlot)?.Combat.Pool;
            string enemyCount=session.IsHost?pool.ActiveCount+"/"+pool.Limit:session.AppliedEnemyCount+" recebidos";
            string effectCount=session.IsHost?"projéteis "+(effects?.ActiveProjectiles??0):"efeitos recebidos "+session.AppliedEffectCount;
            readout.text="DIAGNÓSTICO LOCAL · F8\n"+(1000/average).ToString("0")+" FPS · quadro p95 "+p95.ToString("0.0")+" ms\nMemória Unity "+(Profiler.GetTotalAllocatedMemoryLong()/1048576f).ToString("0")+" MB\nInimigos "+enemyCount+" · "+effectCount+"\nÚltimo estado de rede "+session.LastSnapshotBytes+" bytes";
        }
    }
}
