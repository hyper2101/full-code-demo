using System;
using UnityEngine;

public class EquipmentPosition : MonoBehaviour
{
	private void Awake()
	{
		this.parentCard = base.GetComponentInParent<GameCard>();
		this.startOffset = base.transform.localPosition;
		this.timer = Random.Range(0f, 10f);
		SpriteRenderer shadowRenderer = this.ShadowRenderer;
		SpriteRenderer iconRenderer = this.IconRenderer;
		Color color = new Color(1f, 1f, 1f, 0f);
		iconRenderer.color = color;
		shadowRenderer.color = color;
		this.ShadowRenderer.enabled = (this.IconRenderer.enabled = false);
	}

	private void Update()
	{
		if (this.parentCard.MyBoard == null)
		{
			return;
		}
		if (!this.parentCard.MyBoard.IsCurrent)
		{
			return;
		}
		this.timer += Time.deltaTime * WorldManager.instance.TimeScale;
		this.alpha = Mathf.Lerp(this.alpha, (this.parentCard.ShowInventory && ((this.parentCard.IsWorkerInventory && this.IsWorkerPosition) || (!this.parentCard.IsWorkerInventory && !this.IsWorkerPosition))) ? 1f : 0f, Time.deltaTime * 20f);
		SpriteRenderer shadowRenderer = this.ShadowRenderer;
		SpriteRenderer iconRenderer = this.IconRenderer;
		Color color = new Color(1f, 1f, 1f, this.alpha);
		iconRenderer.color = color;
		shadowRenderer.color = color;
		this.ShadowRenderer.enabled = (this.IconRenderer.enabled = this.alpha > 0.01f);
		float num = this.timer * 0.5f;
		base.transform.localPosition = this.startOffset + new Vector3(this.Perlin(num, 0.2f), this.Perlin(num, 0.6f)) * 0.01f;
	}

	private float Perlin(float x, float y)
	{
		return Mathf.PerlinNoise(x, y) * 2f - 1f;
	}

	public SpriteRenderer ShadowRenderer;

	public SpriteRenderer IconRenderer;

	private Vector3 startOffset;

	private GameCard parentCard;

	private float alpha;

	public bool IsWorkerPosition;

	private float timer;
}
