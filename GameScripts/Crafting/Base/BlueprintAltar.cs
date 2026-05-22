using System;

public class BlueprintAltar : Blueprint
{
	public override bool CanCurrentlyBeMade
	{
		get
		{
			return WorldManager.instance.CurrentBoard.Id == "main" || WorldManager.instance.CurrentBoard.Id == "island";
		}
	}
}
