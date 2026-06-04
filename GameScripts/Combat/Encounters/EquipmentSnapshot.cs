using System;
using System.Collections.Generic;

namespace Mewtations.Combat.Encounters
{
    [Serializable]
    public class EquipmentSnapshot
    {
        // Maps the CatSlotType to the actual CardData equipped in that slot for the encounter
        public Dictionary<Mewtations.Expedition.CatSlotType, CardData> Slots = new Dictionary<Mewtations.Expedition.CatSlotType, CardData>();

        public void AssignItem(Mewtations.Expedition.CatSlotType type, CardData item)
        {
            Slots[type] = item;
        }

        public void RemoveItem(Mewtations.Expedition.CatSlotType type)
        {
            if (Slots.ContainsKey(type))
            {
                Slots.Remove(type);
            }
        }
    }
}
