using System;

public class Dollar : Resource, ICurrency
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
			return this.DollarValue;
		}
		set
		{
			this.DollarValue = value;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Dollar || otherCard is Worker || otherCard is Resource;
	}

	public override void UpdateCard()
	{
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		this.nameOverride = SokLoc.Translate(this.NameTerm, new LocParam[] { LocParam.Create("icon", Icons.Dollar) });
		this.descriptionOverride = SokLoc.Translate(this.DescriptionTerm, new LocParam[] { LocParam.Create("icon", Icons.Dollar) });
	}

	public void UseCurrency(int currencyAmount, bool spawnSmoke = false)
	{
		if (spawnSmoke)
		{
			WorldManager.instance.CreateSmoke(base.Position);
		}
		this.MyGameCard.DestroyCard(false, true);
	}

	public int DollarValue;
}
