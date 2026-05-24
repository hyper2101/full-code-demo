using System;

public class GameplayEventBus
{
    // Card Lifecycle
    public static Action<GameCard> OnCardSpawned;
    public static Action<string> OnCardDestroyed;

    // Container interactions
    public static Action<GameCard, ICardContainer> OnCardInsertedIntoContainer;
    public static Action<GameCard, ICardContainer> OnCardRemovedFromContainer;

    // Combat & Stats
    public static Action<string, int> OnHealthChanged; // uniqueId, newHealth
    public static Action<string> OnCardDeath;

    // Progression
    public static Action<string> OnRealmAdvanced;

    public static void ClearAllSubscribers()
    {
        OnCardSpawned = null;
        OnCardDestroyed = null;
        OnCardInsertedIntoContainer = null;
        OnCardRemovedFromContainer = null;
        OnHealthChanged = null;
        OnCardDeath = null;
        OnRealmAdvanced = null;
    }
}
