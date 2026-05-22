using System;
using System.Collections.Generic;
using System.Linq;

public class BlueprintGrowth : Blueprint
{
	public override void Init(GameDataLoader loader)
	{
		this.growables.Clear();
		this.growables.Add(new BlueprintGrowth.Growable("berry", "idea_growth_status_0", "berrybush", 1, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("apple", "idea_growth_status_1", "apple_tree", 1, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("carrot", "idea_growth_status_2", "carrot", 2, 180f));
		this.growables.Add(new BlueprintGrowth.Growable("onion", "idea_growth_status_3", "onion", 2, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("potato", "idea_growth_status_4", "potato", 2, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("mushroom", "idea_growth_status_5", "mushroom", 2, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("banana", "idea_growth_status_6", "banana_tree", 1, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("cotton", "idea_growth_status_8", "cotton_plant", 1, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("lime", "idea_growth_name_status_9", "lime", 2, 180f));
		this.growables.Add(new BlueprintGrowth.Growable("chili_pepper", "idea_growth_name_status_10", "chili_pepper", 2, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("seaweed", "idea_growth_status_11", "seaweed", 2, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("sugar", "idea_growth_status_12", "sugar_cane", 1, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("wheat", "idea_growth_status_13", "wheat", 3, 120f)
		{
			ResultAction = "special:grow_wheat"
		});
		this.growables.Add(new BlueprintGrowth.Growable("grape", "idea_growth_status_14", "grape_vine", 1, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("olive", "idea_growth_status_15", "olive_tree", 1, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("tomato", "idea_growth_status_16", "tomato_plant", 1, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("stick", "idea_growth_status_17", "tree", 1, 120f));
		this.growables.Add(new BlueprintGrowth.Growable("herbs", "idea_growth_status_18", "herbs", 2, 120f));
		this.PopulateSubprints(loader);
		base.Init(loader);
	}

	private void PopulateSubprints(GameDataLoader loader)
	{
		this.Subprints.Clear();
		for (int i = 0; i < this.growables.Count; i++)
		{
			BlueprintGrowth.Growable growable = this.growables[i];
			if (!(loader.GetCardFromId(growable.ToGrow, false) == null) && !(loader.GetCardFromId(growable.ResultItem, false) == null))
			{
				for (int j = 0; j < this.growMethods.Length; j++)
				{
					string text = this.growMethods[j];
					List<string> list = new List<string>();
					for (int k = 0; k < growable.ResultCount; k++)
					{
						list.Add(growable.ResultItem);
					}
					this.Subprints.Add(new Subprint
					{
						RequiredCards = new string[] { growable.ToGrow, text },
						ExtraResultCards = list.ToArray(),
						StatusTerm = growable.StatusTerm,
						Time = growable.GrowSpeed * this.growSpeedMultiplier[j],
						ResultAction = growable.ResultAction
					});
				}
			}
		}
	}

	public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
	{
		base.BlueprintComplete(rootCard, involvedCards, print);
		if (rootCard.CardData is HeavyFoundation && rootCard.Child != null)
		{
			rootCard = rootCard.Child;
		}
		if (rootCard.CardData.Id == "greenhouse")
		{
			CardData cardData = this.allResultCards.FirstOrDefault<CardData>((CardData c) => this.growables.Any<BlueprintGrowth.Growable>((BlueprintGrowth.Growable x) => x.ToGrow == c.Id));
			if (cardData != null)
			{
				cardData.MyGameCard.BounceTarget = null;
				cardData.MyGameCard.Velocity = null;
				cardData.MyGameCard.SetParent(rootCard);
				this.allResultCards.Remove(cardData);
				WorldManager.instance.Restack(this.allResultCards.Select<CardData, GameCard>((CardData x) => x.MyGameCard).ToList<GameCard>());
				WorldManager.instance.StackSendCheckTarget(rootCard, this.allResultCards[0].MyGameCard, rootCard.CardData.OutputDir, rootCard, true, -1);
			}
		}
	}

	private List<BlueprintGrowth.Growable> growables = new List<BlueprintGrowth.Growable>();

	private string[] growMethods = new string[] { "soil", "poop", "garden", "farm", "greenhouse" };

	private float[] growSpeedMultiplier = new float[] { 1f, 1f, 0.75f, 0.5f, 0.5f };

	public class Growable
	{
		public Growable(string toGrow, string statusTerm, string resultItem, int resultCount, float growSpeed)
		{
			this.ToGrow = toGrow;
			this.StatusTerm = statusTerm;
			this.ResultItem = resultItem;
			this.ResultCount = resultCount;
			this.GrowSpeed = growSpeed;
		}

		public string ToGrow;

		public string StatusTerm;

		public string ResultItem;

		public int ResultCount;

		public float GrowSpeed;

		public string ResultAction = "";
	}
}
