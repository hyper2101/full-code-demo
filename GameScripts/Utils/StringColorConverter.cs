using System;
using Newtonsoft.Json;
using UnityEngine;

internal class StringColorConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Color);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Color color;
		if (ColorUtility.TryParseHtmlString((string)reader.Value, out color))
		{
			return color;
		}
		Debug.LogWarning(string.Format("Failed to parse color \"{0}\"", reader.Value));
		return Color.black;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue(ColorUtility.ToHtmlStringRGBA((Color)value));
	}
}
