using System;
using UnityEngine;
namespace ForjaDasMares
{
    [CreateAssetMenu(menuName="Forja/Campaign level")]
    public sealed class CampaignLevel : ScriptableObject
    {
        public string levelId,displayName,description;
        public int seed=1741,biomeIndex;
        public Vector2Int[] route;
        public RoomCategory[] categories;
        public EncounterDefinition normal,hard,boss;
        public CampaignAdventureDefinition adventure;
        public EncounterDefinition[] combatVariants;
        public bool openLandscape;
        [Range(20,40)] public float sectorSize=28;
        [Range(8,20)] public float passageWidth=14;
        public GameObject[] coverModels;
        public AudioClip hitSound,crateSound,healSound,lootSound;
        [Range(1,5)] public float plateHold=1.5f;
        [Range(1.5f,6)] public float dartInterval=4;
        public AttackDefinition dartAttack;
        public Material mist,challengeMaterial;
        public Color atmosphere=new Color(.13f,.19f,.20f);
        public void Validate()
        {
            if(string.IsNullOrEmpty(levelId)||route==null||categories==null||route.Length!=categories.Length||route.Length<9||route.Length>24||normal==null||hard==null||boss==null||dartAttack==null||mist==null||challengeMaterial==null)
                throw new InvalidOperationException("Incomplete campaign level: "+name);
            var layout=DungeonLayout.Campaign(route,categories);
            if(!layout.Validate(24,out string error))throw new InvalidOperationException(error);
        }
    }
}
