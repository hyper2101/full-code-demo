using System;
using UnityEngine;

namespace Mewtations.Legacy
{
    public static class LegacyRuntimeGuard
    {
        public static bool IsLegacyBoardEntity(CardData cardPrefab)
        {
            if (cardPrefab == null) return false;

            // Kiểm tra Inheritance
            if (cardPrefab is Animal || 
                cardPrefab is BaseVillager || 
                cardPrefab is Curse || 
                cardPrefab is Portal || 
                cardPrefab is PirateBoat ||
                cardPrefab is GoblinAttack ||
                cardPrefab is SadEvent ||
                cardPrefab is AnimalPen ||
                cardPrefab is BreedingPen ||
                cardPrefab is SlaughterHouse ||
                cardPrefab is PettingZoo ||
                cardPrefab is Poop ||
                cardPrefab is DragonEgg ||
                cardPrefab is Happiness ||
                cardPrefab is Unhappiness)
            {
                return true;
            }

            // Kiểm tra ID Pattern dự phòng
            string id = cardPrefab.Id;
            if (!string.IsNullOrEmpty(id))
            {
                if (id.StartsWith("mob_") || 
                    id.StartsWith("curse_") || 
                    id.Contains("portal") || 
                    id == "villager" || 
                    id == "worker" || 
                    id == "kid" || 
                    id == "corpse")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
