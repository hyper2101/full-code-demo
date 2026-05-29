using Mewtations.Core;
using System;

[Mewtations.Core.LegacySystem(Mewtations.Core.LegacyCategory.DeprecatedNarrativeFlow)]
    public class Quest
{
	public string DescriptionTerm
	{
		get
		{
			return "quest_" + this.Id + "_text";
		}
	}

	public string Description
	{
		get
		{
			if (this.DescriptionTermOverride == null)
			{
				return MewtationsLoc.Translate(this.DescriptionTerm);
			}
			if (this.RequiredCount != -1)
			{
				return MewtationsLoc.Translate(this.DescriptionTermOverride, new LocParam[] { LocParam.Create("count", this.RequiredCount.ToString()) });
			}
			return MewtationsLoc.Translate(this.DescriptionTermOverride);
		}
	}

	public Location QuestLocation
	{
		get
		{
			if (this._questLocation == null)
			{
				if (this.QuestGroup.ToString().StartsWith("Island"))
				{
					this._questLocation = new Location?(Location.Island);
				}
				else if (this.QuestGroup.ToString().StartsWith("Forest"))
				{
					this._questLocation = new Location?(Location.Forest);
				}
				else if (this.QuestGroup.ToString().StartsWith("Death"))
				{
					this._questLocation = new Location?(Location.Death);
				}
				else if (this.QuestGroup.ToString().StartsWith("Greed"))
				{
					this._questLocation = new Location?(Location.Greed);
				}
				else if (this.QuestGroup.ToString().StartsWith("Happiness"))
				{
					this._questLocation = new Location?(Location.Happiness);
				}
				else if (this.QuestGroup.ToString().StartsWith("Cities"))
				{
					this._questLocation = new Location?(Location.Cities);
				}
				else
				{
					this._questLocation = new Location?(Location.Mainland);
				}
			}
			return this._questLocation.Value;
		}
	}

	public bool IsMainQuest
	{
		get
		{
			return this.QuestGroup == QuestGroup.Starter || this.QuestGroup == QuestGroup.MainQuest || this.QuestGroup == QuestGroup.Island_Beginnings || this.QuestGroup == QuestGroup.Island_MainQuest || this.QuestGroup == QuestGroup.Forest_MainQuest || this.QuestGroup == QuestGroup.Death_MainQuest || this.QuestGroup == QuestGroup.Happiness_MainQuest || this.QuestGroup == QuestGroup.Greed_MainQuest || this.QuestGroup == QuestGroup.Happiness_Starter || this.QuestGroup == QuestGroup.Death_Starter || this.QuestGroup == QuestGroup.Death_MainQuest || this.QuestGroup == QuestGroup.Discover_Spirits || this.QuestGroup == QuestGroup.Greed_Starter || this.QuestGroup == QuestGroup.Cities_MainQuest || this.QuestGroup == QuestGroup.Cities_Starter;
		}
	}

	public Quest(string id)
	{
		this.Id = id;
	}

	private Location? _questLocation;

	public string DescriptionTermOverride;

	public string Id;

	public int RequiredCount = -1;

	public bool DefaultVisible;

	public bool ShowCompleteAnimation = true;

	public Func<CardData, string, bool> OnActionComplete;

	public Func<CardData, bool> OnCardCreate;

	public Func<string, bool> OnSpecialAction;

	public bool IsSteamAchievement;

	public bool PossibleInPeacefulMode = true;

	public QuestGroup QuestGroup = QuestGroup.Other;
}

