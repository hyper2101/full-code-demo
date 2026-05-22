using System;
using System.Collections.Generic;

public class ConfigUI
{
	public string GetName()
	{
		if (string.IsNullOrEmpty(this.NameTerm))
		{
			return this.Name;
		}
		return SokLoc.Translate(this.NameTerm);
	}

	public string GetTooltip()
	{
		if (string.IsNullOrEmpty(this.TooltipTerm))
		{
			return this.Tooltip;
		}
		return SokLoc.Translate(this.TooltipTerm);
	}

	public string Name;

	public string NameTerm;

	public string Tooltip;

	public string TooltipTerm;

	public string PlaceholderText;

	public bool RestartAfterChange;

	public Action<ConfigEntryBase> OnUI;

	public bool Hidden;

	public Dictionary<string, string> ExtraData = new Dictionary<string, string>();
}
