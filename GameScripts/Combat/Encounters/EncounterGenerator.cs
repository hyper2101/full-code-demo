using System.Collections.Generic;
using UnityEngine;
using GameScripts.Systems.Enemies;

namespace Mewtations.Combat.Encounters
{
    public static class EncounterGenerator
    {
        public static EncounterData Generate(EncounterTemplateSO template, int seed, int level = 1)
        {
            EncounterData data = new EncounterData
            {
                EncounterSeed = seed,
                EncounterName = template.EncounterName,
                Context = template.Context
            };

            System.Random rand = new System.Random(seed);

            if (template.PlacementMode == PlacementMode.Fixed)
            {
                foreach (var fixedSpawn in template.FixedLayout)
                {
                    data.Enemies.Add(new EnemySpawnData
                    {
                        Enemy = new DogEnemyInstance(fixedSpawn.Enemy, level),
                        SlotIndex = fixedSpawn.SlotIndex
                    });
                }
            }
            else if (template.PlacementMode == PlacementMode.Random)
            {
                if (template.EnemyPool != null && template.EnemyPool.Pool.Count > 0)
                {
                    int minCount = template.EnemyCountRange.x;
                    int maxCount = template.EnemyCountRange.y;
                    
                    // Ensure valid range
                    if (minCount < 0) minCount = 0;
                    if (maxCount < minCount) maxCount = minCount;
                    if (maxCount > 9) maxCount = 9; // Max 9 slots (0-8)

                    int count = rand.Next(minCount, maxCount + 1);

                    List<int> availableSlots = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

                    for (int i = 0; i < count; i++)
                    {
                        if (availableSlots.Count == 0) break;

                        // Pick random slot
                        int slotListIndex = rand.Next(0, availableSlots.Count);
                        int selectedSlot = availableSlots[slotListIndex];
                        availableSlots.RemoveAt(slotListIndex);

                        // Pick random enemy from pool based on weights
                        DogEnemyDefinition selectedDef = RollEnemyFromPool(template.EnemyPool.Pool, rand);

                        if (selectedDef != null)
                        {
                            data.Enemies.Add(new EnemySpawnData
                            {
                                Enemy = new DogEnemyInstance(selectedDef, level),
                                SlotIndex = selectedSlot
                            });
                        }
                    }
                }
            }

            return data;
        }

        private static DogEnemyDefinition RollEnemyFromPool(List<WeightedEnemyEntry> pool, System.Random rand)
        {
            int totalWeight = 0;
            foreach (var entry in pool)
            {
                totalWeight += entry.Weight;
            }

            if (totalWeight <= 0) return null;

            int roll = rand.Next(0, totalWeight);
            int currentWeight = 0;

            foreach (var entry in pool)
            {
                currentWeight += entry.Weight;
                if (roll < currentWeight)
                {
                    return entry.Enemy;
                }
            }

            return pool[pool.Count - 1].Enemy;
        }
    }
}
