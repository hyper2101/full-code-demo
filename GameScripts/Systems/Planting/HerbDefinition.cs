using UnityEngine;

namespace Mewtations.Systems.Planting
{
    [CreateAssetMenu(fileName = "NewHerbDefinition", menuName = "Mewtations/Planting/Herb Definition")]
    public class HerbDefinition : ScriptableObject
    {
        public string Id;
        public float GrowthTime = 180f;
        public int HerbTier = 1;
        public int RequiredFieldTier = 1;
        public int WaterConsumption = 1;

        [Header("Visuals")]
        public Sprite SeedSprite;
        public Sprite MatureSprite;
        public GameObject OptionalFX;
    }
}
