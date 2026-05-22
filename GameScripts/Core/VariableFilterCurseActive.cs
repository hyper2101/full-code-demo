using System;

[Serializable]
public class VariableFilterCurseActive : VariableFilter
{
	public override bool IsMet()
	{
		return WorldManager.instance == null || WorldManager.instance.CurseIsActive(this.Curse);
	}

	public CurseType Curse;
}
