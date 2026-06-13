using UnityEngine;
using UnityEngine.UI;

public class RecipeSlotUI : MonoBehaviour
{
    public Image Icon;
    public Text NameText;
    public GameObject NewBadge;
    public Button SlotButton;

    private Blueprint currentBlueprint;
    private bool isUnlocked;

    private void Awake()
    {
        if (SlotButton != null)
        {
            SlotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    public void Init(Blueprint bp, bool unlocked)
    {
        currentBlueprint = bp;
        isUnlocked = unlocked;

        if (isUnlocked)
        {
            if (bp.UseCustomDisplay && bp.CustomRecipeIcon != null)
            {
                Icon.sprite = bp.CustomRecipeIcon;
            }
            else
            {
                Icon.sprite = bp.Icon; // Fallback to base Icon
            }

            if (bp.UseCustomDisplay && !string.IsNullOrEmpty(bp.CustomNameTerm))
            {
                NameText.text = MewtationsLoc.Translate(bp.CustomNameTerm);
            }
            else
            {
                NameText.text = MewtationsLoc.Translate(bp.NameTerm);
            }

            bool isNew = SaveManager.instance.CurrentSave.UnreadRecipeIds.Contains(bp.Id);
            RefreshNewBadge(isNew);
        }
        else
        {
            Icon.sprite = null; // Can be a silhouette or question mark in inspector
            NameText.text = MewtationsLoc.Translate("term_unknown_recipe");
            RefreshNewBadge(false);
        }

        // TODO: Add tooltip hook here for hovering if needed later
        // e.g. EventTrigger for OnPointerEnter/OnPointerExit
    }

    public void RefreshNewBadge(bool isNew)
    {
        if (NewBadge != null)
        {
            NewBadge.SetActive(isNew);
        }
    }

    private void OnSlotClicked()
    {
        if (!isUnlocked) return;
        
        RecipeBookController.Instance.OnRecipeSelected(currentBlueprint, this);
    }
}
