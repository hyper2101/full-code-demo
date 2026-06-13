using UnityEngine;

namespace Mewtations.Cards.Structures
{
    public class TradingPost : CardData
    {
        [Header("Trading Post Settings")]
        [SerializeField] private bool _isShopOpen = false;

        public override bool CanBeSold => false;

        public void OpenShopUI()
        {
            _isShopOpen = true;
            // Open TradingPostUI
        }
    }
}
