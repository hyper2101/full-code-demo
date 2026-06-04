using System;
using System.Collections.Generic;
using UnityEngine;
using GameScripts.Systems.Enemies;

namespace Mewtations.Combat.Encounters
{
    [Serializable]
    public struct WeightedEnemyEntry
    {
        public DogEnemyDefinition Enemy;
        public int Weight;
    }

    [CreateAssetMenu(fileName = "NewEnemyPool", menuName = "Dogma/Encounters/Enemy Pool")]
    public class EnemyPoolSO : ScriptableObject
    {
        public List<WeightedEnemyEntry> Pool = new List<WeightedEnemyEntry>();
    }
}
