using System;
using Shapes;
using TMPro;
using UnityEngine;

public class SellBox : CardTarget
{
	public override void CardDropped(GameCard card)
	{
		WorldManager.instance.SellCard(this.GoldSpawnPosition.position, card, 1f, true);
		base.CardDropped(card);
	}

	public override bool CanHaveCard(GameCard card)
	{
		return WorldManager.instance.CardCanBeSold(card, true, false);
	}

	protected override void Update()
	{
		SpriteRenderer imageSpriteRenderer = this.ImageSpriteRenderer;
		WorldManager instance = WorldManager.instance;
		GameBoard currentBoard = WorldManager.instance.CurrentBoard;
		BoardCurrency? boardCurrency;
		if (currentBoard == null)
		{
			boardCurrency = null;
		}
		else
		{
			BoardOptions boardOptions = currentBoard.BoardOptions;
			boardCurrency = ((boardOptions != null) ? new BoardCurrency?(boardOptions.Currency) : null);
		}
		imageSpriteRenderer.sprite = instance.GetCurrencyIcon(boardCurrency);
		if (WorldManager.instance.CurrentBoard != null)
		{
			this.SellText.text = MewtationsLoc.Translate(WorldManager.instance.CurrentBoard.BoardOptions.SellBoxTerm);
			base.gameObject.name = MewtationsLoc.Translate(WorldManager.instance.CurrentBoard.BoardOptions.SellBoxTerm);
		}
		else
		{
			this.SellText.text = MewtationsLoc.Translate("label_sell");
			base.gameObject.name = MewtationsLoc.Translate("label_sellbox_title");
		}
		if (WorldManager.instance.CurrentBoard != null)
		{
			this.HighlightRectangle.Color = WorldManager.instance.CurrentBoard.CardHighlightColor;
		}
		this.HighlightRectangle.enabled = WorldManager.instance.DraggingCard != null && this.CanHaveCard(WorldManager.instance.DraggingCard);
		this.HighlightRectangle.DashOffset += Time.deltaTime;
		if (this.HighlightRectangle.DashOffset >= 1f)
		{
			this.HighlightRectangle.DashOffset -= 1f;
		}
		base.Update();
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		Vector3 localPosition = base.transform.localPosition;
		localPosition.z = 0f;
		base.transform.localPosition = localPosition;
	}

	public override string GetTooltipText()
	{
		if (WorldManager.instance.CurrentBoard != null)
		{
			return MewtationsLoc.Translate(WorldManager.instance.CurrentBoard.BoardOptions.SellBoxDescription);
		}
		return MewtationsLoc.Translate("label_sellbox_description", new LocParam[] { Extensions.LocParam_Action("sell") });
	}

	public Transform GoldSpawnPosition;

	public SpriteRenderer ImageSpriteRenderer;

	public Rectangle HighlightRectangle;

	public TextMeshPro SellText;
}
