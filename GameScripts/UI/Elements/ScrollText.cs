using System;
using TMPro;
using UnityEngine;

public class ScrollText : MonoBehaviour
{
	private void Start()
	{
		this.myRect = base.GetComponent<RectTransform>();
	}

	private void Update()
	{
		RectTransform rectTransform = (RectTransform)this.myRect.parent;
		Vector2 anchoredPosition = this.myRect.anchoredPosition;
		this.startWaitTimer += Time.deltaTime;
		bool flag = anchoredPosition.y >= this.myRect.sizeDelta.y - rectTransform.sizeDelta.y;
		if (this.startWaitTimer >= 0.75f && this.myRect.sizeDelta.y >= rectTransform.sizeDelta.y && !flag)
		{
			anchoredPosition.y += this.ScrollSpeed * Time.deltaTime;
		}
		if (flag)
		{
			this.endWaitTimer += Time.deltaTime;
			if (this.endWaitTimer >= 1.5f)
			{
				anchoredPosition.y = 0f;
				this.startWaitTimer = 0f;
				this.endWaitTimer = 0f;
			}
		}
		this.myRect.anchoredPosition = anchoredPosition;
	}

	public void ResetScroll()
	{
		Vector2 anchoredPosition = this.myRect.anchoredPosition;
		anchoredPosition.y = 0f;
		this.myRect.anchoredPosition = anchoredPosition;
		this.startWaitTimer = 0f;
		this.endWaitTimer = 0f;
	}

	private void LateUpdate()
	{
		if (this.myText != null && this.lastText != this.myText.text)
		{
			this.ResetScroll();
		}
		this.lastText = ((this.myText != null) ? this.myText.text : "");
	}

	public float ScrollSpeed;

	public TextMeshProUGUI myText;

	private RectTransform myRect;

	private string lastText;

	private float startWaitTimer;

	private float endWaitTimer;
}
