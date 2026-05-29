using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Creditcard : CardData, ICurrency
{
	public CardData Card
	{
		get
		{
			return this;
		}
	}

	public int CurrencyValue
	{
		get
		{
			return this.DollarCount;
		}
		set
		{
			this.DollarCount = value;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Dollar || otherCard.Id == this.Id;
	}

	public override void UpdateCard()
	{
		if (this.IsDamaged)
		{
			base.UpdateCard();
			return;
		}
		List<Dollar> list = (from x in this.MyGameCard.GetChildCards()
			where x.CardData is Dollar
			select x.CardData as Dollar).ToList<Dollar>();
		for (int i = 0; i < list.Count; i++)
		{
			GameCard myGameCard = list[i].MyGameCard;
			Dollar dollar = myGameCard.CardData as Dollar;
			if (dollar != null)
			{
				Creditcard creditcardWithSpace = this.GetCreditcardWithSpace();
				if (creditcardWithSpace != null)
				{
					int num = creditcardWithSpace.MaxDollarCount - creditcardWithSpace.DollarCount;
					if (num > 0)
					{
						if (dollar.DollarValue > num)
						{
							int num2 = dollar.DollarValue - num;
							creditcardWithSpace.DollarCount = creditcardWithSpace.MaxDollarCount;
							myGameCard.DestroyCard(false, true);
							list.AddRange(from x in WorldManager.instance.CreateDollarsFromValue(num2, base.Position, true)
								select x.CardData as Dollar);
						}
						else
						{
							creditcardWithSpace.DollarCount += dollar.DollarValue;
							myGameCard.DestroyCard(false, true);
						}
						if (myGameCard.CardData == list.Last<Dollar>())
						{
							WorldManager.instance.CreateSmoke(base.Position);
						}
					}
				}
				else
				{
					myGameCard.RemoveFromParent();
				}
			}
			WorldManager.instance.Restack(list.Select<Dollar, GameCard>((Dollar x) => x.MyGameCard).ToList<GameCard>());
		}
		this.CitiesValue = this.DollarCount;
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		GameCard myGameCard = this.MyGameCard;
		if (myGameCard != null && myGameCard.CardConnectorChildren.Count > 0 && this.MyGameCard.IsHovered)
		{
			this.descriptionOverride = MewtationsLoc.Translate(this.BankDescriptionTerm, new LocParam[]
			{
				LocParam.Create("count", this.DollarCount.ToString()),
				LocParam.Create("max_count", this.MaxDollarCount.ToString()),
				LocParam.Create("icon", Icons.Dollar)
			});
			this.descriptionOverride = this.descriptionOverride + "\n\n<i>" + base.GetConnectorInfoString(this.MyGameCard) + "</i>";
		}
	}

	private Creditcard GetCreditcardWithSpace()
	{
		GameCard gameCard = this.MyGameCard.GetAllCardsInStack().FirstOrDefault<GameCard>(delegate(GameCard x)
		{
			Creditcard creditcard = x.CardData as Creditcard;
			return creditcard != null && creditcard.DollarCount < creditcard.MaxDollarCount;
		});
		if (gameCard == null)
		{
			return null;
		}
		return gameCard.CardData as Creditcard;
	}

	public override void Clicked()
	{
		if (this.DollarCount > 0)
		{
			int num = Mathf.Min(this.DollarCount, 100);
			WorldManager.instance.CreateDollarsFromValue(num, base.Position, false);
			this.DollarCount -= num;
			WorldManager.instance.CreateSmoke(base.Position);
		}
	}

	public void UseCurrency(int currencyAmount, bool spawnSmoke = false)
	{
		if (spawnSmoke)
		{
			WorldManager.instance.CreateSmoke(base.Position);
		}
		this.DollarCount -= currencyAmount;
	}

	[ExtraData("dollar_count")]
	[HideInInspector]
	public int DollarCount;

	public int MaxDollarCount = 1000;

	public string BankDescriptionTerm;
}
