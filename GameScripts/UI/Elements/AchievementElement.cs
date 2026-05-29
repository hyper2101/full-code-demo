using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementElement : MonoBehaviour
{
	public bool IsNew
	{
		get
		{
			return this.isNew;
		}
	}

	private void Update()
	{
		this.UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (!this.isVisible && !this.isComplete)
		{
			this.AchievementNameText.text = "???";
		}
		else
		{
			this.AchievementNameText.text = this.MyQuest.Description;
		}
		this.MyButton.Image.color = ColorManager.instance.BackgroundColor;
		if (this.MyButton.IsHovered || this.MyButton.IsSelected)
		{
			if (this.isNew || this.isComplete)
			{
				if (!WorldManager.instance.CurrentSave.SeenQuestIds.Contains(this.MyQuest.Id))
				{
					WorldManager.instance.CurrentSave.SeenQuestIds.Add(this.MyQuest.Id);
				}
				this.isNew = false;
			}
			if (this.isVisible || this.isComplete)
			{
				GameScreen.InfoBoxTitle = MewtationsLoc.Translate("label_quest");
				GameScreen.InfoBoxText = this.MyQuest.Description;
			}
			else
			{
				GameScreen.InfoBoxTitle = MewtationsLoc.Translate("label_quest");
				GameScreen.InfoBoxText = MewtationsLoc.Translate("label_quests_complete_more_to_see");
			}
		}
		this.AchievementNameText.color = (this.isComplete ? ColorManager.instance.DisabledColor : Color.black);
		this.Checkmark.color = this.AchievementNameText.color;
		this.Checkbox.color = ColorManager.instance.DisabledColor;
		if (this.isVisible && !this.MyQuest.PossibleInPeacefulMode && WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
		{
			this.MyButton.TooltipText = MewtationsLoc.Translate("label_quest_not_possible_in_peaceful");
		}
		else
		{
			this.MyButton.TooltipText = "";
		}
		this.NewLabel.gameObject.SetActive(this.isNew);
		this.Checkmark.gameObject.SetActive(this.isComplete);
	}

	public void SetQuest(Quest ach)
	{
		this.MyQuest = ach;
		this.isComplete = QuestManager.instance.QuestIsComplete(this.MyQuest);
		this.isVisible = QuestManager.instance.QuestIsVisible(this.MyQuest);
		this.isNew = !WorldManager.instance.CurrentSave.SeenQuestIds.Contains(this.MyQuest.Id) && !this.isComplete;
		this.UpdateVisuals();
	}

	public Quest MyQuest;

	public CustomButton MyButton;

	public Image Checkmark;

	public Image Checkbox;

	public TextMeshProUGUI AchievementNameText;

	public GameObject NewLabel;

	private bool isVisible;

	private bool isComplete;

	private bool isNew;
}
