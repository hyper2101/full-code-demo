using System.Collections.Generic;
using UnityEngine;
using Mewtations.Combat.Encounters;

public class PlayerFormationPanel : MonoBehaviour
{
    [SerializeField] private List<Transform> formationSlots = new List<Transform>();
    
    private PreCombatSession _session;

    public void Initialize(PreCombatSession session)
    {
        _session = session;
        _session.Formation.Clear();
        
        foreach (var slot in formationSlots)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void AssignCatToSlot(CatCardData catData, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= formationSlots.Count) return;

        if (_session.Formation.ContainsKey(slotIndex))
        {
            RemoveCatFromSlot(slotIndex);
        }

        PlayerUnitSnapshot newUnit = new PlayerUnitSnapshot
        {
            CatReference = catData,
            FinalSlotIndex = slotIndex,
            Equipment = new EquipmentSnapshot()
        };

        _session.Formation[slotIndex] = newUnit;
        
        // Spawn visual element here...
    }

    public void RemoveCatFromSlot(int slotIndex)
    {
        if (_session.Formation.ContainsKey(slotIndex))
        {
            _session.Formation.Remove(slotIndex);
            
            foreach (Transform child in formationSlots[slotIndex])
            {
                Destroy(child.gameObject);
            }
        }
    }

    // Equipment Drop logic
    public bool TryEquipToCatInSlot(CardData item, int slotIndex)
    {
        if (!_session.Formation.ContainsKey(slotIndex)) return false;
        
        var catSnap = _session.Formation[slotIndex];
        var catData = catSnap.CatReference;
        
        if (item is Equipable equipable)
        {
            catData.InitializeEquipmentSlots();
            var slotType = equipable.EquipableTypeToCatSlotType();
            
            if (catData.EquipmentSlots.ContainsKey(slotType))
            {
                // Utilize the centralized EquipToSlot method which now creates EquipmentInstance and handles destruction safely
                if (catData.EquipToSlot(item, slotType))
                {
                    // Update snapshot with the newly created EquipmentInstance
                    catSnap.Equipment.AssignItem(slotType, catData.EquipmentSlots[slotType].EquipmentInstance);
                    // Refresh Ordering UI to remove this item
                    Mewtations.UI.Screens.PreCombatScreen.Instance.OrderingInventory.Initialize(_session);
                    return true;
                }
            }
        }
        return false;
    }
}
