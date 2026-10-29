using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class ArpgControlsTests
    {
        InputSettings.BackgroundBehavior oldBackground;InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        RegionalWorld world;ArenaInput input;DesktopRpgControls controls;Mouse mouse;Keyboard keyboard;bool hadMode;int priorMode;
        void Point(Vector3 point,bool down){Vector2 pixel=Camera.main.WorldToScreenPoint(point);var state=new MouseState{position=pixel};if(down)state=state.WithButton(MouseButton.Left);InputSystem.QueueStateEvent(mouse,state);InputSystem.Update();}
        [UnitySetUp]public IEnumerator Load()
        {
            oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            hadMode=PlayerPrefs.HasKey(DesktopRpgControls.ModeKey);priorMode=PlayerPrefs.GetInt(DesktopRpgControls.ModeKey);PlayerPrefs.SetInt(DesktopRpgControls.ModeKey,0);
            yield return SceneManager.LoadSceneAsync("CostaDasRuinas");yield return null;yield return null;
            world=Object.FindFirstObjectByType<RegionalWorld>();input=world.Hero.GetComponent<ArenaInput>();controls=input.Desktop;Assert.NotNull(controls);
            mouse=InputSystem.AddDevice<Mouse>();keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.EnableDevice(mouse);InputSystem.EnableDevice(keyboard);Point(world.Hero.transform.position+Vector3.forward*5,false);yield return null;yield return null;
        }
        [UnityTearDown]public IEnumerator Close()
        {if(mouse!=null)InputSystem.RemoveDevice(mouse);if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(hadMode)PlayerPrefs.SetInt(DesktopRpgControls.ModeKey,priorMode);else PlayerPrefs.DeleteKey(DesktopRpgControls.ModeKey);InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;yield return null;}
        [UnityTest]public IEnumerator HeldFireChangesDirectionAndReleasedShotDoesNotCurve()
        {
            var hero=world.Hero;var pool=hero.GetComponent<HeroCombat>().Pool;Point(hero.transform.position+Vector3.forward*6,true);
            CombatProjectile first=null;float end=Time.time+2;
            while(first==null&&Time.time<end){first=pool.Projectiles.FirstOrDefault(p=>p.OwnedBy(hero.Health));yield return null;}
            TestContext.WriteLine("INPUT_DIAG pressed="+mouse.leftButton.isPressed+" raw="+input.MouseAttackHeld+" pointer="+input.HasPointer+" blocked="+input.CombatBlocked+" ui="+input.PointerIsBlocked+" can="+controls.CanAttack+" device="+mouse.enabled+" health="+hero.Health.Current);
            Assert.NotNull(first,"Holding mouse must release a real projectile");Vector3 committed=first.transform.forward;
            Vector3 target=hero.transform.position+Vector3.left*5;Point(target,true);yield return new WaitForSeconds(.06f);
            if(first.OwnedBy(hero.Health))Assert.Greater(Vector3.Dot(committed,first.transform.forward),.999f,"Already released shots keep their trajectory");
            bool changed=false;end=Time.time+2;
            while(!changed&&Time.time<end){changed=pool.Projectiles.Any(p=>p.OwnedBy(hero.Health)&&Vector3.Dot(p.transform.forward,Vector3.left)>.94f);yield return null;}
            Assert.IsTrue(changed,"Following attacks must use the moved cursor while the same press remains held");
        }
        [UnityTest]public IEnumerator MouseGroundClickWalksAndDoesNotAttackGround()
        {
            PlayerPrefs.SetInt(DesktopRpgControls.ModeKey,1);yield return null;
            Vector3 start=world.Hero.transform.position,goal=start+Vector3.forward*5;int version=world.Hero.GetComponent<HeroCombat>().ActionVersion;
            Point(goal,true);yield return new WaitForSeconds(.1f);Point(goal,false);yield return new WaitForSeconds(1.3f);
            Assert.Greater(Vector3.Distance(start,world.Hero.transform.position),2);Assert.Less(Vector3.Distance(goal,world.Hero.transform.position),1.2f);
            Assert.AreEqual(version,world.Hero.GetComponent<HeroCombat>().ActionVersion);
        }
        [UnityTest]public IEnumerator MouseEnemyClickApproachesAndRetargetsWhileHeld()
        {
            PlayerPrefs.SetInt(DesktopRpgControls.ModeKey,1);input.PointerBlocked=_=>false;yield return new WaitForSeconds(1.2f);
            var camp=world.Definition.camps[0];var enemies=world.Enemies.Actors.Where(e=>e.IsAlive&&(e.transform.position-camp.center).sqrMagnitude<18*18).OrderBy(e=>e.transform.position.x).ToArray();Assert.GreaterOrEqual(enemies.Length,3);
            foreach(var enemy in enemies){enemy.Brain.enabled=false;enemy.Motor.Stop();}
            var motor=world.Hero.GetComponent<PlayerMotor>();motor.Teleport(camp.center+new Vector3(0,.06f,-5),Quaternion.identity);Camera.main.GetComponent<ArenaCamera>().Snap();Vector3 start=world.Hero.transform.position;
            Point(enemies[0].transform.position+Vector3.up*.8f,true);yield return new WaitForSeconds(.35f);Assert.Greater(Vector3.Distance(start,world.Hero.transform.position),.15f,"Out-of-range target should be approached");
            var second=enemies[1];int hp=second.Health.Current;Point(second.transform.position+Vector3.up*.8f,true);yield return new WaitForSeconds(.85f);
            Assert.Less(second.Health.Current,hp,"The same held press should attack the newly hovered enemy");
        }
        [UnityTest]public IEnumerator ClickNavigationGoesAroundCarvedObstacle()
        {
            PlayerPrefs.SetInt(DesktopRpgControls.ModeKey,1);input.PointerBlocked=_=>false;var start=world.Hero.transform.position;
            var block=GameObject.CreatePrimitive(PrimitiveType.Cube);block.layer=9;block.transform.position=start+new Vector3(0,1,3);block.transform.localScale=new Vector3(2,2,1);
            var obstacle=block.AddComponent<UnityEngine.AI.NavMeshObstacle>();obstacle.shape=UnityEngine.AI.NavMeshObstacleShape.Box;obstacle.size=Vector3.one;obstacle.carving=true;
            yield return new WaitForSeconds(1);var goal=start+Vector3.forward*7;Point(goal,true);yield return new WaitForSeconds(.1f);Point(goal,false);
            float until=Time.time+3;bool crossed=false;
            while(Time.time<until){crossed|=block.GetComponent<Collider>().bounds.Contains(world.Hero.transform.position+Vector3.up);yield return null;}
            Assert.IsFalse(crossed,"The character must not cross the obstacle");Assert.Less(Vector3.Distance(goal,world.Hero.transform.position),1.3f);Object.Destroy(block);
        }
        [UnityTest]public IEnumerator ShiftAllowsStationaryGroundAttackInMouseMode()
        {
            PlayerPrefs.SetInt(DesktopRpgControls.ModeKey,1);InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftShift));InputSystem.Update();yield return null;
            Vector3 start=world.Hero.transform.position;Point(start+Vector3.forward*5,true);yield return new WaitForSeconds(.6f);
            Assert.Less(Vector3.Distance(start,world.Hero.transform.position),.15f);Assert.Greater(world.Hero.GetComponent<HeroCombat>().ActionVersion,0);
        }
        [UnityTest]public IEnumerator UiClickDoesNotMoveOrAttack()
        {
            PlayerPrefs.SetInt(DesktopRpgControls.ModeKey,1);input.PointerBlocked=_=>true;yield return null;Vector3 start=world.Hero.transform.position;int version=world.Hero.GetComponent<HeroCombat>().ActionVersion;
            Point(start+Vector3.forward*5,true);yield return new WaitForSeconds(.5f);Assert.Less(Vector3.Distance(start,world.Hero.transform.position),.15f);Assert.AreEqual(version,world.Hero.GetComponent<HeroCombat>().ActionVersion);
        }
        [UnityTest]public IEnumerator KeyboardOverrideStaysNormalizedInMouseMode()
        {
            PlayerPrefs.SetInt(DesktopRpgControls.ModeKey,1);InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.D));InputSystem.Update();Vector3 start=world.Hero.transform.position;yield return new WaitForSeconds(.5f);
            Assert.Greater(Vector3.Distance(start,world.Hero.transform.position),1);Assert.LessOrEqual(input.Move.magnitude,1.001f);Assert.LessOrEqual(world.Hero.GetComponent<PlayerMotor>().PlanarVelocity.magnitude,world.Hero.GetComponent<PlayerMotor>().Speed+.01f);
        }
        [UnityTest]public IEnumerator DeathCancelsNavigationBeforeRespawn()
        {
            PlayerPrefs.SetInt(DesktopRpgControls.ModeKey,1);yield return null;Point(world.Hero.transform.position+Vector3.forward*6,true);yield return new WaitForSeconds(.1f);Point(world.Hero.transform.position+Vector3.forward*6,false);
            world.Hero.Health.ApplyDamage(10000);yield return new WaitForSeconds(.1f);world.Respawn();yield return new WaitForSeconds(.4f);Assert.Less(Vector3.Distance(world.Hero.transform.position,world.Definition.refuge),.2f);
        }
        [Test]public void InvalidDestinationIsRejected(){Assert.IsFalse(controls.Plan(new Vector3(10000,0,10000)));}
        [UnityTest]public IEnumerator ZoomChangesDistanceWithoutChangingCameraPitch()
        {var camera=Camera.main;var follow=camera.GetComponent<ArenaCamera>();Quaternion rotation=camera.transform.rotation;float before=Vector3.Distance(camera.transform.position,world.Hero.transform.position);follow.SetZoom(.75f);yield return new WaitForSeconds(.5f);Assert.Less(Vector3.Distance(camera.transform.position,world.Hero.transform.position),before-1);Assert.Less(Quaternion.Angle(rotation,camera.transform.rotation),.1f);}
    }
}
