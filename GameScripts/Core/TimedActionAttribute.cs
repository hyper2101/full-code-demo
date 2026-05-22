using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TimedActionAttribute : Attribute
{
	public TimedActionAttribute(string id)
	{
		this.Identifier = id;
	}

	public string Identifier;
}
