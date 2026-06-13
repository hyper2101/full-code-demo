using UnityEngine;

namespace Mewtations.UI.Trading
{
    public class TradingPostUI : MonoBehaviour
    {
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void PurchaseRecipe(CardData recipeCard)
        {
            // Spawn a PurchaseOrder configured for this recipe's cost and requirements
            Debug.Log($"Spawned Purchase Order for {recipeCard.name}");
        }
    }
}
