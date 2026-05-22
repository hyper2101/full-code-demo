using System;

public class BoardMonths
{
	public BoardMonths()
	{
		this.MainMonth = 1;
		this.IslandMonth = 0;
		this.ForestMonth = 1;
		this.GreedMonth = 1;
		this.HappinessMonth = 1;
		this.DeathMonth = 1;
		this.CitiesMonth = 1;
	}

	public BoardMonths(SavedMonth saved)
	{
		this.MainMonth = saved.MainMonth;
		this.IslandMonth = saved.IslandMonth;
		this.ForestMonth = saved.ForestMonth;
		this.GreedMonth = saved.GreedMonth;
		this.HappinessMonth = saved.HappinessMonth;
		this.DeathMonth = saved.DeathMonth;
		this.CitiesMonth = saved.CitiesMonth;
	}

	public int GetCurrentMonth()
	{
		GameBoard currentBoard = WorldManager.instance.CurrentBoard;
		if (!(currentBoard != null))
		{
			return 0;
		}
		if (currentBoard.Id == "main")
		{
			return this.MainMonth + this.IslandMonth;
		}
		if (currentBoard.Id == "island")
		{
			return this.MainMonth + this.IslandMonth;
		}
		if (currentBoard.Id == "forest")
		{
			return this.ForestMonth;
		}
		if (currentBoard.Id == "greed")
		{
			return this.GreedMonth;
		}
		if (currentBoard.Id == "happiness")
		{
			return this.HappinessMonth;
		}
		if (currentBoard.Id == "death")
		{
			return this.DeathMonth;
		}
		if (currentBoard.Id == "cities")
		{
			return this.CitiesMonth;
		}
		throw new Exception("Board is not implemented in the BoardMonths");
	}

	public void IncrementMonth()
	{
		GameBoard currentBoard = WorldManager.instance.CurrentBoard;
		if (!(currentBoard != null))
		{
			return;
		}
		if (currentBoard.Id == "main")
		{
			this.MainMonth++;
			return;
		}
		if (currentBoard.Id == "island")
		{
			this.IslandMonth++;
			return;
		}
		if (currentBoard.Id == "forest")
		{
			this.ForestMonth++;
			return;
		}
		if (currentBoard.Id == "greed")
		{
			this.GreedMonth++;
			return;
		}
		if (currentBoard.Id == "happiness")
		{
			this.HappinessMonth++;
			return;
		}
		if (currentBoard.Id == "death")
		{
			this.DeathMonth++;
			return;
		}
		if (currentBoard.Id == "cities")
		{
			this.CitiesMonth++;
			return;
		}
		throw new Exception("Board is not implemented in the BoardMonths");
	}

	public SavedMonth ToSavedMonth()
	{
		return new SavedMonth
		{
			MainMonth = this.MainMonth,
			IslandMonth = this.IslandMonth,
			ForestMonth = this.ForestMonth,
			GreedMonth = this.GreedMonth,
			HappinessMonth = this.HappinessMonth,
			DeathMonth = this.DeathMonth,
			CitiesMonth = this.CitiesMonth
		};
	}

	public void ResetMonths()
	{
		this.MainMonth = 1;
		this.IslandMonth = 0;
		this.ForestMonth = 1;
		this.GreedMonth = 1;
		this.HappinessMonth = 1;
		this.DeathMonth = 1;
		this.CitiesMonth = 1;
	}

	public bool IsEmpty
	{
		get
		{
			return this.MainMonth <= 1 && this.IslandMonth <= 0 && this.ForestMonth <= 1 && this.GreedMonth <= 1 && this.HappinessMonth <= 1 && this.DeathMonth <= 1;
		}
	}

	public int MainMonth;

	public int IslandMonth;

	public int ForestMonth;

	public int GreedMonth;

	public int HappinessMonth;

	public int DeathMonth;

	public int CitiesMonth;
}
