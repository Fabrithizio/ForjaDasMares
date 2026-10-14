namespace ForjaDasMares
{
    [System.Serializable] public struct DungeonReplicaState
    {
        public int room;
        public bool hasKey, finished;
        public uint entered, completed, objectives, fights, doors, interactions, traps;
    }
}
