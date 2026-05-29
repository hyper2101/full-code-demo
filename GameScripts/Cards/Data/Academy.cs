using System;

public class Academy : Landmark
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.Id == "alien")
		{
			return false;
		}
		Worker worker = otherCard as Worker;
		if (worker != null && (worker.GetWorkerType() == WorkerType.Educated || worker.GetWorkerType() == WorkerType.Robot) && CitiesManager.instance.Wellbeing >= 50)
		{
			return true;
		}
		Worker worker2 = otherCard as Worker;
		return worker2 != null && worker2.GetWorkerType() == WorkerType.Normal;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild)
		{
			if (base.AllChildrenMatchPredicate((CardData x) => x is Worker))
			{
				Worker worker = this.MyGameCard.Child.CardData as Worker;
				if ((worker.Id == "educated_worker" || worker.Id == "genius") && WorldManager.instance.GetCardCount("genius") > 0)
				{
					return;
				}
				if ((worker.Id == "robot_worker" || worker.Id == "robot_genius") && WorldManager.instance.GetCardCount("robot_genius") > 0)
				{
					return;
				}
				if (!this.MyGameCard.TimerRunningInStack)
				{
					this.MyGameCard.StartTimer(this.EducationTime, new TimerAction(this.EducateWorkers), MewtationsLoc.Translate("card_academy_status"), base.GetActionId("EducateWorkers"), true, false, false);
					goto IL_0118;
				}
				goto IL_0118;
			}
		}
		this.MyGameCard.CancelTimer(base.GetActionId("EducateWorkers"));
		IL_0118:
		base.UpdateCard();
	}

	[TimedAction("educate_workers")]
	public void EducateWorkers()
	{
		GameCard leafCard = this.MyGameCard.GetLeafCard();
		if (leafCard.CardData is Worker)
		{
			if (leafCard.CardData.Id == "worker")
			{
				WorldManager.instance.ChangeToCard(leafCard, "educated_worker");
				leafCard.RemoveFromStack();
				leafCard.SendIt();
				return;
			}
			if (leafCard.CardData.Id == "robot_worker" && CitiesManager.instance.Wellbeing >= 50 && WorldManager.instance.GetCardCount("robot_genius") == 0)
			{
				WorldManager.instance.ChangeToCard(leafCard, "robot_genius");
				leafCard.RemoveFromStack();
				leafCard.SendIt();
				return;
			}
			if (leafCard.CardData.Id == "educated_worker" && CitiesManager.instance.Wellbeing >= 50 && WorldManager.instance.GetCardCount("genius") == 0)
			{
				WorldManager.instance.ChangeToCard(leafCard, "genius");
				leafCard.RemoveFromStack();
				leafCard.SendIt();
			}
		}
	}

	public float EducationTime = 120f;
}
