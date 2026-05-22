using System;
using UnityEngine;

[Serializable]
public class PlacedSprite
{
	public float Left
	{
		get
		{
			return this.Position.x - this.Size.x * 0.5f;
		}
	}

	public float Right
	{
		get
		{
			return this.Position.x + this.Size.x * 0.5f;
		}
	}

	public float Top
	{
		get
		{
			return this.Position.y + this.Size.y * 0.5f;
		}
	}

	public float Bottom
	{
		get
		{
			return this.Position.y - this.Size.y * 0.5f;
		}
	}

	public Vector2 Position;

	public Vector2 Size;

	public Sprite Sprite;

	public bool IsVisible;

	public Transform Transform;
}
