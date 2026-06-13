using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeVariantUI : MonoBehaviour
{
    public Text CraftingModeText;
    public Image CraftingModeIcon;

    [Header("Containers")]
    public Transform RequirementsContainer;
    public Transform IngredientsContainer;
    public Transform OutputsContainer;

    [Header("Prefabs")]
    public GameObject RequirementSlotPrefab;

    public void Init(Subprint subprint, int variantIndex, Blueprint parentBlueprint)
    {
        // 1. Crafting Mode Display
        if (subprint.WorkerRequirementType != WorkerRequirementType.None)
        {
            CraftingModeText.text = "🐱 " + MewtationsLoc.Translate("term_requires_worker");
        }
        else if (subprint.CraftingMode == CraftingMode.Automatic)
        {
            CraftingModeText.text = "⚙ " + MewtationsLoc.Translate("term_automatic_synthesis");
        }
        else
        {
            CraftingModeText.text = "🛠 " + MewtationsLoc.Translate("term_manual_crafting");
        }

        // Clear containers
        ClearContainer(RequirementsContainer);
        ClearContainer(IngredientsContainer);
        ClearContainer(OutputsContainer);

        // 2. Requirements (Structures & Workers)
        if (subprint.RequiredStructures != null)
        {
            foreach (string structId in subprint.RequiredStructures)
            {
                CreateSlot(structId, 1, RequirementsContainer);
            }
        }

        if (subprint.WorkerRequirementType == WorkerRequirementType.AnyCat)
        {
            // Create a special slot for generic worker
            GameObject slotObj = Instantiate(RequirementSlotPrefab, RequirementsContainer);
            slotObj.GetComponent<RequirementSlotUI>().InitGenericWorker();
        }

        // 3. Ingredients (Consumed - from RequiredCards)
        if (subprint.RequiredCards != null)
        {
            // Group by ID to count amounts
            Dictionary<string, int> ingredientCounts = new Dictionary<string, int>();
            foreach (string cardId in subprint.RequiredCards)
            {
                if (!ingredientCounts.ContainsKey(cardId)) ingredientCounts[cardId] = 0;
                ingredientCounts[cardId]++;
            }

            foreach (var kvp in ingredientCounts)
            {
                CreateSlot(kvp.Key, kvp.Value, IngredientsContainer);
            }
        }

        // 4. Outputs
        if (!string.IsNullOrEmpty(subprint.ResultCard))
        {
            CreateSlot(subprint.ResultCard, 1, OutputsContainer);
        }

        if (subprint.ExtraResultCards != null)
        {
            foreach (string extraId in subprint.ExtraResultCards)
            {
                CreateSlot(extraId, 1, OutputsContainer);
            }
        }
    }

    private void CreateSlot(string cardId, int amount, Transform container)
    {
        GameObject slotObj = Instantiate(RequirementSlotPrefab, container);
        RequirementSlotUI slotUI = slotObj.GetComponent<RequirementSlotUI>();
        if (slotUI != null)
        {
            slotUI.Init(cardId, amount);
        }
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
