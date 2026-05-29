using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Blueprint : CardData, IKnowledge
{
	public string KnowledgeName
	{
		get
		{
			return MewtationsLoc.Translate(this.NameTerm);
		}
	}

	public string KnowledgeText
	{
		get
		{
			return this.GetText();
		}
	}

	public string CardId
	{
		get
		{
			return this.Id;
		}
	}

	public virtual bool CanCurrentlyBeMade
	{
		get
		{
			if (this.Subprints != null && this.Subprints.Count > 0)
			{
				string result = this.Subprints[0].ResultCard;
				if (!string.IsNullOrEmpty(result))
				{
					string lower = result.ToLower();
					if (lower.Contains("talisman"))
					{
						return ChronicleManager.IsHintUnlocked("item_secret_lore_hint_1");
					}
					if (lower == "item_breakthrough_pill")
					{
						return ChronicleManager.IsHintUnlocked("item_secret_lore_hint_2");
					}
				}
			}
			return true;
		}
	}

	public BlueprintGroup Group
	{
		get
		{
			return this.BlueprintGroup;
		}
	}

	public bool IsIslandKnowledge
	{
		get
		{
			return this.BlueprintGroup == BlueprintGroup.Island || this.BlueprintGroup == BlueprintGroup.Sailing;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Blueprint || otherCard is Rumor;
	}

	public virtual void Init(GameDataLoader loader)
	{
		for (int i = 0; i < this.Subprints.Count; i++)
		{
			Subprint subprint = this.Subprints[i];
			subprint.ParentBlueprint = this;
			subprint.SubprintIndex = i;
		}
	}

	public override void UpdateCard()
	{
		this.descriptionOverride = this.GetTooltipText();
		base.UpdateCard();
	}

	public override void OnInitialCreate()
	{
		base.OnInitialCreate();
	}

	protected override string GetTooltipText()
	{
		return this.GetText();
	}

	public string GetText()
	{
		string text = this.Subprints[0].DefaultText();
		if (this.OverrideResultDescription)
		{
			string text2 = MewtationsLoc.Translate(this.ResultDescriptionTerm);
			text = text + "\n\n\"" + text2 + "\"";
		}
		else
		{
			string text3 = this.Subprints[0].ResultCard;
			if (string.IsNullOrEmpty(text3) && this.Subprints[0].ExtraResultCards.Length != 0)
			{
				text3 = this.Subprints[0].ExtraResultCards[0];
			}
			CardData cardPrefab = WorldManager.instance.GetCardPrefab(text3, true);
			if (cardPrefab == null)
			{
				Debug.LogWarning("No result card set for " + this.Id);
				return text;
			}
			cardPrefab.UpdateCardText();
			if (string.IsNullOrEmpty(text3))
			{
				return null;
			}
			Equipable equipable = cardPrefab as Equipable;
			if (equipable != null)
			{
				text = string.Concat(new string[]
				{
					text,
					"\n\n\"",
					cardPrefab.Description,
					"\"\n\n<i>",
					equipable.GetEquipableCombatLevel(),
					"</i>"
				});
			}
			else
			{
				text = text + "\n\n\"" + cardPrefab.Description + "\"";
			}
			if (this.Subprints[0].ResultWellbeing > 0)
			{
				text = text + "\n\n" + MewtationsLoc.Translate("label_blueprint_wellbeing_generation", new LocParam[]
				{
					LocParam.Create("amount", this.Subprints[0].ResultWellbeing.ToString()),
					LocParam.Create("icon", Icons.Wellbeing)
				});
			}
			if (this.Subprints[0].ResultPolution > 0)
			{
				text = text + "\n\n" + MewtationsLoc.Translate("label_blueprint_pollution_generation", new LocParam[]
				{
					LocParam.Create("amount", this.Subprints[0].ResultPolution.ToString()),
					LocParam.Create("icon", Icons.Pollution)
				});
			}
		}

		if (!this.CanCurrentlyBeMade)
		{
			string lockedMsg = MewtationsLoc.CurrentLang == MewtationsLoc.Language.Vietnamese
				? "\n\n<color=red><b>✗ KHÓA: Công thức này đang bị phong ấn. Hãy tìm Cổ Bản Kí Sự tương ứng trong Viễn Chinh để giải mã!</b></color>"
				: "\n\n<color=red><b>✗ LOCKED: This recipe is sealed. Uncover the corresponding Secret Lore Hint in Expedition to decode it!</b></color>";
			text += lockedMsg;
		}

		return text;
	}

	public virtual Subprint GetMatchingSubprint(GameCard card, out SubprintMatchInfo matchInfo)
	{
		matchInfo = default(SubprintMatchInfo);
		foreach (Subprint subprint in this.Subprints)
		{
			if (subprint.StackMatchesSubprint(card, out matchInfo))
			{
				return subprint;
			}
		}
		return null;
	}

	public virtual void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
	{
		List<GameCard> list = new List<GameCard>(involvedCards);
		List<string> allCardsToRemove = print.GetAllCardsToRemove();
		CardData cardData = null;
		List<CardData> list2 = new List<CardData>();
		for (int i = 0; i < allCardsToRemove.Count; i++)
		{
			string[] possibleRemovables = allCardsToRemove[i].Split('|', StringSplitOptions.None);
			GameCard gameCard = list.FirstOrDefault<GameCard>((GameCard x) => possibleRemovables.Contains(x.CardData.Id));
			if (gameCard != null)
			{
				gameCard.DestroyCard(true, true);
				list.Remove(gameCard);
			}
		}
		this.allResultCards.Clear();
		Vector3 vector = ((rootCard != null) ? rootCard.CardData.OutputDir : Vector3.zero);
		if (!string.IsNullOrEmpty(print.ResultCard))
		{
			cardData = WorldManager.instance.CreateCard(rootCard.transform.position, print.ResultCard, false, false, true);
			this.allResultCards.Add(cardData);
		}
		if (!string.IsNullOrEmpty(print.ResultAction))
		{
			GameCard gameCard2 = involvedCards.FirstOrDefault<GameCard>((GameCard x) => x.CardData is Combatable);
			if (gameCard2 != null)
			{
				gameCard2.CardData.ParseAction(print.ResultAction);
			}
			else
			{
				rootCard.CardData.ParseAction(print.ResultAction);
			}
		}
		if (print.ExtraResultCards != null)
		{
			for (int j = 0; j < print.ExtraResultCards.Length; j++)
			{
				CardData cardData2 = WorldManager.instance.CreateCard(rootCard.transform.position, print.ExtraResultCards[j], false, false, true);
				list2.Add(cardData2);
				this.allResultCards.Add(cardData2);
			}
		}
		GameCard gameCard3 = involvedCards.FirstOrDefault<GameCard>((GameCard x) => x.CardData.HasOutputConnector());
		if (this.CombineResultCards)
		{
			WorldManager.instance.Restack(this.allResultCards.Select<CardData, GameCard>((CardData x) => x.MyGameCard).ToList<GameCard>());
			if (gameCard3 != null)
			{
				WorldManager.instance.StackSendCheckTarget(gameCard3, this.allResultCards[0].MyGameCard, vector, gameCard3, true, -1);
			}
			else
			{
				WorldManager.instance.StackSend(this.allResultCards[0].MyGameCard, vector, null, true);
			}
		}
		else
		{
			if (cardData != null)
			{
				if (gameCard3 != null)
				{
					WorldManager.instance.StackSendCheckTarget(gameCard3, cardData.MyGameCard, vector, gameCard3, true, -1);
				}
				else
				{
					WorldManager.instance.StackSend(cardData.MyGameCard, vector, null, true);
				}
			}
			if (list2.Count > 0)
			{
				WorldManager.instance.Restack(list2.Select<CardData, GameCard>((CardData x) => x.MyGameCard).ToList<GameCard>());
				if (gameCard3 != null)
				{
					WorldManager.instance.StackSendCheckTarget(gameCard3, list2[0].MyGameCard, vector, gameCard3, true, -1);
				}
				else
				{
					WorldManager.instance.StackSend(list2[0].MyGameCard, vector, null, true);
				}
			}
		}
		if (print.ResultPolution > 0)
		{
			(WorldManager.instance.CreateCard(rootCard.transform.position, "pollution", true, false, true) as Pollution).PollutionAmount = print.ResultPolution;
		}
		if (print.ResultWellbeing != 0)
		{
			CitiesManager.instance.AddWellbeing(print.ResultWellbeing);
			WorldManager.instance.CreateFloatingText(this.allResultCards[0].MyGameCard, print.ResultWellbeing > 0, print.ResultWellbeing, MewtationsLoc.Translate("label_blueprint_wellbeing"), Icons.Wellbeing, true, 0, 0f, true);
		}
		WorldManager.instance.Restack(list);
	}

	[Header("Prints")]
	public List<Subprint> Subprints = new List<Subprint>();

	public BlueprintGroup BlueprintGroup;

	public bool HideFromIdeasTab;

	public bool IsInvention;

	public bool IsLandmark;

	public bool NeedsExactMatch = true;

	public bool OverrideResultDescription;

	public bool HasMaxAmountOnBoard;

	public bool CombineResultCards;

	public int MaxAmountOnBoard = 1;

	public string ResultDescriptionTerm;

	public bool IgnoreEnergyWorkerDemand;

	protected List<CardData> allResultCards = new List<CardData>();
}
