using UnityEngine;
using UnityEngine.InputSystem;

namespace ForjaDasMares
{
    // Simple test walker for the new city preview: WASD or arrows to walk, Shift to run, mouse wheel to zoom.
    // Only to feel the size of the city; the real hero replaces it when the city is linked to the game.
    [RequireComponent(typeof(CharacterController))]
    public sealed class CidadePasseio : MonoBehaviour
    {
        public Camera view;
        public float speed = 5.7f;
        float zoom = 1f;
        CharacterController body;

        void Awake() { body = GetComponent<CharacterController>(); if (view == null) view = Camera.main; }

        void Update()
        {
            var move = Vector3.zero;
            var keys = Keyboard.current;
            bool run = false;
            if (keys != null)
            {
                if (keys.wKey.isPressed || keys.upArrowKey.isPressed) move.z += 1;
                if (keys.sKey.isPressed || keys.downArrowKey.isPressed) move.z -= 1;
                if (keys.dKey.isPressed || keys.rightArrowKey.isPressed) move.x += 1;
                if (keys.aKey.isPressed || keys.leftArrowKey.isPressed) move.x -= 1;
                run = keys.leftShiftKey.isPressed;
            }
            var mouse = Mouse.current;
            if (mouse != null) zoom = Mathf.Clamp(zoom - mouse.scroll.ReadValue().y * .001f, .5f, 3.5f);
            if (move.sqrMagnitude > 0)
            {
                move.Normalize();
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), Time.deltaTime * 12);
            }
            body.SimpleMove(move * speed * (run ? 2f : 1f));
        }

        void LateUpdate()
        {
            if (view == null) return;
            view.transform.position = transform.position + new Vector3(0, 13f, -9.5f) * zoom;
            view.transform.LookAt(transform.position + Vector3.up);
        }
    }
}
