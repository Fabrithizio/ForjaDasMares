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
        [SerializeField] HeroRoster heroes;
        [SerializeField] TrainingTargetMotion[] movableTargets;
        [SerializeField] Health ally;
        public void ConfigureHeroes(HeroRoster roster, TrainingTargetMotion[] targets, Health friend) { heroes = roster; movableTargets = targets; ally = friend; }
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
        void Start()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Camera camera = followCamera.GetComponent<Camera>();
            Debug.Log($"ARENA_RUNTIME_READY player={player.transform.position} camera={camera.transform.position} mask={camera.cullingMask} quality={QualitySettings.GetQualityLevel()} pipeline={UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline} targetViewport={camera.WorldToViewportPoint(trainingTarget.transform.position + Vector3.up)}");
#endif
        }
        public void ResetArena()
        {
            if (movableTargets != null) foreach (var target in movableTargets) if (target != null) target.ResetTarget();
            if (heroes != null) heroes.ResetHero();
            if (ally != null) ally.Restore();
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
