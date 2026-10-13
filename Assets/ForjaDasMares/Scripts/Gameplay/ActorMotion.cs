using UnityEngine;
namespace ForjaDasMares
{
    public static class ActorMotion
    {
        public static void Move(CharacterController controller, Vector3 displacement, float deltaTime, ref float verticalSpeed, float gravity = -25f)
        {
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
            verticalSpeed += gravity * deltaTime;
            displacement.y = verticalSpeed * deltaTime;
            controller.Move(displacement);
        }
    }
}
