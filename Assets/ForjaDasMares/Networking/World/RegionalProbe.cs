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
            var args=Environment.GetCommandLineArgs();if(Array.IndexOf(args,"-regionProbe")>=0||Array.IndexOf(args,"-catacombProbe")>=0||Array.IndexOf(args,"-actOneProbe")>=0||Array.IndexOf(args,"-townProbe")>=0)new GameObject("RegionalProbeLauncher").AddComponent<RegionalProbe>().tick=1;
        }
        void Update()
        {
            if(world!=null&&Time.timeSinceLevelLoad>5){samples++;totalFrameSeconds+=Time.unscaledDeltaTime;worstFrameSeconds=Mathf.Max(worstFrameSeconds,Time.unscaledDeltaTime);}tick-=Time.unscaledDeltaTime;if(tick>0)return;tick=.3f;
            if(world==null)
            {
                var launchArgs=Environment.GetCommandLineArgs();string targetScene=Array.IndexOf(launchArgs,"-townProbe")>=0?"RefugioUltimoSino":Array.IndexOf(launchArgs,"-actOneProbe")>=0?"CostaAto1":Array.IndexOf(launchArgs,"-catacombProbe")>=0?"CatacumbasDasMares":"CostaDasRuinas";if(SceneManager.GetActiveScene().name!=targetScene){if(!loading){loading=true;SceneManager.LoadSceneAsync(targetScene);DontDestroyOnLoad(gameObject);}return;}
                world=UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-regionProbe");if(i<0)i=Array.IndexOf(args,"-catacombProbe");if(i<0)i=Array.IndexOf(args,"-actOneProbe");if(i<0)i=Array.IndexOf(args,"-townProbe");path=Path.GetFullPath(args[i+1]);Directory.CreateDirectory(path);
            }
            string control=Path.Combine(path,"control.txt");
            if(File.Exists(control))
            {
                string command=File.ReadAllText(control).Trim();File.Delete(control);
                if(command=="capture")ScreenCapture.CaptureScreenshot(Path.Combine(path,"screen.png"));
                else if(command=="exit"){Application.Quit();return;}
                else if(command=="fps")UnityEngine.Object.FindFirstObjectByType<CampaignPerformanceHud>()?.Toggle();
                else if(command=="training")
                {
                    var dummy=GameObject.Find("TrainingDummy");if(dummy!=null)world.Hero.GetComponent<PlayerMotor>().Teleport(dummy.transform.position+Vector3.back*3-Vector3.up*.9f,Quaternion.identity);
                }
                else if(command=="hoverTraining")
                {
                    var dummy=GameObject.Find("TrainingDummy");var mouse=UnityEngine.InputSystem.Mouse.current;
                    if(dummy!=null&&mouse!=null){var pixel=(Vector2)Camera.main.WorldToScreenPoint(dummy.transform.position);UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,new UnityEngine.InputSystem.LowLevel.MouseState{position=pixel});}
                }
                else if(command=="die")world.Hero.Health.ApplyDamage(10000);
                else if(command=="respawn")world.Respawn();
                else if(command.StartsWith("story")&&world.Journey!=null&&int.TryParse(command.Substring(5),out int point)&&point>=0&&point<world.Journey.Definition.points.Length){world.Hero.GetComponent<PlayerMotor>().Teleport(world.Journey.Definition.points[point].position+Vector3.back*1.3f,Quaternion.identity);world.Hero.Health.Restore();world.Interact();}
                else if(command.StartsWith("camp")){int c=int.Parse(command.Substring(4));world.Hero.GetComponent<PlayerMotor>().Teleport(world.Definition.camps[c].center+new Vector3(0,.06f,-10),Quaternion.identity);world.Hero.Health.Restore();}
                else if(command=="clear-near"){foreach(var enemy in world.Enemies.Actors)if(enemy.IsAlive&&(enemy.transform.position-world.Hero.transform.position).sqrMagnitude<24*24)enemy.Health.ApplyDamage(10000);}
            }
            File.WriteAllText(Path.Combine(path,"report.json"),JsonUtility.ToJson(new Report{position=world.Hero.transform.position,hp=world.Hero.Health.Current,enemies=world.Enemies.ActiveCount,cleared=world.ClearedCount,portal=world.PortalReady,storyAccepted=world.Journey!=null&&world.Journey.Accepted,storyComplete=world.Journey!=null&&world.Journey.Completed,clues=world.Journey!=null?world.Journey.ClueCount:0,sampledFrames=samples,averageFrameMs=samples>0?totalFrameSeconds*1000/samples:0,worstFrameMs=worstFrameSeconds*1000,allocatedMB=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()/1048576f,gpu=SystemInfo.graphicsDeviceName}));
        }
        [Serializable]sealed class Report{public Vector3 position;public int hp,enemies,cleared,clues;public bool portal,storyAccepted,storyComplete;public int sampledFrames;public float averageFrameMs,worstFrameMs,allocatedMB;public string gpu;}
#endif
    }
}
