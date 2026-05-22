using System;
using UnityEngine;

public class SewerPipeDrawer : ShapeDrawer
{
	public override Type DrawingType
	{
		get
		{
			return typeof(SewerPipe);
		}
	}

	private void Awake()
	{
		this.propBlock = new MaterialPropertyBlock();
	}

	public override void UpdateShape()
	{
		SewerPipe sewerPipe = (SewerPipe)base.MyShape;
		this.Renderer.sharedMaterial = ((WorldManager.instance.CurrentView == ViewType.Sewer) ? this.SewerFrontMaterial : this.SewerBehindMaterial);
		this.Renderer.GetPropertyBlock(this.propBlock);
		this.propBlock.SetVector("_Start", new Vector4(sewerPipe.Start.x, sewerPipe.Start.z));
		this.propBlock.SetVector("_End", new Vector4(sewerPipe.End.x, sewerPipe.End.z));
		this.propBlock.SetVector("_Middle", new Vector4(sewerPipe.Middle.x, sewerPipe.Middle.z));
		this.Renderer.SetPropertyBlock(this.propBlock);
		Vector3 vector = Vector3.Lerp(sewerPipe.Start, sewerPipe.End, 0.5f);
		vector.y = Mathf.Min(sewerPipe.Start.y, sewerPipe.End.y);
		if (WorldManager.instance.CurrentView != ViewType.Sewer)
		{
			vector.y = 0f;
		}
		Vector3 vector2 = new Vector3(Mathf.Abs(sewerPipe.End.x - sewerPipe.Start.x), 1f, Mathf.Abs(sewerPipe.End.z - sewerPipe.Start.z));
		base.transform.position = vector;
		this.Renderer.transform.localScale = new Vector3(vector2.x + 1.5f, vector2.z + 1.5f, 1f);
	}

	public Renderer Renderer;

	private MaterialPropertyBlock propBlock;

	public Material SewerFrontMaterial;

	public Material SewerBehindMaterial;
}
