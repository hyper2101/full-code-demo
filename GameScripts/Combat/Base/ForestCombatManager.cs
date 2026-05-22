using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForestCombatManager : MonoBehaviour
{
	private void Awake()
	{
		ForestCombatManager.instance = this;
		this.VerifyBlacklistedDrops();
	}

	private void MinimizeUI()
	{
		GameScreen.instance.SetMinimize(true);
		GameScreen.instance.UpdateSidePanelPosition();
	}

	public void ResumeForestCombat()
	{
		if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
		{
			this.LeaveForest();
		}
		if (this.IsWaveOver())
		{
			this.CombatState = ForestCombatState.Cutscene;
			this.LayoutVillagers(true);
			WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.ForestResumeIntro());
		}
		else
		{
			this.CombatState = ForestCombatState.InWave;
		}
		this.MinimizeUI();
	}

	public void InitForestCombat()
	{
		this.CombatState = ForestCombatState.Cutscene;
		this.MinimizeUI();
		this.LayoutVillagers(true);
		WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.ForestIntro());
		QuestManager.instance.SpecialActionComplete("find_dark_forest", null);
	}

	public void PrepareWave()
	{
		WorldManager.instance.CurrentRunVariables.CanDropItem = true;
		int forestWave = WorldManager.instance.CurrentRunVariables.ForestWave;
		Debug.Log(string.Format("Start wave {0} with wicked witch at wave {1}", forestWave, this.WickedWitchWave));
		GameCamera.instance.Screenshake = 0.3f;
		if (forestWave < this.WickedWitchWave)
		{
			this.SpawnWave(forestWave);
		}
		else if (forestWave == this.WickedWitchWave)
		{
			WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "wicked_witch", true, false, true);
			this.SpawnWave(forestWave);
		}
		else
		{
			this.SpawnWave(forestWave);
		}
		ForestCombatManager.StartWaveConflict(forestWave == this.WickedWitchWave);
	}

	private List<SetCardBagType> GetPossibleEnemies(int wave)
	{
		if (wave < 4)
		{
			return this.enemiesBasic;
		}
		return this.enemiesAdvanced;
	}

	private float GetStrengthForWave(int wave)
	{
		return this.WaveStrengthIncrement * (float)wave + this.FirstWaveStrength;
	}

	private void SpawnWave(int wave)
	{
		float num;
		if (wave <= this.WickedWitchWave)
		{
			num = this.GetStrengthForWave(wave);
		}
		else
		{
			num = Random.Range(this.GetStrengthForWave(3), this.GetStrengthForWave(15));
		}
		foreach (CardIdWithEquipment cardIdWithEquipment in SpawnHelper.GetEnemiesToSpawn(this.GetPossibleEnemies(wave), num, true))
		{
			Combatable combatable = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), cardIdWithEquipment, true, false, true) as Combatable;
			combatable.HealthPoints = combatable.ProcessedCombatStats.MaxHealth;
		}
	}

	public void StartWave()
	{
		this.CombatState = ForestCombatState.InWave;
	}

	private static void StartWaveConflict(bool wickedWitchWave)
	{
		List<Combatable> list = WorldManager.instance.CardQuery.GetCards<Combatable>();
		if (wickedWitchWave)
		{
			list = list.OrderByDescending<Combatable, Team>((Combatable x) => x.Team).ToList<Combatable>();
		}
		else
		{
			list = (from x in list
				orderby x.Team descending
				where x.Id != "wicked_witch"
				select x).ToList<Combatable>();
		}
		Conflict conflict = Conflict.StartConflict(list[0]);
		for (int i = 1; i < list.Count; i++)
		{
			conflict.JoinConflict(list[i]);
		}
		Vector3 vector = ForestCombatManager.DetermineVillagerPositionAverage(list);
		conflict.ConflictStartPosition = vector;
		for (int j = 0; j < list.Count; j++)
		{
			Vector3 positionInConflict = conflict.GetPositionInConflict(list[j]);
			list[j].MyGameCard.transform.position = (list[j].MyGameCard.TargetPosition = positionInConflict);
			if (!(list[j] is BaseVillager))
			{
				WorldManager.instance.CreateSmoke(positionInConflict);
			}
		}
	}

	private static Vector3 DetermineVillagerPositionAverage(List<Combatable> combatables)
	{
		Vector3 vector = default(Vector3);
		int num = 0;
		for (int i = 0; i < combatables.Count; i++)
		{
			if (combatables[i] is BaseVillager)
			{
				vector += combatables[i].transform.position;
				num++;
			}
		}
		return vector / (float)num;
	}

	private void FinishWave()
	{
		ForestCombatManager.DeleteAllCorpses();
		QuestManager.instance.SpecialActionComplete("completed_forest_wave", null);
		WorldManager.instance.CurrentRunVariables.ForestWave++;
		WorldManager.instance.CurrentRunVariables.CanDropItem = true;
		int forestWave = WorldManager.instance.CurrentRunVariables.ForestWave;
		this.CombatState = ForestCombatState.Finished;
		if (forestWave < this.WickedWitchWave)
		{
			this.LayoutVillagers(false);
			WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.ForestWaveEnd());
			return;
		}
		if (forestWave == this.WickedWitchWave)
		{
			this.LayoutVillagers(false);
			WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.ForestLastWaveEnd());
			return;
		}
		this.LayoutVillagers(false);
		WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.ForestEndlessWaveEnd());
	}

	private void Update()
	{
		this.CheckResumeCombat();
		if (WorldManager.instance.CurrentGameState == WorldManager.GameState.Playing && WorldManager.instance.CurrentBoard.Id == "forest")
		{
			if (WorldManager.instance.InAnimation)
			{
				this.LayoutVillagers(false);
			}
			if (this.CombatState == ForestCombatState.InWave)
			{
				if (this.IsWaveOver())
				{
					this.FinishWave();
					return;
				}
				if (this.AllVillagersInForestDied())
				{
					this.CombatState = ForestCombatState.Lost;
					WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.ForestWaveLost());
				}
			}
		}
	}

	private bool IsWaveOver()
	{
		bool flag = true;
		foreach (GameCard gameCard in WorldManager.instance.CardQuery.GetAllCardsOnBoard("forest"))
		{
			if (!(gameCard.CardData is Enemy))
			{
				Mob mob = gameCard.CardData as Mob;
				if (mob == null || !mob.IsAggressive)
				{
					continue;
				}
			}
			if (gameCard.CardData.Id == "wicked_witch" && WorldManager.instance.CurrentRunVariables.ForestWave != this.WickedWitchWave)
			{
				break;
			}
			flag = false;
		}
		return flag;
	}

	private bool AllVillagersInForestDied()
	{
		using (List<GameCard>.Enumerator enumerator = WorldManager.instance.CardQuery.GetAllCardsOnBoard("forest").GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.CardData is BaseVillager)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void LeaveForest()
	{
		ForestCombatManager.DeleteAllCorpses();
		List<GameCard> list = (from x in WorldManager.instance.CardQuery.GetAllCardsOnBoard("forest")
			where !x.IsEquipped && x.CardData.MyCardType != CardType.Humans
			select x).ToList<GameCard>();
		list.RemoveAll((GameCard x) => !this.CanDropCard(x.CardData.Id));
		List<GameCard> list2 = (from x in WorldManager.instance.CardQuery.GetAllCardsOnBoard("forest")
			where x.CardData.MyCardType == CardType.Humans
			select x).ToList<GameCard>();
		WorldManager.instance.Restack(list);
		WorldManager.instance.Restack(list2);
		GameBoard boardWithId = WorldManager.instance.GetBoardWithId(WorldManager.instance.CurrentRunVariables.PreviouseBoard);
		if (list.Count > 0)
		{
			WorldManager.instance.SendStackToBoard(list[0], boardWithId, new Vector2(0.4f, 0.5f));
		}
		WorldManager.instance.SendStackToBoard(list2[0], boardWithId, new Vector2(0.5f, 0.5f));
		WorldManager.instance.GoToBoard(boardWithId, delegate
		{
			if (!WorldManager.instance.HasFoundCard("blueprint_stable_portal"))
			{
				WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "blueprint_stable_portal", true, true, true);
			}
		}, "default");
	}

	public void ForestWaveLost()
	{
		GameBoard boardWithId = WorldManager.instance.GetBoardWithId(WorldManager.instance.CurrentRunVariables.PreviouseBoard);
		WorldManager.instance.GoToBoard(boardWithId, delegate
		{
			if (!WorldManager.instance.HasFoundCard("blueprint_stable_portal"))
			{
				WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "blueprint_stable_portal", true, true, true);
			}
			ForestCombatManager.DeleteAllCorpses();
			ForestCombatManager.RemoveForestCards();
			this.CombatState = ForestCombatState.Idle;
		}, "default");
	}

	private static void RemoveForestCards()
	{
		foreach (GameCard gameCard in WorldManager.instance.CardQuery.GetAllCardsOnBoard("forest"))
		{
			gameCard.DestroyCard(false, true);
		}
	}

	private void CheckResumeCombat()
	{
		if (WorldManager.instance.CurrentBoard != null && WorldManager.instance.CurrentBoard.Id == "forest")
		{
			if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
			{
				this.LeaveForest();
			}
			if (WorldManager.instance.CurrentRunVariables.VisitedForest && this.CombatState == ForestCombatState.Idle)
			{
				this.ResumeForestCombat();
				return;
			}
			if (!WorldManager.instance.CurrentRunVariables.VisitedForest && this.CombatState == ForestCombatState.Idle)
			{
				this.InitForestCombat();
				WorldManager.instance.CurrentRunVariables.VisitedForest = true;
				return;
			}
		}
		else if (this.CombatState != ForestCombatState.Idle)
		{
			this.CombatState = ForestCombatState.Idle;
		}
	}

	public bool CanDropCard(string cardId)
	{
		return !this.BlacklistedDropIds.Contains(cardId);
	}

	private void VerifyBlacklistedDrops()
	{
		foreach (string text in this.BlacklistedDropIds)
		{
			if (WorldManager.instance.GameDataLoader.GetCardFromId(text, true) == null)
			{
				Debug.LogError(text + " is not a valid card id");
			}
		}
	}

	public static Vector3 GetWitchPosition()
	{
		return ForestCombatManager.GetVillagersPosition() + new Vector3(0f, 0f, GameCard.CardHeight * 1.2f);
	}

	public static void DeleteAllCorpses()
	{
		foreach (GameCard gameCard in (from x in WorldManager.instance.CardQuery.GetAllCardsOnBoard("forest")
			where x.CardData is Corpse
			select x).ToList<GameCard>())
		{
			gameCard.DestroyCard(false, true);
		}
	}

	public static Vector3 GetVillagersPosition()
	{
		Vector3 vector = WorldManager.instance.GetBoardWithId("forest").MiddleOfBoard();
		float conflictHeight = Conflict.GetConflictHeight();
		vector.z += conflictHeight * 0.25f;
		return vector;
	}

	private void LayoutVillagers(bool hardSetPosition = false)
	{
		List<BaseVillager> cards = WorldManager.instance.CardQuery.GetCards<BaseVillager>();
		if (cards.Count == 0)
		{
			return;
		}
		Vector3 villagersPosition = ForestCombatManager.GetVillagersPosition();
		for (int i = 0; i < cards.Count; i++)
		{
			float num = (float)i - ((float)cards.Count - 1f) * 0.5f;
			Vector3 vector = new Vector3(num * WorldManager.instance.HorizonalCombatOffset, 0f, 0f);
			cards[i].MyGameCard.RemoveFromStack();
			cards[i].MyGameCard.TargetPosition = villagersPosition + vector;
			if (hardSetPosition)
			{
				cards[i].MyGameCard.transform.position = cards[i].MyGameCard.TargetPosition;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(ForestCombatManager.GetVillagersPosition(), 0.3f);
	}

	public static ForestCombatManager instance;

	public ForestCombatState CombatState = ForestCombatState.Idle;

	public List<AudioClip> WitchSounds;

	public float FirstWaveStrength = 10f;

	public float WaveStrengthIncrement = 10f;

	public int WickedWitchWave = 10;

	public List<string> BlacklistedDropIds;

	private List<SetCardBagType> enemiesAdvanced = new List<SetCardBagType>
	{
		SetCardBagType.Forest_BasicEnemy,
		SetCardBagType.Forest_AdvancedEnemy
	};

	private List<SetCardBagType> enemiesBasic = new List<SetCardBagType> { SetCardBagType.Forest_BasicEnemy };
}
