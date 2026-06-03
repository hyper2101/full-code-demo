using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Combat.Encounters
{
    [CreateAssetMenu(fileName = "New Boss Encounter", menuName = "Mewtations/Encounters/BossEncounter")]
    public class BossEncounterDefinition : ScriptableObject
    {
        [Serializable]
        public class BossUnitConfig
        {
            public string CardId;
            [Tooltip("Vector2Int(x, y) = (Depth, Lane) e.g., (0,1) for Frontline Mid Lane")]
            public Vector2Int GridPosition;
            public List<string> EquipmentCardIds = new List<string>();
        }

        public string EncounterId;
        public List<BossUnitConfig> Units = new List<BossUnitConfig>();
        public string RewardPoolId;

        // Validation for encounter data
        private void OnEnable()
        {
            ValidateEncounter();
        }

        private void OnValidate()
        {
            ValidateEncounter();
        }

        public void ValidateEncounter()
        {
            if (Units == null || Units.Count == 0) return;

            HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
            HashSet<string> uniqueBosses = new HashSet<string>();

            foreach (var unit in Units)
            {
                // Vector2Int(Depth, Lane)
                int depth = unit.GridPosition.x;
                int lane = unit.GridPosition.y;

                // Validate bounds (3x3 grid)
                if (depth < 0 || depth > 2 || lane < 0 || lane > 2)
                {
                    Debug.LogError($"[EncounterValidation] {EncounterId}: Boss unit {unit.CardId} position [Depth {depth}, Lane {lane}] is out of 3x3 range! Out-of-bound spawn will fail.");
                }

                // Validate overlapping positions - MUST Error
                if (occupiedPositions.Contains(unit.GridPosition))
                {
                    Debug.LogError($"[EncounterValidation] {EncounterId}: CRITICAL - Multiple units share the same grid position [Depth {depth}, Lane {lane}]! Spawn will be cancelled for overlap.");
                }
                else
                {
                    occupiedPositions.Add(unit.GridPosition);
                }

                // Duplicate unique boss validation
                if (unit.CardId != null && unit.CardId.StartsWith("boss_"))
                {
                    if (uniqueBosses.Contains(unit.CardId))
                    {
                        Debug.LogWarning($"[EncounterValidation] {EncounterId}: Duplicate unique boss {unit.CardId} detected!");
                    }
                    else
                    {
                        uniqueBosses.Add(unit.CardId);
                    }
                }
            }
        }
    }
}
