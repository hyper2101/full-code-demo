using UnityEngine;

public class SpiritualWaterSmall : CardData
{
    public int EssenceValue = 100;

    [Header("Detection")]
    public LayerMask SpiritFieldLayerMask = -1; // Default to all layers

    public override bool DetermineCanHaveCardsWhenIsRoot => false;

    protected override bool CanHaveCard(CardData otherCard)
    {
        return false;
    }

    public override void StoppedDragging()
    {
        base.StoppedDragging();

        Mewtations.Core.StructureInteractionService.TryInteractWithNearbyStructure(this, 1.5f, SpiritFieldLayerMask);
    }
}
