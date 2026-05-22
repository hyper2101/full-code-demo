using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class Colorizer : MonoBehaviour
{
	private void OnValidate()
	{
		this.SetColors();
	}

	private void Start()
	{
		this.SetColors();
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			this.SetColors();
		}
	}

	private void SetColors()
	{
		if (ColorManager.instance == null)
		{
			return;
		}
		Image component = base.GetComponent<Image>();
		if (component != null)
		{
			component.color = ColorManager.instance.GetColor(this.Color);
		}
		TextMeshProUGUI component2 = base.GetComponent<TextMeshProUGUI>();
		if (component2 != null)
		{
			component2.color = ColorManager.instance.GetColor(this.Color);
		}
	}

	public UIColor Color;
}
