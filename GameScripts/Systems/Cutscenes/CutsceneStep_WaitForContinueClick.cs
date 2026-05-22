using System;
using System.Collections;

[Serializable]
public class CutsceneStep_WaitForContinueClick : CutsceneStep
{
	public override IEnumerator Process()
	{
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate(this.ButtonTerm));
		yield break;
	}

	[Term]
	public string ButtonTerm;
}
