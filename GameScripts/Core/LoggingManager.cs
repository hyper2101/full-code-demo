using System;
using System.Collections.Generic;
using ImGuiNET;
using UImGui;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoggingManager : MonoBehaviour
{
	private void Awake()
	{
		LoggingManager.instance = this;
		Application.logMessageReceived += this.HandleLog;
		UImGuiUtility.Layout += this.DrawLogViewer;
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		if (!logString.StartsWith("["))
		{
			logString = string.Format("[{0}] [{1} : Stacklands] {2}{3}", new object[]
			{
				DateTime.Now.ToString("HH:mm:ss"),
				type,
				logString,
				(type == LogType.Exception) ? ("\n" + stackTrace) : ""
			});
		}
		LoggingManager.Logs.Add(new ValueTuple<int, string>((int)type, logString));
	}

	private void Update()
	{
		if (Keyboard.current[Key.F3].wasPressedThisFrame)
		{
			this.LogViewerEnabled = !this.LogViewerEnabled;
		}
	}

	private void DrawLogViewer(global::UImGui.UImGui _)
	{
		if (!this.LogViewerEnabled)
		{
			return;
		}
		ImGui.SetNextWindowSize(new Vector2(520f, 600f), ImGuiCond.FirstUseEver);
		ImGui.Begin("Log");
		if (ImGui.Button("Open log file"))
		{
			Application.OpenURL("file:///" + Application.consoleLogPath);
		}
		ImGui.SameLine();
		if (ImGui.Button("Clear log"))
		{
			LoggingManager.Logs.Clear();
		}
		ImGui.Separator();
		if (ImGui.BeginChild("logsection", new Vector2(0f, -(ImGui.GetStyle().ItemSpacing.y + ImGui.GetFrameHeightWithSpacing())), false, ImGuiWindowFlags.HorizontalScrollbar))
		{
			ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4f, 1f));
			foreach (ValueTuple<int, string> valueTuple in LoggingManager.Logs)
			{
				int item = valueTuple.Item1;
				string item2 = valueTuple.Item2;
				bool flag = false;
				Vector4 zero = Vector4.zero;
				if (item == 2)
				{
					flag = true;
					zero = new Vector4(1f, 1f, 0f, 1f);
				}
				if (item == 0 || item == 4)
				{
					flag = true;
					zero = new Vector4(1f, 0f, 0f, 1f);
				}
				if (flag)
				{
					ImGui.PushStyleColor(ImGuiCol.Text, zero);
				}
				ImGui.TextUnformatted(item2);
				if (flag)
				{
					ImGui.PopStyleColor();
				}
			}
			ImGui.PopStyleVar();
		}
		ImGui.EndChild();
		ImGui.End();
	}

	public static List<ValueTuple<int, string>> Logs = new List<ValueTuple<int, string>>();

	public static LoggingManager instance;

	public bool LogViewerEnabled;
}
