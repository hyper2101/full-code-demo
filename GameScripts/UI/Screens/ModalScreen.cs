using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ModalScreen : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private void Awake()
	{
		ModalScreen.instance = this;
	}

	public void Clear()
	{
		foreach (object obj in this.ButtonParent)
		{
			Object.Destroy(((RectTransform)obj).gameObject);
		}
		foreach (object obj2 in this.InputParent)
		{
			Object.Destroy(((RectTransform)obj2).gameObject);
		}
	}

	public void SetTexts(string title, string text)
	{
		this.TitleText.text = title;
		this.TextText.text = text;
	}

	public void AddOption(string text, Action action)
	{
		CustomButton customButton = Object.Instantiate<CustomButton>(this.ButtonPrefab);
		customButton.transform.SetParentClean(this.ButtonParent);
		customButton.TextMeshPro.text = text;
		customButton.Clicked += action;
	}

	public TMP_InputField AddInput(string confirmText, Action<string> action)
	{
		TMP_InputField input = Object.Instantiate<TMP_InputField>(this.InputPrefab);
		input.transform.SetParentClean(this.InputParent);
		CustomButton customButton = Object.Instantiate<CustomButton>(this.ButtonPrefab);
		customButton.transform.SetParentClean(this.ButtonParent);
		customButton.TextMeshPro.text = confirmText;
		customButton.Clicked += delegate
		{
			action(input.text);
		};
		return input;
	}

	public TMP_InputField AddInputNoButton()
	{
		TMP_InputField tmp_InputField = Object.Instantiate<TMP_InputField>(this.InputPrefab);
		tmp_InputField.transform.SetParentClean(this.InputParent);
		return tmp_InputField;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int num = -1;
		if (Mouse.current != null)
		{
			num = TMP_TextUtilities.FindIntersectingLink(this.TextText, new Vector3(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue(), 0f), null);
		}
		if (num != -1)
		{
			TMP_LinkInfo tmp_LinkInfo = this.TextText.textInfo.linkInfo[num];
			if (tmp_LinkInfo.GetLinkID().StartsWith("https://"))
			{
				Debug.Log(string.Concat(new string[]
				{
					"Clicked '",
					tmp_LinkInfo.GetLinkText(),
					", opening '",
					tmp_LinkInfo.GetLinkID(),
					"' in browser"
				}));
				Application.OpenURL(tmp_LinkInfo.GetLinkID());
			}
		}
	}

	public static ModalScreen instance;

	public CustomButton ButtonPrefab;

	public RectTransform ButtonParent;

	public TMP_InputField InputPrefab;

	public RectTransform InputParent;

	public TextMeshProUGUI TitleText;

	public TextMeshProUGUI TextText;
}
