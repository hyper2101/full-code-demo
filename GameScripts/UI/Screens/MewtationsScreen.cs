using System;
using UnityEngine;

public class MewtationsScreen : MonoBehaviour
{
	public RectTransform Rect
	{
		get
		{
			return base.gameObject.GetComponent<RectTransform>();
		}
	}

	public virtual bool IsFrameRateUncapped { get; }
}
