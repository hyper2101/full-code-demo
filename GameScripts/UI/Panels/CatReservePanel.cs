using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mewtations.Combat.Encounters;

public class CatReservePanel : MonoBehaviour
{
    [SerializeField] private Transform scrollContent;
    [SerializeField] private GameObject catDraggablePrefab;

    public void PopulateSessionCats(PreCombatSession session)
    {
        session.AvailableCats.Clear();

        var allCats = WorldManager.instance.BoardQuery.GetVisibleBoardCards()
            .Where(c => c != null && c.CardData is CatCardData && !c.Destroyed)
            .Select(c => c.CardData as CatCardData)
            .ToList();

        foreach (var cat in allCats)
        {
            if (Mewtations.Combat.Core.CombatEligibilityValidator.IsEligible(cat))
            {
                session.AvailableCats.Add(cat);
            }
        }
    }

    public void RefreshAvailableCats(PreCombatSession session)
    {
        ClearList();
        
        // Only show cats that are NOT currently in the sandbox formation
        var catsInFormation = new HashSet<CatCardData>(session.Formation.Values.Select(v => v.CatReference));

        foreach (var cat in session.AvailableCats)
        {
            if (catsInFormation.Contains(cat)) continue;

            GameObject inst = Instantiate(catDraggablePrefab, scrollContent);
            // Setup the draggable UI element
            // DraggableCatUI ui = inst.GetComponent<DraggableCatUI>();
            // ui.Init(cat);
        }
    }

    private void ClearList()
    {
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }
    }
}
