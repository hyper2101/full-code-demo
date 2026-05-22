using System;

public class Energy : Resource, IEnergy
{
	public int EnergyAmount
	{
		get
		{
			return 1;
		}
	}

	public CardData GetCardData()
	{
		return this;
	}

	public override void OnInitialCreate()
	{
		Battery battery = (Battery)WorldManager.instance.GetNearestCardMatchingPred(this.MyGameCard, (GameCard x) => x.CardData is Battery);
		if (battery != null && battery.StoredEnergy < battery.EnergyCapacity)
		{
			WorldManager.instance.StackSendTo(this.MyGameCard, battery.MyGameCard);
		}
		base.OnInitialCreate();
	}

	public void UseEnergy(int energyAmount)
	{
		WorldManager.instance.CreateMinusElectricity(base.Position);
		this.MyGameCard.DestroyCard(false, true);
	}
}
