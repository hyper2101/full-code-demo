using System;
using UnityEngine;

public class ConflictRectangleDrawer : ShapeDrawer
{
	public ConflictRectangle Rectangle
	{
		get
		{
			return (ConflictRectangle)base.MyShape;
		}
	}

	public override Type DrawingType
	{
		get
		{
			return typeof(ConflictRectangle);
		}
	}

	private void Awake()
	{
		this.propBlock = new MaterialPropertyBlock();
	}

	public override void UpdateShape()
	{
		base.transform.position = this.Rectangle.Center;
		this.Renderer.transform.localScale = new Vector3(this.Rectangle.Size.x, this.Rectangle.Size.y, 1f) + Vector3.one;
		this.Renderer.GetPropertyBlock(this.propBlock);
		this.propBlock.SetVector("_Size", new Vector4(this.Rectangle.Size.x, this.Rectangle.Size.y));
		this.Renderer.SetPropertyBlock(this.propBlock);
	}

	public MeshRenderer Renderer;

	private MaterialPropertyBlock propBlock;
}
