using System;

public class QueuedAnimation
{
	public QueuedAnimation(Action act, string id = null)
	{
		this.OnActivate = act;
		this.Id = id;
	}

	public Action OnActivate;

	public string Id;
}
