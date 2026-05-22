using System;

public class ActionTimeParams
{
	public ActionTimeParams(BaseVillager villager, string actionId, CardData baseCard)
	{
		this.villager = villager;
		this.actionId = actionId;
		this.baseCard = baseCard;
	}

	public BaseVillager villager;

	public string actionId;

	public CardData baseCard;
}
