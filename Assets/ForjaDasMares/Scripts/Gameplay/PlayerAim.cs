using UnityEngine;

namespace ForjaDasMares
{
    [DefaultExecutionOrder(-50)]
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] PlayerMotor motor;
        [SerializeField] Camera viewCamera;
        [SerializeField, Min(1f)] float turnSpeed = 1080f;
        [SerializeField] float groundHeight;
        public void Configure(PlayerMotor playerMotor, Camera camera) { motor = playerMotor; viewCamera = camera; }
        void Awake()
        {
            if(motor!=null&&viewCamera!=null){var input=GetComponent<ArenaInput>();if(input!=null)input.ConfigureDesktop(motor,viewCamera,GetComponent<HeroCombat>());}
            if (motor == null || viewCamera == null) { Debug.LogError("PlayerAim requires motor and camera.", this); enabled = false; }
        }
        void Update()
        {
            if (motor.FacingLocked) return;
            IPlayerCommands commands = motor.Commands;
            if (commands == null) return;
            Vector3 direction = motor.WorldDirection(commands.Move);
            if(commands is IWorldAimCommands worldAim)
            {
                // PC with mouse: the hero does not follow the cursor. He turns to where he walks (the click),
                // and toward the mouse only while the button is held. Attacks and abilities turn him themselves.
                var arena=GetComponent<ArenaInput>();
                if(!UnityEngine.Device.Application.isMobilePlatform&&commands.HasPointer&&arena!=null&&!arena.MouseAttackHeld)
                {
                    direction=motor.PlanarVelocity;
                    if(direction.sqrMagnitude<.04f)return;
                }
                else direction=worldAim.AimDirection;
            }
            else if (commands.HasPointer)
            {
                Ray ray = viewCamera.ScreenPointToRay(commands.PointerPosition);
                Plane plane = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));
                if (plane.Raycast(ray, out float distance)) direction = ray.GetPoint(distance) - transform.position;
            }
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
        }
    }
}
