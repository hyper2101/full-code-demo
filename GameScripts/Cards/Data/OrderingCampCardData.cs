using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class OrderingCampCardData : CardData
{
    public override bool CanHaveCardsWhileHasStatus()
    {
        return true;
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        // Allow backpacks, relics, or food/gold to be packed for the journey
        return otherCard.BackpackCapacity > 0 
            || otherCard.Id.StartsWith("item_ancient_relic_") 
            || otherCard.Id == "resource_gold" 
            || otherCard.Id == "resource_food";
    }

    public override bool DetermineCanHaveCardsWhenIsRoot => true;
}
