using System;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using TMPro;
using UnityEngine;

public class BuyBoosterBox : CardTarget
{
	public BoosterpackData Booster
	{
		get
		{
			return WorldManager.instance.GetBoosterData(this.BoosterId);
		}
	}

	public int GetCost()
	{
		if (this.Booster.BoosterLocation == Location.Cities)
		{
			CardEventType? cardEventType = CitiesManager.instance.ActiveEvent;
			CardEventType cardEventType2 = CardEventType.FinancialCrisis;
			if ((cardEventType.GetValueOrDefault() == cardEventType2) & (cardEventType != null))
			{
				return Mathf.RoundToInt((float)this.Cost * 1.5f / 10f) * 10;
			}
			cardEventType = CitiesManager.instance.ActiveEvent;
			cardEventType2 = CardEventType.PackSale;
			if ((cardEventType.GetValueOrDefault() == cardEventType2) & (cardEventType != null))
			{
				return Mathf.CeilToInt((float)this.Cost * 0.75f / 10f) * 10;
			}
		}
		return this.Cost;
	}

	public override bool CanHaveCard(GameCard card)
	{
		if (!this.MyBoard.IsCurrent)
		{
			return false;
		}
		if (!this.Booster.IsUnlocked)
		{
			return false;
		}
		if (WorldManager.instance.RemovingCards)
		{
			return false;
		}
		if (this.BoardCurrency == BoardCurrency.Gold)
		{
			if (WorldManager.instance.GetCardCountInStack(card, (CardData x) => x.Id == "gold") > 0 || WorldManager.instance.GetAmountInChest(card, "gold") > 0)
			{
				return true;
			}
		}
		else if (this.BoardCurrency == BoardCurrency.Shell)
		{
			if (WorldManager.instance.GetCardCountInStack(card, (CardData x) => x.Id == "shell") > 0 || WorldManager.instance.GetAmountInChest(card, "shell") > 0)
			{
				return true;
			}
		}
		else if (this.BoardCurrency == BoardCurrency.Dollar)
		{
			if (WorldManager.instance.GetCardCountInStack(card, (CardData x) => x is Dollar) > 0 || WorldManager.instance.GetDollarsInCreditcard(card) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public int GetCurrentCost()
	{
		return this.GetCost() - this.StoredCostAmount;
	}

	public override void CardDropped(GameCard card)
	{
		int currentCost = this.GetCurrentCost();
		if (this.BoardCurrency == BoardCurrency.Gold)
		{
			int cardCountInStack = WorldManager.instance.GetCardCountInStack(card, (CardData x) => x.Id == "gold");
			int amountInChest = WorldManager.instance.GetAmountInChest(card, "gold");
			if (cardCountInStack > 0)
			{
				int num = ((cardCountInStack > currentCost) ? currentCost : cardCountInStack);
				WorldManager.instance.RemoveCardsFromStackPred(card, num, (GameCard x) => x.CardData.Id == "gold");
				this.StoredCostAmount += num;
			}
			else if (amountInChest > 0)
			{
				int num = ((amountInChest > currentCost) ? currentCost : amountInChest);
				WorldManager.instance.BuyWithChest(card, num);
				this.StoredCostAmount += num;
			}
		}
		else if (this.BoardCurrency == BoardCurrency.Shell)
		{
			int cardCountInStack2 = WorldManager.instance.GetCardCountInStack(card, (CardData x) => x.Id == "shell");
			int amountInChest2 = WorldManager.instance.GetAmountInChest(card, "shell");
			if (cardCountInStack2 > 0)
			{
				int num2 = ((cardCountInStack2 > currentCost) ? currentCost : cardCountInStack2);
				WorldManager.instance.RemoveCardsFromStackPred(card, num2, (GameCard x) => x.CardData.Id == "shell");
				this.StoredCostAmount += num2;
			}
			else if (amountInChest2 > 0)
			{
				int num2 = ((amountInChest2 > currentCost) ? currentCost : amountInChest2);
				WorldManager.instance.BuyWithChest(card, num2);
				this.StoredCostAmount += num2;
			}
		}
		else
		{
			List<Dollar> list = (from x in card.GetAllCardsInStack()
				where x.CardData is Dollar
				select x.CardData as Dollar).ToList<Dollar>();
			int num3 = list.Sum<Dollar>((Dollar x) => x.DollarValue);
			int dollarsInCreditcard = WorldManager.instance.GetDollarsInCreditcard(card);
			if (num3 > 0)
			{
				int num4 = Mathf.Min(currentCost, num3);
				int num5 = num4;
				for (int i = 0; i < this.takeOrder.Length; i++)
				{
					int curBillAmount = this.takeOrder[i];
					int num6 = num5 / curBillAmount;
					num6 = Mathf.Min(list.Count<Dollar>((Dollar x) => x != null && x.DollarValue == curBillAmount), num6);
					num5 -= num6 * curBillAmount;
					Func<Dollar, bool> <>9__9;
					for (int j = 0; j < num6; j++)
					{
						IEnumerable<Dollar> enumerable = list;
						Func<Dollar, bool> func;
						if ((func = <>9__9) == null)
						{
							func = (<>9__9 = (Dollar x) => x != null && x.DollarValue == curBillAmount);
						}
						Dollar dollar = enumerable.Where<Dollar>(func).FirstOrDefault<Dollar>();
						list.Remove(dollar);
						dollar.MyGameCard.DestroyCard(false, true);
					}
					if (num5 <= 0)
					{
						break;
					}
				}
				if (num5 > 0 && list.Count > 0)
				{
					Dollar dollar2 = list.OrderBy<Dollar, int>((Dollar x) => x.DollarValue).FirstOrDefault<Dollar>();
					int num7 = dollar2.DollarValue - num5;
					list.Remove(dollar2);
					dollar2.MyGameCard.DestroyCard(false, true);
					num4 = currentCost;
					WorldManager.instance.CreateDollarsFromValue(num7, base.transform.position, true);
				}
				WorldManager.instance.Restack(list.Select<Dollar, GameCard>((Dollar x) => x.MyGameCard).ToList<GameCard>());
				this.StoredCostAmount += num4;
			}
			if (dollarsInCreditcard > 0)
			{
				int num4 = ((dollarsInCreditcard > currentCost) ? currentCost : dollarsInCreditcard);
				WorldManager.instance.BuyWithCreditcard(card, num4);
				this.StoredCostAmount += num4;
			}
		}
		WorldManager.instance.CreateSmoke(base.transform.position);
		if (this.StoredCostAmount == this.GetCost())
		{
			this.CreateBoosterPack(null);
			this.StoredCostAmount = 0;
		}
		base.CardDropped(card);
	}

	private void CreateBoosterPack(GameCard card = null)
	{
		QuestManager.instance.SpecialActionComplete("buy_" + this.BoosterId + "_pack", null);
		WorldManager.instance.BoughtBoosterIds.Add(this.BoosterId);
		Boosterpack boosterpack = WorldManager.instance.CreateBoosterpack(base.transform.position, this.BoosterId);
		boosterpack.transform.position = (boosterpack.TargetPosition = this.SpawnTarget.position);
		if (card != null)
		{
			Vector3 vector = new Vector3(0.4f, 0f, 0f);
			boosterpack.transform.position = (boosterpack.TargetPosition = this.SpawnTarget.position + vector);
			card.transform.position = (card.TargetPosition = this.SpawnTarget.position - vector);
		}
		this.UpdateUndiscoveredCards();
	}

	protected override void Update()
	{
		if (!this.MyBoard.IsCurrent)
		{
			return;
		}
		if (this.Booster.IsUnlocked)
		{
			base.gameObject.name = this.Booster.Name;
			if (this.BoardCurrency == BoardCurrency.Gold)
			{
				this.BuyText.text = string.Format("{0}{1}", this.GetCost() - this.StoredCostAmount, Icons.Gold);
			}
			else if (this.BoardCurrency == BoardCurrency.Shell)
			{
				this.BuyText.text = string.Format("{0}{1}", this.GetCost() - this.StoredCostAmount, Icons.Shell);
			}
			else if (this.BoardCurrency == BoardCurrency.Dollar)
			{
				this.BuyText.text = string.Format("{0}{1}", this.GetCost() - this.StoredCostAmount, Icons.Dollar);
			}
			this.NameText.text = this.Booster.Name;
			this.NewLabel.gameObject.SetActive(!WorldManager.instance.CurrentSave.FoundBoosterIds.Contains(this.BoosterId));
		}
		else
		{
			base.gameObject.name = "???";
			this.NameText.text = "???";
			this.BuyText.text = "";
			this.NewLabel.gameObject.SetActive(false);
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

	public void UpdateUndiscoveredCards()
	{
		if (this.Booster.IsUnlocked && this.Booster.UndiscoveredCardCount >= 1 && WorldManager.instance.CurrentSave.FoundBoosterIds.Contains(this.BoosterId))
		{
			this.IdeaIcon.SetActive(true);
			return;
		}
		this.IdeaIcon.SetActive(false);
	}

	public override string GetTooltipText()
	{
		if (this.Booster.IsUnlocked)
		{
			string text = Icons.Gold;
			if (this.BoardCurrency == BoardCurrency.Shell)
			{
				text = Icons.Shell;
			}
			else if (this.BoardCurrency == BoardCurrency.Dollar)
			{
				text = Icons.Dollar;
			}
			return SokLoc.Translate("label_drag_coins_to_buy_pack", new LocParam[]
			{
				LocParam.Create("goldicon", text),
				LocParam.Create("cost", this.GetCost().ToString())
			}) + "\n\n" + this.Booster.GetSummary();
		}
		string text2 = "label_complete_more_quests_for_pack";
		if (this.Booster.BoosterLocation == Location.Island)
		{
			text2 = "label_complete_more_island_quests_for_pack";
		}
		return SokLoc.Translate(text2, new LocParam[] { LocParam.Plural("remaining", this.Booster.RemainingAchievementCountToUnlock) });
	}

	public int Cost;

	public int StoredCostAmount;

	public string BoosterId;

	public Transform SpawnTarget;

	public TextMeshPro BuyText;

	public TextMeshPro NameText;

	public GameObject NewLabel;

	public Rectangle HighlightRectangle;

	public GameObject IdeaIcon;

	public BoardCurrency BoardCurrency;

	private int[] takeOrder = new int[] { 10, 20, 50, 100 };
}
