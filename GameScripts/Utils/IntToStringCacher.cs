using System;
using System.Collections.Generic;

public static class IntToStringCacher
{
	public static string ToStringCached(this int i)
	{
		string text;
		if (!IntToStringCacher.cache.TryGetValue(i, out text))
		{
			IntToStringCacher.cache[i] = i.ToString();
			return IntToStringCacher.cache[i];
		}
		return text;
	}

	public static Dictionary<int, string> cache = new Dictionary<int, string>();
}
