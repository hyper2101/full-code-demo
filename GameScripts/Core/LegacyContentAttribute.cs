using System;

[AttributeUsage(AttributeTargets.Class)]
public class LegacyContentAttribute : Attribute
{
    public string Origin;
    public string Reason;
    public int RemovalPhase;
}
