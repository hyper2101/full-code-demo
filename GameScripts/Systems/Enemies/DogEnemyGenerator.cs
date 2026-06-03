using System.Collections.Generic;
using UnityEngine;
using GameScripts.Systems.Enemies;

namespace Mewtations.Expedition
{
    public class DogEnemyGenerator : MonoBehaviour
    {
        public static DogEnemyGenerator Instance;

        public bool useDogEnemySystem = false;

        public List<DogEnemyDefinition> availableDefinitions = new List<DogEnemyDefinition>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public List<DogEnemyInstance> GenerateEnemiesForLayer(int layer, bool isBoss)
        {
            List<DogEnemyInstance> enemies = new List<DogEnemyInstance>();
            int enemyCount = Random.Range(1, 4);
            if (isBoss) enemyCount = 1;

            if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.ActiveNode != null)
            {
                if (ExpeditionManager.Instance.ActiveNode.Theme == RouteTheme.ThuTrieu && !isBoss)
                {
                    enemyCount += 1;
                }
            }

            int level = layer * 5; // Simplified scaling: level = layer * 5 for now.

            for (int i = 0; i < enemyCount; i++)
            {
                if (availableDefinitions.Count > 0)
                {
                    // Pick a random definition (in a real game, this would be filtered by layer/biome)
                    var def = availableDefinitions[Random.Range(0, availableDefinitions.Count)];
                    enemies.Add(new DogEnemyInstance(def, level));
                }
                else
                {
                    Debug.LogWarning("[DogEnemyGenerator] No DogEnemyDefinitions available!");
                }
            }

            return enemies;
        }

        public List<DogEnemyInstance> GenerateEliteForLayer(int layer)
        {
            List<DogEnemyInstance> enemies = new List<DogEnemyInstance>();
            int level = layer * 5 + 5; // Elite is higher level

            if (availableDefinitions.Count > 0)
            {
                var def = availableDefinitions[Random.Range(0, availableDefinitions.Count)];
                enemies.Add(new DogEnemyInstance(def, level));
            }

            return enemies;
        }
    }
}
