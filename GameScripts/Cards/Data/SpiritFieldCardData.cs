using UnityEngine;
using Mewtations.Systems.Planting;

public class SpiritFieldCardData : CardData
{
    [Header("Spirit Field Properties")]
    public int Tier = 1;
    public int MaxSlots = 4;
    public int MaxWaterPool = 1000;

    [SerializeField]
    private SpiritFieldRuntime _runtime;
    public SpiritFieldRuntime Runtime => _runtime;

    protected override void Awake()
    {
        base.Awake();
        _runtime = GetComponent<SpiritFieldRuntime>();
        if (_runtime == null)
        {
            _runtime = gameObject.AddComponent<SpiritFieldRuntime>();
        }
        _runtime.Initialize(Tier, MaxSlots, MaxWaterPool);
    }

    public override bool DetermineCanHaveCardsWhenIsRoot => false; // Container Entity, not a stack

    protected override bool CanHaveCard(CardData otherCard)
    {
        return false; // Prevent any card from stacking on it
    }
}
