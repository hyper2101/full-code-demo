using UnityEngine;

namespace Mewtations.Core
{
    public enum StructureSlotType
    {
        Generic,
        Seed,
        Ingredient,
        Cat,
        Water,
        Fuel,
        Output
    }

    [System.Serializable]
    public class StructureSlot
    {
        public StructureSlotType SlotType;
        public Vector3 LocalOffset;
        
        public GameCard CurrentCard; 
        public string StoredDataId;  

        public StructureSlot(StructureSlotType type, Vector3 localOffset)
        {
            SlotType = type;
            LocalOffset = localOffset;
        }

        public bool IsEmpty => CurrentCard == null && string.IsNullOrEmpty(StoredDataId);

        public void Clear()
        {
            CurrentCard = null;
            StoredDataId = null;
        }
    }
}
