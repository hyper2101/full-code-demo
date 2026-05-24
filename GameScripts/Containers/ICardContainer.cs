using System.Collections.Generic;

public enum ContainerType
{
    VerticalStack,
    HorizontalSlot,
    HiddenInventory
}

public class ContainerInsertContext
{
    public GameCard SourceCard; 
    public string ContextSource; // e.g. "PlayerDrag", "AutoEquip", "Spawn"
}

public class ContainerTransactionResult
{
    public bool Success;
    public string FailureReason;
    
    public static ContainerTransactionResult Ok() => new ContainerTransactionResult { Success = true };
    public static ContainerTransactionResult Fail(string reason) => new ContainerTransactionResult { Success = false, FailureReason = reason };
}

public interface ICardContainer
{
    bool CanInsert(GameCard card, ContainerInsertContext context);
    ContainerTransactionResult Insert(GameCard card, ContainerInsertContext context);
    void Remove(GameCard card);
    IReadOnlyList<GameCard> GetChildren();
    
    ContainerType Type { get; }
    int GetCapacity();
    GameCard HostCard { get; }
}
