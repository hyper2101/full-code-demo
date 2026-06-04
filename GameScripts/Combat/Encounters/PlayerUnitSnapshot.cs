using System;
using System.Collections.Generic;

namespace Mewtations.Combat.Encounters
{
    [Serializable]
    public class PlayerUnitSnapshot
    {
        public CatCardData CatReference; 
        public int FinalSlotIndex;
        public EquipmentSnapshot Equipment = new EquipmentSnapshot();
    }
}
