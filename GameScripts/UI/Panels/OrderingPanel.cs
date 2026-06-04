using System.Collections.Generic;
using UnityEngine;

public class OrderingPanel : MonoBehaviour
{
    [SerializeField] private Transform inventoryContent;
    [SerializeField] private GameObject equipmentDraggablePrefab;

    public void Initialize()
    {
        ClearList();
        
        // Fetch valid temporary combat items (Ghost UI flow)
        List<string> temporaryItems = GetOrderingItemsFromInventory();

        foreach (var itemId in temporaryItems)
        {
            GameObject inst = Instantiate(equipmentDraggablePrefab, inventoryContent);
            // Setup Ghost UI element
            // GhostEquipmentUI ui = inst.GetComponent<GhostEquipmentUI>();
            // ui.Init(itemId);
        }
    }

    private void ClearList()
    {
        foreach (Transform child in inventoryContent)
        {
            Destroy(child.gameObject);
        }
    }

    private List<string> GetOrderingItemsFromInventory()
    {
        // TODO: Connect to WorldManager to get equipment cards available for combat
        return new List<string>();
    }

    // Ghost UI Swap Logic (called via Drag & Drop on Formation Slots)
    public bool TryEquipToCat(string itemId, int targetSlotIndex)
    {
        // Validate if item can be equipped to the cat in the slot
        // If valid, return true (Swap)
        // If not, return false (Return)
        return true; 
    }
}
