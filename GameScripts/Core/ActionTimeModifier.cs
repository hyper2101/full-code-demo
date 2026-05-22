using System;

public class ActionTimeModifier
{
	public ActionTimeModifier(ActionTimeModifier.ActionTimeModifierFunc modifySpeedFunc, float modifier)
	{
		this.Matches = modifySpeedFunc;
		this.SpeedModifier = modifier;
	}

	public float SpeedModifier;

	public ActionTimeModifier.ActionTimeModifierFunc Matches;

	public delegate bool ActionTimeModifierFunc(ActionTimeParams parameters);
}
