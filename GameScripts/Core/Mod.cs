using System;
using HarmonyLib;
using UnityEngine;

public class Mod : MonoBehaviour
{
	public virtual void Ready()
	{
	}

	public virtual object Call(params object[] args)
	{
		return null;
	}

	public ModManifest Manifest;

	protected internal ModLogger Logger;

	protected internal Harmony Harmony;

	public ConfigFile Config;

	public string Path;
}
