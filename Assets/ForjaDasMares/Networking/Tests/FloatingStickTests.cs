using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ForjaDasMares.Networking.Tests
{
    public sealed class FloatingStickTests
    {
        GameObject root;
        TouchMoveStick stick;
        ArenaInput input;
        EventSystem events;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("RefugioUltimoSino");
            input = Object.FindFirstObjectByType<ArenaInput>();
            root = new GameObject("FloatingStickTest", typeof(Canvas));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            events = EventSystem.current;
            Assert.NotNull(input);
            var baseObject = new GameObject("Stick", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)baseObject.transform; rect.SetParent(root.transform, false);
            rect.anchorMin = rect.anchorMax = Vector2.zero; rect.sizeDelta = new Vector2(160,160); rect.anchoredPosition = new Vector2(110,120);
            var thumb = new GameObject("Thumb", typeof(RectTransform)).GetComponent<RectTransform>(); thumb.SetParent(rect,false);
            stick = baseObject.AddComponent<TouchMoveStick>(); stick.Configure(input,thumb);
            yield return null; Canvas.ForceUpdateCanvases();
        }
        PointerEventData Finger(Vector2 point, int id=1) => new PointerEventData(events) { position=point, pointerId=id };
        Vector2 Centre => RectTransformUtility.WorldToScreenPoint(null,stick.ActivationArea.TransformPoint(stick.ActivationArea.rect.center));
        [UnityTest] public IEnumerator CaptureIsNeutralAndDragIsNormalizedAndOwned()
        {
            Assert.NotNull(stick.ActivationArea);
            var finger=Finger(Centre); stick.OnPointerDown(finger);
            Assert.IsTrue(stick.IsHeld); Assert.Less(input.RawMove.magnitude,.001f);
            var captured=stick.transform.position;
            finger.position += Vector2.one * 300; stick.OnDrag(finger);
            Assert.AreEqual(1,input.RawMove.magnitude,.001f);
            Assert.AreEqual(input.RawMove.x,input.RawMove.y,.001f);
            stick.OnPointerUp(Finger(Centre,2)); Assert.IsTrue(stick.IsHeld);
            yield return null; Assert.Less(Vector3.Distance(captured,stick.transform.position),.01f);
            stick.OnPointerUp(finger); Assert.IsFalse(stick.IsHeld); Assert.Less(input.RawMove.magnitude,.001f);
        }
        [Test] public void RightSideCannotCaptureAndDisableReleases()
        {
            stick.OnPointerDown(Finger(new Vector2(Screen.width*.8f,Screen.height*.5f)));
            Assert.IsFalse(stick.IsHeld);
            var finger=Finger(Centre); stick.OnPointerDown(finger); finger.position += Vector2.right*200; stick.OnDrag(finger);
            Assert.Greater(input.RawMove.x,.9f);
            stick.enabled=false; Assert.IsFalse(stick.IsHeld); Assert.Less(input.RawMove.magnitude,.001f);
            Assert.IsFalse(stick.ActivationArea.gameObject.activeSelf);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        { Object.Destroy(root);  yield return null; }
    }
}

