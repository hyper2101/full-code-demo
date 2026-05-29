using System;
using System.Linq;
using UnityEngine;

public class TimeMachine : Landmark
{
	public override void UpdateCard()
	{
		if (this.IsCharged)
		{
			if (this.MyGameCard.HasChild && (this.MyGameCard.Child.CardData is Worker || this.MyGameCard.Child.CardData is BaseVillager))
			{
				if (WorldManager.instance.CardQuery.GetCardCount<BaseVillager>() <= 1 && this.MyGameCard.Child.CardData is BaseVillager)
				{
					GameCanvas.instance.OneVillagerNeedsToStayPrompt("label_take_time_machine");
					this.MyGameCard.Child.RemoveFromParent();
				}
				if (!this.MyGameCard.TimerRunning)
				{
					this.MyGameCard.StartTimer(20f, new TimerAction(this.UseTimeMachine), MewtationsLoc.Translate("card_time_machine_status_1"), base.GetActionId("UseTimeMachine"), true, false, false);
				}
			}
			else
			{
				this.MyGameCard.CancelTimer(base.GetActionId("UseTimeMachine"));
			}
		}
		else if (this.HasEnergyInput(null))
		{
			this.MyGameCard.StartTimer(240f, new TimerAction(this.ChargeTimeMachine), MewtationsLoc.Translate("card_time_machine_status_2"), base.GetActionId("ChargeTimeMachine"), true, false, false);
			AudioManager.me.PlaySound(this.TimeMachineChargingSound, base.transform, 1f, 0.5f);
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("ChargeTimeMachine"));
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		base.UpdateCardText();
		if (this.IsCharged)
		{
			this.nameOverride = MewtationsLoc.Translate("card_time_machine_name_real");
			this.descriptionOverride = MewtationsLoc.Translate("card_time_machine_desctiption_real");
		}
	}

	[TimedAction("start_time_machine")]
	public void ChargeTimeMachine()
	{
		AudioManager.me.PlaySound(this.TimeMachineDoneSound, base.transform, 1f, 0.5f);
		WorldManager.instance.Cutscene.QueueCutscene("cities_time_machine");
		this.IsCharged = true;
	}

	[TimedAction("use_time_machine")]
	public void UseTimeMachine()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			string text = "main";
			if (WorldManager.instance.CurrentBoard.Id == "main")
			{
				text = "cities";
			}
			GameCanvas.instance.ChangeLocationPrompt(new Action(this.GoAway), new Action(this.Stay), text);
		}
	}

	public void Stay()
	{
		GameCard parent = this.MyGameCard.Parent;
		base.RestackChildrenMatchingPredicate((CardData c) => c is Worker || c is BaseVillager);
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			this.MyGameCard.Parent = parent;
		}
	}

	private void GoAway()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameBoard targetBoard = WorldManager.instance.GetBoardWithId("main");
		if (WorldManager.instance.CurrentBoard.Id == "main")
		{
			targetBoard = WorldManager.instance.GetBoardWithId("cities");
		}
		AudioManager.me.PlaySound(this.TimeMachineUseSound, base.transform, 1f, 0.5f);
		WorldManager.instance.GoToBoard(targetBoard, delegate
		{
			GameCanvas.instance.SetScreen<GameScreen>();
			WorldManager.instance.SendToBoard(this.MyGameCard, targetBoard, new Vector2(0.4f, 0.5f));
			if (this.MyGameCard.GetChildCards().Any<GameCard>((GameCard x) => x.CardData is BaseVillager))
			{
				WorldManager.instance.Cutscene.QueueCutscene("cities_villager_timemachine");
			}
			this.UsedOnce = true;
			this.MyGameCard.RemoveFromStack();
		}, "default");
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!this.UsedOnce)
		{
			return otherCard.Id == "genius" || otherCard.Id == "robot_genius";
		}
		return otherCard is Worker || otherCard is BaseVillager || base.CanHaveCard(otherCard);
	}

	public AudioClip TimeMachineChargingSound;

	public AudioClip TimeMachineUseSound;

	public AudioClip TimeMachineDoneSound;

	[ExtraData("is_charged")]
	public bool IsCharged;

	[ExtraData("used_once")]
	public bool UsedOnce;
}
