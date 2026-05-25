using System;
using System.Collections.Generic;
using UnityEngine;

public class ShrineCardData : CardData
{
	[ExtraData("max_shrine_slots")]
	public int MaxSlots = 2;

	private string _cachedRelicsHash = "";

	public override bool UsesHorizontalSlots
	{
		get
		{
			return true;
		}
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (this.MyGameCard == null) return false;

		// Đếm số lượng card con hiện tại trong stack
		int childCount = 0;
		GameCard curr = this.MyGameCard.Child;
		while (curr != null)
		{
			childCount++;
			curr = curr.Child;
		}

		// Giới hạn số slot của Đền Thờ
		if (childCount >= MaxSlots) return false;

		// Đền Thờ chỉ chấp nhận thẻ Cống Phẩm và Cổ Vật thông qua các thuộc tính generic của CardData
		return otherCard.IsShrineOffering || otherCard.IsAncientRelic;
	}

	public override void UpdateCard()
	{
		base.UpdateCard();

		if (this.MyGameCard != null)
		{
			// 1. Kiểm tra sự thay đổi của các cổ vật trong stack đền thờ (Event-driven)
			string currentHash = GetRelicsHash();
			if (currentHash != _cachedRelicsHash)
			{
				_cachedRelicsHash = currentHash;
				// Bắn sự kiện lên Event Bus để báo cho RelicAutomationSystem cập nhật lập tức
				EventBus.Publish(new OnShrineStackChangedEvent(this));
			}

			// 2. Cập nhật mô tả thẻ theo số slot hiện tại
			this.descriptionOverride = MewtationsLoc.TranslateFormat("shrine_desc_format", 
				"Đền Thờ Thần Mèo cổ kính. Nơi trang bị Cổ Vật và hiến tế Cống Phẩm để mở khóa sức mạnh tự động hóa.\n\n• <b>Số ô Cổ Vật tối đa:</b> <color=#ffdd22>{0}</color>\n• Đặt thẻ <b>Cống Phẩm</b> vào ô để mở rộng thêm ô Đền Thờ vĩnh viễn.", 
				MaxSlots);

			// 3. Quản lý timer dâng nạp Cống Phẩm
			if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "upgrade_shrine")
			{
				if (!HasOfferingInStack())
				{
					this.MyGameCard.CancelTimer("upgrade_shrine");
				}
			}
			else if (!this.MyGameCard.TimerRunning && HasOfferingInStack())
			{
				// Chạy tiến trình tiêu thụ cống phẩm để tăng slot
				this.MyGameCard.StartTimer(15.0f, new TimerAction(this.UpgradeShrineSlots), MewtationsLoc.Translate("shrine_upgrading", "Đang dâng nạp Cống Phẩm lên Đền Thờ..."), "upgrade_shrine", true, false, false);
			}
		}
	}

	private string GetRelicsHash()
	{
		if (this.MyGameCard == null) return "";
		string hash = "";
		GameCard curr = this.MyGameCard.Child;
		while (curr != null)
		{
			if (curr.CardData != null && !curr.Destroyed && curr.CardData.IsAncientRelic)
			{
				hash += curr.CardData.Id + ",";
			}
			curr = curr.Child;
		}
		return hash;
	}

	private bool HasOfferingInStack()
	{
		if (this.MyGameCard == null) return false;
		GameCard curr = this.MyGameCard.Child;
		while (curr != null)
		{
			if (curr.CardData != null && curr.CardData.IsShrineOffering)
			{
				return true;
			}
			curr = curr.Child;
		}
		return false;
	}

	[TimedAction("upgrade_shrine")]
	public void UpgradeShrineSlots()
	{
		if (this.MyGameCard == null) return;

		// Tiêu hủy 1 thẻ cống phẩm trong stack
		GameCard curr = this.MyGameCard.Child;
		bool foundAndDestroyed = false;
		while (curr != null)
		{
			if (curr.CardData != null && curr.CardData.IsShrineOffering && !curr.Destroyed)
			{
				curr.DestroyCard(true, true);
				foundAndDestroyed = true;
				break;
			}
			curr = curr.Child;
		}

		if (foundAndDestroyed)
		{
			// Tăng slot đền thờ
			MaxSlots++;

			// Bắn sự kiện thay đổi stack để tự động cập nhật registry
			EventBus.Publish(new OnShrineStackChangedEvent(this));

			string title = MewtationsLoc.Translate("shrine_upgraded_title", "☯️ ĐỀN THỜ THĂNG CẤP!");
			string text = MewtationsLoc.TranslateFormat("shrine_upgraded_desc", 
				"Thần Mèo đã tiếp nhận Cống Phẩm thành kính của bạn!\n\nLinh quang rực rỡ bùng phát từ đền thờ, mở rộng không gian trận pháp.\n🌟 <b>Số ô Cổ Vật tối đa tăng lên:</b> <color=#ffdd22>{0} ô</color>!", 
				MaxSlots);

			if (Mewtations.Dialogue.DialogueSystem.Instance != null)
			{
				Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { MewtationsLoc.Translate("btn_wonderful", "Tuyệt vời!") }, (cIdx) => {});
			}
		}
	}
}
