using UnityEngine;
namespace ForjaDasMares.Networking
{
    public sealed class CoopActor : MonoBehaviour
    {
        public HeroRoster Roster { get; private set; }
        public PlayerMotor Motor { get; private set; }
        public HeroCombat Combat { get; private set; }
        public HeroPresentation Presentation { get; private set; }
        public ValidatedHeroCommands Commands { get; private set; }
        public Health Health => Roster.Health;
        Vector3 destination; Quaternion orientation; bool hasFrame;
        public void Configure(Transform basis)
        {
            Roster = GetComponent<HeroRoster>(); Motor = GetComponent<PlayerMotor>(); Combat = GetComponent<HeroCombat>(); Presentation = GetComponent<HeroPresentation>();
            Roster.enabled = false; GetComponent<PlayerAim>().enabled = false;
            Commands = GetComponent<ValidatedHeroCommands>(); if (Commands == null) Commands = gameObject.AddComponent<ValidatedHeroCommands>(); Commands.Clear(); Motor.Configure(Commands, basis);
            SetAuthority(false); destination = transform.position; orientation = transform.rotation;
        }
        public void SetAuthority(bool value)
        {
            Motor.enabled = Combat.enabled = value;
            GetComponent<CharacterController>().enabled = value;
            Presentation.ReplicaMode = !value;
        }
        public void Apply(ActorFrame frame)
        {
            if (Roster.Selected != frame.hero) Roster.Select(frame.hero);
            Health.SetReplicaState(frame.hp, frame.maximum);
            destination = frame.position; orientation = Quaternion.Euler(0, frame.yaw, 0);
            if (!hasFrame || (destination - transform.position).sqrMagnitude > 16) transform.SetPositionAndRotation(destination, orientation);
            hasFrame = true; Presentation.ApplyReplica((HeroState)frame.state, frame.animation);
        }
        void LateUpdate()
        {
            if (Motor == null || Motor.enabled || !hasFrame) return;
            float blend = 1 - Mathf.Exp(-18 * Time.unscaledDeltaTime);
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, destination, blend), Quaternion.Slerp(transform.rotation, orientation, blend));
        }
    }
}
