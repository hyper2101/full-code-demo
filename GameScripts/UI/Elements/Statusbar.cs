using System;
using UnityEngine;

public class Statusbar : MonoBehaviour
{
	private void Start()
	{
		this.startScale = base.transform.localScale;
		this.hiddenScale = this.startScale;
		this.hiddenScale.y = 0f;
		base.transform.localScale = this.hiddenScale;
		this.propBlock = new MaterialPropertyBlock();
		this.Renderer.GetPropertyBlock(this.propBlock);
	}

	private void Update()
	{
		float num = Mathf.Clamp01(this.ParentCard.CurrentTimerTime / this.ParentCard.TargetTimerTime);
		if (this.Paused)
		{
			this.blinkTimer += Time.deltaTime;
			if (this.blinkTimer >= 0.3f)
			{
				this.blink = !this.blink;
				this.blinkTimer = 0f;
			}
		}
		if (this.ParentCard != null && this.ParentCard.CurrentStatusbar != this)
		{
			this.DestroyMe = true;
		}
		this.FillAmount = num;
		this.propBlock.SetFloat("_FillAmount", this.FillAmount);
		this.propBlock.SetFloat("_Pause", (this.Paused && this.blink) ? 1f : 0f);
		this.Renderer.SetPropertyBlock(this.propBlock);
		Vector3 vector;
		if (this.DestroyMe)
		{
			vector = this.hiddenScale;
			if (base.transform.localScale.y < 0.01f)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			vector = this.startScale;
		}
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, vector, Time.deltaTime * 22f);
		if (this.ParentCard != null)
		{
			Vector3 vector2 = Vector3.zero;
			GameCard rootCard = this.ParentCard.GetRootCard();
			if (rootCard.StatusEffectElements.Count > 0)
			{
				vector2 += this.ExtraOffset;
			}
			base.transform.position = rootCard.transform.position + this.Offset + vector2;
			return;
		}
		this.DestroyMe = true;
	}

	public float FillAmount;

	public float StatusTime;

	public bool DestroyMe;

	public GameCard ParentCard;

	public Vector3 Offset;

	public Vector3 ExtraOffset;

	public MeshRenderer Renderer;

	private MaterialPropertyBlock propBlock;

	private Vector3 startScale;

	private Vector3 hiddenScale;

	public bool Paused;

	private float blinkTimer;

	private bool blink;
}
