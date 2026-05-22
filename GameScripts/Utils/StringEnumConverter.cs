using System;
using Newtonsoft.Json;
using UnityEngine;

internal class StringEnumConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType.IsEnum;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		object obj;
		try
		{
			obj = (Enum)Enum.ToObject(objectType, EnumHelper.ParseEnum(objectType, reader.Value.ToString(), null));
		}
		catch (Exception)
		{
			Debug.LogWarning(string.Format("Failed to parse enum ({0}) {1}", objectType, reader.Value.ToString()));
			obj = (Enum)Enum.ToObject(objectType, 0);
		}
		return obj;
	}
}
