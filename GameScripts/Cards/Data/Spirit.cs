using System;
using UnityEngine;

public class Spirit : CardData
{
	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	public bool IsReturning
	{
		get
		{
			return this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == base.GetActionId("LeaveWithSpirit");
		}
	}

	private bool finishedSpiritRun
	{
		get
		{
			return (WorldManager.instance.CurrentBoard.Id == "happiness" && WorldManager.instance.CurrentSave.FinishedHappiness) || (WorldManager.instance.CurrentBoard.Id == "greed" && WorldManager.instance.CurrentSave.FinishedGreed) || (WorldManager.instance.CurrentBoard.Id == "death" && WorldManager.instance.CurrentSave.FinishedDeath);
		}
	}

	public override void OnInitialCreate()
	{
		AudioManager.me.PlaySound2D(this.CreateSound, 1f, 0.5f);
		base.OnInitialCreate();
	}

	protected override void Awake()
	{
		base.Awake();
	}

	public override void UpdateCard()
	{
		base.UpdateCard();
		this.SpiritMovement();
		this.TryReturnToMainland();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return base.GetChildCount() + (otherCard.GetChildCount() + 1) <= this.MaxCapacity && !(otherCard is Enemy) && !(otherCard is Harvestable);
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public void TryReturnToMainland()
	{
		if (this.MyGameCard.HasChild && this.finishedSpiritRun)
		{
			if (!this.IsReturning)
			{
				this.MyGameCard.StartTimer(30f, new TimerAction(this.LeaveWithSpirit), MewtationsLoc.Translate("card_spirit_status_1"), base.GetActionId("LeaveWithSpirit"), true, false, false);
				return;
			}
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("LeaveWithSpirit"));
		}
	}

	[TimedAction("leave_spirit")]
	public void LeaveWithSpirit()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			GameCanvas.instance.LeaveSpiritWorldPrompt(new Action(this.ReturnToMainland), new Action(this.Stay));
		}
	}

	public void ReturnToMainland()
	{
		GameBoard targetBoard = WorldManager.instance.GetBoardWithId(WorldManager.instance.CurrentRunVariables.PreviouseBoard);
		GameBoard board = WorldManager.instance.CurrentBoard;
		WorldManager.instance.GoToBoard(targetBoard, delegate
		{
			GameCanvas.instance.SetScreen<GameScreen>();
			GameCard child = this.MyGameCard.Child;
			child.RemoveFromParent();
			WorldManager.instance.SendStackToBoard(child, targetBoard, new Vector2(0.4f, 0.5f));
			WorldManager.instance.RemoveAllCardsFromBoard(board.Id, true);
			WorldManager.instance.ResetBoughtBoostersOnLocation(board.Location);
			if (board.Id == "greed")
			{
				DemandManager.instance.ResetDemands();
				WorldManager.instance.BoardMonths.GreedMonth = 1;
			}
			if (board.Id == "happiness")
			{
				WorldManager.instance.CurrentRunVariables.VillagersUnhappyMonthCount = 0;
				WorldManager.instance.CurrentRunVariables.VillagersHappyMonthCount = 0;
				WorldManager.instance.BoardMonths.HappinessMonth = 1;
			}
			if (board.Id == "death")
			{
				WorldManager.instance.BoardMonths.DeathMonth = 1;
			}
		}, "spirit");
	}

	public void CreateBackgroundPlane()
	{
		GameObject gameObject = Object.Instantiate<GameObject>(PrefabManager.instance.SpiritBackgroundPlanePrefab);
		gameObject.transform.SetParent(this.MyGameCard.Visuals);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.GetComponent<MeshRenderer>().material = GameCamera.instance.TempSpiritBackgroundMaterial;
		this.MyGameCard.MinY = 0.25f;
	}

	public override void OnDestroyCard()
	{
		WorldManager.instance.CreateSmoke(base.transform.position);
		base.OnDestroyCard();
	}

	public void Stay()
	{
		this.MyGameCard.RemoveFromStack();
	}

	private void SpiritMovement()
	{
		this.MyGameCard.TargetPosition += Vector3.left * 0.001f * Mathf.Cos(Time.time);
		this.MyGameCard.TargetPosition += Vector3.forward * 0.0005f * Mathf.Cos(Time.time * 0.5f);
	}

	public AudioClip CreateSound;

	public int MaxCapacity = 10;
}
