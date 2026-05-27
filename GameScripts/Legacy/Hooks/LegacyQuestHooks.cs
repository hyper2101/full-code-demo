using System;

namespace Mewtations.Core
{
    public static class LegacyQuestHooks
    {
        public static void CheckSteamAchievements()
        {
            if (LegacyRuntimeFlags.EnableQuestHooks && QuestManager.instance != null) QuestManager.instance.CheckSteamAchievements();
        }

        public static bool QuestIsComplete(string questId)
        {
            if (!LegacyRuntimeFlags.EnableQuestHooks || QuestManager.instance == null) return true; 
            return QuestManager.instance.QuestIsComplete(questId);
        }

        public static void CheckPacksUnlocked()
        {
            if (LegacyRuntimeFlags.EnableQuestHooks && QuestManager.instance != null) QuestManager.instance.CheckPacksUnlocked();
        }

        public static void UpdateCurrentQuests()
        {
            if (LegacyRuntimeFlags.EnableQuestHooks && QuestManager.instance != null) QuestManager.instance.UpdateCurrentQuests();
        }

        public static void SpecialActionComplete(string actionId, CardData cardData = null)
        {
            if (LegacyRuntimeFlags.EnableQuestHooks && QuestManager.instance != null) QuestManager.instance.SpecialActionComplete(actionId, cardData);
        }

        public static void CardCreated(CardData cardData)
        {
            if (LegacyRuntimeFlags.EnableQuestHooks && QuestManager.instance != null) QuestManager.instance.CardCreated(cardData);
        }

        public static BoosterpackData JustUnlockedPack()
        {
            if (!LegacyRuntimeFlags.EnableQuestHooks || QuestManager.instance == null) return null;
            return QuestManager.instance.JustUnlockedPack();
        }
    }
}
