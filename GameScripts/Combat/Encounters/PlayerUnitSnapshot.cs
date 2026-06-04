using System;
using System.Collections.Generic;

namespace Mewtations.Combat.Encounters
{
    [Serializable]
    public class EquipmentSnapshot
    {
        // Stores string IDs of the equipment to avoid referencing runtime objects
        public List<string> EquipmentIds = new List<string>();
    }

    [Serializable]
    public class PlayerUnitSnapshot
    {
        public CatCardData CatReference; 
        public int FinalSlotIndex;
        public EquipmentSnapshot Equipment = new EquipmentSnapshot();
    }
}
