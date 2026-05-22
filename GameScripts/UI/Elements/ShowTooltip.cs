using System;
using UnityEngine;

public class ShowTooltip : MonoBehaviour
{
	private void Update()
	{
		if (InputController.instance.CurrentScheme == ControlScheme.KeyboardMouse)
		{
			GameObject mouseOverObject = GameCanvas.instance.MouseOverObject;
			if (mouseOverObject != null && (mouseOverObject.transform.IsChildOf(base.transform) || mouseOverObject.transform == base.transform))
			{
				if (!string.IsNullOrEmpty(this.MyTooltipTerm))
				{
					Tooltip.Text = SokLoc.Translate(this.MyTooltipTerm);
					return;
				}
				Tooltip.Text = this.MyTooltipText;
			}
		}
	}

	public string MyTooltipTerm;

	public string MyTooltipText;
}
