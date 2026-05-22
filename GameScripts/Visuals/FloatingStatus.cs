using System;
using TMPro;
using UnityEngine;

public class FloatingStatus : Interactable
{
	public void StartAnimation(GameCard parent, bool isPositive, int amount, string descriptionText, string iconTag, bool desiredBehaviour, int index = 1, float disappearTime = 1f, bool closeOnHover = false)
	{
		Transform transform = ((parent.WorkerHolder != null) ? parent.WorkerHolder.transform : parent.transform);
		base.transform.parent = transform;
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		base.gameObject.name = parent.CardData.Name;
		this.ParentCard = parent;
		this.yOffset = 0.12f * (float)index;
		string text = (isPositive ? "+" : "-");
		this.Text.color = (desiredBehaviour ? ColorManager.instance.FloatingTextColorSuccess : ColorManager.instance.FloatingTextColorFailed);
		this.Text.text = string.Format("{0}{1}{2}", text, Mathf.Abs(amount), iconTag);
		this.disappearTime = disappearTime;
		this.descriptionText = descriptionText;
		this.closeOnHover = closeOnHover;
		this.timer = 0f;
		this.Text.alpha = 1f;
		this.InAnimation = true;
		base.gameObject.SetActive(true);
	}

	public void StopAnimation()
	{
		this.InAnimation = false;
		if (base.transform != null)
		{
			base.transform.parent = null;
		}
		base.gameObject.SetActive(false);
	}

	protected override void Start()
	{
		this.MyBoard = WorldManager.instance.CurrentBoard;
		base.Start();
	}

	public override string GetTooltipText()
	{
		return this.descriptionText;
	}

	protected override void ClampPos()
	{
	}

	protected override void Update()
	{
		Vector3 vector = ((this.ParentCard.CardData.HasAnyStatusEffect() || this.ParentCard.GetCardWithStatusInStack() != null) ? this.StatusOffset : this.NoStatusOffset);
		vector.y += this.yOffset;
		base.transform.localPosition = FRILerp.Spring(base.transform.localPosition, vector, 12f, 12f, ref this.positionVelocity);
		if (this.InAnimation && (this.disappearTime > 0f || this.timer <= 1f) && Vector3.Distance(base.transform.localPosition, vector) < 0.01f)
		{
			this.timer += Time.deltaTime;
			if (this.timer <= 1f && this.disappearTime != 0f)
			{
				this.Text.alpha = Mathf.Lerp(1f, 0f, this.timer * 4f);
			}
			if (this.disappearTime > 0f && this.timer > this.disappearTime)
			{
				this.StopAnimation();
			}
		}
		Vector3 vector2 = Vector3.one;
		if (this.IsHovered)
		{
			if (this.closeOnHover)
			{
				this.RemoveTimerStarted = true;
				this.RemoveTimer = 0f;
			}
			vector2 = Vector3.one * 1.2f;
			Tooltip.Text = this.descriptionText;
		}
		this.RemoveTimer += Time.deltaTime * 0.5f;
		if (this.RemoveTimerStarted && this.RemoveTimer > 1f)
		{
			this.RemoveTimerStarted = false;
			this.StopAnimation();
		}
		base.transform.localScale = FRILerp.Spring(base.transform.localScale, vector2, 30f, 30f, ref this.scaleVelo);
	}

	public bool InAnimation;

	public GameCard ParentCard;

	public TextMeshPro Text;

	public Vector3 NoStatusOffset;

	public Vector3 StatusOffset;

	private float yOffset;

	private Vector3 endPos;

	private Vector3 positionVelocity;

	private float disappearTime;

	private string descriptionText;

	private bool closeOnHover;

	private bool RemoveTimerStarted;

	private float RemoveTimer;

	private float timer;

	private Vector3 scaleVelo;
}
