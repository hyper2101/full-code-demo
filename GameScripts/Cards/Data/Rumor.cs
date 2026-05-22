using System;

public class Rumor : CardData, IKnowledge
{
	public BlueprintGroup Group
	{
		get
		{
			return this.KnowledgeGroup;
		}
	}

	public string CardId
	{
		get
		{
			return this.Id;
		}
	}

	public string KnowledgeName
	{
		get
		{
			return base.FullName;
		}
	}

	public string KnowledgeText
	{
		get
		{
			return base.Description;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Blueprint || otherCard is Rumor;
	}

	public bool IsIslandKnowledge
	{
		get
		{
			return this.KnowledgeGroup == BlueprintGroup.Island || this.KnowledgeGroup == BlueprintGroup.Sailing;
		}
	}

	public BlueprintGroup KnowledgeGroup;
}
