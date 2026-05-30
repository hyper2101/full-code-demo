using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.Systems.Threat
{
    [Serializable]
    public class EnemyPool
    {
        [Tooltip("List of valid enemies that can be spawned from this pool.")]
        public List<string> ValidEnemyIDs = new List<string>();
        
        // Cấu trúc đơn giản cho Phase 1, sẽ mở rộng ở Phase 5.
    }
}
