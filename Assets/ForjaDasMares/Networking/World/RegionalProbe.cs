using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ForjaDasMares.Networking
{
    // Opt-in local development probe; never enabled by gameplay or a network message.
    public sealed class RegionalProbe:MonoBehaviour
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        RegionalWorld world;string path;float tick;bool loading;int samples;float totalFrameSeconds,worstFrameSeconds;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot()
        {
            var args=Environment.GetCommandLineArgs();if(Array.IndexOf(args,"-regionProbe")>=0||Array.IndexOf(args,"-catacombProbe")>=0)new GameObject("RegionalProbeLauncher").AddComponent<RegionalProbe>().tick=1;
        }
        void Update()
        {
            if(world!=null&&Time.timeSinceLevelLoad>5){samples++;totalFrameSeconds+=Time.unscaledDeltaTime;worstFrameSeconds=Mathf.Max(worstFrameSeconds,Time.unscaledDeltaTime);}tick-=Time.unscaledDeltaTime;if(tick>0)return;tick=.3f;
            if(world==null)
            {
                var launchArgs=Environment.GetCommandLineArgs();string targetScene=Array.IndexOf(launchArgs,"-catacombProbe")>=0?"CatacumbasDasMares":"CostaDasRuinas";if(SceneManager.GetActiveScene().name!=targetScene){if(!loading){loading=true;SceneManager.LoadSceneAsync(targetScene);DontDestroyOnLoad(gameObject);}return;}
                world=UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-regionProbe");if(i<0)i=Array.IndexOf(args,"-catacombProbe");path=Path.GetFullPath(args[i+1]);Directory.CreateDirectory(path);
            }
            string control=Path.Combine(path,"control.txt");
            if(File.Exists(control))
            {
                string command=File.ReadAllText(control).Trim();File.Delete(control);
                if(command=="capture")ScreenCapture.CaptureScreenshot(Path.Combine(path,"screen.png"));
                else if(command=="exit"){Application.Quit();return;}
                else if(command=="die")world.Hero.Health.ApplyDamage(10000);
                else if(command=="respawn")world.Respawn();
                else if(command.StartsWith("camp")){int c=int.Parse(command.Substring(4));world.Hero.GetComponent<PlayerMotor>().Teleport(world.Definition.camps[c].center+new Vector3(0,.06f,-10),Quaternion.identity);world.Hero.Health.Restore();}
                else if(command=="clear-near"){foreach(var enemy in world.Enemies.Actors)if(enemy.IsAlive&&(enemy.transform.position-world.Hero.transform.position).sqrMagnitude<24*24)enemy.Health.ApplyDamage(10000);}
            }
            File.WriteAllText(Path.Combine(path,"report.json"),JsonUtility.ToJson(new Report{position=world.Hero.transform.position,hp=world.Hero.Health.Current,enemies=world.Enemies.ActiveCount,cleared=world.ClearedCount,portal=world.PortalReady,sampledFrames=samples,averageFrameMs=samples>0?totalFrameSeconds*1000/samples:0,worstFrameMs=worstFrameSeconds*1000,allocatedMB=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()/1048576f,gpu=SystemInfo.graphicsDeviceName}));
        }
        [Serializable]sealed class Report{public Vector3 position;public int hp,enemies,cleared;public bool portal;public int sampledFrames;public float averageFrameMs,worstFrameMs,allocatedMB;public string gpu;}
#endif
    }
}
