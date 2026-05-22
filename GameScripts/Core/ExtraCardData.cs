using System;
using UnityEngine;

[Serializable]
public class ExtraCardData
{
	public ExtraCardData(string attributeId, string value)
	{
		this.AttributeId = attributeId;
		this.StringValue = value;
	}

	public ExtraCardData(string attributeId, int value)
	{
		this.AttributeId = attributeId;
		this.IntValue = value;
	}

	public ExtraCardData(string attributeId, float value)
	{
		this.AttributeId = attributeId;
		this.FloatValue = value;
	}

	public ExtraCardData(string attributeId, Vector3 value)
	{
		this.AttributeId = attributeId;
		this.VectorValue = value;
	}

	public ExtraCardData(string attributeId, bool value)
	{
		this.AttributeId = attributeId;
		this.BoolValue = value;
	}

	public override string ToString()
	{
		return string.Format("{0} - {1} - {2} - {3}", new object[] { this.AttributeId, this.StringValue, this.IntValue, this.FloatValue });
	}

	public string AttributeId;

	public string StringValue;

	public int IntValue;

	public float FloatValue;

	public Vector3 VectorValue;

	public bool BoolValue;
}
