using System;
using System.Collections.Generic;
using UnityEngine;

public class StructureAttachmentSystem
{
    private WorldManager _world;

    public StructureAttachmentSystem(WorldManager world)
    {
        _world = world;
    }

    /// <summary>
    /// Thử tìm Structure gần nhất đang overlap và attach thẻ vào đó.
    /// Trả về true nếu thẻ đã được attach (không cho phép Stack system chạy nữa).
    /// </summary>
    public bool TryAttachToNearbyStructure(GameCard draggedCard)
    {
        List<GameCard> overlapping = draggedCard.GetOverlappingCards();
        if (overlapping == null || overlapping.Count == 0) return false;

        foreach (GameCard overlapCard in overlapping)
        {
            if (overlapCard == null || overlapCard.Destroyed) continue;

            if (overlapCard.CardData is IStructureContainer container)
            {
                string validSlotId = container.GetValidSlotFor(draggedCard.CardData);
                if (!string.IsNullOrEmpty(validSlotId))
                {
                    return RequestAttach(overlapCard, draggedCard, validSlotId, AttachContext.Drop());
                }
            }
        }

        return false;
    }

    public bool RequestAttach(GameCard parentStructure, GameCard childCard, string slotId, AttachContext context)
    {
        Debug.Log($"[Attach] Card={childCard.CardData?.Id} Parent={parentStructure.CardData?.Id} Slot={slotId} Reason={context.Reason}");

        if (parentStructure.CardData is IStructureContainer container)
        {
            StructureSlotData slot = container.GetSlotById(slotId);
            if (slot != null)
            {
                // Nếu không bypass validation thì kiểm tra type
                if (!context.BypassValidation && !slot.AcceptedTypes.Contains(childCard.CardData.Id) && slot.AcceptedTypes.Count > 0)
                {
                    return false;
                }

                slot.SlotOccupants.Add(childCard);
                childCard.RemoveFromStack();
                
                Vector3 targetPos = parentStructure.transform.position + slot.LocalOffset;
                childCard.transform.position = targetPos;
                childCard.BounceTarget = null;
                childCard.Velocity = null;
                childCard.SetStructureParent(parentStructure, context);

                container.OnCardAttached(childCard, slotId);
                return true;
            }
        }
        return false;
    }

    public bool RequestDetach(GameCard childCard, AttachContext context)
    {
        Debug.Log($"[Detach] Card={childCard.CardData?.Id} Reason={context.Reason}");
        
        if (childCard.HasStructureParent())
        {
            GameCard parentStructure = childCard.StructureParent;
            
            if (parentStructure.CardData is IStructureContainer container)
            {
                // Scan all slots to find and remove the card
                string foundSlotId = null;
                foreach (var slot in container.GetAllSlots())
                {
                    if (slot.SlotOccupants.Contains(childCard))
                    {
                        slot.SlotOccupants.Remove(childCard);
                        foundSlotId = slot.SlotId;
                        break; // Card can only be in one slot
                    }
                }

                childCard.SetStructureParent(null, context);

                if (foundSlotId != null)
                {
                    container.OnCardDetached(childCard, foundSlotId);
                }
                
                return true;
            }
        }

        return false;
    }

    public void LateUpdate()
    {
        if (_world == null || _world.BoardQuery == null) return;

        // Xử lý Reconcile: Ép thẻ dính vào đúng vị trí của Slot (khi Structure di chuyển)
        foreach (var card in _world.BoardQuery.GetVisibleBoardCards())
        {
            if (card == null || card.Destroyed || card.BeingDragged) continue;

            if (card.CardData is IStructureContainer container)
            {
                foreach (var slot in container.GetAllSlots())
                {
                    ReconcileSlot(card, slot);
                }
            }
        }
    }

    private void ReconcileSlot(GameCard structureCard, StructureSlotData slot)
    {
        if (slot.SlotOccupants.Count == 0) return;
        
        Vector3 targetPos = structureCard.transform.position + slot.LocalOffset;
        
        foreach (var occupant in slot.SlotOccupants)
        {
            if (occupant == null || occupant.Destroyed || occupant.BeingDragged) continue;
            
            // Smooth snap
            occupant.TargetPosition = targetPos;
            occupant.transform.position = Vector3.Lerp(occupant.transform.position, targetPos, Time.deltaTime * 20f);
        }
    }
}
