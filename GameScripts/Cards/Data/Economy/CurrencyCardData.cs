using Mewtations.Systems.Economy;
using UnityEngine;

namespace Mewtations.Cards.Economy
{
    public abstract class CurrencyCardData : CardData, ICurrency
    {
        [Header("Currency Settings")]
        [SerializeField] private int _rawValue;
        [SerializeField] private CurrencyTier _tier;
        [SerializeField] private bool _canSubstituteLowerTier = true;

        public int RawValue => _rawValue;
        public CurrencyTier Tier => _tier;
        public bool CanSubstituteLowerTier => _canSubstituteLowerTier;
    }
}
