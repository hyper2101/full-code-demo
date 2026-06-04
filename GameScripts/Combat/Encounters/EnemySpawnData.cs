using System;
using GameScripts.Systems.Enemies;

namespace Mewtations.Combat.Encounters
{
    [Serializable]
    public class EnemySpawnData
    {
        public DogEnemyInstance Enemy;
        public int SlotIndex;
    }
}
