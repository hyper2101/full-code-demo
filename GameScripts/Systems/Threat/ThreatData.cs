using System.Collections.Generic;
using UnityEngine;
using GameScripts.Systems.Threat.Generation;

namespace GameScripts.Systems.Threat
{
    [CreateAssetMenu(fileName = "NewThreatData", menuName = "Threat System/Threat Data")]
    public class ThreatData : ScriptableObject
    {
        public string ThreatID;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("Generation Settings")]
        public EnemyPool EnemyPool;
        public string CardPrefabId = "threat_base";
        
        [Tooltip("The next threat level if this threat is ignored and escalates.")]
        public ThreatData NextEscalation;

        [Header("Tribute & Rewards")]
        public RewardPackData VictoryRewardPack;
        public List<string> TributeRequiredItems = new List<string>();
        
        [Header("Penalties")]
        public ThreatPenaltyType PenaltyType = ThreatPenaltyType.LockExpedition;
    }
}
