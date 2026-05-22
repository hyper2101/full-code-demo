using System;
using UnityEngine;

public class TransportArrowMainlandDrawer : ShapeDrawer
{
	public TransportArrowMainland Cable
	{
		get
		{
			return (TransportArrowMainland)base.MyShape;
		}
	}

	public override Type DrawingType
	{
		get
		{
			return typeof(TransportArrowMainland);
		}
	}

	private void Awake()
	{
		this.propBlock = new MaterialPropertyBlock();
	}

	public override void UpdateShape()
	{
		this.Renderer.sharedMaterial = ((WorldManager.instance.CurrentView == ViewType.Transport) ? this.FrontMaterial : this.BehindMaterial);
		this.Renderer.GetPropertyBlock(this.propBlock);
		this.propBlock.SetVector("_Start", new Vector4(this.Cable.Start.x, this.Cable.Start.z));
		this.propBlock.SetVector("_End", new Vector4(this.Cable.End.x, this.Cable.End.z));
		this.propBlock.SetVector("_Middle", new Vector4(this.Cable.Middle.x, this.Cable.Middle.z));
		this.Renderer.SetPropertyBlock(this.propBlock);
		Vector3 vector = Vector3.Lerp(this.Cable.Start, this.Cable.End, 0.5f);
		vector.y = Mathf.Min(this.Cable.Start.y, this.Cable.End.y);
		if (WorldManager.instance.CurrentView != ViewType.Transport)
		{
			vector.y = 0f;
		}
		Vector3 vector2 = new Vector3(Mathf.Abs(this.Cable.End.x - this.Cable.Start.x), 1f, Mathf.Abs(this.Cable.End.z - this.Cable.Start.z));
		base.transform.position = vector;
		this.Renderer.transform.localScale = new Vector3(vector2.x + 1.5f, vector2.z + 1.5f, 1f);
	}

	public Renderer Renderer;

	private MaterialPropertyBlock propBlock;

	public Material FrontMaterial;

	public Material BehindMaterial;
}
