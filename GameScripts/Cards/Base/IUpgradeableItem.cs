namespace Mewtations.Legacy.Stacklands
{
    public interface IUpgradeableItem
    {
        int UpgradeTier { get; }
        int MaxUpgradeTier { get; }
        bool CanUpgrade(RefinementMaterialCardData material);
        void ApplyUpgrade(RefinementMaterialCardData material);
    }
}
