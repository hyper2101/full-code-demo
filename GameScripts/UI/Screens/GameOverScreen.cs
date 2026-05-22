using System;
using TMPro;
using UnityEngine;

public class GameOverScreen : SokScreen
{
	private void Awake()
	{
		this.BackButton.Clicked += delegate
		{
			TransitionScreen.instance.StartTransition(delegate
			{
				WorldManager.instance.ClearRoundAndRestart();
			}, 0.2f);
		};
	}

	private void OnEnable()
	{
		this.StatsText.maxVisibleLines = 0;
	}

	private void Update()
	{
		string text = "";
		text = text + SokLoc.Translate("label_you_reached_moon", new LocParam[] { LocParam.Create("moon", WorldManager.instance.Time.CurrentMonth.ToString()) }) + "\n";
		text = text + SokLoc.Translate("label_quests_completed", new LocParam[] { LocParam.Plural("count", WorldManager.instance.QuestsCompleted) }) + "\n";
		text = text + SokLoc.Translate("label_new_cards_found", new LocParam[] { LocParam.Plural("count", WorldManager.instance.NewCardsFound) }) + "\n";
		this.StatsText.text = text;
		this.timer += Time.deltaTime;
		if (this.timer >= 0.3f)
		{
			this.timer = 0f;
			TextMeshProUGUI statsText = this.StatsText;
			int maxVisibleLines = statsText.maxVisibleLines;
			statsText.maxVisibleLines = maxVisibleLines + 1;
		}
	}

	public CustomButton BackButton;

	public TextMeshProUGUI StatsText;

	private float timer;
}
