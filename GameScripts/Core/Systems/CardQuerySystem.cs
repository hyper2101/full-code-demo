using System;
using System.Collections.Generic;
using System.Linq;

public class CardQuerySystem
{
    private WorldManager _world;

    public CardQuerySystem(WorldManager world)
    {
        _world = world;
    }

    public int GetCardCount<T>(Predicate<T> pred) where T : CardData
    {
        int num = 0;
        GameBoard currentBoard = _world.CurrentBoard;
        for (int i = _world.AllCards.Count - 1; i >= 0; i--)
        {
            GameCard gameCard = _world.AllCards[i];
            if (!(gameCard.MyBoard != currentBoard))
            {
                T t = gameCard.CardData as T;
                if (t != null && (pred == null || pred(t)))
                {
                    num++;
                }
            }
        }
        return num;
    }

    public int GetCardCount<T>() where T : CardData
    {
        return GetCardCount<T>(null);
    }

    public T GetCard<T>() where T : CardData
    {
        for (int i = 0; i < _world.AllCards.Count; i++)
        {
            GameCard gameCard = _world.AllCards[i];
            if (gameCard.MyBoard.IsCurrent && gameCard.CardData is T)
            {
                return (T)((object)gameCard.CardData);
            }
        }
        return default(T);
    }

    public T GetCard<T>(GameBoard board) where T : CardData
    {
        foreach (GameCard gameCard in _world.AllCards)
        {
            if (!(gameCard.MyBoard.Id != board.Id) && gameCard.CardData is T)
            {
                return (T)((object)gameCard.CardData);
            }
        }
        return default(T);
    }

    public CardData GetCard(string cardId)
    {
        foreach (GameCard gameCard in _world.AllCards)
        {
            if (gameCard.MyBoard.IsCurrent && gameCard.CardData.Id == cardId)
            {
                return gameCard.CardData;
            }
        }
        return null;
    }

    public List<GameCard> GetAllCardsOnBoard(string board)
    {
        return _world.AllCards.Where<GameCard>((GameCard card) => card.MyBoard.Id == board).ToList<GameCard>();
    }

    public List<CardData> GetCards(string cardId)
    {
        List<CardData> list = new List<CardData>();
        foreach (GameCard gameCard in _world.AllCards)
        {
            if (gameCard.MyBoard.IsCurrent && gameCard.CardData.Id == cardId)
            {
                list.Add(gameCard.CardData);
            }
        }
        return list;
    }

    public List<T> GetCardsImplementingInterface<T>()
    {
        if (!typeof(T).IsInterface)
        {
            throw new ArgumentException();
        }
        List<T> list = new List<T>();
        GameBoard currentBoard = _world.CurrentBoard;
        foreach (GameCard gameCard in _world.AllCards)
        {
            if (!(gameCard.MyBoard != currentBoard))
            {
                CardData cardData = gameCard.CardData;
                if (cardData is T)
                {
                    T t = cardData as T;
                    list.Add(t);
                }
            }
        }
        return list;
    }

    public List<T> GetCardsImplementingInterfaceNonAlloc<T>(List<T> list)
    {
        if (!typeof(T).IsInterface)
        {
            throw new ArgumentException();
        }
        list.Clear();
        GameBoard currentBoard = _world.CurrentBoard;
        for (int i = _world.AllCards.Count - 1; i >= 0; i--)
        {
            GameCard gameCard = _world.AllCards[i];
            if (!(gameCard.MyBoard != currentBoard))
            {
                CardData cardData = gameCard.CardData;
                if (cardData is T)
                {
                    T t = cardData as T;
                    list.Add(t);
                }
            }
        }
        return list;
    }

    public List<T> GetCards<T>() where T : CardData
    {
        List<T> list = new List<T>();
        for (int i = 0; i < _world.AllCards.Count; i++)
        {
            GameCard gameCard = _world.AllCards[i];
            if (gameCard.MyBoard.IsCurrent)
            {
                T t = gameCard.CardData as T;
                if (t != null)
                {
                    list.Add(t);
                }
            }
        }
        return list;
    }

    public void GetCardsNonAlloc<T>(List<T> list) where T : CardData
    {
        list.Clear();
        GameBoard currentBoard = _world.CurrentBoard;
        foreach (GameCard gameCard in _world.AllCards)
        {
            if (!(gameCard.MyBoard != currentBoard))
            {
                T t = gameCard.CardData as T;
                if (t != null)
                {
                    list.Add(t);
                }
            }
        }
    }

    public List<T> GetCards<T>(GameBoard board) where T : CardData
    {
        List<T> list = new List<T>();
        foreach (GameCard gameCard in _world.AllCards)
        {
            if (!(gameCard.MyBoard.Id != board.Id))
            {
                T t = gameCard.CardData as T;
                if (t != null)
                {
                    list.Add(t);
                }
            }
        }
        return list;
    }

    public void GetCardsNonAlloc<T>(GameBoard board, List<T> list) where T : CardData
    {
        list.Clear();
        foreach (GameCard gameCard in _world.AllCards)
        {
            if (!(gameCard.MyBoard.Id != board.Id))
            {
                T t = gameCard.CardData as T;
                if (t != null)
                {
                    list.Add(t);
                }
            }
        }
    }

    public List<Boosterpack> GetAllBoostersOnBoard(string board)
    {
        return _world.AllBoosters.Where<Boosterpack>((Boosterpack booster) => booster.MyBoard.Id == board).ToList<Boosterpack>();
    }
}
