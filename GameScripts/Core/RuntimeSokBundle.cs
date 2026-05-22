using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RuntimeSokBundle : ISokBundle
{
	public bool Load(string id)
	{
		string text = "";
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			text = Path.Combine(new string[]
			{
				Application.dataPath,
				"../",
				id,
				"PC",
				id
			});
		}
		else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
		{
			text = Path.Combine(new string[]
			{
				Application.dataPath,
				"../../",
				id,
				"macOS",
				id
			});
		}
		this.myAssetBundle = AssetBundle.LoadFromFile(text);
		if (this.myAssetBundle == null)
		{
			Debug.LogError("No asset bundle found at path " + Path.GetFullPath(text));
			return false;
		}
		return true;
	}

	public List<T> LoadAssets<T>() where T : Object
	{
		return this.myAssetBundle.LoadAllAssets<T>().ToList<T>();
	}

	private AssetBundle myAssetBundle;
}
