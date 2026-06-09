using UnityEngine;

public class SeedCardData : CardData
{
    [Header("Seed Properties")]
    public string TargetHerbDefinitionId;
    public int RequiredFieldTier = 1;
    public float GrowthTime = 180f;

    [Header("Detection")]
    public LayerMask SpiritFieldLayerMask = -1; // Default to all layers, can be configured in Inspector

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
