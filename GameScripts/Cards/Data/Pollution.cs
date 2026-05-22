using System;
using System.Linq;

public class Pollution : CardData
{
	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild)
		{
			foreach (GameCard gameCard in this.MyGameCard.GetChildCards())
			{
				Pollution pollution = gameCard.CardData as Pollution;
				if (pollution != null)
				{
					this.PollutionAmount += pollution.PollutionAmount;
					gameCard.RemoveFromStack();
					gameCard.DestroyCard(true, true);
				}
			}
		}
		this.MyGameCard.SpecialIcon.sprite = SpriteManager.instance.PollutionIcon;
		this.MyGameCard.SpecialValue = new int?(this.PollutionAmount);
		base.UpdateCard();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == this.Id;
	}

	public override void OnInitialCreate()
	{
		AudioManager.me.PlaySound(AudioManager.me.SpawnPollution, base.transform, 1f, 0.25f);
		RecyclingCenter recyclingCenter = (from x in WorldManager.instance.CardQuery.GetCards<RecyclingCenter>()
			where !x.IsOverflowing && x.HasEnergyInput(null) && x.HasSewerConnected()
			orderby x.StoredPollution
			select x).FirstOrDefault<RecyclingCenter>();
		Landfill landfill = (from x in WorldManager.instance.CardQuery.GetCards<Landfill>()
			where !x.IsOverflowing && x.HasSewerConnected()
			orderby x.StoredPollution
			select x).FirstOrDefault<Landfill>();
		if (recyclingCenter != null && landfill != null)
		{
			if (recyclingCenter.StoredPollution <= landfill.StoredPollution)
			{
				WorldManager.instance.StackSendTo(this.MyGameCard, recyclingCenter.MyGameCard);
				return;
			}
			WorldManager.instance.StackSendTo(this.MyGameCard, landfill.MyGameCard);
			return;
		}
		else
		{
			if (recyclingCenter != null)
			{
				WorldManager.instance.StackSendTo(this.MyGameCard, recyclingCenter.MyGameCard);
				return;
			}
			if (landfill != null)
			{
				WorldManager.instance.StackSendTo(this.MyGameCard, landfill.MyGameCard);
				return;
			}
			Pollution pollution = (from x in WorldManager.instance.CardQuery.GetCards<Pollution>()
				where x != this && x.MyGameCard.BounceTarget == null
				select x).FirstOrDefault<Pollution>();
			if (pollution != null)
			{
				WorldManager.instance.StackSendTo(this.MyGameCard, pollution.MyGameCard);
			}
			return;
		}
	}

	public int PollutionEventAmount = 50;

	[ExtraData("pollution_amount")]
	public int PollutionAmount = 1;
}
