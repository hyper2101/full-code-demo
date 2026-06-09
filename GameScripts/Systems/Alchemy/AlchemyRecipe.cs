using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Systems.Alchemy
{
    [System.Serializable]
    public class AlchemyRecipe
    {
        public string ResultCardId;
        public float RefiningDuration;
        
        // Use a simple array for serialized dictionaries to define exact counts
        public IngredientRequirement[] RequiredIngredients;

        public bool IsExactMatch(Dictionary<string, int> currentIngredients)
        {
            // Total count must match perfectly
            int totalRequired = 0;
            foreach (var req in RequiredIngredients)
            {
                totalRequired += req.Count;
                if (!currentIngredients.ContainsKey(req.CardId) || currentIngredients[req.CardId] != req.Count)
                {
                    return false;
                }
            }

            int totalCurrent = 0;
            foreach (var kvp in currentIngredients)
            {
                totalCurrent += kvp.Value;
            }

            return totalRequired == totalCurrent;
        }
    }

    [System.Serializable]
    public struct IngredientRequirement
    {
        public string CardId;
        public int Count;
    }
}
