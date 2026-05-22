using System;
using UnityEngine;
using UnityEngine.UI;

public class MultilineScrollbarHider : MonoBehaviour
{
	private void Update()
	{
		bool flag = this.Scrollbar.size < 1f || this.Scrollbar.value != 0f;
		this.Scrollbar.gameObject.SetActive(flag);
	}

	public Scrollbar Scrollbar;
}
