using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CutsceneStep_PanCamera : CutsceneStep
{
	public override IEnumerator Process()
	{
		GameCamera.instance.TargetCardOverride = null;
		float num = GameCamera.instance.MaxZoom + WorldManager.instance.CurrentBoard.WorldSizeIncrease;
		Bounds worldBounds = WorldManager.instance.CurrentBoard.WorldBounds;
		Vector3 left = new Vector3(worldBounds.center.x - worldBounds.extents.x * 0.8f, num, worldBounds.center.z);
		Vector3 right = new Vector3(worldBounds.center.x + worldBounds.extents.x * 0.8f, num, worldBounds.center.z);
		GameCamera.instance.TargetPositionOverride = new Vector3?(left);
		GameCamera.instance.CameraPositionDistanceOverride = new float?(this.CameraDistance);
		yield return new WaitForSeconds(1f);
		float timer = 0f;
		while (timer <= this.Duration)
		{
			timer += Time.deltaTime;
			GameCamera.instance.TargetPositionOverride = new Vector3?(Vector3.Lerp(left, right, timer / this.Duration));
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		GameCamera.instance.TargetPositionOverride = null;
		GameCamera.instance.CameraPositionDistanceOverride = null;
		yield break;
	}

	public float Duration = 5f;

	public float CameraDistance = 12f;
}
