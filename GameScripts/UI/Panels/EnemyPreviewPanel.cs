using System.Collections.Generic;
using UnityEngine;
using Mewtations.Combat.Encounters;

public class EnemyPreviewPanel : MonoBehaviour
{
    // References to 9 UI slots representing the enemy side (SlotIndex 0-8)
    [SerializeField] private List<Transform> previewSlots = new List<Transform>();
    
    // Prefab to spawn for each enemy preview
    [SerializeField] private GameObject enemyPreviewPrefab;

    public void Setup(EncounterData encounterData)
    {
        ClearPreview();

        if (encounterData == null || encounterData.Enemies == null) return;

        foreach (var spawnData in encounterData.Enemies)
        {
            if (spawnData.SlotIndex >= 0 && spawnData.SlotIndex < previewSlots.Count)
            {
                Transform slotParent = previewSlots[spawnData.SlotIndex];
                GameObject previewInst = Instantiate(enemyPreviewPrefab, slotParent);
                
                // Initialize the preview UI (e.g., set sprite, HP text from spawnData.Enemy)
                // EnemyPreviewElement element = previewInst.GetComponent<EnemyPreviewElement>();
                // if (element != null) element.Init(spawnData.Enemy);
            }
        }
    }

    private void ClearPreview()
    {
        foreach (var slot in previewSlots)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
