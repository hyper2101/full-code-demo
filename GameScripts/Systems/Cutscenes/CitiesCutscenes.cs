using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CitiesCutscenes
{
	public static string Title
	{
		get
		{
			return WorldManager.instance.CutsceneTitle;
		}
		set
		{
			WorldManager.instance.CutsceneTitle = value;
		}
	}

	public static string Text
	{
		get
		{
			return WorldManager.instance.CutsceneText;
		}
		set
		{
			WorldManager.instance.CutsceneText = value;
		}
	}

	private static QueuedAnimation currentAnimation
	{
		set
		{
			WorldManager.instance.currentAnimation = value;
		}
	}

	public static IEnumerator CitiesTornado()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		CitiesCutscenes.Title = "";
		GameCard spirit = CitiesCutscenes.FindOrCreateGameCard("greed_spirit", null);
		GameCamera.instance.TargetPositionOverride = new Vector3?(spirit.transform.position);
		CitiesCutscenes.Text = MewtationsLoc.Translate("label_greed_outro_wear_crown");
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		spirit.DestroyCard(false, true);
		CitiesCutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator CitiesFinancialCrisis()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		CitiesCutscenes.Title = MewtationsLoc.Translate("cutscene_cities_financial_crisis_title");
		GameCard gameCard = CitiesCutscenes.FindOrCreateGameCard("financial_crisis", null);
		GameCamera.instance.TargetCardOverride = gameCard;
		CitiesCutscenes.Text = MewtationsLoc.Translate("cutscene_cities_financial_crisis_text");
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
		CitiesCutscenes.Text = MewtationsLoc.Translate("cutscene_cities_financial_crisis_text_1");
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		CitiesCutscenes.Stop(false);
		yield break;
	}

	private static List<GameCard> GetCardsToDamage()
	{
		List<GameCard> list = (from x in WorldManager.instance.CardQuery.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id)
			where x.CardData.IsBuilding && !x.CardData.IsDamaged
			select x).ToList<GameCard>();
		list = list.OrderBy<GameCard, float>((GameCard x) => Random.value).ToList<GameCard>();
		float num = (float)(CitiesManager.instance.Wellbeing / 10);
		int num2 = Mathf.Clamp(Mathf.RoundToInt((float)list.Count / Random.Range(10f - num, 5f - num)), 2, 5);
		return list.Take<GameCard>(Mathf.Min(num2, list.Count)).ToList<GameCard>();
	}

	public static IEnumerator CitiesEarthQuake(GameCard origin)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCard gameCard = CitiesCutscenes.FindOrCreateGameCard("earthquake", null);
		GameCamera.instance.TargetCardOverride = gameCard;
		CitiesCutscenes.Title = MewtationsLoc.Translate("label_cities_earthquake_title");
		CitiesCutscenes.Text = MewtationsLoc.Translate("label_cities_earthquake_text");
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
		List<GameCard> cardsToDamage = CitiesCutscenes.GetCardsToDamage();
		foreach (GameCard gameCard2 in cardsToDamage)
		{
			GameCamera.instance.TargetCardOverride = gameCard2;
			gameCard2.CardData.SetCardDamaged(CardDamageType.Damaged);
			gameCard2.CancelAnyTimer();
			gameCard2.RotWobble(2f);
			AudioManager.me.PlaySound2D(AudioManager.me.DamagedCardSound, Random.Range(0.9f, 1.1f), 0.4f);
			GameCamera.instance.Screenshake = 0.5f;
			yield return new WaitForSeconds(1f);
		}
		List<GameCard>.Enumerator enumerator = default(List<GameCard>.Enumerator);
		GameCamera.instance.TargetCardOverride = null;
		CitiesCutscenes.Text = MewtationsLoc.Translate("label_cities_earthquake_text_1");
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		origin.DestroyCard(true, true);
		CitiesCutscenes.Stop(false);
		yield break;
		yield break;
	}

	public static IEnumerator CitiesDrought(GameCard origin)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		CitiesCutscenes.Title = MewtationsLoc.Translate("label_cities_drought_title");
		CitiesCutscenes.Text = MewtationsLoc.Translate("label_cities_drought_text");
		GameCard gameCard = CitiesCutscenes.FindOrCreateGameCard("drought", null);
		GameCamera.instance.TargetCardOverride = gameCard;
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
		List<GameCard> list = (from x in WorldManager.instance.CardQuery.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id)
			where x.CardData is Farmland && !x.CardData.IsDamaged
			select x).ToList<GameCard>();
		list.Sort((GameCard a, GameCard b) => Random.Range(-1, 1));
		float num = (float)(CitiesManager.instance.Wellbeing / 10);
		int num2 = Mathf.Clamp(Mathf.RoundToInt((float)list.Count / Random.Range(10f - num, 5f - num)), 2, 5);
		list = list.Take<GameCard>(Mathf.Min(num2, list.Count)).ToList<GameCard>();
		foreach (GameCard gameCard2 in list)
		{
			GameCamera.instance.TargetCardOverride = gameCard2;
			gameCard2.CardData.SetCardDamaged(CardDamageType.Drought);
			gameCard2.CancelAnyTimer();
			gameCard2.RotWobble(2f);
			AudioManager.me.PlaySound2D(AudioManager.me.DroughtStart, Random.Range(0.9f, 1.1f), 0.4f);
			GameCamera.instance.Screenshake = 0.5f;
			yield return new WaitForSeconds(1f);
		}
		List<GameCard>.Enumerator enumerator = default(List<GameCard>.Enumerator);
		GameCamera.instance.TargetCardOverride = null;
		CitiesCutscenes.Text = MewtationsLoc.Translate("label_cities_drought_text_1");
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		origin.DestroyCard(true, true);
		CitiesCutscenes.Stop(false);
		yield break;
		yield break;
	}

	public static IEnumerator CitiesWildFire(GameCard origin)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		CitiesCutscenes.Title = MewtationsLoc.Translate("label_cities_wildfire_title");
		CitiesCutscenes.Text = MewtationsLoc.Translate("label_cities_wildfire_text");
		GameCard gameCard = CitiesCutscenes.FindOrCreateGameCard("wildfire", null);
		GameCamera.instance.TargetCardOverride = gameCard;
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
		List<GameCard> cardsToDamage = CitiesCutscenes.GetCardsToDamage();
		foreach (GameCard gameCard2 in cardsToDamage)
		{
			GameCamera.instance.TargetCardOverride = gameCard2;
			gameCard2.CardData.SetCardDamaged(CardDamageType.Fire);
			gameCard2.CancelAnyTimer();
			AudioManager.me.PlaySound2D(AudioManager.me.OnFireCardSound, Random.Range(0.9f, 1.1f), 0.4f);
			GameCamera.instance.Screenshake = 0.2f;
			yield return new WaitForSeconds(1f);
		}
		List<GameCard>.Enumerator enumerator = default(List<GameCard>.Enumerator);
		GameCamera.instance.TargetCardOverride = null;
		CitiesCutscenes.Text = MewtationsLoc.Translate("label_cities_wildfire_text_1");
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		origin.DestroyCard(true, true);
		CitiesCutscenes.Stop(false);
		yield break;
		yield break;
	}

	public static IEnumerator DinoBoss(Laboratory laboratory, CardData fossil)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(laboratory.transform.position);
		CitiesCutscenes.Text = MewtationsLoc.Translate("label_dino_laboratory");
		CitiesCutscenes.Title = "";
		if (!WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
		{
			string text = MewtationsLoc.Translate("label_start_dino");
			string text2 = MewtationsLoc.Translate("label_cancel_dino");
			yield return CitiesCutscenes.WaitForAnswer(new string[] { text, text2 });
		}
		else
		{
			string text = MewtationsLoc.Translate("label_well_done");
			yield return CitiesCutscenes.WaitForAnswer(new string[] { text });
		}
		if (WorldManager.instance.ContinueButtonIndex == 0)
		{
			fossil.MyGameCard.DestroyCard(true, true);
			if (!WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
			{
				WorldManager.instance.CreateCard(laboratory.transform.position, "dino", true, false, true);
			}
			else
			{
				QuestManager.instance.ActionComplete(laboratory, "cities_defeat_trex", null);
			}
		}
		else
		{
			fossil.MyGameCard.RemoveFromStack();
		}
		laboratory.InCutscene = false;
		CitiesCutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator CitiesParticleCollider(GameCard origin)
	{
		ParticleCollider collider = origin.CardData as ParticleCollider;
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetCardOverride = origin;
		CitiesCutscenes.Title = MewtationsLoc.Translate("cutscene_cities_particle_collider_title");
		CitiesCutscenes.Text = MewtationsLoc.Translate("cutscene_cities_particle_collider_text");
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("cutscene_cities_particle_collider_switch"));
		collider.ColliderRunning = true;
		origin.CardData.DestroyChildrenMatchingPredicateAndRestack((CardData x) => x.Id == "uranium", 2);
		AudioManager.me.PlaySound2D(AudioManager.me.ColliderRunningSound, 1f, 0.5f);
		GameCamera.instance.Screenshake = 0.5f;
		yield return new WaitForSeconds(4.5f);
		CardData cardData = WorldManager.instance.CreateCard(origin.Position, "quantum_entangled_uranium", true, true, true);
		collider.ColliderRunning = false;
		GameCamera.instance.Screenshake = 0f;
		GameCamera.instance.TargetCardOverride = null;
		CitiesCutscenes.Text = MewtationsLoc.Translate("cutscene_cities_particle_collider_text_1");
		GameCamera.instance.TargetCardOverride = cardData;
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		collider.CutsceneQueued = false;
		CitiesCutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator CitiesStopDisaster()
	{
		CutsceneScreen.instance.IsAdvisorCutscene = true;
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		CitiesCutscenes.Title = "";
		CitiesCutscenes.Text = MewtationsLoc.Translate("label_greed_outro_wear_crown");
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		CitiesCutscenes.Stop(false);
		yield break;
	}

	private static void Stop(bool keepCameraPosition = false)
	{
		CitiesCutscenes.Text = "";
		CitiesCutscenes.Title = "";
		GameCamera.instance.TargetPositionOverride = null;
		GameCamera.instance.CameraPositionDistanceOverride = null;
		GameCamera.instance.TargetCardOverride = null;
		CutsceneScreen.instance.IsEndOfMonthCutscene = false;
		CutsceneScreen.instance.IsAdvisorCutscene = false;
		CutsceneScreen.instance.CheckAdvisorCutscene();
		if (keepCameraPosition)
		{
			GameCamera.instance.KeepCameraAtCurrentPos();
		}
		GameCanvas.instance.SetScreen<GameScreen>();
		CitiesCutscenes.currentAnimation = null;
	}

	public static IEnumerator WaitForAnswer(params string[] answers)
	{
		CutsceneScreen.instance.CreateMultipleOptions(answers);
		WorldManager.instance.ContinueClicked = false;
		while (!WorldManager.instance.ContinueClicked)
		{
			yield return null;
			if (!(GameCanvas.instance.CurrentScreen is CutsceneScreen))
			{
				GameCanvas.instance.SetScreen<CutsceneScreen>();
			}
		}
		CutsceneScreen.instance.ClearMultipleOptions();
		WorldManager.instance.ShowContinueButton = false;
		yield break;
	}

	public static IEnumerator WaitForContinueClicked(string text)
	{
		WorldManager.instance.ContinueClicked = false;
		WorldManager.instance.ContinueButtonText = text;
		WorldManager.instance.ShowContinueButton = true;
		while (!WorldManager.instance.ContinueClicked)
		{
			yield return null;
			if (!(GameCanvas.instance.CurrentScreen is CutsceneScreen))
			{
				GameCanvas.instance.SetScreen<CutsceneScreen>();
			}
		}
		WorldManager.instance.ShowContinueButton = false;
		yield break;
	}

	public static GameCard FindOrCreateGameCard(string cardId, Vector3? position = null)
	{
		CardData cardData = WorldManager.instance.GetCard(cardId);
		if (cardData == null)
		{
			if (position != null)
			{
				cardData = WorldManager.instance.CreateCard(position.Value, cardId, true, false, true);
			}
			else
			{
				cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), cardId, true, false, true);
			}
		}
		if (cardData == null)
		{
			return null;
		}
		return cardData.MyGameCard;
	}

	public static IEnumerator CitiesGameOver()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetCardOverride = null;
		GameCamera.instance.TargetPositionOverride = null;
		yield return new WaitForSeconds(2f);
		CitiesCutscenes.Text = MewtationsLoc.Translate("label_final_demand_merchant");
		yield return CitiesCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		yield break;
	}
}
