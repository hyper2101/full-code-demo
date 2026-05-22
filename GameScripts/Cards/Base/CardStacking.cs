using System.Collections.Generic;
using UnityEngine;

public class CardStacking
{
    private GameCard _card;

    public GameCard Parent;
    public GameCard Child;
    public GameCard LastParent;

    public CardStacking(GameCard card)
    {
        _card = card;
    }

    public bool HasParent => Parent != null;

    public bool HasChild => Child != null;

    private List<GameCard> cardsInvolved = new List<GameCard>();

    public void SetChild(GameCard card)
    {
        cardsInvolved.Clear();
        cardsInvolved.Add(_card);
        if (card == _card)
        {
            Debug.LogError("Child is same as Parent");
            return;
        }
        if (card == null)
        {
            if (Child != null)
            {
                cardsInvolved.Add(Child);
                Child.Stacking.Parent = null;
            }
            Child = null;
            NotifyStackUpdate(cardsInvolved);
            return;
        }
        Child = card;
        card.Stacking.Parent = _card;
        cardsInvolved.Add(card);
        NotifyStackUpdate(cardsInvolved);
    }

    public void SetParent(GameCard card)
    {
        cardsInvolved.Clear();
        cardsInvolved.Add(_card);
        if (card == _card)
        {
            Debug.LogError("Child is same as Parent");
            return;
        }
        if (card == null)
        {
            if (Parent != null)
            {
                cardsInvolved.Add(Parent);
                Parent.Stacking.Child = null;
            }
            Parent = null;
            NotifyStackUpdate(cardsInvolved);
            return;
        }
        Parent = card;
        card.Stacking.Child = _card;
        cardsInvolved.Add(card);
        NotifyStackUpdate(cardsInvolved);
    }

    public void RemoveFromStack()
    {
        SetParent(null);
        SetChild(null);
    }

    private void NotifyStackUpdate(List<GameCard> cardsInvolvedList)
    {
        foreach (GameCard gameCard in cardsInvolvedList)
        {
            gameCard.GetRootCard().StackUpdate = true;
            gameCard.StackUpdate = true;
        }
    }

    public void RemoveFromParent()
    {
        if (Parent != null)
        {
            Parent.Stacking.SetChild(null);
        }
        SetParent(null);
    }
}
