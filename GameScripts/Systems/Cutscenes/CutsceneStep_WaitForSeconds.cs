using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CutsceneStep_WaitForSeconds : CutsceneStep
{
	public override IEnumerator Process()
	{
		yield return new WaitForSeconds(this.Seconds);
		yield break;
	}

	public float Seconds;
}
