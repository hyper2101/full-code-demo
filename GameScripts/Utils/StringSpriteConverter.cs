using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

internal class StringSpriteConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Sprite);
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
			obj = ResourceHelper.LoadSpriteFromPath(Path.Combine(GameDataLoader.instance.CurrentlyLoadingMod.Path, "Icons", reader.Value.ToString()));
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Debug.LogWarning(string.Format("Failed to read sprite from path Icons/{0}", reader.Value));
			obj = null;
		}
		return obj;
	}
}
