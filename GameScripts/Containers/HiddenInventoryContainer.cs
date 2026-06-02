using System.Collections.Generic;

public class HiddenInventoryContainer : ICardContainer
{
    public GameCard HostCard { get; private set; }
    private List<GameCard> items = new List<GameCard>();
    private int capacity;

    public HiddenInventoryContainer(GameCard host, int capacity = 5)
    {
        HostCard = host;
        this.capacity = capacity;
    }

    public bool CanInsert(GameCard card, ContainerInsertContext context)
    {
        return items.Count < capacity;
    }

    public ContainerTransactionResult Insert(GameCard card, ContainerInsertContext context)
    {
        if (!CanInsert(card, context))
        {
            return ContainerTransactionResult.Fail("Inventory is full.");
        }
        if (items.Contains(card))
        {
            return ContainerTransactionResult.Fail("Card already in inventory.");
        }
        
        items.Add(card);
        GameplayEventBus.OnCardInsertedIntoContainer?.Invoke(card, this);
        return ContainerTransactionResult.Ok();
    }

    public void Remove(GameCard card)
    {
        if (items.Contains(card))
        {
            items.Remove(card);
            GameplayEventBus.OnCardRemovedFromContainer?.Invoke(card, this);
        }
    }

    public IReadOnlyList<GameCard> GetChildren()
    {
        return items.AsReadOnly();
    }

    public int GetCapacity()
    {
        return capacity;
    }

    public void SetCapacity(int newCapacity)
    {
        // As per constraints:
        // Do not delete existing items, do not drop items, do not recreate container.
        // Just update the capacity variable. If newCapacity < items.Count, it just blocks new inserts.
        this.capacity = newCapacity;
    }

    public ContainerType Type => ContainerType.HiddenInventory;
}
