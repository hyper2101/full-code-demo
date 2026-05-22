using System;
using TMPro;
using UnityEngine;

public class ScrollTextHorizontal : MonoBehaviour
{
	private void Start()
	{
		this.myRect = base.GetComponent<RectTransform>();
		this.myText = base.GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		RectTransform rectTransform = (RectTransform)this.myRect.parent;
		Vector2 anchoredPosition = this.myRect.anchoredPosition;
		this.startWaitTimer += Time.deltaTime;
		bool flag = anchoredPosition.x <= -(this.myRect.sizeDelta.x - rectTransform.sizeDelta.x);
		if (this.startWaitTimer >= 0.75f && this.myRect.sizeDelta.x >= rectTransform.sizeDelta.x && !flag)
		{
			anchoredPosition.x -= this.ScrollSpeed * Time.deltaTime;
		}
		if (flag)
		{
			this.endWaitTimer += Time.deltaTime;
			if (this.endWaitTimer >= 1.5f)
			{
				anchoredPosition.x = 0f;
				this.startWaitTimer = 0f;
				this.endWaitTimer = 0f;
			}
		}
		this.myRect.anchoredPosition = anchoredPosition;
	}

	private void LateUpdate()
	{
		if (this.lastText != this.myText.text)
		{
			Vector2 anchoredPosition = this.myRect.anchoredPosition;
			anchoredPosition.x = 0f;
			this.myRect.anchoredPosition = anchoredPosition;
			this.startWaitTimer = 0f;
			this.endWaitTimer = 0f;
		}
		this.lastText = this.myText.text;
	}

	public float ScrollSpeed;

	private TextMeshProUGUI myText;

	private RectTransform myRect;

	private string lastText;

	private float startWaitTimer;

	private float endWaitTimer;
}
