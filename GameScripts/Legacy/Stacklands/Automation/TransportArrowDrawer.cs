using Mewtations.Core;
using System;
using UnityEngine;

[Mewtations.Core.LegacySystem(Mewtations.Core.LegacyCategory.DeprecatedAutomation)]
    public class TransportArrowDrawer : TopologyGraphRenderer
{
	public override Type DrawingType
	{
		get
		{
			return typeof(TransportArrow);
		}
	}

	private void Awake()
	{
		this.propBlock = new MaterialPropertyBlock();
	}

	public override void UpdateShape()
	{
		TransportArrow transportArrow = (TransportArrow)base.MyShape;
		this.Renderer.sharedMaterial = ((WorldManager.instance.CurrentView == ViewType.Transport) ? this.FrontMaterial : this.BehindMaterial);
		this.Renderer.GetPropertyBlock(this.propBlock);
		this.propBlock.SetVector(this.start, new Vector4(transportArrow.Start.x, transportArrow.Start.z));
		this.propBlock.SetVector(this.end, new Vector4(transportArrow.End.x, transportArrow.End.z));
		this.propBlock.SetVector(this.middle, new Vector4(transportArrow.Middle.x, transportArrow.Middle.z));
		this.Renderer.SetPropertyBlock(this.propBlock);
		Vector3 vector = Vector3.Lerp(transportArrow.Start, transportArrow.End, 0.5f);
		vector.y = Mathf.Min(transportArrow.Start.y, transportArrow.End.y);
		if (WorldManager.instance.CurrentView != ViewType.Transport)
		{
			vector.y = 0f;
		}
		Vector3 vector2 = new Vector3(Mathf.Abs(transportArrow.End.x - transportArrow.Start.x), 1f, Mathf.Abs(transportArrow.End.z - transportArrow.Start.z));
		base.transform.position = vector;
		this.Renderer.transform.localScale = new Vector3(vector2.x + 1.5f, vector2.z + 1.5f, 1f);
	}

	public Renderer Renderer;

	private MaterialPropertyBlock propBlock;

	public Material FrontMaterial;

	public Material BehindMaterial;

	private int start = Shader.PropertyToID("_Start");

	private int end = Shader.PropertyToID("_End");

	private int middle = Shader.PropertyToID("_Middle");
}


