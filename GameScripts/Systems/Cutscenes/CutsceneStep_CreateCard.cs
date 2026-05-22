using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CutsceneStep_CreateCard : CutsceneStep
{
	public override IEnumerator Process()
	{
		if (this.FindOrCreate && WorldManager.instance.GetCard(this.CardId) != null)
		{
			yield break;
		}
		Vector3 vector = Vector3.zero;
		if (this.Location == CutsceneStep_CreateCard.SpawnLocation.MiddleOfBoard)
		{
			vector = WorldManager.instance.MiddleOfBoard();
		}
		else if (this.Location == CutsceneStep_CreateCard.SpawnLocation.Random)
		{
			vector = WorldManager.instance.GetRandomSpawnPosition();
		}
		else if (this.Location == CutsceneStep_CreateCard.SpawnLocation.AtCard)
		{
			vector = WorldManager.instance.GetCard(this.OtherCardId).transform.position;
		}
		else if (this.Location == CutsceneStep_CreateCard.SpawnLocation.AtFocussed)
		{
			IGameCardOrCardData targetCardOverride = GameCamera.instance.TargetCardOverride;
			if (targetCardOverride != null)
			{
				vector = targetCardOverride.Position + Vector3.left * 1.5f;
			}
			else
			{
				vector = WorldManager.instance.MiddleOfBoard();
			}
		}
		CardData cardData = WorldManager.instance.CreateCard(vector, this.CardId, true, false, true);
		if (this.MakeSmoke)
		{
			WorldManager.instance.CreateSmoke(vector);
		}
		if (this.SendCard)
		{
			cardData.MyGameCard.SendIt();
		}
		yield break;
	}

	[Card]
	public string CardId;

	public CutsceneStep_CreateCard.SpawnLocation Location;

	[Card]
	public string OtherCardId;

	[Header("Options")]
	public bool FindOrCreate;

	public bool SendCard;

	public bool MakeSmoke;

	public enum SpawnLocation
	{
		Random,
		MiddleOfBoard,
		AtCard,
		AtFocussed
	}
}
