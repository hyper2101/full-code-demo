using System;

namespace Mewtations.Core
{
    /// <summary>
    /// Delegation layer to isolate WorldManager from legacy board side-effects.
    /// Thin hook with no business logic. Maps side effects for legacy boards without coupling WorldManager.
    /// </summary>
    public static class LegacyBoardTransitionHooks
    {
        public static void HandleBoardEnterSideEffects(string boardId)
        {
            // 1. Notify legacy quest system of board entry
            LegacyQuestHooks.TriggerSpecialAction("board_" + boardId);

            // 2. Map side effects based on specific legacy boards
            if (boardId == "cities")
            {
                LegacyCitiesHooks.ResetWellbeingToStart();
            }
        }
    }
}
