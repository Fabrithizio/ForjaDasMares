using System;
using System.Collections;
using UnityEngine;
namespace ForjaDasMares
{
    // Development-only executable validation. Normal play never creates this object.
    public sealed class ProgressionSmokeProbe : MonoBehaviour
    {
        string mode;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void StartIfRequested()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            var args=Environment.GetCommandLineArgs(); for(int i=0;i<args.Length-1;i++) if(args[i]=="-progressionSmoke")
            { var probe=new GameObject("ProgressionSmokeProbe").AddComponent<ProgressionSmokeProbe>(); probe.mode=args[i+1]; break; }
#endif
        }
        IEnumerator Start()
        {
            yield return new WaitForSeconds(.3f); var bridge=UnityEngine.Object.FindFirstObjectByType<ProgressionSceneBridge>();
            if(bridge==null||bridge.Service==null) { Debug.LogError("PROGRESSION_SMOKE_FAILED initialization"); Application.Quit(2); yield break; }
            if(mode=="write")
            {
                var armor=bridge.Service.Profile.inventory.Find(item=>item.definitionId=="brasa-armor");
                if(armor==null||!bridge.Equip(armor.instanceId,EquipmentSlot.Armor)||!bridge.Service.SetOptions(.6f,true,true)) { Fail(bridge.Service.Message); yield break; }
                var dungeon=bridge.GetComponent<DungeonSession>(); if(dungeon==null) dungeon=UnityEngine.Object.FindFirstObjectByType<DungeonSession>();
                var room=dungeon.Generator.Rooms[3]; var motor=bridge.Heroes.GetComponent<PlayerMotor>(); motor.Teleport(room.Center+Vector3.up*.06f,Quaternion.identity); dungeon.Enter(room);
                DungeonInteractable chest=null; foreach(var item in room.interactions) if(item.Kind==DungeonInteractionKind.Chest) chest=item;
                motor.Teleport(chest.transform.position+new Vector3(0,.06f,-1),Quaternion.identity);
                if(!chest.Use()) { Fail("chest"); yield break; } bridge.Heroes.Health.ApplyDamage(9999);
                if(bridge.Service.Profile.inventory.Count!=13||bridge.Service.Profile.experience!=5||bridge.Service.Profile.coins!=5) { Fail("write totals"); yield break; }
                Debug.Log("PROGRESSION_SMOKE_WRITE_OK inventory=13 xp=5 coins=5 armor=equipped options=saved");
            }
            else if(mode=="read")
            {
                var profile=bridge.Service.Profile; var loadout=profile.Loadout("warrior"); var armor=loadout!=null?profile.Item(loadout.items[2]):null;
                if(profile.inventory.Count!=13||profile.experience!=5||profile.coins!=5||armor==null||armor.definitionId!="brasa-armor"||Mathf.Abs(profile.options.volume-.6f)>.001f||!profile.options.largeText||bridge.Heroes.Health.Maximum!=160) { Fail("reload state"); yield break; }
                Debug.Log("PROGRESSION_SMOKE_READ_OK inventory=13 xp=5 coins=5 armor=equipped maxHealth=160 options=restored");
            }
            else { Fail("unknown mode"); yield break; }
            yield return new WaitForSeconds(.3f); Application.Quit(0);
        }
        void Fail(string reason) { Debug.LogError("PROGRESSION_SMOKE_FAILED "+reason); Application.Quit(3); }
    }
}
