using System;

namespace Mewtations.Expedition
{
    public enum ExpeditionDifficulty
    {
        Easy,
        Medium,
        Hard,
        Elite,
        Legendary
    }

    public class ExpeditionRouteDefinition
    {
        public string SlotId { get; set; }
        public string DisplayName { get; set; }
        public ExpeditionDifficulty Difficulty { get; set; }
        // Placeholder for UnlockCondition, could be a delegate or an enum later
        public string UnlockConditionId { get; set; } 
        public bool IsUnlocked { get; set; }
    }
}
