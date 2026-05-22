using System;
using TMPro;
using UnityEngine;

public class NotificationElement : MonoBehaviour
{
	private void Start()
	{
		this.Button.Clicked += delegate
		{
			Action onClicked = this.OnClicked;
			if (onClicked != null)
			{
				onClicked();
			}
			Object.Destroy(base.gameObject);
		};
	}

	private void Update()
	{
		this.timer += WorldManager.instance.TimeScale * Time.deltaTime;
		if (this.timer > 30f)
		{
			Object.Destroy(base.gameObject);
		}
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.one, Time.deltaTime * 12f);
	}

	public CustomButton Button;

	public TextMeshProUGUI NotificationTitle;

	public TextMeshProUGUI NotificationText;

	public Action OnClicked;

	private float timer;
}
