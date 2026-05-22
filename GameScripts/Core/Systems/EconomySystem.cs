using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EconomySystem
{
    private WorldManager _world;

    public EconomySystem(WorldManager world)
    {
        _world = world;
    }

    public int GetCountInChests(string cardId)
    {
        int num = 0;
        foreach (GameCard gameCard in _world.AllCards)
        {
            if (gameCard.MyBoard.IsCurrent)
            {
                ResourceChest resourceChest = gameCard.CardData as ResourceChest;
                if (resourceChest != null && resourceChest.HeldCardId == cardId)
                {
                    num += resourceChest.ResourceCount;
                }
                Chest chest = gameCard.CardData as Chest;
                if (chest != null && chest.HeldCardId == cardId)
                {
                    num += chest.CoinCount;
                }
            }
        }
        return num;
    }

    public int GetShellCount(bool includeInChest)
    {
        int num = 0;
        if (includeInChest)
        {
            num = GetCountInChests("shell");
        }
        return _world.GetCardCount<Shell>() + num;
    }

    public int GetGoldCount(bool includeInChest)
    {
        int num = 0;
        if (includeInChest)
        {
            num = GetCountInChests("gold");
        }
        return _world.GetCardCount<Gold>() + num;
    }

    public int GetDollarCount(bool includeInChest)
    {
        int num = 0;
        if (includeInChest)
        {
            num = GetDollarInBank();
        }
        List<Dollar> dollarsList = new List<Dollar>();
        _world.GetCardsNonAlloc<Dollar>(dollarsList);
        int num2 = 0;
        for (int i = 0; i < dollarsList.Count; i++)
        {
            num2 += dollarsList[i].DollarValue;
        }
        return num2 + num;
    }

    public int GetDollarInBank()
    {
        List<Creditcard> creditcardsList = new List<Creditcard>();
        _world.GetCardsNonAlloc<Creditcard>(creditcardsList);
        int num = 0;
        for (int i = 0; i < creditcardsList.Count; i++)
        {
            num += creditcardsList[i].DollarCount;
        }
        return num;
    }

    public bool BoughtWithChest(GameCard card, int count, string heldCardId)
    {
        return card.GetAllCardsInStack().Sum<GameCard>(delegate(GameCard x)
        {
            Chest chest = x.CardData as Chest;
            if (chest == null || !(chest.HeldCardId == heldCardId))
            {
                return 0;
            }
            return chest.CoinCount;
        }) >= count;
    }

    public int GetAmountInChest(GameCard card, string heldCardId)
    {
        return card.GetAllCardsInStack().Sum<GameCard>(delegate(GameCard x)
        {
            Chest chest = x.CardData as Chest;
            if (chest == null || !(chest.HeldCardId == heldCardId))
            {
                return 0;
            }
            return chest.CoinCount;
        });
    }

    public void BuyWithChest(GameCard childCard, int toUse)
    {
        List<Chest> list = (from x in childCard.GetAllCardsInStack()
            where x.CardData is Chest
            select x.CardData as Chest).ToList<Chest>();
        for (int i = 0; i < list.Count; i++)
        {
            Chest chest = list[i];
            int num = Mathf.Min(toUse, chest.CoinCount);
            chest.CoinCount -= num;
            toUse -= num;
            if (toUse <= 0)
            {
                break;
            }
        }
        if (childCard.HasParent)
        {
            childCard.RemoveFromStack();
            childCard.SendIt();
        }
    }

    public bool BoughtWithGold(GameCard card, int count, bool checkStackAllSame = false)
    {
        return _world.GetCardCountInStack(card, (CardData x) => x.Id == "gold") >= count;
    }

    public bool BoughtWithShells(GameCard card, int count, bool checkStackAllSame = false)
    {
        return _world.GetCardCountInStack(card, (CardData x) => x.Id == "shell") >= count;
    }

    public int GetDollarsInCreditcard(GameCard card)
    {
        return card.GetAllCardsInStack().Sum<GameCard>(delegate(GameCard x)
        {
            Creditcard creditcard = x.CardData as Creditcard;
            if (creditcard == null)
            {
                return 0;
            }
            return creditcard.DollarCount;
        });
    }

    public void BuyWithCreditcard(GameCard childCard, int toUse)
    {
        List<Creditcard> list = (from x in childCard.GetAllCardsInStack()
            where x.CardData is Creditcard
            select x.CardData as Creditcard).ToList<Creditcard>();
        for (int i = 0; i < list.Count; i++)
        {
            Creditcard creditcard = list[i];
            int num = Mathf.Min(toUse, creditcard.DollarCount);
            creditcard.DollarCount -= num;
            toUse -= num;
            if (toUse <= 0)
            {
                break;
            }
        }
        if (childCard.HasParent)
        {
            childCard.RemoveFromStack();
            childCard.SendIt();
        }
    }
}
