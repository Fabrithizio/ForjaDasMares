using UnityEngine;
namespace ForjaDasMares
{
    [CreateAssetMenu(menuName="Forja/Campaign adventure")]
    public sealed class CampaignAdventureDefinition : ScriptableObject
    {
        [Range(1,3)] public int suppliesPerSector=2;
        [Range(10,100)] public int supplyHealth=24;
        [Range(30,250)] public int sealHealth=65;
        [Range(1,3)] public int sealGoal=3;
        [Range(.1f,.6f)] public float fountainHealing=.35f;
        [Range(1,3)] public int fountains=2;
        public Color sealColor=new Color(.25f,.85f,1);
        public Color healingColor=new Color(.25f,1,.55f);
    }
}
