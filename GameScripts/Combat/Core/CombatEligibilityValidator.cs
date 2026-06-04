using Mewtations.Expedition;

namespace Mewtations.Combat.Core
{
    public static class CombatEligibilityValidator
    {
        public static bool IsEligible(CatCardData cat, out string reason)
        {
            if (cat == null)
            {
                reason = "Cat is null.";
                return false;
            }

            if (cat.HealthPoints <= 0)
            {
                reason = "Mèo đã gục ngã (HP = 0).";
                return false;
            }

            if (cat.HasScar(Mewtations.Combat.PermanentScar.Paralyzed))
            {
                reason = "Mèo đang bị Tê Liệt.";
                return false;
            }

            if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.ActiveCats.Contains(cat))
            {
                reason = "Mèo đang đi Viễn Chinh.";
                return false;
            }

            if (cat.IsLocked || (cat.MyGameCard != null && cat.MyGameCard.IsLocked))
            {
                reason = "Mèo đang bị khóa hành động.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public static bool IsEligible(CatCardData cat)
        {
            return IsEligible(cat, out _);
        }
    }
}
