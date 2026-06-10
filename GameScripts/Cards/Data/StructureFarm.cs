using UnityEngine;
using System.Collections.Generic;
using Mewtations.Core;

// Phase 4: Farm Structure Migration
// The Farm is now just a host container with 1 Slot (or more).
// It does NOT manage the timer of the seed.
public class StructureFarm : CardData
{
    public StructureSlotData SeedSlot;
    
    // We keep a reference to the attachment system for easy access
    private StructureAttachmentSystem _attachmentSystem;

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

    private void Start()
    {
        if (WorldManager.instance != null)
        {
            _attachmentSystem = WorldManager.instance.Attachment;
        }
    }

    // This overrides the old CanHaveCard from Farmland/Garden.
    // Instead of stacking, we use the Attachment System.
    protected override bool CanHaveCard(CardData otherCard)
    {
        // Stacklands core expects true if they can stack.
        // We return false for seeds so that normal stacking fails, 
        // allowing our AttachmentSystem to catch it during DropCard!
        // (However, for now, we can just return true and intercept in AttachmentSystem).
        return false; 
    }

    // Logic update cho Farm host. Hiện tại nó không làm gì cả, 
    // vì Seed tự quản lý Progress của nó.
    public override void UpdateCard()
    {
        base.UpdateCard();
        
        // Optional: Update visuals for the slot if it's occupied.
        if (SeedSlot.SlotOccupants.Count > 0)
        {
            GameCard seedCard = SeedSlot.SlotOccupants[0];
            if (seedCard != null)
            {
                // Từ từ nam châm hút seed về LocalOffset
                Vector3 targetPos = transform.position + SeedSlot.LocalOffset;
                seedCard.transform.position = Vector3.Lerp(seedCard.transform.position, targetPos, Time.deltaTime * 10f);
            }
        }
    }
}
