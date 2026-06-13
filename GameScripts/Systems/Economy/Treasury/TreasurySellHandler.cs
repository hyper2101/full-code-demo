using UnityEngine;
using Mewtations.Systems.Economy;

namespace Mewtations.UI.Treasury
{
    public class TreasurySellHandler : MonoBehaviour
    {
        public void ProcessSell(CardData itemToSell)
        {
            if (itemToSell == null || !itemToSell.CanBeSold || itemToSell.SellValue <= 0)
            {
                Debug.LogWarning("Item cannot be sold.");
                return;
            }

            CurrencyTier tierToSpawn = itemToSell.SellTier;
            int amountToSpawn = itemToSell.SellValue;

            Debug.Log($"Sold {itemToSell.name} for {amountToSpawn} {tierToSpawn}");
            // Logic to physically spawn `amountToSpawn` cards of `tierToSpawn` goes here.
            // DO NOT flatten value into shards!
        }
    }
}
