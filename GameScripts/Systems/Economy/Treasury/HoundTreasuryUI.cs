using UnityEngine;

namespace Mewtations.UI.Treasury
{
    public class HoundTreasuryUI : MonoBehaviour
    {
        // Reference to handlers
        // public TreasurySellHandler SellHandler;
        // public TreasuryExchangeHandler ExchangeHandler;
        // public TreasuryLoanHandler LoanHandler;

        public void Open()
        {
            gameObject.SetActive(true);
            // Lock camera/pan here
        }

        public void Close()
        {
            gameObject.SetActive(false);
            // Unlock camera/pan here
        }
    }
}
