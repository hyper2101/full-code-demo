using System.Collections.Generic;

public class VerticalStackContainer : ICardContainer
{
    public GameCard HostCard { get; private set; }
    private GameCard childCard;

    public VerticalStackContainer(GameCard host)
    {
        HostCard = host;
    }

    public bool CanInsert(GameCard card, ContainerInsertContext context)
    {
        if (childCard != null) return false;
        if (HostCard == null || HostCard.CardData == null) return false;
        if (card == null || card.CardData == null) return false;
        
        return HostCard.CardData.CanHaveCardOnTop(card.CardData, false);
    }

    public ContainerTransactionResult Insert(GameCard card, ContainerInsertContext context)
    {
        if (!CanInsert(card, context)) 
        {
            return ContainerTransactionResult.Fail("Cannot insert into stack: validation failed or stack is full.");
        }
        
        childCard = card;
        GameplayEventBus.OnCardInsertedIntoContainer?.Invoke(card, this);
        return ContainerTransactionResult.Ok();
    }

    public void Remove(GameCard card)
    {
        if (childCard == card)
        {
            childCard = null;
            GameplayEventBus.OnCardRemovedFromContainer?.Invoke(card, this);
        }
    }

    public IReadOnlyList<GameCard> GetChildren()
    {
        if (childCard != null)
        {
            return new List<GameCard> { childCard }.AsReadOnly();
        }
        return new List<GameCard>().AsReadOnly();
    }

    public int GetCapacity()
    {
        return 1;
    }

    public ContainerType Type => ContainerType.VerticalStack;
}
