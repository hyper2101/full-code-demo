using System;
using UnityEngine;

public class CardAnimation
{
	public virtual void Start()
	{
		this.Position = (this.TargetPosition = this.StartPosition);
	}

	public virtual void Update()
	{
		this.timer += Time.deltaTime;
	}

	public Vector3 StartPosition;

	public Vector3 EndPosition;

	public Vector3 Position;

	public Vector3 TargetPosition;

	public bool IsDone;

	public bool HasStarted;

	protected float timer;

	public bool IsBlocking = true;
}
