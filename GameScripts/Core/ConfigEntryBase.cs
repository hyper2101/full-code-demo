using System;

public abstract class ConfigEntryBase
{
	public abstract object BoxedValue { get; set; }

	public string Name;

	public Type ValueType;

	public ConfigFile Config;

	public ConfigUI UI = new ConfigUI();
}
