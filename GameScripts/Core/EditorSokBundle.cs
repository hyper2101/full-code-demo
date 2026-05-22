using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorSokBundle : ISokBundle
{
	public bool Load(string id)
	{
		this.id = id;
		return true;
	}

	public List<T> LoadAssets<T>() where T : Object
	{
		return new List<T>();
	}

	private string id;
}
