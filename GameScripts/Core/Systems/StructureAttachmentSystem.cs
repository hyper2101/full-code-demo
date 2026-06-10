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

            // [PHASE 4: FARM MIGRATION]
            if (overlapCard.CardData is StructureFarm farm)
            {
                if (farm.SeedSlot.SlotOccupants.Count == 0 && farm.SeedSlot.AcceptedTypes.Contains(draggedCard.CardData.Id))
                {
                    return RequestAttach(overlapCard, draggedCard, farm.SeedSlot.SlotId, "DropResolver");
                }
            }
            // [PHASE 4: SHRINE MIGRATION]
            else if (overlapCard.CardData is ShrineCardData shrine)
            {
                if (shrine.IsValidOffering(draggedCard.CardData))
                {
                    foreach (var slot in shrine.ShrineSlots)
                    {
                        if (slot.SlotOccupants.Count == 0)
                        {
                            return RequestAttach(overlapCard, draggedCard, slot.SlotId, "DropResolver");
                        }
                    }
                }
            }
            // [PHASE 4: BREAKTHROUGH ARRAY MIGRATION]
            else if (overlapCard.CardData is BreakthroughArrayCardData arrayData)
            {
                string validSlotId = arrayData.GetValidSlotFor(draggedCard.CardData);
                if (!string.IsNullOrEmpty(validSlotId))
                {
                    return RequestAttach(overlapCard, draggedCard, validSlotId, "DropResolver");
                }
            }
            // [PHASE 4: GOD CAT MIGRATION]
            else if (overlapCard.CardData is CatGodMouth godMouth)
            {
                string validSlotId = godMouth.GetValidSlotFor(draggedCard.CardData);
                if (!string.IsNullOrEmpty(validSlotId))
                {
                    return RequestAttach(overlapCard, draggedCard, validSlotId, "DropResolver");
                }
            }
        }

        return false;
    }

    public bool RequestAttach(GameCard parentStructure, GameCard childCard, string slotId, string reason = "DropResolver")
    {
        Debug.Log($"[Attach] Card={childCard.CardData?.Id} Parent={parentStructure.CardData?.Id} Slot={slotId} Reason={reason}");

        if (parentStructure.CardData is StructureFarm farm && slotId == farm.SeedSlot.SlotId)
        {
            farm.SeedSlot.SlotOccupants.Add(childCard);
            childCard.RemoveFromStack();
            
            Vector3 targetPos = parentStructure.transform.position + farm.SeedSlot.LocalOffset;
            childCard.transform.position = targetPos;
            childCard.BounceTarget = null;
            childCard.Velocity = null;

            SeedRuntime seed = childCard.gameObject.GetComponent<SeedRuntime>();
            if (seed == null) seed = childCard.gameObject.AddComponent<SeedRuntime>();
            
            seed.Initialize(childCard.CardData.Id + "bush", 120f); 
            seed.StartGrowing();

            childCard.StructureParent = parentStructure;
            return true;
        }
        else if (parentStructure.CardData is ShrineCardData shrine)
        {
            foreach (var slot in shrine.ShrineSlots)
            {
                if (slot.SlotId == slotId)
                {
                    slot.SlotOccupants.Add(childCard);
                    childCard.RemoveFromStack();

                    Vector3 targetPos = parentStructure.transform.position + slot.LocalOffset;
                    childCard.transform.position = targetPos;
                    childCard.BounceTarget = null;
                    childCard.Velocity = null;
                    childCard.StructureParent = parentStructure;
                    return true;
                }
            }
        }
        else if (parentStructure.CardData is BreakthroughArrayCardData arrayData)
        {
            StructureSlotData slot = arrayData.GetSlotById(slotId);
            if (slot != null)
            {
                slot.SlotOccupants.Add(childCard);
                childCard.RemoveFromStack();

                Vector3 targetPos = parentStructure.transform.position + slot.LocalOffset;
                childCard.transform.position = targetPos;
                childCard.BounceTarget = null;
                childCard.Velocity = null;
                childCard.StructureParent = parentStructure;
                return true;
            }
        }
        else if (parentStructure.CardData is CatGodMouth godMouth)
        {
            StructureSlotData slot = godMouth.GetSlotById(slotId);
            if (slot != null)
            {
                slot.SlotOccupants.Add(childCard);
                childCard.RemoveFromStack();

                Vector3 targetPos = parentStructure.transform.position + slot.LocalOffset;
                childCard.transform.position = targetPos;
                childCard.BounceTarget = null;
                childCard.Velocity = null;
                childCard.StructureParent = parentStructure;
                return true;
            }
        }
        return false;
    }

    public bool RequestDetach(GameCard childCard, string reason = "UserDrag")
    {
        Debug.Log($"[Detach] Card={childCard.CardData?.Id} Reason={reason}");
        
        // Quét xem thẻ này đang ở trong slot nào
        SeedRuntime seed = childCard.gameObject.GetComponent<SeedRuntime>();
        if (seed != null)
        {
            seed.StopGrowing(); // Lưu lại tiến trình
        }

        // Logic gỡ thẻ khỏi danh sách SlotOccupants của các Structure
        if (childCard.HasStructureParent())
        {
            GameCard parentStructure = childCard.StructureParent;
            
            if (parentStructure.CardData is StructureFarm farm)
            {
                farm.SeedSlot.SlotOccupants.Remove(childCard);
            }
            else if (parentStructure.CardData is ShrineCardData shrine)
            {
                foreach (var slot in shrine.ShrineSlots) slot.SlotOccupants.Remove(childCard);
            }
            else if (parentStructure.CardData is BreakthroughArrayCardData arrayData)
            {
                foreach (var slot in arrayData.Slots) slot.SlotOccupants.Remove(childCard);
            }
            else if (parentStructure.CardData is CatGodMouth godMouth)
            {
                foreach (var slot in godMouth.MouthSlots) slot.SlotOccupants.Remove(childCard);
            }

            childCard.StructureParent = null;
            return true;
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

            if (card.CardData is StructureFarm farm)
            {
                ReconcileSlot(card, farm.SeedSlot);
            }
            else if (card.CardData is ShrineCardData shrine)
            {
                foreach (var slot in shrine.ShrineSlots) ReconcileSlot(card, slot);
            }
            else if (card.CardData is BreakthroughArrayCardData arrayData)
            {
                foreach (var slot in arrayData.Slots) ReconcileSlot(card, slot);
            }
            else if (card.CardData is CatGodMouth godMouth)
            {
                foreach (var slot in godMouth.MouthSlots) ReconcileSlot(card, slot);
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
