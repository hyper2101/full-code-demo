using UnityEngine;

public class CardVisuals
{
    private GameCard _card;

    public CardPalette myCardPalette;

    public CardVisuals(GameCard card)
    {
        _card = card;
    }

    public void UpdateIcon()
    {
        if (_card.CardData.MyCardType == CardType.Ideas)
        {
            if (_card.CardData.CardUpdateType == CardUpdateType.Main)
            {
                _card.IconRenderer.sprite = SpriteManager.instance.IdeaIcon;
            }
            else if (_card.CardData.CardUpdateType == CardUpdateType.Island)
            {
                _card.IconRenderer.sprite = SpriteManager.instance.IslandIdeaIcon;
            }
            else if (_card.CardData.CardUpdateType == CardUpdateType.Spirit)
            {
                _card.IconRenderer.sprite = SpriteManager.instance.SpiritIdeaIcon;
            }
            else if (_card.CardData.CardUpdateType == CardUpdateType.Cities)
            {
                _card.IconRenderer.sprite = SpriteManager.instance.CitiesIdeaIcon;
            }
            else
            {
                _card.IconRenderer.sprite = SpriteManager.instance.IdeaIcon;
            }
        }
        if (_card.CardData.Icon != null)
        {
            _card.IconRenderer.sprite = _card.CardData.Icon;
        }
    }

    public void UpdateCardPalette()
    {
        myCardPalette = ColorManager.instance.GetPaletteForCard(_card.CardData);
    }
}
