using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ReflectionHelper
{
	public static IEnumerable<Type> GetSafeTypes(this Assembly assembly)
	{
		IEnumerable<Type> enumerable;
		try
		{
			enumerable = assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			enumerable = ex.Types.Where<Type>((Type x) => x != null);
		}
		catch (Exception)
		{
			enumerable = new List<Type>();
		}
		return enumerable;
	}
}
