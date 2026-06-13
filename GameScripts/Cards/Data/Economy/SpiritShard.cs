using Mewtations.Systems.Economy;
using UnityEngine;

namespace Mewtations.Cards.Economy
{
    public class SpiritShard : CurrencyCardData
    {
        private void Reset()
        {
        }

        public override bool CanBeSold => false;
    }
}
