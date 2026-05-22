using System;

public class ActionTimeBase
{
	public ActionTimeBase(ActionTimeBase.ActionTimeBaseSpeedFunc baseSpeedFunc, float baseSpeed)
	{
		this.Matches = baseSpeedFunc;
		this.BaseSpeed = baseSpeed;
	}

	public ActionTimeBase.ActionTimeBaseSpeedFunc Matches;

	public float BaseSpeed;

	public delegate bool ActionTimeBaseSpeedFunc(ActionTimeParams parameters);
}
