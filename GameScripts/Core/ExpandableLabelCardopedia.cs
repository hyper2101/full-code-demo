using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpandableLabelCardopedia : MonoBehaviour
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
			Action onExpand = this.OnExpand;
			if (onExpand == null)
			{
				return;
			}
			onExpand();
		};
		this.MyButton.SetColor = false;
	}

	public void SetExpanded(bool expanded)
	{
		this.IsExpanded = expanded;
		foreach (CardopediaEntryElement cardopediaEntryElement in this.Children)
		{
			cardopediaEntryElement.IsEnabled = expanded;
		}
	}

	public void ShowChildrenCardopedia()
	{
		foreach (CardopediaEntryElement cardopediaEntryElement in this.Children)
		{
			if (!CardopediaScreen.instance.IsSearching)
			{
				cardopediaEntryElement.IsEnabled = this.IsExpanded && cardopediaEntryElement.IsFilteredUpdate;
			}
			else
			{
				cardopediaEntryElement.IsEnabled = cardopediaEntryElement.wasFound && cardopediaEntryElement.IsFiltered && this.IsExpanded;
			}
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

	public List<CardopediaEntryElement> Children = new List<CardopediaEntryElement>();

	public bool IsExpanded = true;

	public object Tag;
}
