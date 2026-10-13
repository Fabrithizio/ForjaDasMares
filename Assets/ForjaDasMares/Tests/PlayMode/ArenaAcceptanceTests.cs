using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ForjaDasMares.Tests
{
    public sealed class ArenaAcceptanceTests
    {
        ArenaSession session;
        PlayerMotor motor;
        MeleeAttack combat;
        ArenaCamera follow;

        [UnitySetUp]
        public IEnumerator LoadArena()
        {
            SceneManager.LoadScene("ArenaTest");
            yield return null;
            yield return null;
            session = Object.FindFirstObjectByType<ArenaSession>();
            Assert.That(session, Is.Not.Null, "Saved arena must contain a configured ArenaSession.");
            motor = session.Player;
            combat = motor.GetComponent<MeleeAttack>();
            follow = Object.FindFirstObjectByType<ArenaCamera>();
            motor.enabled = false;
            motor.GetComponent<PlayerAim>().enabled = false;
            motor.GetComponent<ArenaInput>().enabled = false;
            combat.enabled = false;
            motor.GetComponent<HeroCombat>().enabled = false;
            motor.GetComponent<HeroRoster>().enabled = false;
            session.enabled = false;
        }

        Vector3 Travel(Vector2 input, float deltaTime, int steps)
        {
            session.ResetArena();
            Vector3 start = motor.transform.position;
            for (int i = 0; i < steps; i++) motor.Simulate(deltaTime, input);
            Vector3 displacement = motor.transform.position - start; displacement.y = 0f;
            return displacement;
        }

        [Test]
        public void AllEightDirectionsMoveAndDiagonalSpeedIsNormalized()
        {
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right, new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1) };
            float baseline = Travel(Vector2.up, 0.01f, 60).magnitude;
            Assert.That(baseline, Is.GreaterThan(2.8f));
            foreach (Vector2 direction in directions)
            {
                Vector3 traveled = Travel(direction, 0.01f, 60);
                Assert.That(traveled.magnitude, Is.EqualTo(baseline).Within(0.04f), "Direction " + direction);
                Assert.That(Vector3.Dot(traveled.normalized, motor.WorldDirection(direction).normalized), Is.GreaterThan(0.99f));
            }
        }

        [Test]
        public void DisplacementRemainsConsistentAtThirtySixtyAndOneHundredTwentyFps()
        {
            float a = Travel(Vector2.up, 1f / 30f, 18).magnitude;
            float b = Travel(Vector2.up, 1f / 60f, 36).magnitude;
            float c = Travel(Vector2.up, 1f / 120f, 72).magnitude;
            Assert.That(Mathf.Abs(a - c), Is.LessThan(0.12f));
            Assert.That(Mathf.Abs(b - c), Is.LessThan(0.08f));
        }

        [Test]
        public void MovementAndDodgeCannotCrossPerimeterWall()
        {
            motor.Teleport(new Vector3(10.5f, 0.06f, -5f), Quaternion.identity);
            Assert.That(motor.TryDodge(new Vector2(1, -1)), Is.True);
            for (int i = 0; i < 120; i++) motor.Simulate(1f / 60f, new Vector2(1, -1));
            Assert.That(motor.transform.position.x, Is.InRange(11.4f, 11.8f));
            Assert.That(Mathf.Abs(motor.transform.position.z + 5f), Is.LessThan(0.15f));
        }

        [Test]
        public void ObstacleBlocksAndPlayerCanMoveAlongItsFace()
        {
            motor.Teleport(new Vector3(1.5f, 0.06f, 0f), Quaternion.identity);
            for (int i = 0; i < 60; i++) motor.Simulate(0.02f, new Vector2(1, -1));
            Assert.That(motor.transform.position.x, Is.LessThan(2.75f));
            Vector3 start = motor.transform.position;
            for (int i = 0; i < 30; i++) motor.Simulate(0.02f, new Vector2(1, 1));
            Assert.That(motor.transform.position.z - start.z, Is.GreaterThan(2f));
        }

        void Swing()
        {
            Assert.That(combat.TryBeginAttack(), Is.True);
            combat.Simulate(0.09f);
            combat.Simulate(0.4f);
        }

        [Test]
        public void StrikeDealsDamageOnceEvenWithMultipleTargetCollidersAndResetRestoresEverything()
        {
            motor.Teleport(new Vector3(0, 0.06f, 0), Quaternion.identity);
            BoxCollider extra = session.TrainingTarget.gameObject.AddComponent<BoxCollider>();
            extra.center = new Vector3(0, 0.95f, 0); extra.size = Vector3.one;
            Physics.SyncTransforms();
            Assert.That(combat.TryBeginAttack(), Is.True);
            Assert.That(combat.TryBeginAttack(), Is.False, "A second attack during recovery must be rejected.");
            combat.Simulate(0.09f);
            Assert.That(session.TrainingTarget.Current, Is.EqualTo(75));
            combat.Simulate(0.4f);
            Swing(); Swing(); Swing();
            Assert.That(session.TrainingTarget.Current, Is.Zero);
            session.ResetArena();
            Assert.That(session.TrainingTarget.Current, Is.EqualTo(100));
            Assert.That(Vector3.Distance(motor.transform.position, session.Spawn.position), Is.LessThan(0.001f));
            Assert.That(combat.IsAttacking, Is.False);
            Object.Destroy(extra);
        }

        [Test]
        public void AttackCannotHitThroughWallsOrBehindPlayer()
        {
            motor.Teleport(new Vector3(0, 0.06f, 3), Quaternion.identity);
            Swing();
            Assert.That(session.TrainingTarget.Current, Is.EqualTo(100));
            motor.Teleport(new Vector3(0, 0.06f, 0), Quaternion.identity);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.layer = LayerMask.NameToLayer("ArenaSolid"); wall.transform.position = new Vector3(0, 1, 0.7f); wall.transform.localScale = new Vector3(2, 2, 0.2f);
            Physics.SyncTransforms();
            Swing();
            Assert.That(session.TrainingTarget.Current, Is.EqualTo(100));
            Object.Destroy(wall);
        }

        [Test]
        public void CameraKeepsPlayerFramedOnWideAndNarrowScreensAndAvoidsPerimeter()
        {
            Camera view = follow.GetComponent<Camera>();
            foreach (float aspect in new[] { 16f / 9f, 4f / 3f, 20f / 9f, 9f / 16f })
            {
                view.aspect = aspect; follow.Snap();
                Vector3 point = view.WorldToViewportPoint(motor.transform.position + Vector3.up * 0.9f);
                Assert.That(point.z, Is.GreaterThan(view.nearClipPlane));
                Assert.That(point.x, Is.InRange(0.25f, 0.75f)); Assert.That(point.y, Is.InRange(0.25f, 0.75f));
            }
            motor.Teleport(new Vector3(10.8f, 0.06f, -8f), Quaternion.identity); follow.Snap();
            Assert.That(Physics.CheckSphere(view.transform.position, 0.25f, 1 << LayerMask.NameToLayer("ArenaSolid")), Is.False);
            motor.Teleport(new Vector3(11.6f, 0.06f, -5f), Quaternion.identity); follow.Snap();
            foreach (float height in new[] { 0.05f, 1.8f })
            {
                Vector3 point = view.WorldToViewportPoint(motor.transform.position + Vector3.up * height);
                Assert.That(point.z, Is.GreaterThan(view.nearClipPlane));
                Assert.That(point.y, Is.InRange(0.05f, 0.95f), "The whole hero should remain visible near a wall.");
            }
        }

        [Test]
        public void SavedSceneHasNoMissingComponentsAndInputAssetContainsExpectedBindings()
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    foreach (Component component in child.GetComponents<Component>()) Assert.That(component, Is.Not.Null, child.name);
            Assert.That(follow.Target, Is.EqualTo(motor.transform));
            Assert.That(follow.GetComponent<AudioListener>(), Is.Not.Null, "Arena camera requires the enabled audio module and AudioListener.");
            Assert.That(motor.Commands, Is.Not.Null);
            Assert.That(session.TrainingTarget.Maximum, Is.EqualTo(100));
            Assert.That(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline, Is.Not.Null, "URP must be active, not only installed.");
            InputActionAsset actions = motor.GetComponent<ArenaInput>().Actions;
            foreach (string actionName in new[] { "Move", "Point", "Attack", "Dodge", "Reset" })
                Assert.That(actions.FindAction("Gameplay/" + actionName, true), Is.Not.Null);
            Assert.That(actions.FindAction("Gameplay/Move").bindings.Count, Is.GreaterThanOrEqualTo(11));
            InputSystemUIInputModule module = Object.FindFirstObjectByType<InputSystemUIInputModule>();
            Assert.That(module.actionsAsset, Is.EqualTo(actions));
            Assert.That(module.point.action, Is.Not.Null); Assert.That(module.leftClick.action, Is.Not.Null);
        }

        [Test]
        public void RealInputSystemAdapterReadsNormalizedKeyboardAndGamepadCommands()
        {
            var previousMode = InputSystem.settings.updateMode;
            var previousBackground = InputSystem.settings.backgroundBehavior;
#if UNITY_EDITOR
            var previousFocus = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            // Batchmode has no focused Game view. Route synthetic test devices to player input.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            ArenaInput reader = motor.GetComponent<ArenaInput>();
            reader.enabled = true;
            try
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.D)); InputSystem.Update();
                Assert.That(reader.Move.magnitude, Is.EqualTo(1f).Within(0.001f));
                Assert.That(reader.Move.x, Is.GreaterThan(0.6f)); Assert.That(reader.Move.y, Is.GreaterThan(0.6f));
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
                InputSystem.QueueStateEvent(gamepad, new GamepadState { leftStick = Vector2.left }); InputSystem.Update();
                Assert.That(reader.Move.x, Is.LessThan(-0.9f));
                InputSystem.QueueStateEvent(gamepad, new GamepadState()); InputSystem.Update();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R, Key.Space)); InputSystem.Update();
                Assert.That(reader.TakeReset(), Is.True); Assert.That(reader.TakeReset(), Is.False);
                Assert.That(reader.TakeDodge(), Is.True);
            }
            finally
            {
                reader.enabled = false;
                InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(gamepad);
                InputSystem.settings.updateMode = previousMode;
                InputSystem.settings.backgroundBehavior = previousBackground;
#if UNITY_EDITOR
                InputSystem.settings.editorInputBehaviorInPlayMode = previousFocus;
#endif
            }
        }

        [UnityTest]
        public IEnumerator CameraSmoothlyFollowsAChangedPlayerPosition()
        {
            Vector3 oldPosition = follow.transform.position;
            motor.Teleport(new Vector3(3f, 0.06f, -5f), Quaternion.identity);
            yield return new WaitForSeconds(0.65f);
            Assert.That(Vector3.Distance(follow.transform.position, oldPosition), Is.GreaterThan(1f));
            Vector3 point = follow.GetComponent<Camera>().WorldToViewportPoint(motor.transform.position + Vector3.up * 0.9f);
            Assert.That(point.x, Is.EqualTo(0.5f).Within(0.08f));
            Assert.That(point.y, Is.EqualTo(0.5f).Within(0.08f));
        }

        [UnityTest]
        public IEnumerator TouchWidgetsSendVirtualGamepadCommandsWithoutSubmittingReset()
        {
            ArenaHud hud = Object.FindFirstObjectByType<ArenaHud>();
            TouchMoveStick stick = hud.GetComponentInChildren<TouchMoveStick>(true);
            OnScreenButton attackButton = null;
            foreach (OnScreenButton button in hud.GetComponentsInChildren<OnScreenButton>(true))
                if (button.controlPath == "<Gamepad>/buttonSouth") attackButton = button;
            Assert.That(attackButton, Is.Not.Null);
            // Test the UI event adapter directly; physical multi-touch remains a device test.
            stick.transform.parent.gameObject.SetActive(true);
            ArenaInput reader = motor.GetComponent<ArenaInput>(); reader.enabled = true;
            yield return null;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(null, stick.transform.position);
            var pointer = new PointerEventData(EventSystem.current) { position = center };
            stick.OnPointerDown(pointer);
            pointer.position += Vector2.right * stick.Radius * hud.GetComponent<Canvas>().scaleFactor;
            stick.OnDrag(pointer);
            yield return null;
            Assert.That(reader.Move.x, Is.GreaterThan(0.8f));
            stick.OnPointerUp(pointer);
            yield return null;
            Assert.That(reader.Move.magnitude, Is.LessThan(0.01f));
            motor.Teleport(new Vector3(2f, 0.06f, -6f), Quaternion.identity);
            EventSystem.current.SetSelectedGameObject(hud.GetComponentInChildren<Button>().gameObject);
            attackButton.OnPointerDown(pointer);
            yield return null;
            Assert.That(reader.TakeAttack(), Is.True);
            Assert.That(Vector3.Distance(motor.transform.position, session.Spawn.position), Is.GreaterThan(1f));
            attackButton.OnPointerUp(pointer);
        }
    }
}
