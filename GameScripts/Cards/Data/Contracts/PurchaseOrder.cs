using UnityEngine;
using Mewtations.Systems.Economy;
using System.Collections.Generic;

namespace Mewtations.Cards.Contracts
{
    public class PurchaseOrder : CardData
    {
        [Header("Purchase Details")]
        public int RequiredValue;
        public CurrencyTier MinimumTierRequired;
        public CardData TargetItemPrefab;

        private int _currentValue = 0;

        public override bool CanBeSold => false;

        public void InsertCurrency(ICurrency currency)
        {
            // Enforce denomination rules: lower denominations cannot satisfy higher requirements
            if (currency.Tier < MinimumTierRequired)
            {
                Debug.LogWarning("Insufficient denomination hierarchy. Purchase Order rejects lower tier currency.");
                // Return currency to board
                return;
            }

            _currentValue += currency.RawValue;
            Debug.Log($"Inserted {currency.RawValue}. Total: {_currentValue}/{RequiredValue}");

            if (_currentValue >= RequiredValue)
            {
                CompletePurchase();
            }
        }

        private void CompletePurchase()
        {
            // Spawn TargetItemPrefab
            // Destroy this PurchaseOrder
            Debug.Log("Purchase Complete! Spawning item.");
        }
    }
}
