using System;
using TMPro;
using UnityEngine;

public class StatusEffectElement : Hoverable
{
	private void Awake()
	{
		this.startScale = base.transform.localScale;
		this.propBlock = new MaterialPropertyBlock();
		this.StatusRenderer.GetPropertyBlock(this.propBlock);
	}

	public void SetStatusEffect(GameCard parentCard, StatusEffect effect)
	{
		this.MyStatusEffect = effect;
		this.ParentCard = parentCard;
		this.StatusRenderer.sprite = effect.Sprite;
	}

	public void Update()
	{
		Vector3 zero = this.startScale;
		if (this.DestroyMe)
		{
			zero = Vector3.zero;
			if (zero.x < 0.001f)
			{
				Object.Destroy(base.gameObject);
				this.ParentCard.StatusEffectElements.Remove(this);
			}
		}
		if (this.MyStatusEffect.FillAmount != null)
		{
			this.propBlock.SetFloat("_FillAmount", this.MyStatusEffect.FillAmount.Value);
		}
		else
		{
			this.propBlock.SetFloat("_FillAmount", 2f);
		}
		this.propBlock.SetTexture("_MainTex", this.MyStatusEffect.Sprite.texture);
		this.propBlock.SetColor("_ColorA", this.MyStatusEffect.ColorA);
		this.propBlock.SetColor("_ColorB", this.MyStatusEffect.ColorB);
		this.StatusRenderer.SetPropertyBlock(this.propBlock);
		if (this.MyStatusEffect.StatusNumberColor != null)
		{
			this.TextMesh.color = this.MyStatusEffect.StatusNumberColor.Value;
		}
		if (this.MyStatusEffect.StatusNumber != null)
		{
			this.TextMesh.text = this.MyStatusEffect.StatusNumber.ToString();
		}
		else
		{
			this.TextMesh.text = string.Empty;
		}
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, zero, Time.deltaTime * 12f);
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, this.TargetLocalPosition, Time.deltaTime * 12f);
	}

	public override string GetTitle()
	{
		return MewtationsLoc.Translate("label_status_effect", new LocParam[] { LocParam.Create("status", this.MyStatusEffect.Name) });
	}

	public override string GetDescription()
	{
		if (!string.IsNullOrEmpty(this.MyStatusEffect.Lore))
		{
			return "<i>" + this.MyStatusEffect.Lore + "</i>\n\n" + this.MyStatusEffect.Description;
		}
		return this.MyStatusEffect.Description;
	}

	[HideInInspector]
	public StatusEffect MyStatusEffect;

	public SpriteRenderer StatusRenderer;

	public Vector3 TargetLocalPosition;

	private Vector3 startScale;

	public bool DestroyMe;

	public GameCard ParentCard;

	public TextMeshPro TextMesh;

	private MaterialPropertyBlock propBlock;
}
