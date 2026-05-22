using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class CutsceneStep_FocusCameraOnOverflowingLandfill : CutsceneStep
{
	public override IEnumerator Process()
	{
		List<GameCard> allCardsOnBoard = WorldManager.instance.CardQuery.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id);
		int i = 0;
		while (i < allCardsOnBoard.Count)
		{
			RecyclingCenter recyclingCenter = allCardsOnBoard[i].CardData as RecyclingCenter;
			if (recyclingCenter == null || !recyclingCenter.IsOverflowing)
			{
				Landfill landfill = allCardsOnBoard[i].CardData as Landfill;
				if (landfill == null || !landfill.IsOverflowing)
				{
					i++;
					continue;
				}
			}
			GameCamera.instance.TargetCardOverride = allCardsOnBoard[i];
			yield break;
		}
		yield break;
	}
}
