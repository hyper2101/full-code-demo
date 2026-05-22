using System;
using UnityEngine;

public class CityAdvisor : CardData
{
	public override void OnInitialCreate()
	{
		AudioManager.me.PlaySound2D(this.AdvisorSound, 1f, 0.1f);
		base.OnInitialCreate();
	}

	protected override void Awake()
	{
		base.Awake();
	}

	public override void UpdateCard()
	{
		if (!CutsceneScreen.instance.IsAdvisorCutscene && !this.MyGameCard.IsDemoCard)
		{
			this.MyGameCard.DestroyCard(false, true);
		}
		base.UpdateCard();
		this.AdvisorMovement();
	}

	public void SetAdditionalOffset()
	{
	}

	private void AdvisorMovement()
	{
		Vector3 vector = GameCamera.instance.ScreenPosToWorldPos(new Vector2((float)Screen.width, (float)Screen.height) * 0.5f);
		if (GameCamera.instance.TargetCardOverride != null)
		{
			vector += Vector3.forward;
		}
		vector += Vector3.left * 0.05f * Mathf.Cos(Time.time);
		vector += Vector3.forward * 0.01f * Mathf.Cos(Time.time * 0.5f);
		this.MyGameCard.TargetPosition = vector;
	}

	public AudioClip AdvisorSound;
}
