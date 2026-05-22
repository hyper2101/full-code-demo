using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnumHelper
{
	public static int ExtendEnum(Type enumType, string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Name is either an empty string or only contains whitespace");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException("Type " + enumType.ToString() + " is not an Enum");
		}
		int hashCode = enumType.GetHashCode();
		if (!EnumHelper.Stuff.ContainsKey(hashCode))
		{
			EnumHelper.Stuff[hashCode] = new Dictionary<string, int>();
		}
		if (EnumHelper.Stuff[hashCode].ContainsKey(name.ToLower()))
		{
			return EnumHelper.Stuff[hashCode][name.ToLower()];
		}
		int num = Enum.GetValues(enumType).Length + EnumHelper.Stuff[hashCode].Count;
		EnumHelper.Stuff[hashCode][name.ToLower()] = num;
		return num;
	}

	public static T ExtendEnum<T>(string name) where T : Enum
	{
		return (T)((object)Enum.ToObject(typeof(T), EnumHelper.ExtendEnum(typeof(T), name)));
	}

	public static int ParseEnum(Type enumType, string name, int? defaultValue = null)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Name is either an empty string or only contains whitespace");
		}
		if (!enumType.IsEnum)
		{
			throw new ArgumentException("Type " + enumType.ToString() + " is not an Enum");
		}
		int hashCode = enumType.GetHashCode();
		if (EnumHelper.Stuff.ContainsKey(hashCode) && EnumHelper.Stuff[hashCode].ContainsKey(name.ToLower()))
		{
			return EnumHelper.Stuff[hashCode][name.ToLower()];
		}
		int num;
		try
		{
			num = (int)Enum.Parse(enumType, name, true);
		}
		catch (Exception ex)
		{
			if (defaultValue == null)
			{
				throw ex;
			}
			Debug.LogWarning(string.Format("Could not parse '{0}' as {1}, falling back to {2}.{3}", new object[]
			{
				name,
				enumType,
				enumType,
				Enum.ToObject(enumType, defaultValue)
			}));
			num = defaultValue.Value;
		}
		return num;
	}

	public static T ParseEnum<T>(string name, int? defaultValue = null) where T : Enum
	{
		return (T)((object)Enum.ToObject(typeof(T), EnumHelper.ParseEnum(typeof(T), name, defaultValue)));
	}

	public static string GetName<T>(int num) where T : Enum
	{
		return EnumHelper.GetNames<T>()[num];
	}

	public static string GetName(Type enumType, int num)
	{
		return EnumHelper.GetNames(enumType)[num];
	}

	public static List<string> GetNames(Type enumType)
	{
		if (!enumType.IsEnum)
		{
			throw new ArgumentException("Type " + enumType.ToString() + " is not an Enum");
		}
		List<string> list = Enum.GetNames(enumType).ToList<string>();
		int hashCode = enumType.GetHashCode();
		if (EnumHelper.Stuff.ContainsKey(hashCode))
		{
			list.AddRange(EnumHelper.Stuff[hashCode].Keys);
		}
		return list;
	}

	public static List<string> GetNames<T>() where T : Enum
	{
		return EnumHelper.GetNames(typeof(T));
	}

	public static Dictionary<int, Dictionary<string, int>> Stuff = new Dictionary<int, Dictionary<string, int>>();
}
