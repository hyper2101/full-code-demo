using System;

namespace Mewtations.Systems.Planting
{
    [Serializable]
    public class SpiritualWaterPool
    {
        public int CurrentEssence;
        public int MaxEssence;
        public float DrainRate = 1f;
        public float GrowthMultiplier = 2f;

        public SpiritualWaterPool() { }

        public SpiritualWaterPool(int maxEssence)
        {
            MaxEssence = maxEssence;
            CurrentEssence = 0;
        }

        public void AddEssence(int amount)
        {
            CurrentEssence += amount;
            if (CurrentEssence > MaxEssence)
            {
                CurrentEssence = MaxEssence;
            }
        }
    }
}
