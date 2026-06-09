using UnityEngine;

namespace Mewtations.Core
{
    public static class StructureInteractionService
    {
        /// <summary>
        /// Call this from CardData.StoppedDragging() to attempt inserting the card into a nearby structure.
        /// </summary>
        public static bool TryInteractWithNearbyStructure(CardData cardData, float radius = 1.5f, int layerMask = -1)
        {
            if (cardData == null || cardData.MyGameCard == null || cardData.MyGameCard.Destroyed) return false;
            Collider[] colliders = Physics.OverlapSphere(cardData.transform.position, radius, layerMask);
            
            foreach (var col in colliders)
            {
                BaseStructureRuntime structure = col.GetComponentInParent<BaseStructureRuntime>();
                if (structure != null)
                {
                    // Attempt to insert
                    if (structure.TryInsertCard(cardData))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
