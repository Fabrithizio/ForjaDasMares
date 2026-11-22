using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    // F8: FPS and memory on this device. Frame time is not a separate CPU/GPU measurement.
    public sealed class CampaignPerformanceHud:MonoBehaviour
    {
        RegionalWorld region;GameObject panel;Text readout;float[] frames=new float[240],sorted=new float[240];int count,index;float refresh;bool visible;
        public bool IsVisible=>panel!=null&&panel.activeSelf;
        public string Readout=>readout!=null?readout.text:"";
        public void BuildRegional(Transform parent,RegionalWorld owner)
        {
            region=owner;var r=RusticHudTheme.Rect(parent,"PerformanceReadout",new Vector2(0,1),new Vector2(18,-244),new Vector2(300,98));panel=r.gameObject;RusticHudTheme.Panel(r,RusticHudTheme.Ink,12);
            readout=RusticHudTheme.Label(r,"Measurements",new Vector2(0,1),new Vector2(10,-6),new Vector2(280,88),"",14);panel.SetActive(false);
        }
        public void Toggle(){visible=!visible;if(panel!=null)panel.SetActive(visible);}
        public int SampleCount=>count;
        void Update()
        {
            if(panel==null)return;if(Keyboard.current!=null&&Keyboard.current.f8Key.wasPressedThisFrame)Toggle();
            bool gameplay=region!=null&&region.Hero!=null&&GetComponent<CoopInventoryHud>()?.IsOpen!=true&&GetComponent<RegionalSettingsMenu>()?.IsOpen!=true;
            // Menu frames must not make combat measurements look faster.
            if(!gameplay){count=index=0;refresh=0;panel.SetActive(false);return;}
            float dt=Time.unscaledDeltaTime;if(dt>0&&dt<1){frames[index]=dt*1000;index=(index+1)%frames.Length;count=Mathf.Min(count+1,frames.Length);}
            bool show=visible;panel.SetActive(show);refresh-=dt;if(!show||refresh>0||count==0)return;refresh=.5f;
            float sum=0;for(int i=0;i<count;i++){sum+=frames[i];sorted[i]=frames[i];}Array.Sort(sorted,0,count);float average=sum/count,p95=sorted[Mathf.Clamp(Mathf.CeilToInt(count*.95f)-1,0,count-1)];
            readout.text="DESEMPENHO LOCAL · F8\n"+(1000/average).ToString("0")+" FPS · média "+average.ToString("0.0")+" ms\nQuadro p95 "+p95.ToString("0.0")+" ms · Unity "+(Profiler.GetTotalAllocatedMemoryLong()/1048576f).ToString("0")+" MB\nInimigos ativos "+region.Enemies.ActiveCount;
        }
    }
}
