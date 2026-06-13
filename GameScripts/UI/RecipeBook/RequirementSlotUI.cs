using UnityEngine;
using UnityEngine.UI;

public class RequirementSlotUI : MonoBehaviour
{
    public Image Icon;
    public Text AmountText;
    
    // UI placeholder for tooltip
    public GameObject TooltipTarget; 

    private string currentLocKey;
    private string tooltipText;

    public void Init(string cardId, int amount = 1)
    {
        string displayId = cardId;
        bool isOrCondition = false;
        
        if (cardId.Contains("|"))
        {
            isOrCondition = true;
            string[] parts = cardId.Split('|');
            displayId = parts[0]; // Use the first item's icon and base name
        }

        CardData cardData = WorldManager.instance.GetCardPrefab(displayId, true);
        if (cardData != null)
        {
            Icon.sprite = cardData.Icon;
            currentLocKey = cardData.NameTerm;
            
            if (isOrCondition)
            {
                // Provide a generic localized suffix like " or similar"
                tooltipText = MewtationsLoc.Translate(currentLocKey) + " " + MewtationsLoc.Translate("term_or_similar");
            }
            else
            {
                tooltipText = MewtationsLoc.Translate(currentLocKey);
            }

            if (amount > 1)
            {
                AmountText.text = "x" + amount;
                AmountText.gameObject.SetActive(true);
            }
            else
            {
                AmountText.gameObject.SetActive(false);
            }
        }
        else
        {
            // Missing data fallback
            AmountText.gameObject.SetActive(false);
            currentLocKey = "term_missing_card";
            tooltipText = MewtationsLoc.Translate(currentLocKey);
        }
    }

    public void InitGenericWorker()
    {
        // Placeholder for generic worker requirement
        currentLocKey = "term_any_worker";
        tooltipText = MewtationsLoc.Translate(currentLocKey);
        AmountText.gameObject.SetActive(false);
    }

    // Example Tooltip Hook
    public void OnPointerEnter()
    {
        // TooltipUI.Show(tooltipText);
    }

    public void OnPointerExit()
    {
        // TooltipUI.Hide();
    }
}
