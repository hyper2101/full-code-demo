using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CutsceneStep_Pause : CutsceneStep
{
	public override IEnumerator Process()
	{
		yield return new WaitForSeconds(this.Time);
		yield break;
	}

	public float Time;
}
