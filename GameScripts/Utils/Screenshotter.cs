using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Screenshotter : MonoBehaviour
{
	private void Awake()
	{
		Screenshotter.instance = this;
	}

	private void Start()
	{
		List<ScreenshotDescription> list = new List<ScreenshotDescription>();
		list.Add(new ScreenshotDescription(1920, 1080)
		{
			Description = "schinese",
			Language = "Chinese (Simplified)"
		});
		list.Add(new ScreenshotDescription(1920, 1080)
		{
			Description = "tchinese",
			Language = "Chinese (Traditional)"
		});
		list.Add(new ScreenshotDescription(1920, 1080)
		{
			Description = "koreana",
			Language = "Korean"
		});
		list.Add(new ScreenshotDescription(1920, 1080)
		{
			Description = "english",
			Language = "English"
		});
		this.Descriptions.AddRange(list);
	}

	private void LateUpdate()
	{
		if (InputController.instance.GetKeyDown(Key.F7))
		{
			base.StartCoroutine(this.TakeAllScreenshots());
		}
	}

	private IEnumerator TakeAllScreenshots()
	{
		DateTime curTime = DateTime.Now;
		foreach (ScreenshotDescription screenshotDescription in this.Descriptions)
		{
			if (screenshotDescription.IncludeInScreenshots)
			{
				screenshotDescription.TakenAt = curTime;
				yield return this.TakeScreenshot(screenshotDescription);
			}
		}
		List<ScreenshotDescription>.Enumerator enumerator = default(List<ScreenshotDescription>.Enumerator);
		yield break;
		yield break;
	}

	public IEnumerator TakeScreenshot(ScreenshotDescription sd)
	{
		this.IsScreenshotting = true;
		GameCanvas.instance.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
		GameCanvas.instance.Canvas.worldCamera = GameCamera.instance.MyCam;
		GameCanvas.instance.Canvas.sortingOrder = 2;
		GameCanvas.instance.Canvas.sortingLayerName = "Above";
		if (sd.ShowUI)
		{
			GameCanvas.instance.SetUIToggle(true);
		}
		else
		{
			GameCanvas.instance.SetUIToggle(false);
		}
		string originalLanguage = MewtationsLoc.instance.CurrentLanguage;
		MewtationsLoc.instance.SetLanguage(sd.Language);
		if (sd.ControlSchemeOverride != null)
		{
			InputController.instance.SchemeOverride = sd.ControlSchemeOverride;
		}
		for (int i = 0; i < 5; i++)
		{
			Canvas.ForceUpdateCanvases();
		}
		if (GameCanvas.instance != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(GameCanvas.instance.transform as RectTransform);
		}
		yield return null;
		yield return new WaitForEndOfFrame();
		bool flag;
		string text;
		Screenshotter.MakeScreenshot(sd, out flag, out text);
		GameCanvas.instance.SetUIToggle(true);
		Canvas.ForceUpdateCanvases();
		MewtationsLoc.instance.SetLanguage(originalLanguage);
		GameCanvas.instance.Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		InputController.instance.SchemeOverride = null;
		if (flag)
		{
			Debug.Log("Screenshot saved to " + text);
		}
		this.IsScreenshotting = false;
		yield break;
	}

	public static void MakeScreenshot(ScreenshotDescription sd, out bool success, out string targetPath)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(new RenderTextureDescriptor(sd.Width, sd.Height, RenderTextureFormat.ARGB32, 32)
		{
			sRGB = true,
			stencilFormat = GraphicsFormat.R8_UInt
		});
		temporary.antiAliasing = 8;
		Camera.main.targetTexture = temporary;
		Camera.main.Render();
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = temporary;
		Texture2D texture2D = new Texture2D(temporary.width, temporary.height, sd.AlphaBackground ? TextureFormat.ARGB32 : TextureFormat.RGB24, false, true);
		texture2D.ReadPixels(new Rect(0f, 0f, (float)temporary.width, (float)temporary.height), 0, 0);
		texture2D.Apply();
		Screenshotter.WriteTexture(texture2D, sd, out success, out targetPath);
		Camera.main.targetTexture = null;
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
	}

	private static void WriteTexture(Texture2D tex, ScreenshotDescription desc, out bool success, out string targetPath)
	{
		string text = Path.Combine(Application.persistentDataPath, "screenshots");
		Directory.CreateDirectory(text);
		success = true;
		string text2 = string.Format("{0} {1}.png", desc.TakenAt.ToFileTimeUtc(), desc.Description);
		targetPath = Path.Combine(text, text2);
		try
		{
			File.WriteAllBytes(targetPath, tex.EncodeToPNG());
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Format("Saving screenshot failed\n{0}", ex));
			success = false;
		}
	}

	public static Screenshotter instance;

	[HideInInspector]
	public List<ScreenshotDescription> Descriptions = new List<ScreenshotDescription>();

	public bool IsScreenshotting;
}
