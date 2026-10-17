using UnityEngine;
namespace ForjaDasMares
{
    public enum DungeonInteractionKind { Chest, Crystal, Rest, TrapLever, Exit }
    public sealed class DungeonInteractable : MonoBehaviour
    {
        DungeonSession session;
        DungeonRoom room;
        Transform movingPart;
        public DungeonInteractionKind Kind { get; private set; }
        public bool Used { get; private set; }
        public string Label => Used ? "" : Kind == DungeonInteractionKind.Chest ? "ABRIR BAU" : Kind == DungeonInteractionKind.Crystal ? "ATIVAR CRISTAL / CHAVE" : Kind == DungeonInteractionKind.Rest ? "DESCANSAR" : Kind == DungeonInteractionKind.TrapLever ? "DESLIGAR ARMADILHA" : "SAIR DA MASMORRA";
        public void Configure(DungeonSession owner, DungeonRoom context, DungeonInteractionKind kind, Transform part) { session = owner; room = context; Kind = kind; movingPart = part; }
        public bool CanUse => isActiveAndEnabled && !Used && (session.OpenLandscape||session.CurrentRoom == room) && session.HasLivingPlayers && (Kind != DungeonInteractionKind.Exit || room.completed);
        public bool Use() => UseBy(session.Heroes.Health);
        public bool UseBy(Health player)
        {
            if (!CanUse || player == null || !player.IsAlive || player.Faction != CombatFaction.Player || (player.transform.position - transform.position).sqrMagnitude > 2.1f * 2.1f) return false;
            if(Physics.Linecast(player.transform.position+Vector3.up,transform.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore))return false;
            if (Kind == DungeonInteractionKind.Chest && !session.TryOpenChest(room.node.id)) return false;
            if (Kind == DungeonInteractionKind.Exit && !session.TryExitDungeon()) return false;
            Used = true;
            switch (Kind)
            {
                case DungeonInteractionKind.Chest: movingPart.localRotation = Quaternion.Euler(-70, 0, 0); break;
                case DungeonInteractionKind.Crystal: movingPart.localScale *= 0.5f; session.CompleteObjective(room, true); break;
                case DungeonInteractionKind.Rest:
                    if(session.Party==null)player.Restore();
                    else for(int i=0;i<4;i++){var ally=session.Party.Get(i);if(ally!=null&&ally.IsAlive&&ally.gameObject.activeInHierarchy&&room.Contains(ally.transform.position)&&!Physics.Linecast(transform.position+Vector3.up,ally.transform.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore))ally.Restore();}
                    break;
                case DungeonInteractionKind.TrapLever: movingPart.localRotation = Quaternion.Euler(50, 0, 0); session.DisableTrap(room); break;
                case DungeonInteractionKind.Exit: break;
            }
            return true;
        }
        public void ApplyReplicaUsed(bool value)
        {
            if (Used == value) return; Used = value;
            if (Kind == DungeonInteractionKind.Chest) movingPart.localRotation = Quaternion.Euler(value ? -70 : 0, 0, 0);
            else if (Kind == DungeonInteractionKind.Crystal) movingPart.localScale *= value ? .5f : 2f;
            else if (Kind == DungeonInteractionKind.TrapLever) movingPart.localRotation = Quaternion.Euler(value ? 50 : 0, 0, 0);
        }
    }
}
