using System;

public class ShowTooltipInteractable : Interactable
{
	protected override void ClampPos()
	{
	}

	protected override void Update()
	{
	}

	protected override void LateUpdate()
	{
	}

	public override string GetTooltipText()
	{
		base.name = MewtationsLoc.Translate(this.TooltipTitleTerm);
		return MewtationsLoc.Translate(this.TooltipTextTerm);
	}

	public string TooltipTextTerm;

	public string TooltipTitleTerm;
}
