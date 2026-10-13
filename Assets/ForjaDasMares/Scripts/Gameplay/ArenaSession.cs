using UnityEngine;

namespace ForjaDasMares
{
    public sealed class ArenaSession : MonoBehaviour
    {
        [SerializeField] PlayerMotor player;
        [SerializeField] Transform spawn;
        [SerializeField] MeleeAttack combat;
        [SerializeField] ArenaCamera followCamera;
        [SerializeField] Health trainingTarget;
        [SerializeField] LayerMask solidGeometry;
        [SerializeField] float fallThreshold = -4f;
        public PlayerMotor Player => player;
        public Health TrainingTarget => trainingTarget;
        public Transform Spawn => spawn;
        public void Configure(PlayerMotor motor, Transform respawn, MeleeAttack attack, ArenaCamera camera, Health target, LayerMask solids)
        { player = motor; spawn = respawn; combat = attack; followCamera = camera; trainingTarget = target; solidGeometry = solids; }
        void Awake()
        {
            if (player == null || spawn == null || combat == null || followCamera == null || trainingTarget == null)
            { Debug.LogError("ArenaSession has incomplete references.", this); enabled = false; }
        }
        void Update() { if (player.Commands.TakeReset() || player.transform.position.y < fallThreshold) ResetArena(); }
        public void ResetArena()
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            Vector3 center = spawn.position + Vector3.up * controller.height * 0.5f;
            float half = Mathf.Max(0f, controller.height * 0.5f - controller.radius);
            if (Physics.CheckCapsule(center - Vector3.up * half, center + Vector3.up * half, controller.radius, solidGeometry, QueryTriggerInteraction.Ignore))
            { Debug.LogError("Respawn blocked. Move the SpawnPoint clear of arena obstacles.", spawn); return; }
            player.Teleport(spawn.position, spawn.rotation);
            combat.ResetAttack();
            trainingTarget.Restore();
            followCamera.Snap();
        }
    }
}
