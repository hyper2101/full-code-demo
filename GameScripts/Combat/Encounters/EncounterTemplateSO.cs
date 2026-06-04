using System.Collections.Generic;
using UnityEngine;
using GameScripts.Systems.Enemies;

namespace Mewtations.Combat.Encounters
{
    [System.Serializable]
    public struct FixedEnemySpawn
    {
        public DogEnemyDefinition Enemy;
        public int SlotIndex;
    }

    [CreateAssetMenu(fileName = "NewEncounterTemplate", menuName = "Dogma/Encounters/Encounter Template")]
    public class EncounterTemplateSO : ScriptableObject
    {
        public string EncounterName;
        public EncounterContext Context;
        public PlacementMode PlacementMode;
        
        [Header("Random Mode Settings")]
        public EnemyPoolSO EnemyPool;
        public Vector2Int EnemyCountRange;
        
        [Header("Fixed Mode Settings")]
        public List<FixedEnemySpawn> FixedLayout = new List<FixedEnemySpawn>();
    }
}
