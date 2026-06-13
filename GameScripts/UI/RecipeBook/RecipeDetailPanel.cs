using UnityEngine;
using UnityEngine.UI;

public class RecipeDetailPanel : MonoBehaviour
{
    [Header("Header Elements")]
    public Image RecipeIcon;
    public Text RecipeName;
    public Text RecipeDescription;

    [Header("Variants Area")]
    public Transform VariantContainer;
    public GameObject VariantPrefab;

    public void ShowDetails(Blueprint bp)
    {
        gameObject.SetActive(true);

        if (bp.UseCustomDisplay && bp.CustomRecipeIcon != null)
        {
            RecipeIcon.sprite = bp.CustomRecipeIcon;
        }
        else
        {
            RecipeIcon.sprite = bp.Icon;
        }

        if (bp.UseCustomDisplay && !string.IsNullOrEmpty(bp.CustomNameTerm))
        {
            RecipeName.text = MewtationsLoc.Translate(bp.CustomNameTerm);
        }
        else
        {
            RecipeName.text = MewtationsLoc.Translate(bp.NameTerm);
        }

        if (bp.UseCustomDisplay && !string.IsNullOrEmpty(bp.CustomDescriptionTerm))
        {
            RecipeDescription.text = MewtationsLoc.Translate(bp.CustomDescriptionTerm);
        }
        else
        {
            // Try to extract from GetText() or fallback
            RecipeDescription.text = bp.GetText();
        }

        // Clear existing variants
        foreach (Transform child in VariantContainer)
        {
            Destroy(child.gameObject);
        }

        // Generate Subprint details (Multi-variant support)
        if (bp.Subprints != null && bp.Subprints.Count > 0)
        {
            int variantIndex = 1;
            foreach (var subprint in bp.Subprints)
            {
                GameObject variantObj = Instantiate(VariantPrefab, VariantContainer);
                RecipeVariantUI variantUI = variantObj.GetComponent<RecipeVariantUI>();
                if (variantUI != null)
                {
                    variantUI.Init(subprint, variantIndex, bp);
                }
                variantIndex++;
            }
        }
    }

    public void ClearDetails()
    {
        gameObject.SetActive(false);
    }
}
