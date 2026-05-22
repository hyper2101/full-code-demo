using System;

public class BlueprintHappiness : Blueprint
{
	public override bool CanCurrentlyBeMade
	{
		get
		{
			return WorldManager.instance.CurseIsActive(CurseType.Happiness);
		}
	}
}
