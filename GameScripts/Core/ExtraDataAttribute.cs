using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ExtraDataAttribute : Attribute
{
	public ExtraDataAttribute(string id)
	{
		this.Identifier = id;
	}

	public string Identifier;
}
