using UnityEngine;
using Mewtations.Cards.Economy;

namespace Mewtations.Cards.Structures
{
    public class HoundTreasury : CardData
    {
        [Header("Treasury Settings")]
        [SerializeField] private bool _isTreasuryOpen = false;

        public override bool CanBeSold => false;

        // Future interactions for RMB to open UI will be hooked up here
        // usually via an input handler or click event on the physical card.
        public void OpenTreasuryUI()
        {
            _isTreasuryOpen = true;
            // Disable camera drag and world interaction
            // Open HoundTreasuryUI
        }
    }
}
