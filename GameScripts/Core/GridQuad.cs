using System;
using UnityEngine;

public class GridQuad : MonoBehaviour
{
	private void Update()
	{
		Vector3 vector = GameCamera.instance.ScreenPosToWorldPos(new Vector3((float)Screen.width, (float)Screen.height) * 0.5f);
		vector.y = 0.2f;
		base.transform.position = vector;
		this.meshRenderer.enabled = WorldManager.instance.gridAlpha >= 0.001f;
	}

	public MeshRenderer meshRenderer;
}
