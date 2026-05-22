using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpandableLabel : MonoBehaviour
{
	public event Action OnExpand;

	public void SetText(string text)
	{
		this.LabelText.text = text;
	}

	public void SetCallback(Action callback)
	{
		this.OnExpand += callback;
	}

	private void Start()
	{
		this.MyButton.Clicked += delegate
		{
			this.SetExpanded(!this.IsExpanded);
			if (this.IsExpanded)
			{
				Action onExpand = this.OnExpand;
				if (onExpand == null)
				{
					return;
				}
				onExpand();
			}
		};
		this.MyButton.SetColor = false;
	}

	public void SetExpanded(bool expanded)
	{
		this.IsExpanded = expanded;
		foreach (GameObject gameObject in this.Children)
		{
			gameObject.gameObject.SetActive(expanded);
		}
	}

	private void Update()
	{
		this.PlusImage.sprite = ((!this.IsExpanded) ? this.PlusSprite : this.MinusSprite);
	}

	public CustomButton MyButton;

	public Image PlusImage;

	public TextMeshProUGUI LabelText;

	public Sprite PlusSprite;

	public Sprite MinusSprite;

	public List<GameObject> Children = new List<GameObject>();

	public bool IsExpanded = true;

	public object Tag;
}
