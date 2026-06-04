using System;
using System.Collections.Generic;

namespace Mewtations.Combat.Encounters
{
    [Serializable]
    public class EncounterSetupSnapshot
    {
        public EncounterData Encounter;
        public List<PlayerUnitSnapshot> PlayerTeam = new List<PlayerUnitSnapshot>();
        public List<Combatable> LegacyEnemies; // For backward compatibility wrapper only
    }
}
