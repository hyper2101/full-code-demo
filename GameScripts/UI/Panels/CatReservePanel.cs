using System.Collections.Generic;
using UnityEngine;

public class CatReservePanel : MonoBehaviour
{
    [SerializeField] private Transform scrollContent;
    [SerializeField] private GameObject catDraggablePrefab;

    public void RefreshAvailableCats()
    {
        ClearList();
        
        // Fetch valid cats from the main inventory (WorldManager / GameDataLoader)
        // For now, this is a stub that should be filled based on actual inventory logic
        List<string> availableCatIds = GetAvailableCatIdsFromInventory();

        foreach (var catId in availableCatIds)
        {
            GameObject inst = Instantiate(catDraggablePrefab, scrollContent);
            // Setup the draggable UI element
            // DraggableCatUI ui = inst.GetComponent<DraggableCatUI>();
            // ui.Init(catId);
        }
    }

    private void ClearList()
    {
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }
    }

    private List<string> GetAvailableCatIdsFromInventory()
    {
        // TODO: Connect to WorldManager to get CatCardData instances on the board
        return new List<string>();
    }
}
