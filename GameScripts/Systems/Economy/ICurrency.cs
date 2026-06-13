namespace Mewtations.Systems.Economy
{
    public interface ICurrency
    {
        int RawValue { get; }
        CurrencyTier Tier { get; }
        bool CanSubstituteLowerTier { get; }
    }
}
