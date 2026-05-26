using System;
using TMPro;
using UnityEngine;

public class HitText : MonoBehaviour
{
	private void Awake()
	{
		this.startScale = base.transform.localScale;
		base.transform.localScale *= this.InitialScaleUp;
	}

	public void SetVeryEffective(bool veryEffective)
	{
		this.IsVeryEffective = veryEffective;
		if (this.IsVeryEffective)
		{
			this.VeryEffectiveText = Object.Instantiate<TextMeshPro>(PrefabManager.instance.IsVeryEffectiveText);
		}
	}

	private void Start()
	{
		this.SetPosition();
	}

	private void OnDestroy()
	{
		if (this.VeryEffectiveText != null)
		{
			Object.Destroy(this.VeryEffectiveText.gameObject);
		}
	}

	private void Update()
	{
		this.timer += Time.deltaTime;
		if (this.TargetCombatable.CurrentHitText != this || this.TargetCombatable == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (this.timer >= 0.8f)
		{
			float a = this.TextMesh.color.a;
			Color color = this.TextMesh.color;
			color.a = Mathf.Lerp(color.a, 0f, Time.deltaTime * 12f);
			this.TextMesh.color = color;
			if (this.BackgroundRenderer != null)
			{
				color = this.BackgroundRenderer.color;
				color.a = a;
				this.BackgroundRenderer.color = color;
			}
			if (this.VeryEffectiveText != null)
			{
				color = this.VeryEffectiveText.color;
				color.a = a;
				this.VeryEffectiveText.color = color;
			}
			if (color.a < 0.01f)
			{
				Object.Destroy(base.gameObject);
			}
		}
		this.SetPosition();
		base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, base.transform.localEulerAngles.y, 0f);
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, this.startScale, Time.deltaTime * 10f);
		if (this.VeryEffectiveText != null)
		{
			this.VeryEffectiveText.transform.localScale = Vector3.Lerp(this.VeryEffectiveText.transform.localScale, Vector3.one, Time.deltaTime * 10f);
		}
	}

	private void SetPosition()
	{
		GameCard myGameCard = this.TargetCombatable.MyGameCard;
		if (this.VeryEffectiveText != null)
		{
			this.VeryEffectiveText.transform.rotation = Quaternion.AngleAxis(-14f, Vector3.up) * myGameCard.transform.rotation;
			this.VeryEffectiveText.transform.position = myGameCard.transform.position;
		}
		if (!this.IsMiss)
		{
			base.transform.rotation = myGameCard.transform.rotation;
			base.transform.position = myGameCard.HitTextPosition.position;
			return;
		}
		base.transform.rotation = Camera.main.transform.rotation;
	}

	public TextMeshPro TextMesh;

	[HideInInspector]
	public Combatable TargetCombatable;

	public bool IsMiss;

	public SpriteRenderer BackgroundRenderer;

	private float timer;

	private Vector3 startScale;

	public float InitialScaleUp = 2f;

	public TextMeshPro VeryEffectiveText;

	public bool IsVeryEffective;
}
