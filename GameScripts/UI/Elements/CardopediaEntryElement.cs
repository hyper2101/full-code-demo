using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardopediaEntryElement : MonoBehaviour
{
	public void SetCardData(CardData cardData)
	{
		this.MyCardData = cardData;
		this.wasFound = WorldManager.instance.CurrentSave.FoundCardIds.Contains(cardData.Id) || (DebugOptions.Default.UnlockAllInCardopedia && Application.isEditor);
		this.IsNew = WorldManager.instance.CurrentSave.NewCardopediaIds.Contains(cardData.Id);
		this.HasUndiscoveredCards = cardData.HasUndiscoveredCardInDrops() && this.wasFound;
		this.UpdateUndiscoveredCardsIcon();
		this.UpdateIsNew();
		if (cardData.CardUpdateType == CardUpdateType.Spirit)
		{
			this.UpdateImage.sprite = this.SpiritIcon;
			this.tooltip.MyTooltipTerm = "label_cardopedia_spirit";
		}
		else if (cardData.CardUpdateType == CardUpdateType.Forest)
		{
			this.UpdateImage.sprite = this.ForestIcon;
			this.tooltip.MyTooltipTerm = "label_cardopedia_forest";
		}
		else if (cardData.CardUpdateType == CardUpdateType.Island)
		{
			this.UpdateImage.sprite = this.IslandIcon;
			this.tooltip.MyTooltipTerm = "label_cardopedia_island";
		}
		else if (cardData.CardUpdateType == CardUpdateType.Order)
		{
			this.UpdateImage.sprite = this.OrderIcon;
			this.tooltip.MyTooltipTerm = "label_cardopedia_order";
		}
		else if (cardData.CardUpdateType == CardUpdateType.Cities)
		{
			this.UpdateImage.sprite = this.CitiesIcon;
			this.tooltip.MyTooltipTerm = "label_cardopedia_cities";
		}
		else if (cardData.CardUpdateType == CardUpdateType.Mod)
		{
			this.UpdateImage.sprite = this.ModIcon;
			this.tooltip.MyTooltipTerm = "label_cardopedia_modded";
		}
		else
		{
			this.UpdateImage.sprite = this.MainIcon;
			this.tooltip.MyTooltipTerm = "label_cardopedia_main";
		}
		this.UpdateText();
	}

	private void UpdateUndiscoveredCardsIcon()
	{
		this.UndiscoveredTransform.gameObject.SetActive(this.HasUndiscoveredCards && !this.IsNew);
	}

	private void Update()
	{
		this.UpdateIsNew();
		if (this.Button.IsHovered || this.Button.IsSelected)
		{
			if (this.IsNew)
			{
				this.wasHoveredAndNew = true;
				return;
			}
		}
		else if (this.wasHoveredAndNew)
		{
			this.wasHoveredAndNew = false;
			this.IsNew = false;
			WorldManager.instance.CurrentSave.NewCardopediaIds.Remove(this.MyCardData.Id);
			SaveManager.instance.Save(false);
			this.UpdateUndiscoveredCardsIcon();
		}
	}

	private void UpdateIsNew()
	{
		this.NewTextTransform.gameObject.SetActive(this.IsNew);
		this.NewBackgroundTransform.gameObject.SetActive(this.IsNew);
		if (this.IsNew)
		{
			this.NewBackgroundTransform.sizeDelta = new Vector2(this.NewTextTransform.rect.width, this.NewTextTransform.rect.height);
			this.NewBackgroundTransform.position = this.NewTextTransform.position;
		}
	}

	public void Cull(bool cull)
	{
		if (this.isCulled == cull)
		{
			return;
		}
		this.isCulled = cull;
		for (int i = 0; i < this.CanvasRenderers.Count; i++)
		{
			this.CanvasRenderers[i].cull = cull;
		}
		if (!this.isCulled)
		{
			this.NewBackgroundTransform.sizeDelta = new Vector2(this.NewTextTransform.rect.width + 0.001f, this.NewTextTransform.rect.height);
			this.UpdateImage.rectTransform.sizeDelta = new Vector2(this.UpdateImage.rectTransform.rect.width + 0.001f, this.UpdateImage.rectTransform.rect.height);
			this.UndiscoveredTransform.sizeDelta = new Vector2(this.UndiscoveredTransform.rect.width + 0.001f, this.UndiscoveredTransform.rect.height);
		}
	}

	public void UpdateText()
	{
		if (this.MyCardData != null)
		{
			this.MyCardData.UpdateCardText();
		}
		if (this.wasFound)
		{
			this.Button.TextMeshPro.text = "• " + this.MyCardData.Name;
			return;
		}
		this.Button.TextMeshPro.text = "• ???";
	}

	public CustomButton Button;

	[HideInInspector]
	public CardData MyCardData;

	public RectTransform NewTextTransform;

	public RectTransform NewBackgroundTransform;

	public RectTransform UndiscoveredTransform;

	public RectTransform UpdateTypeTransform;

	public ShowTooltip tooltip;

	public Image UpdateImage;

	public Sprite MainIcon;

	public Sprite SpiritIcon;

	public Sprite ForestIcon;

	public Sprite IslandIcon;

	public Sprite ModIcon;

	public Sprite OrderIcon;

	public Sprite CitiesIcon;

	public bool wasFound;

	public bool IsFiltered;

	public bool IsFilteredUpdate;

	public bool HasUndiscoveredCards;

	public bool IsNew;

	public bool IsEnabled;

	private bool wasHoveredAndNew;

	public List<CanvasRenderer> CanvasRenderers;

	private bool isCulled;
}
