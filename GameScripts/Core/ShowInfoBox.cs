using System;
using UnityEngine;

public class ShowInfoBox : MonoBehaviour
{
	private void Start()
	{
		this.rectTransform = base.GetComponent<RectTransform>();
	}

	private void Update()
	{
		if (InputController.instance.CurrentScheme == ControlScheme.KeyboardMouse)
		{
			if (GameCanvas.instance.AboveMeOrMyChildren(this.rectTransform, GameCanvas.instance.MouseOverObject))
			{
				GameScreen.InfoBoxTitle = this.InfoBoxTitle;
				GameScreen.InfoBoxText = this.InfoBoxText;
				return;
			}
		}
		else if (InputController.instance.CurrentSchemeIsController && GameCanvas.instance.SelectedObject == base.gameObject)
		{
			GameScreen.InfoBoxTitle = this.InfoBoxTitle;
			GameScreen.InfoBoxText = this.InfoBoxText;
		}
	}

	public string InfoBoxTitle;

	public string InfoBoxText;

	private RectTransform rectTransform;
}
