using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Mewtations.Core
{
    public static class ConsequenceResolver
    {
        /// <summary>
        /// Applies the consequence securely. Follows strict safety rules:
        /// - Does not delete the last cat.
        /// - Does not destroy the main building.
        /// - Does not delete quest items.
        /// </summary>
        public static void ApplyConsequence(ConsequenceData data)
        {
            if (WorldManager.instance == null || WorldManager.instance.CurrentBoard == null) return;
            
            var allCards = WorldManager.instance.CurrentBoard.GetAllCards();
            
            switch (data.Type)
            {
                case ConsequenceType.LoseCat:
                    var cats = allCards.Where(c => c.CardData != null && c.CardData.Id == "cat" && !c.Destroyed).ToList();
                    // Safety Rule: Do not delete the last cat
                    if (cats.Count > 1) 
                    {
                        var randomCat = cats[Random.Range(0, cats.Count)];
                        randomCat.DestroyCard(true, true);
                        Debug.Log($"[ConsequenceResolver] Applied LoseCat consequence on {randomCat.CardData.Id}");
                    }
                    else
                    {
                        Debug.LogWarning("[ConsequenceResolver] Prevented deleting the last cat.");
                    }
                    break;
                    
                case ConsequenceType.DestroyBuilding:
                    // Example safety rule: Check if it's not the main building (e.g. 'base')
                    var buildings = allCards.Where(c => c.CardData != null && c.CardData.MyCardType == CardType.Structures && c.CardData.Id != "base" && !c.Destroyed).ToList();
                    if (buildings.Count > 0)
                    {
                        var randomBuilding = buildings[Random.Range(0, buildings.Count)];
                        randomBuilding.DestroyCard(true, true);
                        Debug.Log($"[ConsequenceResolver] Applied DestroyBuilding consequence on {randomBuilding.CardData.Id}");
                    }
                    else
                    {
                        Debug.LogWarning("[ConsequenceResolver] No safe buildings to destroy.");
                    }
                    break;
                    
                case ConsequenceType.LoseResource:
                    var resources = allCards.Where(c => c.CardData != null && c.CardData.MyCardType == CardType.Resources && !c.CardData.IsQuestItem && !c.Destroyed).ToList();
                    // Safety Rule: Do not delete quest items (already filtered out above)
                    int amountToRemove = Mathf.Min(data.Magnitude, resources.Count);
                    for (int i = 0; i < amountToRemove; i++)
                    {
                        resources[i].DestroyCard(true, true);
                    }
                    Debug.Log($"[ConsequenceResolver] Applied LoseResource consequence. Removed {amountToRemove} resources.");
                    break;
            }
        }
    }
}
