using System.Collections.Generic;
using UnityEngine;
using Mewtations.Systems.Planting;
using Mewtations.Core;

public class SpiritFieldRuntime : BaseStructureRuntime
{
    public SpiritualWaterPool WaterPool;
    public int Tier = 1;
    public int MaxSlots = 4;

    private float _waterDrainAccumulator = 0f;
    private SpiritFieldCardData _spiritFieldCard;

    public void Initialize(int tier, int maxSlots, int maxWaterPool)
    {
        Tier = tier;
        MaxSlots = maxSlots;
        Slots.Clear();
        
        // Define fixed positions for the 4 slots
        Vector3[] offsets = new Vector3[]
        {
            new Vector3(-0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, 0.5f),
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, -0.5f)
        };

        for (int i = 0; i < MaxSlots; i++)
        {
            StructureSlot newSlot = new StructureSlot(StructureSlotType.Seed, i < offsets.Length ? offsets[i] : Vector3.zero);
            Slots.Add(newSlot);
        }

        if (WaterPool == null)
        {
            WaterPool = new SpiritualWaterPool(maxWaterPool);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _spiritFieldCard = GetComponent<SpiritFieldCardData>();
    }

    public override bool IsRuntimeActive => false; // Active only when there are active slots

    public override bool TryInsertCard(CardData incomingData)
    {
        if (incomingData is SeedCardData seedCard)
        {
            if (seedCard.RequiredFieldTier > this.Tier)
            {
                GameNotificationSystem.RecordLog(MewtationsLoc.Translate("drug_field_tier_too_low"));
                return false;
            }

            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].IsEmpty)
                {
                    Slots[i].StoredDataId = seedCard.TargetHerbDefinitionId;
                    Slots[i].MaxProgress = seedCard.GrowthTime;
                    Slots[i].CurrentProgress = 0f;
                    Slots[i].IsComplete = false;
                    seedCard.MyGameCard.DestroyCard(true, true);
                    return true;
                }
            }

            GameNotificationSystem.RecordLog(MewtationsLoc.Translate("drug_field_no_empty_slot"));
            return false;
        }

        if (incomingData is SpiritualWaterSmall waterCard)
        {
            if (WaterPool.CurrentEssence < WaterPool.MaxEssence)
            {
                WaterPool.AddEssence(waterCard.EssenceValue);
                waterCard.MyGameCard.DestroyCard(true, true);
                return true; // We accept the water, but it doesn't take a slot
            }
            
            GameNotificationSystem.RecordLog(MewtationsLoc.Translate("drug_field_water_pool_full"));
            return false;
        }

        return false; // Reject anything else
    }

    protected override void UpdateGameplayLogic()
    {
        if (!_spiritFieldCard.IsOn) return;

        float delta = Time.deltaTime;
        
        // Tick growth
        for (int i = 0; i < Slots.Count; i++)
        {
            var slot = Slots[i];
            if (slot.IsEmpty || slot.IsComplete) continue;

            float growthAmount = delta;
            if (WaterPool.CurrentEssence > 0)
            {
                growthAmount *= WaterPool.GrowthMultiplier;
                float drain = WaterPool.DrainRate * delta;
                
                _waterDrainAccumulator += drain;
                if (_waterDrainAccumulator >= 1f)
                {
                    int drainInt = Mathf.FloorToInt(_waterDrainAccumulator);
                    WaterPool.CurrentEssence -= drainInt;
                    _waterDrainAccumulator -= drainInt;
                    if (WaterPool.CurrentEssence < 0) WaterPool.CurrentEssence = 0;
                }
            }

            slot.CurrentProgress += growthAmount;
            if (slot.CurrentProgress >= slot.MaxProgress)
            {
                slot.CurrentProgress = slot.MaxProgress;
                slot.IsComplete = true;
                
                GameNotificationSystem.RecordLog(MewtationsLoc.Translate("drug_field_matured"));
            }
        }

        // Try to eject mature plants (1 per update to stagger)
        for (int i = 0; i < Slots.Count; i++)
        {
            var slot = Slots[i];
            if (!slot.IsEmpty && slot.IsComplete)
            {
                if (TryEjectPlant(slot))
                {
                    slot.Clear();
                    break; // Eject one at a time
                }
            }
        }
    }

    private bool TryEjectPlant(StructureSlot slot)
    {
        GameCard root = _cardData.MyGameCard.GetRootCard();
        Vector3 spawnPos = root.transform.position;
        
        Collider[] colliders = Physics.OverlapSphere(spawnPos, 1.5f);
        int cardCount = 0;
        foreach (var col in colliders)
        {
            if (col.GetComponentInParent<GameCard>() != null) cardCount++;
        }

        if (cardCount > 10) return false;

        CardData newCard = WorldManager.instance.CreateCard(spawnPos, slot.StoredDataId, true, false, true);
        if (newCard != null)
        {
            Vector2 randDir = UnityEngine.Random.insideUnitCircle.normalized;
            newCard.MyGameCard.BounceTarget = spawnPos + new Vector3(randDir.x, 0, randDir.y) * 1.5f;
            return true;
        }

        return false;
    }
}
