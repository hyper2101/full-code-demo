using UnityEngine;

public class HerbCardData : CardData
{
    public int HerbTier = 1;

    public override bool DetermineCanHaveCardsWhenIsRoot => false;

    protected override bool CanHaveCard(CardData otherCard)
    {
        return false;
    }
}
