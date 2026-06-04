using System;
using System.Collections.Generic;

namespace Mewtations.Combat.Encounters
{
    [Serializable]
    public class EncounterData
    {
        public int Id;
        public int EncounterSeed;
        public string EncounterName;
        public EncounterContext Context;
        
        public List<EnemySpawnData> Enemies = new List<EnemySpawnData>();
        
        /// <summary>
        /// Maximum number of turns allowed for this encounter. If exceeded, results in Defeat by timeout.
        /// </summary>
        public int TurnLimit = 30; // Default limit for encounters like Dog Tax

        // public RewardTable Rewards; // TODO: Implement RewardTable when phase 3/4 arrives
    }
}
