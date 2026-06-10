using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mewtations.Combat.Encounters;

public class OrderingPanel : MonoBehaviour
{
    [SerializeField] private Transform inventoryContent;
    [SerializeField] private GameObject equipmentDraggablePrefab;

    public void PopulateSessionEquipment(PreCombatSession session)
    {
        session.AvailableEquipment.Clear();

        if (WorldManager.instance == null || WorldManager.instance.BoardQuery == null) return;

        // Find the single Ordering Card (as per Phase 1f assumption)
        var ringCard = WorldManager.instance.BoardQuery.GetVisibleBoardCards().FirstOrDefault(c => c != null && c.CardData is Mewtations.Legacy.Stacklands.OrderingCardData && !c.Destroyed);
        
        if (ringCard != null && ringCard.InventoryContainer != null)
        {
            var children = ringCard.InventoryContainer.GetChildren();
            session.AvailableEquipment.AddRange(children.Select(c => c.CardData));
        }
    }

    public void Initialize(PreCombatSession session)
    {
        ClearList();
        
        // Only show items that aren't already equipped in the sandbox
        var equippedItems = new HashSet<CardData>();
        foreach (var pSnap in session.Formation.Values)
        {
            if (pSnap.Equipment != null)
            {
                foreach (var item in pSnap.Equipment.Slots.Values)
                {
                    equippedItems.Add(item);
                }
            }
        }

        foreach (var item in session.AvailableEquipment)
        {
            if (equippedItems.Contains(item)) continue;

            GameObject inst = Instantiate(equipmentDraggablePrefab, inventoryContent);
            // Setup Ghost UI element
            // GhostEquipmentUI ui = inst.GetComponent<GhostEquipmentUI>();
            // ui.Init(item);
        }
    }

    private void ClearList()
    {
        foreach (Transform child in inventoryContent)
        {
            Destroy(child.gameObject);
        }
    }

    // Ghost UI Swap Logic (called via Drag & Drop on Formation Slots)
    public bool TryEquipToCat(CardData item, int targetSlotIndex)
    {
        // Validation logic: check if the sandbox formation has a cat in targetSlotIndex
        // and if the item is a valid Equipable for that CatSlotType
        return true; 
    }
}
