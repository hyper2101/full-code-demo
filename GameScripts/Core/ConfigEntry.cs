using System;

public class ConfigEntry<T> : ConfigEntryBase
{
	public T Value
	{
		get
		{
			return (T)((object)(this._cached ? this._cachedValue : this.Config.GetValue<T>(this.Name)));
		}
		set
		{
			this._cached = true;
			this._cachedValue = value;
			this.Config.SetValue(this.Name, value);
			Action<T> onChanged = this.OnChanged;
			if (onChanged == null)
			{
				return;
			}
			onChanged(value);
		}
	}

	public override object BoxedValue
	{
		get
		{
			return this.Value;
		}
		set
		{
			this.Value = (T)((object)value);
		}
	}

	public ConfigEntry(string name, ConfigFile config, object defaultValue = null, ConfigUI ui = null)
	{
		this.Name = name;
		this.Config = config;
		this.BoxedValue = this.Config.GetValue(this.Name, typeof(T)) ?? defaultValue;
		this.ValueType = typeof(T);
		this._cached = true;
		if (ui != null)
		{
			this.UI = ui;
		}
		this.Config.Entries.Add(this);
	}

	internal object _cachedValue;

	internal bool _cached;

	public Action<T> OnChanged;
}
