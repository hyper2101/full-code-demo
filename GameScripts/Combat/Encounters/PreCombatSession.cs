using System.Collections.Generic;

namespace Mewtations.Combat.Encounters
{
    public class PreCombatSession
    {
        public EncounterData Encounter { get; set; }
        
        // Final combat layout (Sandbox copy)
        public Dictionary<int, PlayerUnitSnapshot> Formation { get; set; } = new Dictionary<int, PlayerUnitSnapshot>();

        // Valid sandbox sources
        public List<CatCardData> AvailableCats { get; set; } = new List<CatCardData>();
        public List<CardData> AvailableEquipment { get; set; } = new List<CardData>();

        public void Clear()
        {
            Encounter = null;
            Formation.Clear();
            AvailableCats.Clear();
            AvailableEquipment.Clear();
        }
    }
}
