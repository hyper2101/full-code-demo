using Mewtations.Core;
using System;
using UnityEngine;

[Mewtations.Core.LegacySystem(Mewtations.Core.LegacyCategory.DeprecatedAutomation)]
    public class EnergyCableDrawer : TopologyGraphRenderer
{
	public override Type DrawingType
	{
		get
		{
			return typeof(EnergyCable);
		}
	}

	private void Awake()
	{
		this.propBlock = new MaterialPropertyBlock();
	}

	private Material GetCurrentMaterial(EnergyCable cable)
	{
		if (WorldManager.instance.CurrentView != ViewType.Energy)
		{
			return this.BehindMaterial;
		}
		if (cable.IsLowVoltage)
		{
			return this.LowVoltageMaterial;
		}
		return this.HighVoltageMaterial;
	}

	public override void UpdateShape()
	{
		EnergyCable energyCable = (EnergyCable)base.MyShape;
		this.CableRenderer.sharedMaterial = this.GetCurrentMaterial(energyCable);
		this.CableRenderer.GetPropertyBlock(this.propBlock);
		this.propBlock.SetVector(this.start, new Vector4(energyCable.Start.x, energyCable.Start.z));
		this.propBlock.SetVector(this.end, new Vector4(energyCable.End.x, energyCable.End.z));
		this.propBlock.SetVector(this.middle, new Vector4(energyCable.Middle.x, energyCable.Middle.z));
		this.CableRenderer.SetPropertyBlock(this.propBlock);
		Vector3 vector = Vector3.Lerp(energyCable.Start, energyCable.End, 0.5f);
		vector.y = Mathf.Min(energyCable.Start.y, energyCable.End.y);
		if (WorldManager.instance.CurrentView != ViewType.Energy)
		{
			vector.y = 0f;
		}
		Vector3 vector2 = new Vector3(Mathf.Abs(energyCable.End.x - energyCable.Start.x), 1f, Mathf.Abs(energyCable.End.z - energyCable.Start.z));
		base.transform.position = vector;
		this.CableRenderer.transform.localScale = new Vector3(vector2.x + 1.5f, vector2.z + 1.5f, 1f);
	}

	public Renderer CableRenderer;

	private MaterialPropertyBlock propBlock;

	public Material LowVoltageMaterial;

	public Material BehindMaterial;

	public Material HighVoltageMaterial;

	private int start = Shader.PropertyToID("_Start");

	private int end = Shader.PropertyToID("_End");

	private int middle = Shader.PropertyToID("_Middle");
}


