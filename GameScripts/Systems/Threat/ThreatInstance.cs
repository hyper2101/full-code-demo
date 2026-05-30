using System;
using System.Collections.Generic;
using GameScripts.Systems.Threat.Generation;

namespace GameScripts.Systems.Threat
{
    [Serializable]
    public class ThreatInstance
    {
        [NonSerialized]
        public ThreatData BaseData;
        public string BaseDataId;

        public ThreatState State;
        public ThreatSourceType Source;

        // Snapshot Data (Fixed at creation time)
        public int TargetLevel;
        public EnemyTeamData GeneratedEnemyTeam;
        public string VictoryRewardPackId;
        public List<string> RequiredTributeItems;
        public string SpawnedCardUniqueId;

        // Timing
        public int DaysRemaining; // Used for Warning countdown or Cooldown duration
        
        public ThreatInstance(ThreatData data, ThreatSourceType source)
        {
            BaseData = data;
            if (data != null) BaseDataId = data.ThreatID;
            
            Source = source;
            State = ThreatState.Scheduled;
            RequiredTributeItems = new List<string>();
        }
    }
}
