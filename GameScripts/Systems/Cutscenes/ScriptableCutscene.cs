using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableCutscene", menuName = "ScriptableObjects/ScriptableCutscene", order = 1)]
public class ScriptableCutscene : ScriptableObject
{
	public bool IsCitiesCutscene
	{
		get
		{
			return this.CutsceneId.StartsWith("cities");
		}
	}

	public string CutsceneId;

	public bool IsAdvisorCutscene;

	public bool AdvisorWarning;

	[SerializeReference]
	public List<CutsceneStep> CutsceneSteps = new List<CutsceneStep>();
}
