using System;

namespace Mewtations.Systems.Planting
{
    [Serializable]
    public class PlantSlotRuntime
    {
        public int SlotIndex;
        public string PlantId; // HerbDefinitionId
        public float CurrentGrowth;
        public float GrowthDuration;
        public bool IsMature;

        public PlantSlotRuntime(int slotIndex)
        {
            SlotIndex = slotIndex;
            PlantId = null;
            CurrentGrowth = 0f;
            GrowthDuration = 0f;
            IsMature = false;
        }

        public void Clear()
        {
            PlantId = null;
            CurrentGrowth = 0f;
            GrowthDuration = 0f;
            IsMature = false;
        }
    }
}
