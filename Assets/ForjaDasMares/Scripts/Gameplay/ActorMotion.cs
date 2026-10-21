using UnityEngine;
namespace ForjaDasMares
{
    public static class ActorMotion
    {
        public static void Move(CharacterController controller, Vector3 displacement, float deltaTime, ref float verticalSpeed, float gravity = -25f)
        {
            if(deltaTime<=0)return;
            int steps=Mathf.Clamp(Mathf.CeilToInt(deltaTime/.05f),1,32);float stepTime=deltaTime/steps;
            Vector3 step=displacement/steps;
            for(int i=0;i<steps;i++)
            {
                if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
                verticalSpeed += gravity * stepTime;step.y=verticalSpeed*stepTime;
                controller.Move(step);
            }
        }
    }
}
