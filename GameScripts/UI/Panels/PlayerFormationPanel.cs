using System.Collections.Generic;
using UnityEngine;
using Mewtations.Combat.Encounters;

public class PlayerFormationPanel : MonoBehaviour
{
    // References to 9 UI slots representing the player side (SlotIndex 0-8)
    [SerializeField] private List<Transform> formationSlots = new List<Transform>();
    
    // Mapping of slot index to assigned Cat Card ID and Equipment IDs
    private Dictionary<int, PlayerUnitSnapshot> currentFormation = new Dictionary<int, PlayerUnitSnapshot>();

    public void Initialize()
    {
        currentFormation.Clear();
        // Clear visually...
        foreach (var slot in formationSlots)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public bool HasAnyCatAssigned()
    {
        return currentFormation.Count > 0;
    }

    public List<PlayerUnitSnapshot> GetPlayerSnapshots()
    {
        List<PlayerUnitSnapshot> snapshots = new List<PlayerUnitSnapshot>();
        foreach (var kvp in currentFormation)
        {
            snapshots.Add(kvp.Value);
        }
        return snapshots;
    }

    // Called via Drag & Drop events
    public void AssignCatToSlot(string catCardId, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= formationSlots.Count) return;

        // Check if slot is occupied
        if (currentFormation.ContainsKey(slotIndex))
        {
            // Handle swap or return to reserve
            RemoveCatFromSlot(slotIndex);
        }

        PlayerUnitSnapshot newUnit = new PlayerUnitSnapshot
        {
            CatCardId = catCardId,
            FinalSlotIndex = slotIndex,
            Equipment = new EquipmentSnapshot()
        };

        currentFormation[slotIndex] = newUnit;
        
        // Spawn visual element here...
    }

    public void RemoveCatFromSlot(int slotIndex)
    {
        if (currentFormation.ContainsKey(slotIndex))
        {
            currentFormation.Remove(slotIndex);
            
            // Clean up visual element here...
            foreach (Transform child in formationSlots[slotIndex])
            {
                Destroy(child.gameObject);
            }
        }
    }
}
