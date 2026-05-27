using Mewtations.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Mewtations.Core.LegacySystem(Mewtations.Core.LegacyCategory.DeprecatedNarrativeFlow)]
    public class QuestManager : MonoBehaviour
{
	private void Awake()
	{
		QuestManager.instance = this;
		this.AllQuests = QuestManager.GetAllQuests();
		foreach (Quest quest in this.AllQuests)
		{
			this.idToQuest[quest.Id] = quest;
		}
		this.sortedQuests = this.AllQuests.OrderBy<Quest, QuestGroup>((Quest x) => x.QuestGroup).ToList<Quest>();
		Debug.Log(string.Format("{0} Quests Found!", this.AllQuests.Count));
	}

	private void Start()
	{
		this.UpdateCurrentQuests();
	}

	public void CheckSteamAchievements()
	{
		if (Application.isEditor)
		{
			return;
		}
		foreach (Quest quest in this.AllQuests)
		{
			if (quest.IsSteamAchievement && this.QuestIsComplete(quest))
			{
				AchievementHelper.UnlockAchievement(quest.Id);
			}
		}
	}

	private Quest GetQuestWithId(string id)
	{
		Quest quest;
		this.idToQuest.TryGetValue(id, out quest);
		return quest;
	}

	public void UpdateCurrentQuests()
	{
		this.CurrentQuests.Clear();
		List<Quest> list = this.AllQuests.OrderBy<Quest, QuestGroup>((Quest x) => x.QuestGroup).ToList<Quest>();
		for (int i = 0; i < list.Count; i++)
		{
			Quest quest = list[i];
			if (!this.QuestIsComplete(quest))
			{
				this.CurrentQuests.Add(quest);
			}
		}
		if (GameScreen.instance != null)
		{
			GameScreen.instance.UpdateQuestLog();
		}
	}

	public void CheckPacksUnlocked()
	{
		if (this.BoosterIsUnlocked(WorldManager.instance.GetBoosterData("structures"), false))
		{
			this.SpecialActionComplete("unlocked_all_packs", null);
		}
		if (this.BoosterIsUnlocked(WorldManager.instance.GetBoosterData("island_locations"), false))
		{
			this.SpecialActionComplete("unlocked_all_island_packs", null);
		}
		BoosterpackData boosterData = WorldManager.instance.GetBoosterData("cities_ideas_2");
		if (boosterData != null && this.BoosterIsUnlocked(boosterData, false))
		{
			this.SpecialActionComplete("unlocked_all_cities_packs", null);
		}
		foreach (BuyBoosterBox buyBoosterBox in WorldManager.instance.AllBoosterBoxes)
		{
			if (buyBoosterBox.Booster.IsUnlocked)
			{
				QuestManager.instance.SpecialActionComplete("unlocked_" + buyBoosterBox.Booster.BoosterId + "_pack", null);
			}
		}
	}

	public bool QuestIsVisible(Quest quest)
	{
		int num = this.sortedQuests.IndexOf(quest);
		return num == 0 || this.sortedQuests[num - 1].QuestGroup != quest.QuestGroup || quest.DefaultVisible || quest.QuestGroup == QuestGroup.Island_Misc || quest.QuestGroup == QuestGroup.Death_Misc || quest.QuestGroup == QuestGroup.Equipment || (quest.QuestGroup == QuestGroup.Discover_Spirits && WorldManager.instance.CurrentRunVariables.FinishedDemon && this.QuestIsComplete(this.sortedQuests.Find((Quest x) => x.QuestGroup == quest.QuestGroup))) || (this.QuestIsComplete(this.sortedQuests[num - 1]) || this.QuestIsComplete(this.sortedQuests[num]));
	}

	public bool BoosterIsUnlocked(BoosterpackData p, bool allowDebug = true)
	{
		return (allowDebug && Application.isEditor && DebugOptions.Default.DebugUnlockBoosters) || this.RemainingQuestCountToComplete(p) <= 0;
	}

	public int RemainingQuestCountToComplete(BoosterpackData p)
	{
		return Mathf.Min(this.AllQuests.Count, p.MinAchievementCount) - this.GetCompletedQuestCount(p.BoosterLocation);
	}

	private List<BoosterpackData> CurrentlyActiveBoosterpacks()
	{
		List<BoosterpackData> boosterPackDatas = WorldManager.instance.BoosterPackDatas;
		this.activeBoosterpackDatas.Clear();
		foreach (BoosterpackData boosterpackData in boosterPackDatas)
		{
			if (boosterpackData.BoosterLocation == WorldManager.instance.CurrentBoard.Location && boosterpackData.MinAchievementCount > 0)
			{
				this.activeBoosterpackDatas.Add(boosterpackData);
			}
		}
		return this.activeBoosterpackDatas;
	}

	public BoosterpackData NextPackUnlock()
	{
		List<BoosterpackData> list = this.CurrentlyActiveBoosterpacks();
		int completedQuestCount = this.GetCompletedQuestCount(WorldManager.instance.CurrentBoard.Location);
		for (int i = 0; i < list.Count; i++)
		{
			if (completedQuestCount < list[i].MinAchievementCount)
			{
				return list[i];
			}
		}
		return null;
	}

	public BoosterpackData JustUnlockedPack()
	{
		List<BoosterpackData> list = this.CurrentlyActiveBoosterpacks();
		int completedQuestCount = this.GetCompletedQuestCount(WorldManager.instance.CurrentBoard.Location);
		for (int i = 0; i < list.Count; i++)
		{
			if (completedQuestCount == list[i].MinAchievementCount)
			{
				return list[i];
			}
		}
		return null;
	}

	public int GetCompletedQuestCount(Location loc)
	{
		return this.GetCompletedQuestCountOnLocation(loc);
	}

	private int GetCompletedQuestCountOnLocation(Location loc)
	{
		List<string> completedAchievementIds = WorldManager.instance.CurrentSave.CompletedAchievementIds;
		int num = 0;
		foreach (string text in completedAchievementIds)
		{
			Quest questWithId = this.GetQuestWithId(text);
			if (questWithId != null && questWithId.QuestLocation == loc)
			{
				num++;
			}
		}
		return num;
	}

	public bool QuestIsComplete(Quest quest)
	{
		return this.QuestIsComplete(quest.Id);
	}

	public bool QuestIsComplete(string id)
	{
		return WorldManager.instance.CurrentSave.CompletedAchievementIds.Contains(id);
	}

	public bool IsInactiveSpiritQuest(Quest quest, Location currentLocation)
	{
		Location questLocation = quest.QuestLocation;
		return (questLocation == Location.Death || questLocation == Location.Greed || questLocation == Location.Happiness) && !(WorldManager.instance.CurrentBoard == null) && quest.QuestLocation != currentLocation;
	}

	public void ActionComplete(CardData card, string action, CardData focusCard = null)
	{
		bool flag = false;
		Location location = WorldManager.instance.CurrentBoard.Location;
		foreach (Quest quest in this.CurrentQuests)
		{
			if (!this.IsInactiveSpiritQuest(quest, location) && quest.OnActionComplete != null && quest.OnActionComplete(card, action))
			{
				if (focusCard == null)
				{
					this.MarkQuestComplete(quest, card);
				}
				else
				{
					this.MarkQuestComplete(quest, focusCard);
				}
				flag = true;
			}
		}
		if (flag)
		{
			this.UpdateCurrentQuests();
		}
	}

	public void SpecialActionComplete(string action, CardData card = null)
	{
		bool flag = false;
		Location location = WorldManager.instance.CurrentBoard.Location;
		foreach (Quest quest in this.CurrentQuests)
		{
			if (!this.IsInactiveSpiritQuest(quest, location) && quest.OnSpecialAction != null && quest.OnSpecialAction(action))
			{
				this.MarkQuestComplete(quest, card);
				flag = true;
			}
		}
		if (flag)
		{
			this.UpdateCurrentQuests();
		}
	}

	public void CardCreated(CardData card)
	{
		bool flag = false;
		Location location = WorldManager.instance.CurrentBoard.Location;
		foreach (Quest quest in this.CurrentQuests)
		{
			if (!this.IsInactiveSpiritQuest(quest, location) && quest.OnCardCreate != null && quest.OnCardCreate(card))
			{
				this.MarkQuestComplete(quest, card);
				flag = true;
			}
		}
		if (flag)
		{
			this.UpdateCurrentQuests();
		}
	}

	public void DebugUnlockAllQuests()
	{
		foreach (Quest quest in QuestManager.GetAllQuests())
		{
			this.MarkQuestComplete(quest, null);
		}
	}

	public bool AnyIslandQuestComplete()
	{
		return this.AllQuests.Any<Quest>((Quest x) => x.QuestLocation == Location.Island && this.QuestIsComplete(x));
	}

	private void MarkQuestComplete(Quest quest, CardData card = null)
	{
		SaveGame currentSave = WorldManager.instance.CurrentSave;
		if (!currentSave.CompletedAchievementIds.Contains(quest.Id))
		{
			currentSave.CompletedAchievementIds.Add(quest.Id);
			WorldManager.instance.QuestsCompleted++;
			WorldManager.instance.QuestCompleted(quest);
			if (quest.IsSteamAchievement)
			{
				AchievementHelper.UnlockAchievement(quest.Id);
			}
		}
	}

	public static List<Quest> GetAllQuests()
	{
		List<Quest> list = new List<Quest>();
		foreach (FieldInfo fieldInfo in typeof(AllQuests).GetFields(BindingFlags.Static | BindingFlags.Public))
		{
			if (fieldInfo.FieldType == typeof(Quest))
			{
				Quest quest = (Quest)fieldInfo.GetValue(null);
				list.Add(quest);
			}
		}
		return list;
	}

	public static QuestManager instance;

	public List<Quest> AllQuests = new List<Quest>();

	public List<Quest> CurrentQuests = new List<Quest>();

	private Dictionary<string, Quest> idToQuest = new Dictionary<string, Quest>();

	private List<Quest> sortedQuests;

	private List<BoosterpackData> activeBoosterpackDatas = new List<BoosterpackData>();
}


