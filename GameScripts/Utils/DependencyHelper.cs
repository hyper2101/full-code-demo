using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DependencyHelper
{
	public static List<ModManifest> GetValidModLoadOrder(List<ModManifest> mods)
	{
		List<ModManifest> list = new List<ModManifest>();
		ModManifest modManifest = new ModManifest
		{
			Id = "Stacklands"
		};
		foreach (ModManifest modManifest2 in mods)
		{
			modManifest.Dependencies.Add(modManifest2.Id);
		}
		DependencyHelper.Resolve(mods, modManifest, list, new List<ModManifest>());
		return list.Take<ModManifest>(list.Count - 1).ToList<ModManifest>();
	}

	private static void Resolve(List<ModManifest> mods, ModManifest node, List<ModManifest> resolved, List<ModManifest> unresolved)
	{
		unresolved.Add(node);
		using (List<string>.Enumerator enumerator = node.Dependencies.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				string edge2 = enumerator.Current;
				if (resolved.Find((ModManifest x) => x.Id == edge2) == null)
				{
					if (unresolved.Find((ModManifest x) => x.Id == edge2) != null)
					{
						throw new Exception("CIRCULAR DEP " + node.Id + "<->" + edge2);
					}
					ModManifest modManifest = mods.Find((ModManifest x) => x.Id == edge2);
					if (modManifest == null)
					{
						throw new Exception("COULD NOT FIND " + edge2);
					}
					DependencyHelper.Resolve(mods, modManifest, resolved, unresolved);
				}
			}
		}
		using (List<string>.Enumerator enumerator = node.OptionalDependencies.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				string edge = enumerator.Current;
				if (resolved.Find((ModManifest x) => x.Id == edge) == null)
				{
					if (unresolved.Find((ModManifest x) => x.Id == edge) != null)
					{
						throw new Exception("CIRCULAR DEP " + node.Id + "<->" + edge);
					}
					ModManifest modManifest2 = mods.Find((ModManifest x) => x.Id == edge);
					if (modManifest2 != null)
					{
						DependencyHelper.Resolve(mods, modManifest2, resolved, unresolved);
					}
					else
					{
						Debug.LogWarning("Missing optional dependency for " + node.Id + ": " + edge);
					}
				}
			}
		}
		resolved.Add(node);
		unresolved.Remove(node);
	}
}
