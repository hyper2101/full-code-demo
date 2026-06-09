using System;
using System.Collections.Generic;

[Serializable]
public class RunVariables
{
	public int CrabsKilled;

	public int PirateBoatsBribed;

	public int StrangePortalSpawns;

	public bool VisitedIsland;

	public bool VisitedForest;

	public bool VisitedHappiness;

	public bool VisitedGreed;

	public bool VisitedDeath;

	public bool ShamanVisited;

	public bool FinishedDemon;

	public bool FinishedDemonLord;

	public bool FinishedKraken;

	public bool FinishedWickedWitch;

	public string PreviouseBoard = "main";

	public int LastGoToBoardMonth;

	public int PirateBoatsSpawned;

	public int ForestWave = 1;

	public int IslandMonths;

	public int DeathMonths;

	public int LastDemandMonth;

	public int VillagersHappyMonthCount;

	public int VillagersUnhappyMonthCount;

	public bool CompletedHappinessSpirit;

	public bool CompletedGreedSpirit;

	public bool CompletedDeathSpirit;

	public bool CanDropItem = true;

	public List<string> PlayedCutsceneIds = new List<string>();

	public List<string> SpawnedEventIds = new List<string>();

	public bool OpenedFirstTrash;

	public bool HasCitiesBoard;

	public List<string> BuiltLandmarks = new List<string>();

	public int GlobalEntityCounter = 0;

	public bool HasCatReachedLevel9;
	public bool HasFoundBlackAltarBlueprint;
}

