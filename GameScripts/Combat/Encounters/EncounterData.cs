using System;
using System.Collections.Generic;

namespace Mewtations.Combat.Encounters
{
    [Serializable]
    public class EncounterData
    {
        public int EncounterSeed;
        public string EncounterName;
        public EncounterContext Context;
        
        public List<EnemySpawnData> Enemies = new List<EnemySpawnData>();

        // public RewardTable Rewards; // TODO: Implement RewardTable when phase 3/4 arrives
    }
}
