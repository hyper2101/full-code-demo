using System;
using System.IO;
using UnityEngine;

public static class FileHelper
{
	public static string LoadFile(string id)
	{
		string text = Path.Combine(PlatformHelper.CurrentSavesDirectory, "save_" + id + ".sav");
		if (!File.Exists(text))
		{
			return "";
		}
		return File.ReadAllText(text);
	}

	public static bool SaveFile(string id, string content)
	{
		string text = Path.Combine(PlatformHelper.CurrentSavesDirectory, "save_" + id + ".sav");
		try
		{
			File.WriteAllText(text, content);
		}
		catch (Exception ex)
		{
			Debug.Log(string.Format("Exception while writing save '{0}'. {1}", id, ex));
			return false;
		}
		return true;
	}

	public static bool SaveFile(string id, string content, string subDir)
	{
		string text = Path.Combine(PlatformHelper.CurrentSavesDirectory, subDir);
		FileHelper.MakeOrCreatePath(text);
		string text2 = Path.Combine(text, "save_" + id + ".sav");
		try
		{
			File.WriteAllText(text2, content);
		}
		catch (Exception ex)
		{
			Debug.Log(string.Format("Exception while writing save '{0}'. {1}", id, ex));
			return false;
		}
		return true;
	}

	public static void DeleteFile(string id)
	{
		string text = Path.Combine(PlatformHelper.CurrentSavesDirectory, "save_" + id + ".sav");
		try
		{
			File.Delete(text);
		}
		catch (Exception ex)
		{
			Debug.Log(string.Format("Exception while deleting save '{0}'. {1}", id, ex));
		}
	}

	public static void MakeOrCreatePath(string path)
	{
		if (Directory.Exists(path))
		{
			return;
		}
		Directory.CreateDirectory(path);
	}

	public static void ArchiveFile(string fullFileName)
	{
		string text = Path.Combine(PlatformHelper.CurrentSavesDirectory, "SaveArchive");
		FileInfo fileInfo = new FileInfo(fullFileName);
		string text2 = Path.Combine(text, fileInfo.Name + ".sav");
		FileHelper.MakeOrCreatePath(text);
		try
		{
			File.Move(fullFileName, text2);
		}
		catch (Exception ex)
		{
			Debug.Log(string.Format("Exception while deleting save '{0}'. {1}", fullFileName, ex));
		}
	}

	public static string LoadPresetFile(string id)
	{
		string text = Path.Combine(Application.dataPath + "/PresetSaves", "preset_" + id + ".json");
		if (!File.Exists(text))
		{
			return "";
		}
		return File.ReadAllText(text);
	}

	public static bool SavePresetFile(string id, string content)
	{
		string text = Path.Combine(Application.dataPath + "/PresetSaves", "preset_" + id + ".json");
		try
		{
			File.WriteAllText(text, content);
		}
		catch (Exception ex)
		{
			Debug.Log(string.Format("Exception while writing save '{0}'. {1}", id, ex));
			return false;
		}
		return true;
	}
}
