using System;
using System.Collections;

[Serializable]
public class CutsceneStep_WaitForContinueClick : CutsceneStep
{
	public override IEnumerator Process()
	{
		yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate(this.ButtonTerm));
		yield break;
	}

	[Term]
	public string ButtonTerm;
}
