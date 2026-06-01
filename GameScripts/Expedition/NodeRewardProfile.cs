using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Expedition
{
    [CreateAssetMenu(fileName = "NewNodeRewardProfile", menuName = "Dogma/Expedition/Node Reward Profile")]
    public class NodeRewardProfile : ScriptableObject
    {
        public string ProfileId;
        
        [Header("Generation Settings")]
        public int MinCards = 1;
        public int MaxCards = 3;
        
        [Header("Random Loot Pool")]
        public List<NodeRewardEntry> Entries = new List<NodeRewardEntry>();
        
        [Header("Guaranteed Fixed Cards")]
        public List<NodeRewardFixedEntry> GuaranteedEntries = new List<NodeRewardFixedEntry>();

        public List<string> RollLoot()
        {
            List<string> rolled = new List<string>();

            // Add guaranteed
            foreach (var fixedEntry in GuaranteedEntries)
            {
                for (int i = 0; i < fixedEntry.Count; i++)
                {
                    rolled.Add(fixedEntry.CardId);
                }
            }

            // Roll random
            if (Entries.Count > 0)
            {
                int count = UnityEngine.Random.Range(MinCards, MaxCards + 1);
                int totalWeight = 0;
                foreach (var entry in Entries)
                {
                    totalWeight += entry.Weight;
                }

                for (int i = 0; i < count; i++)
                {
                    int roll = UnityEngine.Random.Range(0, totalWeight);
                    int currentWeight = 0;
                    foreach (var entry in Entries)
                    {
                        currentWeight += entry.Weight;
                        if (roll < currentWeight)
                        {
                            rolled.Add(entry.CardId);
                            break;
                        }
                    }
                }
            }

            return rolled;
        }
    }

    [Serializable]
    public class NodeRewardEntry
    {
        public string CardId;
        public int Weight = 100;
    }

    [Serializable]
    public class NodeRewardFixedEntry
    {
        public string CardId;
        public int Count = 1;
    }
}
