using UnityEngine;
using System.Collections.Generic;
using Mewtations.Core;

// Phase 4: Farm Structure Migration
// The Farm is now just a host container with 1 Slot (or more).
// It does NOT manage the timer of the seed.
public class StructureFarm : CardData, IStructureContainer
{
    public StructureSlotData SeedSlot;

    protected override void Awake()
    {
        base.Awake();
        
        // Define the slot (Position is local relative to the card)
        SeedSlot = new StructureSlotData
        {
            SlotId = "seed_slot_0",
            LocalOffset = new Vector3(0, 0.1f, 0.2f),
            OccupancyPolicy = OccupancyPolicy.Single,
            AcceptedTypes = new List<string> { "berry", "apple", "carrot", "onion", "potato" } // Expand as needed
        };
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        return false; 
    }

    public string GetValidSlotFor(CardData cardData)
    {
        if (SeedSlot.SlotOccupants.Count == 0 && SeedSlot.AcceptedTypes.Contains(cardData.Id))
        {
            return SeedSlot.SlotId;
        }
        return null;
    }

    public StructureSlotData GetSlotById(string slotId)
    {
        if (slotId == SeedSlot.SlotId) return SeedSlot;
        return null;
    }

    public IEnumerable<StructureSlotData> GetAllSlots()
    {
        yield return SeedSlot;
    }

    public void OnCardAttached(GameCard childCard, string slotId)
    {
        if (slotId == SeedSlot.SlotId)
        {
            SeedRuntime seed = childCard.gameObject.GetComponent<SeedRuntime>();
            if (seed == null) seed = childCard.gameObject.AddComponent<SeedRuntime>();
            
            seed.Initialize(childCard.CardData.Id + "bush", 120f); 
            seed.StartGrowing();
        }
    }

    public void OnCardDetached(GameCard childCard, string slotId)
    {
        if (slotId == SeedSlot.SlotId)
        {
            SeedRuntime seed = childCard.gameObject.GetComponent<SeedRuntime>();
            if (seed != null)
            {
                seed.StopGrowing(); // Lưu lại tiến trình
            }
        }
    }
}
