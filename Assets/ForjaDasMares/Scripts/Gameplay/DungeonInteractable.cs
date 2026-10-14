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
        public bool CanUse => !Used && session.CurrentRoom == room && session.Heroes.Health.IsAlive && (Kind != DungeonInteractionKind.Exit || room.completed);
        public bool Use()
        {
            if (!CanUse || (session.Heroes.transform.position - transform.position).sqrMagnitude > 2.1f * 2.1f) return false;
            if (Kind == DungeonInteractionKind.Chest && !session.TryOpenChest(room.node.id)) return false;
            if (Kind == DungeonInteractionKind.Exit && !session.TryExitDungeon()) return false;
            Used = true;
            switch (Kind)
            {
                case DungeonInteractionKind.Chest: movingPart.localRotation = Quaternion.Euler(-70, 0, 0); break;
                case DungeonInteractionKind.Crystal: movingPart.localScale *= 0.5f; session.CompleteObjective(room, true); break;
                case DungeonInteractionKind.Rest: session.Heroes.Health.Restore(); break;
                case DungeonInteractionKind.TrapLever: movingPart.localRotation = Quaternion.Euler(50, 0, 0); session.DisableTrap(room); break;
                case DungeonInteractionKind.Exit: break;
            }
            return true;
        }
    }
}
