using System;

[Serializable]
public class VariableFilterIsRunWithCurse : VariableFilter
{
	public override bool IsMet()
	{
		return WorldManager.instance == null || (this.Curse == CurseType.Death && WorldManager.instance.CurrentRunOptions.IsDeathEnabled) || (this.Curse == CurseType.Happiness && WorldManager.instance.CurrentRunOptions.IsHappinessEnabled);
	}

	public CurseType Curse;
}
