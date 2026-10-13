using UnityEngine;
using UnityEngine.AI;
namespace ForjaDasMares
{
    [RequireComponent(typeof(CharacterController), typeof(NavMeshAgent))]
    public sealed class EnemyMotor : MonoBehaviour
    {
        [SerializeField] Health health;
        CharacterController controller;
        NavMeshAgent agent;
        EnemyDefinition profile;
        Vector3 velocity, impulse, goal, previous, rushDirection;
        float verticalSpeed, repath, stalled, rushTime, rushSpeed;
        bool moving;
        public bool Ready => agent != null && agent.enabled && agent.isOnNavMesh;
        public bool IsRushing => rushTime > 0;
        public Vector3 Velocity => velocity;
        public NavMeshAgent Agent => agent;
        public void Configure(Health hp) => health = hp;
        void Cache() { if (controller == null) controller = GetComponent<CharacterController>(); if (agent == null) agent = GetComponent<NavMeshAgent>(); }
        void Awake() { Cache(); health.Impact += ReceiveImpact; }
        void OnDestroy() { if (health != null) health.Impact -= ReceiveImpact; }
        public void Prepare(EnemyDefinition data)
        {
            Cache(); profile = data; agent.enabled = false; controller.enabled = false;
            controller.height = data.height; controller.radius = Mathf.Min(data.radius, data.height * 0.45f); controller.center = Vector3.up * data.height * 0.5f;
            agent.agentTypeID = data.agentTypeId; agent.radius = data.radius; agent.height = data.height; agent.speed = data.speed; agent.acceleration = data.acceleration;
            agent.angularSpeed = 480; agent.updatePosition = agent.updateRotation = agent.updateUpAxis = false; agent.autoRepath = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance; agent.stoppingDistance = 0.15f;
            velocity = impulse = Vector3.zero; verticalSpeed = repath = stalled = rushTime = 0; moving = false; previous = transform.position;
        }
        public bool Activate()
        {
            controller.enabled = true; agent.enabled = true;
            if (!agent.isOnNavMesh) { agent.enabled = false; controller.enabled = false; return false; }
            agent.isStopped = true; agent.nextPosition = transform.position; return true;
        }
        public void MoveTo(Vector3 destination)
        {
            if (!Ready) return;
            var filter = new NavMeshQueryFilter { agentTypeID = profile.agentTypeId, areaMask = NavMesh.AllAreas };
            if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, 2, filter)) { moving = false; return; }
            if (!moving || Vector3.SqrMagnitude(goal - hit.position) > 0.16f || repath <= 0)
            { goal = hit.position; agent.SetDestination(goal); repath = 0.65f; }
            moving = true;
        }
        public void Stop() { moving = false; if (Ready) agent.isStopped = true; }
        public void Face(Vector3 point, float deltaTime)
        {
            Vector3 direction = point - transform.position; direction.y = 0;
            if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 480 * deltaTime);
        }
        public bool StartRush(Vector3 direction, float duration, float speed)
        {
            if (!Ready || !health.IsAlive || health.ControlMultiplier == 0) return false;
            direction.y = 0; rushDirection = direction.normalized; rushTime = duration; rushSpeed = speed; Stop(); return true;
        }
        void ReceiveImpact(Vector3 direction, float force)
        {
            if (profile == null || !health.IsAlive) return;
            direction.y = 0; impulse = Vector3.ClampMagnitude(impulse + direction.normalized * force * (1 - profile.knockbackResistance), 10);
            if (profile.staggerDuration > 0) health.Stun(profile.staggerDuration);
        }
        public void Tick(float deltaTime)
        {
            if (!Ready || !health.IsAlive || deltaTime <= 0) return;
            repath -= deltaTime;
            bool blocked = health.ControlMultiplier == 0;
            if (blocked) rushTime = 0;
            agent.speed = profile.speed * health.ControlMultiplier; agent.isStopped = !moving || blocked || IsRushing;
            Vector3 desired = moving && !blocked && !IsRushing ? agent.desiredVelocity : Vector3.zero; desired.y = 0;
            velocity = Vector3.MoveTowards(velocity, desired, profile.acceleration * deltaTime);
            float dash = Mathf.Min(deltaTime, rushTime);
            Vector3 displacement = velocity * (deltaTime - dash) + rushDirection * (rushSpeed * dash) + impulse * deltaTime;
            rushTime = Mathf.Max(0, rushTime - deltaTime); impulse = Vector3.MoveTowards(impulse, Vector3.zero, 20 * deltaTime);
            ActorMotion.Move(controller, displacement, deltaTime, ref verticalSpeed);
            agent.nextPosition = transform.position;
            Vector3 travel = transform.position - previous; travel.y = 0; previous = transform.position;
            if (moving && !blocked && desired.sqrMagnitude > 0.1f && travel.sqrMagnitude < 0.00004f) stalled += deltaTime; else stalled = 0;
            if (stalled > 0.8f) { agent.ResetPath(); agent.SetDestination(goal); stalled = 0; repath = 0.8f; }
        }
        public void Deactivate()
        {
            Cache(); if (Ready) agent.ResetPath(); agent.enabled = false; controller.enabled = false; moving = false;
            velocity = impulse = Vector3.zero; rushTime = verticalSpeed = 0; profile = null;
        }
    }
}
