using System;

public class BlueprintAdmireCoin : Blueprint
{
	public override bool CanCurrentlyBeMade
	{
		get
		{
			return WorldManager.instance.CurseIsActive(CurseType.Happiness);
		}
	}
}
