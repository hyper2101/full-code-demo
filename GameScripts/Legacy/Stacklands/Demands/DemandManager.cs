using Mewtations.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Mewtations.Core.LegacySystem(Mewtations.Core.LegacyCategory.DeprecatedEconomyLoop)]
    public class DemandManager : MonoBehaviour
{
	private void Awake()
	{
		DemandManager.instance = this;
		this.AllDemands = WorldManager.instance.GameDataLoader.Demands;
		if (this.AllDemands.Count < 1)
		{
			Debug.LogError("No Demands were loaded");
		}
	}

	public Demand GetCurrentDemand()
	{
		DemandEvent activeDemand = WorldManager.instance.CurrentRunVariables.ActiveDemand;
		if (activeDemand == null)
		{
			return null;
		}
		return activeDemand.Demand;
	}

	public IEnumerator CheckDemands(int month)
	{
		if (this.CanReceiveDemand)
		{
			DemandEvent activeDemand = WorldManager.instance.CurrentRunVariables.ActiveDemand;
			Demand currentDemand = this.GetCurrentDemand();
			if (currentDemand != null)
			{
				if (currentDemand.IsFinalDemand && WorldManager.instance.CardQuery.GetCard<DragonEgg>())
				{
					yield return GreedCutscenes.FinalDemandEndSuccess(false);
				}
				else if (activeDemand.MonthCompleted == month)
				{
					yield return this.FinishDemand(activeDemand);
				}
			}
			else
			{
				Demand demandToStart = this.GetDemandToStart(month);
				if (demandToStart != null)
				{
					yield return this.StartDemand(demandToStart);
				}
			}
		}
		yield break;
	}

	public string GetRandomStartDescription(Demand demand)
	{
		return MewtationsLoc.Translate(demand.GetStartTerm(), new LocParam[]
		{
			LocParam.Create("cardsToGet", string.Format("{0} x {1}", demand.Amount, WorldManager.instance.GameDataLoader.GetCardFromId(demand.CardToGet, true).Name)),
			LocParam.Create("month", demand.Duration.ToString()),
			LocParam.Create("monthFinished", (WorldManager.instance.Time.CurrentMonth + demand.Duration - 1).ToString())
		});
	}

	public string GetDemandStartDescription(Demand demand, DemandEvent demandEvent = null)
	{
		return MewtationsLoc.Translate("label_" + demand.DemandId + "_text", new LocParam[]
		{
			LocParam.Create("amount", demand.Amount.ToString()),
			LocParam.Create("monthFinished", (((demandEvent == null) ? WorldManager.instance.Time.CurrentMonth : demandEvent.MonthStarted) + demand.Duration - 1).ToString())
		});
	}

	public string GetRandomSuccessDescription(Demand demand)
	{
		return MewtationsLoc.Translate(demand.GetSuccessTerm(), new LocParam[]
		{
			LocParam.Create("cardsToGet", string.Format("{0} x {1}", demand.Amount, WorldManager.instance.GameDataLoader.GetCardFromId(demand.CardToGet, true).Name)),
			LocParam.Create("month", demand.Duration.ToString())
		});
	}

	public string GetRandomFailedDescription(Demand demand)
	{
		return MewtationsLoc.Translate(demand.GetFailedTerm(), new LocParam[]
		{
			LocParam.Create("cardsToGet", string.Format("{0} x {1}", demand.Amount, WorldManager.instance.GameDataLoader.GetCardFromId(demand.CardToGet, true).Name)),
			LocParam.Create("month", demand.Duration.ToString())
		});
	}

	public Demand GetDemandToStart(int month)
	{
		int num = month - WorldManager.instance.CurrentRunVariables.LastDemandMonth;
		if (WorldManager.instance.CurrentRunVariables.PreviousDemandEvents.Count == 0)
		{
			return this.GetPossibleDemands().FirstOrDefault<Demand>();
		}
		if (num == 1)
		{
			return this.GetPossibleDemands().FirstOrDefault<Demand>();
		}
		return null;
	}

	public List<Demand> GetPossibleDemands()
	{
		return this.AllDemands.Where<Demand>((Demand x) => WorldManager.instance.CurrentRunVariables.PreviousDemandEvents.FindIndex((DemandEvent e) => e.DemandId == x.DemandId) == -1).OrderBy<Demand, int>(delegate(Demand demand)
		{
			if (demand.IsFinalDemand)
			{
				return 5;
			}
			if (demand.Difficulty == DemandDifficulty.easy)
			{
				return 1;
			}
			if (demand.Difficulty == DemandDifficulty.medium)
			{
				return 2;
			}
			if (demand.Difficulty == DemandDifficulty.hard)
			{
				return 3;
			}
			return 4;
		}).ToList<Demand>();
	}

	public IEnumerator StartDemand(Demand demand)
	{
		AudioManager.me.PlaySound2D(this.StartDemandSound, 0.9f, 0.3f);
		QuestManager.instance.SpecialActionComplete("demand_start", null);
		if (demand.IsFinalDemand)
		{
			yield return GreedCutscenes.FinalDemandStart(this.AllDemands.Find((Demand x) => x.IsFinalDemand));
		}
		else
		{
			yield return GreedCutscenes.StartDemand(demand);
		}
		if (WorldManager.instance.CurrentRunVariables.PreviousDemandEvents.Count == 2)
		{
			yield return GreedCutscenes.NewVillager();
		}
		if (WorldManager.instance.CurrentRunVariables.PreviousDemandEvents.Count == 5)
		{
			WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("greed_middle");
		}
		if (WorldManager.instance.CurrentRunVariables.PreviousDemandEvents.Count == 9)
		{
			WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("greed_end");
		}
		yield break;
	}

	public void QuestStarted(Demand demand)
	{
		WorldManager.instance.CurrentRunVariables.ActiveDemand = new DemandEvent(demand.DemandId, WorldManager.instance.Time.CurrentMonth, demand.Duration, WorldManager.instance.CurrentBoard.Id);
	}

	public IEnumerator FinishDemand(DemandEvent demandEvent)
	{
		WorldManager.instance.CutsceneTitle = "";
		WorldManager.instance.CutsceneText = "";
		Demand demandById = this.GetDemandById(demandEvent.DemandId);
		demandEvent.Completed = true;
		AudioManager.me.PlaySound2D(this.FinishDemandSound, 0.9f, 0.3f);
		yield return WorldManager.instance.FinishDemand(demandById, demandEvent);
		yield break;
	}

	public void DemandFinishedSuccess(Demand demand)
	{
		foreach (CardAmountPair cardAmountPair in demand.SuccessCards)
		{
			for (int i = 0; i < cardAmountPair.Amount; i++)
			{
				WorldManager.instance.CreateCard(base.transform.position, cardAmountPair.CardId, false, false, true).MyGameCard.SendIt();
			}
		}
	}

	public List<Combatable> SpawnEnemies()
	{
		float num = (float)(this.GetTimesDemandFailed() * 20);
		Combatable combatable = WorldManager.instance.GetCardPrefab("royal_guard", true) as Combatable;
		Combatable combatable2 = WorldManager.instance.GetCardPrefab("royal_archer", true) as Combatable;
		Combatable combatable3 = WorldManager.instance.GetCardPrefab("royal_mage", true) as Combatable;
		List<CardIdWithEquipment> enemiesToSpawn = SpawnHelper.GetEnemiesToSpawn(new List<Combatable> { combatable, combatable2, combatable3 }, num);
		List<Combatable> list = new List<Combatable>();
		foreach (CardIdWithEquipment cardIdWithEquipment in enemiesToSpawn)
		{
			Vector3 vector = ((list.Count == 0) ? WorldManager.instance.GetRandomSpawnPosition() : list[0].Position);
			Combatable combatable4 = WorldManager.instance.CreateCard(vector, cardIdWithEquipment, false, false, true) as Combatable;
			WorldManager.instance.CreateSmoke(combatable4.Position);
			combatable4.HealthPoints = combatable4.ProcessedCombatStats.MaxHealth;
			combatable4.MyGameCard.SendIt();
			list.Add(combatable4);
		}
		return list;
	}

	public Demand GetDemandById(string demandId)
	{
		return this.AllDemands.Find((Demand x) => x.DemandId == demandId);
	}

	public int GetTimesDemandFailed()
	{
		return Mathf.Max(1, WorldManager.instance.CurrentRunVariables.PreviousDemandEvents.Count<DemandEvent>((DemandEvent x) => !x.Successful));
	}

	public void ResetDemands()
	{
		WorldManager.instance.CurrentRunVariables.PreviousDemandEvents.Clear();
		WorldManager.instance.CurrentRunVariables.ActiveDemand = null;
		this.CanReceiveDemand = true;
	}

	public static DemandManager instance;

	public bool CanReceiveDemand = true;

	public List<AudioClip> StartDemandSound;

	public List<AudioClip> FinishDemandSound;

	public List<AudioClip> FailedDemandSound;

	[HideInInspector]
	public List<Demand> AllDemands = new List<Demand>();

	public List<string> StartDemandLocTerms;

	public List<string> FailedDemandLocTerms;

	public List<string> SuccessDemandLocTerms;
}

