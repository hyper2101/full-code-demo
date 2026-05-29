using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpdatePopup : MonoBehaviour
{
	private void Awake()
	{
		this.CloseUpdateInfoButton.Clicked += delegate
		{
			if (PlatformHelper.IsTestBuild && WorldManager.instance.IsCitiesDlcActive())
			{
				GameCanvas.instance.ShowEarlyAccessModal();
			}
			base.gameObject.SetActive(false);
		};
		this.CloseUpdateInfoButton.ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
		{
			nav.selectOnUp = (this.BuyDLCButton.gameObject.activeInHierarchy ? this.BuyDLCButton : null);
			nav.selectOnDown = (nav.selectOnLeft = (nav.selectOnRight = null));
			return nav;
		};
		this.BuyDLCButton.ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
		{
			nav.selectOnUp = (nav.selectOnLeft = (nav.selectOnRight = null));
			nav.selectOnDown = this.CloseUpdateInfoButton;
			return nav;
		};
		this.BuyDLCButton.Clicked += delegate
		{
			SteamFriends.ActivateGameOverlayToWebPage("https://store.steampowered.com/app/2867570/Stacklands_2000", EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Default);
		};
		this.UpdatePopupText();
	}

	private void OnEnable()
	{
		EventSystem.current.SetSelectedGameObject(this.CloseUpdateInfoButton.gameObject);
	}

	private void Update()
	{
		this.UpdatePopupText();
	}

	private void UpdatePopupText()
	{
		this.UpdateTitle.text = MewtationsLoc.Translate("label_update_title_cities");
		if (WorldManager.instance.IsCitiesDlcActive())
		{
			this.UpdateText.text = MewtationsLoc.Translate("label_update_text_cities");
			this.BuyDLCButton.gameObject.SetActive(false);
			return;
		}
		this.UpdateText.text = MewtationsLoc.Translate("label_update_text_cities_locked");
		this.BuyDLCButton.gameObject.SetActive(true);
	}

	public TextMeshProUGUI UpdateText;

	public TextMeshProUGUI UpdateTitle;

	public CustomButton CloseUpdateInfoButton;

	public CustomButton BuyDLCButton;
}
