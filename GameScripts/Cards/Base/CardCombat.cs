using UnityEngine;

public class CardCombat
{
    private GameCard _card;

    public CardCombat(GameCard card)
    {
        _card = card;
    }

    public Combatable Combatable => _card.CardData as Combatable;

    public bool InConflict => Combatable != null && Combatable.InConflict;

    public bool InAttack => Combatable != null && Combatable.InAttack;
}
