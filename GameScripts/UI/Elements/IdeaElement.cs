using System;
using TMPro;
using UnityEngine;

public class IdeaElement : MonoBehaviour
{
	private void Update()
	{
		this.NewLabel.gameObject.SetActive(this.IsNew);
		if (this.MyButton.IsHovered || this.MyButton.IsSelected)
		{
			if (this.IsNew)
			{
				WorldManager.instance.CurrentSave.NewKnowledgeIds.Remove(this.MyKnowledge.CardId);
				this.IsNew = false;
			}
			GameScreen.InfoBoxTitle = this.MyKnowledge.KnowledgeName;
			GameScreen.InfoBoxText = this.MyKnowledge.KnowledgeText;
		}
		this.MyButton.Image.color = ColorManager.instance.BackgroundColor;
	}

	public void SetKnowledge(IKnowledge knowledge)
	{
		this.MyKnowledge = knowledge;
		this.IsNew = WorldManager.instance.CurrentSave.NewKnowledgeIds.Contains(this.MyKnowledge.CardId);
		this.MyText.text = "• " + this.MyKnowledge.KnowledgeName;
	}

	public CustomButton MyButton;

	[HideInInspector]
	public IKnowledge MyKnowledge;

	public TextMeshProUGUI MyText;

	public GameObject NewLabel;

	public bool IsNew;
}
